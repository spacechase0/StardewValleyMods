using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game;
using MoonMisadventures.Game.Items;
using MoonMisadventures.Game.Locations;
using MoonMisadventures.Game.Monsters;
using MoonMisadventures.Game.Projectiles;
using MoonMisadventures.VirtualProperties;
using Netcode;
using Newtonsoft.Json.Linq;
using SpaceCore.Events;
using SpaceCore.Interface;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.GameData.WorldMaps;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

/* Art:
 *  paradigmnomad (most art)
 *  finalbossblues https://finalbossblues.itch.io/dark-dimension-tileset (recolored by paradigmnomad)
 *  ... more ...
 * Music:
 *  https://lowenergygirl.itch.io/space-journey (Into the Spaceship)
 */

namespace MoonMisadventures
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public Configuration Config;

        internal static DepthStencilState DefaultStencilOverride = null;
        internal static DepthStencilState StencilBrighten = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };
        internal static DepthStencilState StencilDarken = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 0,
            DepthBufferEnable = false,
        };
        internal static DepthStencilState StencilRenderOnDark = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.NotEqual,
            StencilPass = StencilOperation.Keep,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        public override void Entry( IModHelper helper )
        {
            I18n.Init(helper.Translation);

            Log.Monitor = Monitor;
            instance = this;
            I18n.Init(Helper.Translation);

            Config = Helper.ReadConfig<Configuration>();

            Assets.Load( helper.ModContent );
            SoundEffect mainMusic = SoundEffect.FromFile( Path.Combine( Helper.DirectoryPath, "assets", "into-the-spaceship.wav" ) );
            Game1.soundBank.AddCue( new CueDefinition( "into-the-spaceship", mainMusic, 2, loop: true ) );

            SoundEffect laser = SoundEffect.FromFile(Path.Combine(Helper.DirectoryPath, "assets", "laserShoot.wav"));
            Game1.soundBank.AddCue(new CueDefinition("mm_laser", laser, 3));

            Helper.ConsoleCommands.Add( "mm_key", "Gives you the lunar key.", OnKeyCommand );
            Helper.ConsoleCommands.Add( "mm_infuse", "Opens the celestial infuser menu.", OnInfuseCommand );

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Specialized.LoadStageChanged += OnLoadStageChanged;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Display.RenderingWorld += OnRenderingWorld;
            Helper.Events.Display.RenderedWorld += OnRenderedWorld;
            Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.Content.AssetRequested += OnAssetRequested;

            SpaceEvents.AddWalletItems += AddWalletItems;
            SpaceEvents.AfterGiftGiven += AfterGiftGiven;

            var necklaceDef = new NecklaceDataDefinition();
            ItemRegistry.ItemTypes.Add(necklaceDef);
            Helper.Reflection.GetField< Dictionary<string, IItemDataDefinition>>( typeof( ItemRegistry ), "IdentifierLookup" ).GetValue()[necklaceDef.Identifier] = necklaceDef;

            var harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
            //harmony.Patch( AccessTools.Method( "StardewValley.Game1:_draw" ), transpiler: new HarmonyMethod( typeof( Patches.Game1CatchLightingRenderPatch ).GetMethod( "Transpiler" ) ) );
        }

        public T Load<T>( IAssetInfo asset )
        {
            return default( T );
        }
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            //Assets.ApplyEdits(e);

            if (e.NameWithoutLocale.IsEquivalentTo(ModManifest.UniqueID + "/Necklaces"))
            {
                e.LoadFrom(() => new Dictionary<string, NecklaceData>
                {
                    { "looting", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Looting_Name(),
                        Description = I18n.Item_Necklace_Looting_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 0,
                    } },
                    { "shocking", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Shocking_Name(),
                        Description = I18n.Item_Necklace_Shocking_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 1,
                    } },
                    { "speed", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Speed_Name(),
                        Description = I18n.Item_Necklace_Speed_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 2,
                    } },
                    { "health", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Health_Name(),
                        Description = I18n.Item_Necklace_Health_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 3,
                    } },
                    { "cooling", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Cooling_Name(),
                        Description = I18n.Item_Necklace_Cooling_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 4,
                    } },
                    { "lunar", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Lunar_Name(),
                        Description = I18n.Item_Necklace_Lunar_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 5,
                        CanBeSelectedAtAltar = false,
                    } },
                    { "water", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Water_Name(),
                        Description = I18n.Item_Necklace_Water_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 6,
                    } },
                    { "sea", new NecklaceData()
                    {
                        DisplayName = I18n.Item_Necklace_Sea_Name(),
                        Description = I18n.Item_Necklace_Sea_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/necklaces.png",
                        TextureIndex = 7,
                    } },
                }, AssetLoadPriority.Exclusive);
            }
            foreach (string file in Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "dga")))
            {
                string filename = Path.GetFileName(file);
                if (e.NameWithoutLocale.Name.EndsWith(".png") && e.NameWithoutLocale.IsEquivalentTo("spacechase0.MoonMisadventures/assets/" + filename))
                {
                    e.LoadFrom(() => Helper.ModContent.Load<Texture2D>("assets/dga/" + filename), AssetLoadPriority.Exclusive);
                }
            }
            if (e.NameWithoutLocale.IsEquivalentTo( "TerrainFeatures/hoeDirt" ) && Game1.currentLocation is LunarLocation)
            {
                e.LoadFrom(() => Helper.ModContent.Load<Texture2D>("assets/hoedirt.png"), AssetLoadPriority.High);
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/BuildingsData"))
            {
                e.Edit((asset) =>
                {
                    var bData = asset.AsDictionary<string, BuildingData>().Data;
                    bData.Add("spacechase0.MoonMisadventures_MoonObelisk", new()
                    {
                        Name = I18n.Building_Obelisk_Name(),
                        Description = I18n.Building_Obelisk_Description(),
                        Texture = "spacechase0.MoonMisadventures/assets/obelisk.png",
                        BuildMaterials = new[]
                        {
                            new BuildingMaterial()
                            {
                                ItemId = ItemIds.MythiciteBar,
                                Amount = 10,
                            },
                            new BuildingMaterial()
                            {
                                ItemId = ItemIds.StellarEssence,
                                Amount = 25,
                            },
                            new BuildingMaterial()
                            {
                                ItemId = ItemIds.SoulSapphire,
                                Amount = 3,
                            },
                        }.ToList(),
                        BuildCost = 2000000,
                        BuildCondition = "PLAYER_HAS_FLAG Any firstUfoTravel",
                        Size = new(3, 2),
                        DefaultAction = "ObeliskWarp Custom_MM_MoonFarm 7 11 true",
                    });
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/LocationContexts"))
            {
                e.Edit((asset) =>
                {
                    var locData = asset.AsDictionary<string, LocationContextData>().Data;
                    locData.Add("Moon", new()
                    {
                        DefaultValidPlantableLocations = new[] { "Custom_MM_MoonFarm" }.ToList(),
                        PlayRandomAmbientSounds = false,
                        AllowRainTotem = false,
                        WeatherConditions = new[]
                        {
                            new WeatherCondition()
                            {
                                Condition = "",
                                Weather = "Sun",
                            }
                        }.ToList(),
                        ReviveLocations = new[]
                        {
                            new ReviveLocation()
                            {
                                Location = "Custom_MM_MoonLandingArea",
                                Position = new( 9, 31 )
                            }
                        }.ToList(),
                        PassOutLocations = new[]
                        {
                            new ReviveLocation()
                            {
                                Location = "Custom_MM_MoonFarm",
                                Position = new(7, 11)
                            }
                        }.ToList(),
                    });
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/WorldMap"))
            {
                e.Edit((asset) =>
                {
                    var regionData = new WorldMapRegionData();
                    regionData.BaseTexture.Add(new()
                    {
                        Id = "moon_bg",
                        Texture = "spacechase0.MoonMisadventures/assets/map.png",
                        SourceRect = new Rectangle( 0, 0, 300, 180 )
                    });
                    var mapData = regionData.MapAreas;
                    mapData.Add(new WorldMapAreaData()
                    {
                        Id = "moon_farm",
                        PixelArea = new Rectangle(194, 91, 24, 24),
                        WorldPositions = new(new[] {
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonFarm",
                                TileArea = new Rectangle(0, 0, 49, 39),
                                MapPixelArea = new Rectangle(194, 91, 24, 24),
                            },
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonFarmHouse",
                                TileArea = new Rectangle(0, 0, 19, 11),
                                MapPixelArea = new Rectangle(199, 97, 1, 1),
                            },
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonFarmCave",
                                TileArea = new Rectangle(0, 0, 19, 11),
                                MapPixelArea = new Rectangle(220, 106, 1, 1),
                            },
                        }),
                        Tooltips = new(new[] {
                            new WorldMapTooltipData()
                            {
                                Text = I18n.Location_LunarFarm(),
                            }
                        }),
                        ScrollText = I18n.Location_LunarFarm(),
                    });
                    mapData.Add(new WorldMapAreaData()
                    {
                        Id = "moon_planetview",
                        PixelArea = new Rectangle(216, 82, 7, 11),
                        WorldPositions = new(new[] {
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonPlanetOverlook",
                                TileArea = new Rectangle(0, 0, 49, 39),
                                MapPixelArea = new Rectangle(216, 82, 7, 11),
                            },
                        }),
                        ScrollText = I18n.Location_PlanetOverlook(),
                    });
                    mapData.Add(new WorldMapAreaData()
                    {
                        Id = "moon_temple",
                        PixelArea = new Rectangle(170, 91, 9, 12),
                        WorldPositions = new(new[] {
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonInfuserRoom",
                                TileArea = new Rectangle(0, 0, 29, 29),
                                MapPixelArea = new Rectangle(170, 91, 9, 12),
                            },
                        }),
                        ScrollText = I18n.Location_MoonTemple(),
                    });
                    mapData.Add(new WorldMapAreaData()
                    {
                        Id = "moon_landingarea",
                        PixelArea = new Rectangle(165, 109, 26, 21),
                        WorldPositions = new(new[] {
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonLandingArea",
                                TileArea = new Rectangle(0, 0, 34, 37),
                                MapPixelArea = new Rectangle(165, 109, 26, 21),
                            },
                        }),
                        Tooltips = new(new[] {
                            new WorldMapTooltipData()
                            {
                                Text = I18n.Location_LandingArea(),
                            }
                        }),
                        ScrollText = I18n.Location_LandingArea(),
                    });
                    mapData.Add(new WorldMapAreaData()
                    {
                        Id = "moon_asteroidsentrance",
                        PixelArea = new Rectangle(147, 89, 13, 17),
                        WorldPositions = new(new[] {
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonAsteroidsEntrance",
                                TileArea = new Rectangle(0, 0, 49, 94),
                                MapPixelArea = new Rectangle(147, 89, 13, 17),
                            },
                        }),
                        Tooltips = new(new[] {
                            new WorldMapTooltipData()
                            {
                                Text = I18n.Location_AsteroidsEntrance(),
                            }
                        }),
                        ScrollText = I18n.Location_AsteroidsEntrance(),
                    });
                    mapData.Add(new WorldMapAreaData()
                    {
                        Id = "moon_asteroids",
                        PixelArea = new Rectangle(64, 40, 1, 1),
                        WorldPositions = new(new[] {
                            new WorldMapAreaPositionData()
                            {
                                LocationContext = "Moon",
                                LocationName = "Custom_MM_MoonAsteroidsDungeon",
                                TileArea = new Rectangle(0, 0, 149, 149),
                                MapPixelArea = new Rectangle(64, 40, 1, 1),
                            },
                        }),
                        Tooltips = new(new[] {
                            new WorldMapTooltipData()
                            {
                                Text = I18n.Location_Asteroids(),
                            }
                        }),
                        ScrollText = I18n.Location_Asteroids(),
                    });
                    // TODO: Mountain top
                    /*
                    mapData.Add("mountaintop", new()
                    {
                        AreaID = "mountaintop",
                        Group = "SDV",
                        Texture = "LooseSprites/map",
                        Zones = new(new[] {
                            new WorldMapAreaZone()
                            {
                                ValidAreas = new List<string>( new[] { "Custom_MM_MountainTop" } ),
                                MapTileCorners = "0 0 47 47",
                                MapImageCorners = "210 1 211 1",
                                DisplayName = "???",
                            },
                        }),
                    });
                    */
                    asset.AsDictionary<string, WorldMapRegionData>().Data.Add("Moon", regionData);
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectContextTags"))
            {
                e.Edit((asset) =>
                {
                    var tagsData = asset.AsDictionary<string, string>().Data;
                    tagsData.Add("LunarWheatSeeds", "plantable_context_moon");
                    tagsData.Add("SunbloomSeeds", "plantable_context_moon");
                    tagsData.Add("StarPetalSeeds", "plantable_context_moon");
                    tagsData.Add("VoidBlossomSeeds", "plantable_context_moon");
                    tagsData.Add("SoulSproutSeeds", "plantable_context_moon");
                    return;
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation") ||
                e.NameWithoutLocale.IsEquivalentTo("Data/Boots"))
            {
                string type = e.NameWithoutLocale.IsEquivalentTo("Data/Boots") ? "Boots" : "Object";

                e.Edit((asset) =>
                {
                    var data = Helper.ModContent.Load<Dictionary<string, JToken>>("assets/item-data.json");
                    foreach (var entry in data)
                    {
                        if (entry.Key.StartsWith(type+":"))
                        {
                            string key = entry.Key.Substring(type.Length + 1);
                            string val = entry.Value.ToString();
                            while (val.Contains("{{i18n:"))
                            {
                                int x = val.IndexOf("{{i18n:");
                                int y = val.IndexOf("}}", x);

                                val = val.Substring(0, x) + Helper.Translation.Get(val.Substring(x + "{{i18n:".Length, y - x - "{{i18n:".Length)) + val.Substring(y + 2);
                            }
                            asset.AsDictionary<string, string>().Data.Add(ModManifest.UniqueID + "_" + key, val);
                        }
                    }
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/FarmAnimals"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, FarmAnimalData>().Data;
                    dict.Add("Lunar Cow", new()
                    {
                        DisplayName = I18n.FarmAnimal_LunarCow(),
                        House = "Barn",
                        DaysToMature = 5,
                        CanGetPregnant = true,
                        HarvestType = FarmAnimalHarvestType.HarvestWithTool,
                        HarvestTool = "Milk Pail",
                        ProduceItemIds = new( new[]
                        {
                            new FarmAnimalProduce()
                            {
                                Id = "Default",
                                ItemId = "spacechase0.MoonMisadventures_GalaxyMilk",
                            }
                        } ),
                        DeluxeProduceItemIds = new(new[]
                        {
                            new FarmAnimalProduce()
                            {
                                Id = "Default",
                                ItemId = "spacechase0.MoonMisadventures_GalaxyMilk",
                            }
                        }),
                        ProfessionForHappinessBoost = 3,
                        ProfessionForQualityBoost = 3,
                        ProfessionForFasterProduce = -1,
                        Sound = "cow",
                        Texture = "spacechase0.MoonMisadventures/assets/cow.png",
                        SpriteWidth = 32,
                        SpriteHeight = 32,
                        GrassEatAmount = 4,
                        HappinessDrain = 10,
                        UpDownPetHitboxTileSize = new Vector2(1, 1.75f),
                        LeftRightPetHitboxTileSize = new Vector2(1.75f, 1.25f),
                        BabyUpDownPetHitboxTileSize = new Vector2(1, 1.75f),
                        BabyLeftRightPetHitboxTileSize = new Vector2(1.75f, 1),
                    });
                    dict.Add("Lunar Chicken", new()
                    {
                        DisplayName = I18n.FarmAnimal_LunarChicken(),
                        House = "Coop",
                        DaysToMature = 3,
                        CanGetPregnant = false,
                        HarvestType = FarmAnimalHarvestType.DropOvernight,
                        ProduceItemIds = new(new[]
                        {
                            new FarmAnimalProduce()
                            {
                                Id = "Default",
                                ItemId = "spacechase0.MoonMisadventures_GalaxyEgg",
                            }
                        }),
                        DeluxeProduceItemIds = new(new[]
                        {
                            new FarmAnimalProduce()
                            {
                                Id = "Default",
                                ItemId = "spacechase0.MoonMisadventures_GalaxyEgg",
                            }
                        }),
                        ProfessionForHappinessBoost = 2,
                        ProfessionForQualityBoost = 2,
                        ProfessionForFasterProduce = -1,
                        Sound = "cluck",
                        Texture = "spacechase0.MoonMisadventures/assets/chicken.png",
                        SpriteWidth = 16,
                        SpriteHeight = 16,
                        EmoteOffset = new( 0, -16 ),
                        GrassEatAmount = 2,
                        HappinessDrain = 14,
                        UpDownPetHitboxTileSize = new Vector2(1, 1),
                        LeftRightPetHitboxTileSize = new Vector2(1, 1),
                        BabyUpDownPetHitboxTileSize = new Vector2(1, 1),
                        BabyLeftRightPetHitboxTileSize = new Vector2(1, 1),
                    });
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, MachineData>().Data;

                    dict["(BC)13"].OutputRules.Add(new()
                    {
                        Id = "spacechase0.MoonMisadventures_SmeltMythiciteOre",
                        Triggers = new(new[]
                        {
                            new MachineOutputTriggerRule()
                            {
                                Trigger = MachineOutputTrigger.ItemPlacedInMachine,
                                RequiredItemId = "(O)spacechase0.MoonMisadventures_MythiciteOre",
                                RequiredCount = 5,
                            }
                        }),
                        OutputItem = new(new[]
                        {
                            new MachineItemOutput()
                            {
                                Id = "(O)spacechase0.MoonMisadventures_MythiciteBar",
                                ItemId = "(O)spacechase0.MoonMisadventures_MythiciteBar",
                            }
                        }),
                        MinutesUntilReady = 720,
                    });
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Tools"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, ToolData>().Data;

                    dict.Add("spacechase0.MoonMisadventures.AnimalGloves",
                             new()
                             {
                                 ClassName = "AnimalGauntlets, MoonMisadventures",
                                 Name = "AnimalGauntlets",
                                 DisplayName = I18n.Tool_AnimalGauntlets_Name(),
                                 Description = I18n.Tool_AnimalGauntlets_Description(),
                                 Texture = "spacechase0.MoonMisadventures/assets/animal-gauntlets.png",
                                SpriteIndex = 0,
                             });
                    dict.Add("spacechase0.MoonMisadventures.LaserGun",
                             new()
                             {
                                 ClassName = "LaserGun, MoonMisadventures",
                                 Name = "LaserGun",
                                 DisplayName = I18n.Tool_LaserGun_Name(),
                                 Description = I18n.Tool_LaserGun_Description(),
                                 Texture = "spacechase0.MoonMisadventures/assets/animal-gauntlets.png",
                                 SpriteIndex = 0,
                             });
                    dict.Add("spacechase0.MoonMisadventures.RadioactiveAxe",
                             new()
                             {
                                 ClassName = "Axe",
                                 Name = "Radioactive Axe",
                                 SalePrice = 100000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:Axe.cs.1]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:Axe.cs.14019]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-radioactive.png",
                                 SpriteIndex = 238,
                                 MenuSpriteIndex = 264,
                                 UpgradeLevel = 5,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)IridiumAxe",
                                 CanBeLostOnDeath = false,
                             });
                    dict.Add("spacechase0.MoonMisadventures.MythiciteAxe",
                             new()
                             {
                                 ClassName = "Axe",
                                 Name = "Mythicite Axe",
                                 SalePrice = 250000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:Axe.cs.1]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:Axe.cs.14019]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-mythicite.png",
                                 SpriteIndex = 238,
                                 MenuSpriteIndex = 264,
                                 UpgradeLevel = 6,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)RadioactiveAxe",
                                 CanBeLostOnDeath = false,
                             });
                    dict.Add("spacechase0.MoonMisadventures.RadioactivePickaxe",
                             new()
                             {
                                 ClassName = "Pickaxe",
                                 Name = "Radioactive Pickaxe",
                                 SalePrice = 100000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:Pickaxe.cs.14184]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:Pickaxe.cs.14185]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-radioactive.png",
                                 SpriteIndex = 154,
                                 MenuSpriteIndex = 180,
                                 UpgradeLevel = 5,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)IridiumPickaxe",
                                 CanBeLostOnDeath = false,
                             });
                    dict.Add("spacechase0.MoonMisadventures.MythicitePickaxe",
                             new()
                             {
                                 ClassName = "Pickaxe",
                                 Name = "Mythicite Pickaxe",
                                 SalePrice = 250000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:Pickaxe.cs.14184]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:Pickaxe.cs.14185]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-mythicite.png",
                                 SpriteIndex = 154,
                                 MenuSpriteIndex = 180,
                                 UpgradeLevel = 6,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)RadioactivePickaxe",
                                 CanBeLostOnDeath = false,
                             });
                    dict.Add("spacechase0.MoonMisadventures.RadioactiveWateringCan",
                             new()
                             {
                                 ClassName = "WateringCan",
                                 Name = "Radioactive Watering Can",
                                 SalePrice = 100000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:WateringCan.cs.14324]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:WateringCan.cs.14325]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-radioactive.png",
                                 SpriteIndex = 322,
                                 MenuSpriteIndex = 345,
                                 UpgradeLevel = 5,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)IridiumWateringCan",
                                 CanBeLostOnDeath = false,
                             });
                    dict.Add("spacechase0.MoonMisadventures.MythiciteWateringCan",
                             new()
                             {
                                 ClassName = "WateringCan",
                                 Name = "Mythicite Watering Can",
                                 SalePrice = 250000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:WateringCan.cs.14324]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:WateringCan.cs.14325]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-mythicite.png",
                                 SpriteIndex = 322,
                                 MenuSpriteIndex = 345,
                                 UpgradeLevel = 6,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)RadioactiveWateringCan",
                                 CanBeLostOnDeath = false,
                             });
                    dict.Add("spacechase0.MoonMisadventures.RadioactiveHoe",
                             new()
                             {
                                 ClassName = "Hoe",
                                 Name = "Radioactive Hoe",
                                 SalePrice = 100000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:Hoe.cs.14101]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:Hoe.cs.14102]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-radioactive.png",
                                 SpriteIndex = 70,
                                 MenuSpriteIndex = 96,
                                 UpgradeLevel = 5,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)IridiumHoe",
                                 CanBeLostOnDeath = false,
                             });
                    dict.Add("spacechase0.MoonMisadventures.MythiciteHoe",
                             new()
                             {
                                 ClassName = "Hoe",
                                 Name = "Mythicite Hoe",
                                 SalePrice = 250000,
                                 DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:Hoe.cs.14101]",
                                 Description = "[LocalizedText Strings\\StringsFromCSFiles:Hoe.cs.14102]",
                                 Texture = "spacechase0.MoonMisadventures/assets/tools-mythicite.png",
                                 SpriteIndex = 70,
                                 MenuSpriteIndex = 96,
                                 UpgradeLevel = 6,
                                 ApplyUpgradeLevelToDisplayName = true,
                                 ConventionalUpgradeFrom = "(T)RadioactiveHoe",
                                 CanBeLostOnDeath = false,
                             });
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit((asset) =>
                {
                    var locs = asset.Data as Dictionary<string, LocationData>;

                    // TODO: Move over to CreateLocations

                    locs.Add("Custom_MM_MountainTop", new()
                    {
                        DisplayName = I18n.Location_MountainTop(),
                        DefaultArrivalTile = new(22, 24),
                    });
                    locs.Add("Custom_MM_MoonLandingArea", new()
                    {
                        DisplayName = I18n.Location_LandingArea(),
                        DefaultArrivalTile = new(9, 31),
                    });
                    locs.Add("Custom_MM_MoonAsteroidsEntrance", new()
                    {
                        DisplayName = I18n.Location_AsteroidsEntrance(),
                        DefaultArrivalTile = new(25, 26),
                    });
                    locs.Add("Custom_MM_MoonFarm", new()
                    {
                        DisplayName = I18n.Location_LunarFarm(),
                        DefaultArrivalTile = new(7,11),
                    });
                    locs.Add("Custom_MM_MoonFarmCave", new()
                    {
                        DisplayName = I18n.Location_LunarFarm(),
                        DefaultArrivalTile = new(6, 8),
                    });
                    locs.Add("Custom_MM_MoonFarmHouse", new()
                    {
                        DisplayName = I18n.Location_LunarFarm(),
                        DefaultArrivalTile = new(9, 9),
                    });
                    locs.Add("Custom_MM_MoonPlanetOverlook", new()
                    {
                        DisplayName = I18n.Location_PlanetOverlook(),
                        DefaultArrivalTile = new(24, 31),
                    });
                    locs.Add("Custom_MM_UfoInterior", new()
                    {
                        DisplayName = I18n.Location_UfoInterior(),
                        DefaultArrivalTile = new(12, 15),
                    });
                    locs.Add("Custom_MM_MoonInfuserRoom", new()
                    {
                        DisplayName = I18n.Location_MoonTemple(),
                        DefaultArrivalTile = new(15, 22),
                    });
                });
            }

            string currType = null;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Crops"))
                currType = "Crop";
            if (e.NameWithoutLocale.IsEquivalentTo("Data/weapons"))
                currType = "Weapon";
            if (currType == null)
                return;

            e.Edit((asset) =>
            {
                var data = Helper.ModContent.Load<Dictionary<string, JToken>>("assets/item-data.json");
                foreach (var entry in data)
                {
                    if (entry.Key.StartsWith(currType + ":") && entry.Value is JObject jobj)
                    {

                        string key = entry.Key.Substring(currType.Length + 1);

                        switch (currType)
                        {
                            case "Crop":
                                asset.AsDictionary<string, CropData>().Data.Add(ModManifest.UniqueID + "_" + key, jobj.ToObject<CropData>());
                                break;
                            case "Weapon":
                                asset.AsDictionary<string, WeaponData>().Data.Add(ModManifest.UniqueID + "_" + key, jobj.ToObject<WeaponData>());
                                break;

                        }
                    }
                }
            });
        }

        private void OnKeyCommand( string cmd, string[] args )
        {
            //Game1.player.addItemByMenuIfNecessary(new Necklace("speed"));
            Game1.player.team.get_hasLunarKey().Value = true;
        }

        private void OnInfuseCommand( string cmd, string[] args )
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new InfuserMenu();
        }

        private void OnGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(this.Config),
                    titleScreenOnly: true
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_FlashingUfo_Name,
                    tooltip: I18n.Config_FlashingUfo_Description,
                    getValue: () => this.Config.FlashingUfo,
                    setValue: value => this.Config.FlashingUfo = value
                );
            }

            var sc = Helper.ModRegistry.GetApi< ISpaceCoreApi >( "spacechase0.SpaceCore" );
            sc.RegisterSerializerType( typeof( MountainTop ) );
            sc.RegisterSerializerType( typeof( LunarLocation ) );
            sc.RegisterSerializerType( typeof( MoonLandingArea ) );
            sc.RegisterSerializerType( typeof( AsteroidsEntrance ) );
            sc.RegisterSerializerType( typeof( AsteroidsDungeon ) );
            sc.RegisterSerializerType( typeof( BoomEye ) );
            sc.RegisterSerializerType( typeof( BoomProjectile ) );
            sc.RegisterSerializerType( typeof( AsteroidProjectile ) );
            sc.RegisterSerializerType( typeof( LunarFarm ) );
            sc.RegisterSerializerType( typeof( LunarFarmCave ) );
            sc.RegisterSerializerType( typeof( AnimalGauntlets ) );
            sc.RegisterSerializerType( typeof( Necklace ) );
            sc.RegisterSerializerType( typeof( MoonPlanetOverlook ) );
            sc.RegisterSerializerType( typeof( UfoInterior ) );
            sc.RegisterSerializerType( typeof( LunarFarmHouse ) );
            sc.RegisterSerializerType( typeof( MoonInfuserRoom ) );
            sc.RegisterSerializerType( typeof( LunarSlime ) );
            sc.RegisterSerializerType( typeof( UfoInteriorArsenal ) );
            sc.RegisterSerializerType( typeof( CrystalBehemoth ) );
            sc.RegisterCustomProperty( typeof( FarmerTeam ), "hasLunarKey", typeof( NetBool ), AccessTools.Method( typeof( FarmerTeam_LunarKey ), nameof( FarmerTeam_LunarKey.get_hasLunarKey ) ), AccessTools.Method( typeof( FarmerTeam_LunarKey ), nameof( FarmerTeam_LunarKey.set_hasLunarKey ) ) );
            sc.RegisterCustomProperty( typeof( Farmer ), "necklaceItem", typeof( NetRef< Item > ), AccessTools.Method( typeof( Farmer_Necklace ), nameof( Farmer_Necklace.get_necklaceItem ) ), AccessTools.Method( typeof( Farmer_Necklace ), nameof( Farmer_Necklace.set_necklaceItem ) ) );
        }

        private void OnTimeChanged( object sender, TimeChangedEventArgs e )
        {
            AsteroidsDungeon.UpdateLevels10Minutes( e.NewTime );
        }

        private void OnUpdateTicked( object sender, UpdateTickedEventArgs e )
        {
            var necklace = Game1.player.get_necklaceItem().Value as Necklace;
            if ( necklace != null )
            {
                switch ( necklace.ItemId )
                {
                    case "speed":
                        {
                            var buff = Game1.player.buffs.AppliedBuffs.FirstOrDefault( b => b.Key == "necklace" ).Value;
                            if ( buff == null )
                            {
                                buff = new Buff("necklace", "necklace", I18n.Necklace(), 10 * 7000, effects: new BuffEffects() { Speed = { 3 } });
                                Game1.player.buffs.Apply( buff );
                            }
                            buff.millisecondsDuration = 1000;
                        }
                        break;
                    case "cooling":
                        {
                            if ( Game1.player.currentLocation is VolcanoDungeon volcano )
                            {
                                for ( int ix = -1; ix <= 1; ++ix )
                                {
                                    for ( int iy = -1; iy <= 1; ++iy )
                                    {
                                        var spot = Game1.player.Tile + new Vector2( ix, iy );
                                        if ( volcano.isTileOnMap( spot ) && volcano.waterTiles[ ( int ) spot.X, ( int ) spot.Y ] && !volcano.cooledLavaTiles.ContainsKey( spot ) )
                                            volcano.coolLavaEvent.Fire( new Point( ( int ) spot.X, ( int ) spot.Y ) );
                                    }
                                }
                            }
                        }
                        break;
                    case "sea":
                        {
                            if ( Game1.player.CurrentTool is FishingRod fr )
                            {
                                if ( fr.timeUntilFishingBite != -1 )
                                {
                                    fr.fishingBiteAccumulator += (int)(Game1.currentGameTime.ElapsedGameTime.Milliseconds * 1.5);
                                }
                                else if ( Game1.activeClickableMenu is BobberBar bb )
                                {
                                    if ( Helper.Reflection.GetField< bool >( bb, "bobberInBar" ).GetValue() )
                                    {
                                        var distCatchField = Helper.Reflection.GetField<float>( bb, "distanceFromCatching" );
                                        distCatchField.SetValue( distCatchField.GetValue() + 0.003f );
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            AsteroidsDungeon.ClearAllLevels();
        }

        private void OnLoadStageChanged( object sender, LoadStageChangedEventArgs e )
        {
            if ( e.NewStage == LoadStage.CreatedInitialLocations || e.NewStage == LoadStage.SaveAddedLocations )
            {
                Game1.locations.Add( new MountainTop( Helper.ModContent ) );
                Game1.locations.Add( new MoonLandingArea( Helper.ModContent ) );
                Game1.locations.Add( new AsteroidsEntrance( Helper.ModContent ) );
                Game1.locations.Add( new LunarFarm( Helper.ModContent ) );
                Game1.locations.Add( new LunarFarmCave( Helper.ModContent ) );
                Game1.locations.Add( new MoonPlanetOverlook( Helper.ModContent ) );
                Game1.locations.Add( new UfoInterior( Helper.ModContent ) );
                Game1.locations.Add( new LunarFarmHouse( Helper.ModContent ) );
                Game1.locations.Add( new MoonInfuserRoom( Helper.ModContent ) );
                Game1.locations.Add( new UfoInteriorArsenal( Helper.ModContent ) );
            }
        }

        private void OnMenuChanged( object sender, MenuChangedEventArgs e )
        {
            if ( e.NewMenu is ShopMenu shop )
            {
                string clintUpgradeDialogue = Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11474"), Game1.dialogueFont, 304);
                if ( shop.potraitPersonDialogue != clintUpgradeDialogue )
                    return;

                Tool orig = Game1.player.getToolFromName( "Axe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Axe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { Price = tool.UpgradeLevel == 5 ? 100000 : 250000, Stock = 1, TradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
                }

                orig = Game1.player.getToolFromName( "Watering Can" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new WateringCan() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { Price = tool.UpgradeLevel == 5 ? 100000 : 250000, Stock = 1, TradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
                }

                orig = Game1.player.getToolFromName( "Pickaxe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Pickaxe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { Price = tool.UpgradeLevel == 5 ? 100000 : 250000, Stock = 1, TradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
                }

                orig = Game1.player.getToolFromName( "Hoe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Hoe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { Price = tool.UpgradeLevel == 5 ? 100000 : 250000, Stock = 1, TradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
                }
            }
            else if ( e.NewMenu is AnimalQueryMenu aquery )
            {
                if (Game1.currentLocation is LunarLocation)
                {
                    // We don't want the move animal button at all.
                    // Hide it off screen, make it unreachable with controllers
                    aquery.moveHomeButton.bounds = new Rectangle(99999, 99999, 1, 1);
                    aquery.textBoxCC.downNeighborID = aquery.sellButton.myID;
                    aquery.sellButton.upNeighborID = aquery.textBoxCC.myID;
                }
            }
        }

        private void OnRenderingWorld( object sender, RenderingWorldEventArgs e )
        {
            if ( Game1.background is SpaceBackground )
            {
                // This part doesn't do anything normally (https://github.com/MonoGame/MonoGame/issues/5441),
                // but SpriteMaster makes it work. So need this for compatibility.
                if ( Game1.graphics.PreferredDepthStencilFormat != DepthFormat.Depth24Stencil8 )
                {
                    Game1.graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
                    Game1.graphics.ApplyChanges();
                }

                DefaultStencilOverride = StencilDarken;
                Game1.graphics.GraphicsDevice.Clear( ClearOptions.Stencil, Color.Transparent, 0, 0 );
            }
        }

        private void OnRenderedWorld( object sender, RenderedWorldEventArgs e )
        {
            DefaultStencilOverride = null;
        }

        private void OnReturnedToTitle( object sender, ReturnedToTitleEventArgs e )
        {
            AsteroidsDungeon.ClearAllLevels();
        }

        private void OnWarped( object sender, WarpedEventArgs e )
        {
            if ( e.OldLocation is LunarLocation || e.NewLocation is LunarLocation )
            {
                Helper.GameContent.InvalidateCache( "TerrainFeatures/hoeDirt" );
            }

            if ( e.NewLocation?.NameOrUniqueName == "Mine" )
            {
                e.NewLocation.setMapTile(43, 10, 173, "Buildings", "Warp 21 39 Custom_MM_MountainTop", 1);
            }
        }

        private void AddWalletItems( object sender, EventArgs e )
        {
            var page = sender as NewSkillsPage;
            if ( Game1.player.team.get_hasLunarKey().Value )
                page.specialItems.Add( new ClickableTextureComponent(
                    name: "", bounds: new Rectangle( -1, -1, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom ),
                    label: null, hoverText: I18n.Item_LunarKey_Name(),
                    texture: Assets.LunarKey, sourceRect: new Rectangle( 0, 0, 16, 16 ), scale: 4f, drawShadow: true ) );
        }

        private void AfterGiftGiven( object sender, EventArgsGiftGiven e )
        {
            if ( e.Gift.ItemId == ItemIds.SoulSapphire )
            {
                var farmer = sender as Farmer;
                foreach ( string key in Game1.objectInformation.Keys )
                {
                    var obj = new StardewValley.Object(key, 1);
                    if ( !obj.canBeGivenAsGift() || obj.questItem || obj.ParentSheetIndex == 809 )
                        continue;
                    if ( !farmer.giftedItems[ e.Npc.Name ].ContainsKey( key ) && ( !( obj.Name == "Stone" ) || key == "390" ) )
                        farmer.giftedItems[ e.Npc.Name ].Add( key, 0 );
                }
            }
        }
    }
}
