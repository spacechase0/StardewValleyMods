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
using StardewValley.Locations;
using StardewValley.Menus;
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
    public class Mod : StardewModdingAPI.Mod, IAssetLoader, IAssetEditor
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

            Assets.Load( helper.Content );
            SoundEffect mainMusic = SoundEffect.FromFile( Path.Combine( Helper.DirectoryPath, "assets", "into-the-spaceship.wav" ) );
            Game1.soundBank.AddCue( new CueDefinition( "into-the-spaceship", mainMusic, 2, loop: true ) );

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

            SpaceEvents.AddWalletItems += AddWalletItems;
            SpaceEvents.AfterGiftGiven += AfterGiftGiven;

            var necklaceDef = new NecklaceDataDefinition();
            ItemDataDefinition.ItemTypes.Add(necklaceDef);
            ItemDataDefinition.IdentifierLookup[necklaceDef.Identifier] = necklaceDef;

            var harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
            harmony.Patch( AccessTools.Method( "StardewModdingAPI.Framework.SGame:DrawImpl" ), transpiler: new HarmonyMethod( typeof( Patches.Game1CatchLightingRenderPatch ).GetMethod( "Transpiler" ) ) );
        }

        public bool CanLoad<T>( IAssetInfo asset )
        {
            if (asset.AssetNameEquals(ModManifest.UniqueID + "/Necklaces"))
                return true;
            foreach (string file in Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "dga")))
            {
                string filename = Path.GetFileName(file);
                if (asset.AssetName.EndsWith( ".png" ) && asset.AssetNameEquals("spacechase0.MoonMisadventures/assets/" + filename))
                    return true;
            }
            if ( Game1.currentLocation is LunarLocation )
            {
                return asset.AssetNameEquals( "TerrainFeatures/hoeDirt" );
            }
            return false;
        }

        public T Load<T>( IAssetInfo asset )
        {
            if (asset.AssetNameEquals(ModManifest.UniqueID + "/Necklaces"))
            {
                return (T)(object)new Dictionary<string, NecklaceData>
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
                };
            }
            foreach (string file in Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "dga")))
            {
                string filename = Path.GetFileName(file);
                if (asset.AssetName.EndsWith(".png") && asset.AssetNameEquals("spacechase0.MoonMisadventures/assets/" + filename))
                {
                    return (T)(object)Helper.Content.Load<Texture2D>("assets/dga/" + filename);
                }
            }
            if ( Game1.currentLocation is LunarLocation )
            {
                if ( asset.AssetNameEquals( "TerrainFeatures/hoeDirt" ) )
                    return ( T ) ( object ) Assets.HoeDirt;
            }
            return default( T );
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/BuildingsData"))
                return true;
            if (asset.AssetNameEquals("Data/LocationContexts"))
                return true;
            if (asset.AssetNameEquals("Data/InGameMap"))
                return true;
            if (asset.AssetNameEquals("Data/ObjectContextTags"))
                return true;
            if (asset.AssetNameEquals("Data/ObjectInformation"))
                return true;
            if (asset.AssetNameEquals("Data/Boots"))
                return true;
            if (asset.AssetNameEquals("Data/Crops"))
                return true;
            if (asset.AssetNameEquals("Data/weapons"))
                return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/BuildingsData"))
            {
                var bData = asset.AsDictionary<string, BuildingData>().Data;
                bData.Add("spacechase0.MoonMisadventures_MoonObelisk", new()
                {
                    ID = "spacechase0.MoonMisadventures_MoonObelisk",
                    Name = I18n.Building_Obelisk_Name(),
                    Description = I18n.Building_Obelisk_Description(),
                    Texture = "spacechase0.MoonMisadventures/assets/obelisk.png",
                    BuildMaterials = new[]
                    {
                        new BuildingMaterial()
                        {
                            ItemID = ItemIds.MythiciteBar,
                            Amount = 10,
                        },
                        new BuildingMaterial()
                        {
                            ItemID = ItemIds.StellarEssence,
                            Amount = 25,
                        },
                        new BuildingMaterial()
                        {
                            ItemID = ItemIds.SoulSapphire,
                            Amount = 3,
                        },
                    }.ToList(),
                    BuildCost = 2000000,
                    BuildCondition = "PLAYER_HAS_FLAG Any firstUfoTravel",
                    Size = new( 3, 2 ),
                    DefaultAction = "ObeliskWarp Custom_MM_MoonFarm 7 11 true",
                });
            }
            if (asset.AssetNameEquals("Data/LocationContexts"))
            {
                var locData = asset.AsDictionary<string, LocationContextData>().Data;
                locData.Add("Moon", new()
                {
                    Name = "Moon",
                    DefaultValidPlantableLocations = new[] { "Custom_MM_MoonFarm" }.ToList(),
                    WeatherConditions = new[]
                    {
                        new LocationContextData.WeatherCondition()
                        {
                            Condition = "",
                            Weather = "Sun",
                        }
                    }.ToList()
                });
            }
            if (asset.AssetNameEquals("Data/InGameMap"))
            {
                var mapData = asset.AsDictionary<string, IngameMapAreaData>().Data;
                mapData.Add("moon_bg", new IngameMapAreaData()
                {
                    AreaID = "moon_bg",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    SourceRect = "0 0 300 180",
                    DestRect = "0 0 300 180",
                    Zones = new(),
                });
                mapData.Add("moon_farm", new IngameMapAreaData()
                {
                    AreaID = "moon_farm",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "200 104 64 32",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonFarm" } ),
                            MapTileCorners = "0 0 49 39",
                            MapImageCorners = "200 104 264 136",
                        },
                    } ),
                    DisplayName = I18n.Location_LunarFarm(),
                });
                mapData.Add("moon_farmhouse", new IngameMapAreaData()
                {
                    AreaID = "moon_farmhouse",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "210 118 1 1",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonFarmHouse" } ),
                            MapTileCorners = "0 0 19 11",
                            MapImageCorners = "210 118 211 119",
                        },
                    } ),
                    DisplayName = I18n.Location_LunarFarm(),
                });
                mapData.Add("moon_farmcave", new IngameMapAreaData()
                {
                    AreaID = "moon_farmcave",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "248 30 1 1",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonFarmCave" } ),
                            MapTileCorners = "0 0 19 11",
                            MapImageCorners = "248 30 249 31",
                        },
                    }),
                    DisplayName = I18n.Location_LunarFarm(),
                });
                mapData.Add("moon_planetoverlook", new IngameMapAreaData()
                {
                    AreaID = "moon_planetoverlook",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "235 70 32 32",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonPlanetOverlook" } ),
                            MapTileCorners = "0 0 49 49",
                            MapImageCorners = "235 70 267 102",
                        },
                    }),
                    DisplayName = "???",
                });
                mapData.Add("moon_temple", new IngameMapAreaData()
                {
                    AreaID = "moon_temple",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "182 105 16 16",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonInfuserRoom" } ),
                            MapTileCorners = "0 0 29 29",
                            MapImageCorners = "182 105 198 121",
                        },
                    }),
                    DisplayName = "???",
                });
                mapData.Add("moon_landingarea", new IngameMapAreaData()
                {
                    AreaID = "moon_landingarea",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "152 121 48 16",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonLandingArea" } ),
                            MapTileCorners = "0 0 34 37",
                            MapImageCorners = "152 121 200 137",
                        },
                    }),
                    DisplayName = I18n.Location_LandingArea(),
                });
                mapData.Add("moon_asteroidsentrance", new IngameMapAreaData()
                {
                    AreaID = "moon_asteroidsentrance",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "152 113 8 8",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonAsteroidsEntrance" } ),
                            MapTileCorners = "0 0 49 94",
                            MapImageCorners = "152 113 160 121",
                        },
                    }),
                    DisplayName = I18n.Location_AsteroidsEntrance(),
                });
                mapData.Add("moon_asteroids", new IngameMapAreaData()
                {
                    AreaID = "moon_asteroids",
                    Group = "moon",
                    Texture = "spacechase0.MoonMisadventures/assets/map.png",
                    DestRect = "20 20 128 80",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MoonAsteroidsDungeon" } ),
                            MapTileCorners = "0 0 149 149",
                            MapImageCorners = "20 20 148 100",
                        },
                    }),
                    DisplayName = I18n.Location_Asteroids(),
                });
                mapData.Add("mountaintop", new()
                {
                    AreaID = "mountaintop",
                    Group = "SDV",
                    Texture = "LooseSprites/map",
                    Zones = new(new[] {
                        new IngameMapAreaZone()
                        {
                            ValidAreas = new List<string>( new[] { "Custom_MM_MountainTop" } ),
                            MapTileCorners = "0 0 47 47",
                            MapImageCorners = "210 1 211 1",
                            DisplayName = "???",
                        },
                    }),
                });
            }
            if (asset.AssetNameEquals("Data/ObjectContextTags"))
            {
                var tagsData = asset.AsDictionary<string, string>().Data;
                tagsData.Add("LunarWheatSeeds", "plantable_context_moon");
                tagsData.Add("SunbloomSeeds", "plantable_context_moon");
                tagsData.Add("StarPetalSeeds", "plantable_context_moon");
                tagsData.Add("VoidBlossomSeeds", "plantable_context_moon");
                tagsData.Add("SoulSproutSeeds", "plantable_context_moon");
                return;
            }
            string currType = null;
            if (asset.AssetNameEquals("Data/ObjectInformation"))
                currType = "Object";
            if (asset.AssetNameEquals("Data/Boots"))
                currType = "Boots";
            if (asset.AssetNameEquals("Data/Crops"))
                currType = "Crop";
            if (asset.AssetNameEquals("Data/weapons"))
                currType = "Weapon";
            if (currType == null)
                return;

            var data = Helper.Content.Load<Dictionary<string, string>>("assets/item-data.json");
            foreach (var entry in data)
            {
                if (entry.Key.StartsWith(currType + ":"))
                {
                    string key = entry.Key.Substring(currType.Length + 1);
                    string val = entry.Value;
                    while (val.Contains("{{i18n:"))
                    {
                        int x = val.IndexOf("{{i18n:");
                        int y = val.IndexOf("}}", x);

                        val = val.Substring(0, x) + Helper.Translation.Get( val.Substring( x + "{{i18n:".Length, y - x - "{{i18n:".Length ) ) + val.Substring(y + 2);
                    }
                    asset.AsDictionary<string, string>().Data.Add(ModManifest.UniqueID + "_" + key, val);
                }
            }
        }

        private void OnKeyCommand( string cmd, string[] args )
        {
            Game1.player.addItemByMenuIfNecessary(new Necklace("speed"));
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
            sc.RegisterSerializerType( typeof( LunarAnimal ) );
            sc.RegisterSerializerType( typeof( AnimalGauntlets ) );
            sc.RegisterSerializerType( typeof( Necklace ) );
            sc.RegisterSerializerType( typeof( MoonPlanetOverlook ) );
            sc.RegisterSerializerType( typeof( UfoInterior ) );
            sc.RegisterSerializerType( typeof( LunarFarmHouse ) );
            sc.RegisterSerializerType( typeof( MoonInfuserRoom ) );
            sc.RegisterSerializerType( typeof( LunarSlime ) );
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
                switch ( necklace.ItemID )
                {
                    case "speed":
                        {
                            var buff = Game1.player.buffs.appliedBuffs.FirstOrDefault( b => b.Key == "necklace" ).Value;
                            if ( buff == null )
                            {
                                buff = new Buff("necklace", "necklace", I18n.Necklace(), 10 * 7000, buff_effects: new BuffEffects() { speed = { 3 } });
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
                                        var spot = Game1.player.getTileLocation() + new Vector2( ix, iy );
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
                Game1.locations.Add( new MountainTop( Helper.Content ) );
                Game1.locations.Add( new MoonLandingArea( Helper.Content ) );
                Game1.locations.Add( new AsteroidsEntrance( Helper.Content ) );
                Game1.locations.Add( new LunarFarm( Helper.Content ) );
                Game1.locations.Add( new LunarFarmCave( Helper.Content ) );
                Game1.locations.Add( new MoonPlanetOverlook( Helper.Content ) );
                Game1.locations.Add( new UfoInterior( Helper.Content ) );
                Game1.locations.Add( new LunarFarmHouse( Helper.Content ) );
                Game1.locations.Add( new MoonInfuserRoom( Helper.Content ) );
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
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { price = tool.UpgradeLevel == 5 ? 100000 : 250000, stock = 1, tradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
                }

                orig = Game1.player.getToolFromName( "Watering Can" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new WateringCan() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { price = tool.UpgradeLevel == 5 ? 100000 : 250000, stock = 1, tradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
                }

                orig = Game1.player.getToolFromName( "Pickaxe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Pickaxe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { price = tool.UpgradeLevel == 5 ? 100000 : 250000, stock = 1, tradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
                }

                orig = Game1.player.getToolFromName( "Hoe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Hoe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new ItemStockInformation() { price = tool.UpgradeLevel == 5 ? 100000 : 250000, stock = 1, tradeItem = tool.UpgradeLevel == 5 ? "910" : ItemIds.MythiciteBar } );
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
            if ( e.OldLocation is LunarLocation ^ e.NewLocation is LunarLocation )
                Helper.Content.InvalidateCache( "TerrainFeatures/hoeDirt" );

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
            if ( e.Gift.ItemID == ItemIds.SoulSapphire )
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
