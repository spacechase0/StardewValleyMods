using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JsonAssets.Data;
using JsonAssets.Other.ContentPatcher;
using JsonAssets.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using Spacechase.Shared.Harmony;
using SpaceCore;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

// TODO: Refactor recipes

namespace JsonAssets
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private ContentInjector1 content1;
        private ContentInjector2 content2;
        internal ExpandedPreconditionsUtilityAPI epu;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("ja_summary", "Summary of JA ids", this.doCommands);
            helper.ConsoleCommands.Add("ja_unfix", "Unfix IDs once, in case IDs were double fixed.", this.doCommands);

            helper.Events.Display.MenuChanged += this.onMenuChanged;
            helper.Events.GameLoop.Saving += this.onSaving;
            helper.Events.Player.InventoryChanged += this.onInventoryChanged;
            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.GameLoop.SaveCreated += this.onCreated;
            helper.Events.GameLoop.UpdateTicked += this.onTick;
            helper.Events.Specialized.LoadStageChanged += this.onLoadStageChanged;
            helper.Events.Multiplayer.PeerContextReceived += this.clientConnected;

            helper.Content.AssetEditors.Add(this.content1 = new ContentInjector1());
            helper.Content.AssetLoaders.Add(this.content1);

            TileSheetExtensions.RegisterExtendedTileSheet("Maps\\springobjects", 16);
            TileSheetExtensions.RegisterExtendedTileSheet("TileSheets\\Craftables", 32);
            TileSheetExtensions.RegisterExtendedTileSheet("TileSheets\\crops", 32);
            TileSheetExtensions.RegisterExtendedTileSheet("TileSheets\\fruitTrees", 80);
            TileSheetExtensions.RegisterExtendedTileSheet("Characters\\Farmer\\shirts", 32);
            TileSheetExtensions.RegisterExtendedTileSheet("Characters\\Farmer\\pants", 688);
            TileSheetExtensions.RegisterExtendedTileSheet("Characters\\Farmer\\hats", 80);

            HarmonyPatcher.Apply(this,
                new CropPatcher(),
                new FencePatcher(),
                new ForgeMenuPatcher(),
                new Game1Patcher(),
                new GiantCropPatcher(),
                new ItemPatcher(),
                new ObjectPatcher(),
                new RingPatcher()
            );
        }

        private Api api;
        public override object GetApi()
        {
            return this.api ?? (this.api = new Api(this.loadData));
        }

        private Dictionary<string, KeyValuePair<int, int>> MakeIdMapping(IDictionary<string, int> oldIds, IDictionary<string, int> newIds)
        {
            var ret = new Dictionary<string, KeyValuePair<int, int>>();
            if (oldIds != null)
            {
                foreach (var oldId in oldIds)
                {
                    ret.Add(oldId.Key, new KeyValuePair<int, int>(oldId.Value, -1));
                }
            }
            foreach (var newId in newIds)
            {
                if (ret.ContainsKey(newId.Key))
                    ret[newId.Key] = new KeyValuePair<int, int>(ret[newId.Key].Key, newId.Value);
                else
                    ret.Add(newId.Key, new KeyValuePair<int, int>(-1, newId.Value));
            }
            return ret;
        }

        private void PrintIdMapping(string header, Dictionary<string, KeyValuePair<int, int>> mapping)
        {
            Log.info(header);
            Log.info("-------------------------");

            int len = 0;
            foreach (var entry in mapping)
                len = Math.Max(len, entry.Key.Length);

            foreach (var entry in mapping)
            {
                Log.info(string.Format("{0,-" + len + "} | {1,5} -> {2,-5}",
                                          entry.Key,
                                          entry.Value.Key == -1 ? "" : entry.Value.Key.ToString(),
                                          entry.Value.Value == -1 ? "" : entry.Value.Value.ToString()));
            }
            Log.info("");
        }

        private void doCommands(string cmd, string[] args)
        {
            if (!this.didInit)
            {
                Log.info("A save must be loaded first.");
                return;
            }

            if (cmd == "ja_summary")
            {
                var objs = this.MakeIdMapping(this.oldObjectIds, this.objectIds);
                var crops = this.MakeIdMapping(this.oldCropIds, this.cropIds);
                var ftrees = this.MakeIdMapping(this.oldFruitTreeIds, this.fruitTreeIds);
                var bigs = this.MakeIdMapping(this.oldBigCraftableIds, this.bigCraftableIds);
                var hats = this.MakeIdMapping(this.oldHatIds, this.hatIds);
                var weapons = this.MakeIdMapping(this.oldWeaponIds, this.weaponIds);
                var clothings = this.MakeIdMapping(this.oldClothingIds, this.clothingIds);

                this.PrintIdMapping("Object IDs", objs);
                this.PrintIdMapping("Crop IDs", crops);
                this.PrintIdMapping("Fruit Tree IDs", ftrees);
                this.PrintIdMapping("Big Craftable IDs", bigs);
                this.PrintIdMapping("Hat IDs", hats);
                this.PrintIdMapping("Weapon IDs", weapons);
                this.PrintIdMapping("Clothing IDs", clothings);
            }
            else if (cmd == "ja_unfix")
            {
                this.locationsFixedAlready.Clear();
                this.fixIdsEverywhere(reverse: true);
            }
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.epu = this.Helper.ModRegistry.GetApi<ExpandedPreconditionsUtilityAPI>("Cherry.ExpandedPreconditionsUtility");
            this.epu.Initialize(false, this.ModManifest.UniqueID);

            ContentPatcherIntegration.Initialize();
        }

        private bool firstTick = true;
        private void onTick(object sender, UpdateTickedEventArgs e)
        {
            // This needs to run after GameLaunched, because of the event 
            if (this.firstTick)
            {
                this.firstTick = false;

                Log.info("Loading content packs...");
                foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
                    try
                    {
                        this.loadData(contentPack);
                    }
                    catch (Exception e1)
                    {
                        Log.error("Exception loading content pack: " + e1);
                    }
                if (Directory.Exists(Path.Combine(this.Helper.DirectoryPath, "ContentPacks")))
                {
                    foreach (string dir in Directory.EnumerateDirectories(Path.Combine(this.Helper.DirectoryPath, "ContentPacks")))
                        try
                        {
                            this.loadData(dir);
                        }
                        catch (Exception e2)
                        {
                            Log.error("Exception loading content pack: " + e2);
                        }
                }
                this.api.InvokeItemsRegistered();

                this.resetAtTitle();
            }

        }

        private static readonly Regex nameToId = new("[^a-zA-Z0-9_.]");
        private void loadData(string dir)
        {
            // read initial info
            IContentPack temp = this.Helper.ContentPacks.CreateFake(dir);
            ContentPackData info = temp.ReadJsonFile<ContentPackData>("content-pack.json");
            if (info == null)
            {
                Log.warn($"\tNo {dir}/content-pack.json!");
                return;
            }

            // load content pack
            string id = Mod.nameToId.Replace(info.Name, "");
            IContentPack contentPack = this.Helper.ContentPacks.CreateTemporary(dir, id: id, name: info.Name, description: info.Description, author: info.Author, version: new SemanticVersion(info.Version));
            this.loadData(contentPack);
        }

        internal Dictionary<IManifest, List<string>> objectsByContentPack = new();
        internal Dictionary<IManifest, List<string>> cropsByContentPack = new();
        internal Dictionary<IManifest, List<string>> fruitTreesByContentPack = new();
        internal Dictionary<IManifest, List<string>> bigCraftablesByContentPack = new();
        internal Dictionary<IManifest, List<string>> hatsByContentPack = new();
        internal Dictionary<IManifest, List<string>> weaponsByContentPack = new();
        internal Dictionary<IManifest, List<string>> clothingByContentPack = new();
        internal Dictionary<IManifest, List<string>> bootsByContentPack = new();

        public void RegisterObject(IManifest source, ObjectData obj)
        {
            this.objects.Add(obj);

            if (obj.Recipe != null && obj.Recipe.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = obj.Recipe.PurchaseFrom,
                    Price = obj.Recipe.PurchasePrice,
                    PurchaseRequirements = obj.Recipe.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", obj.Recipe.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(obj.id, 1, true, obj.Recipe.PurchasePrice, 0),
                });
                if (obj.Recipe.AdditionalPurchaseData != null)
                {
                    foreach (var entry in obj.Recipe.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(obj.id, 1, true, entry.PurchasePrice, 0),
                        });
                    }
                }
            }
            if (obj.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = obj.PurchaseFrom,
                    Price = obj.PurchasePrice,
                    PurchaseRequirements = obj.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", obj.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(obj.id, int.MaxValue, false, obj.PurchasePrice, 0),
                });
                if (obj.AdditionalPurchaseData != null)
                {
                    foreach (var entry in obj.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(obj.id, int.MaxValue, false, entry.PurchasePrice, 0),
                        });
                    }
                }
            }

            // save ring
            if (obj.Category == ObjectData.Category_.Ring)
                this.myRings.Add(obj);

            // Duplicate check
            if (this.dupObjects.ContainsKey(obj.Name))
                Log.error($"Duplicate object: {obj.Name} just added by {source.Name}, already added by {this.dupObjects[obj.Name].Name}!");
            else
                this.dupObjects[obj.Name] = source;

            if (!this.objectsByContentPack.ContainsKey(source))
                this.objectsByContentPack.Add(source, new List<string>());
            this.objectsByContentPack[source].Add(obj.Name);
        }

        public void RegisterCrop(IManifest source, CropData crop, Texture2D seedTex)
        {
            this.crops.Add(crop);

            // save seeds
            crop.seed = new ObjectData
            {
                texture = seedTex,
                Name = crop.SeedName,
                Description = crop.SeedDescription,
                Category = ObjectData.Category_.Seeds,
                Price = crop.SeedSellPrice == -1 ? crop.SeedPurchasePrice : crop.SeedSellPrice,
                CanPurchase = crop.SeedPurchasePrice > 0,
                PurchaseFrom = crop.SeedPurchaseFrom,
                PurchasePrice = crop.SeedPurchasePrice,
                PurchaseRequirements = crop.SeedPurchaseRequirements ?? new List<string>(),
                AdditionalPurchaseData = crop.SeedAdditionalPurchaseData ?? new List<PurchaseData>(),
                NameLocalization = crop.SeedNameLocalization,
                DescriptionLocalization = crop.SeedDescriptionLocalization
            };

            // TODO: Clean up this chunk
            // I copy/pasted it from the unofficial update decompiled
            string str = "";
            string[] array = new[] { "spring", "summer", "fall", "winter" }
                .Except(crop.Seasons)
                .ToArray();
            foreach (string season in array)
            {
                str += $"/z {season}";
            }
            if (str != "")
            {
                string strtrimstart = str.TrimStart(new[] { '/' });
                if (crop.SeedPurchaseRequirements != null && crop.SeedPurchaseRequirements.Count > 0)
                {
                    for (int index = 0; index < crop.SeedPurchaseRequirements.Count; index++)
                    {
                        if (this.SeasonLimiter.IsMatch(crop.SeedPurchaseRequirements[index]))
                        {
                            crop.SeedPurchaseRequirements[index] = strtrimstart;
                            Log.warn($"        Faulty season requirements for {crop.SeedName}!\n        Fixed season requirements: {crop.SeedPurchaseRequirements[index]}");
                        }
                    }
                    if (!crop.SeedPurchaseRequirements.Contains(str.TrimStart('/')))
                    {
                        Log.trace($"        Adding season requirements for {crop.SeedName}:\n        New season requirements: {strtrimstart}");
                        crop.seed.PurchaseRequirements.Add(strtrimstart);
                    }
                }
                else
                {
                    Log.trace($"        Adding season requirements for {crop.SeedName}:\n        New season requirements: {strtrimstart}");
                    crop.seed.PurchaseRequirements.Add(strtrimstart);
                }
            }

            if (crop.seed.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = crop.seed.PurchaseFrom,
                    Price = crop.seed.PurchasePrice,
                    PurchaseRequirements = crop.seed.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", crop.seed.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(crop.seed.id, int.MaxValue, false, crop.seed.PurchasePrice),
                    ShowWithStocklist = true,
                });
                if (crop.seed.AdditionalPurchaseData != null)
                {
                    foreach (var entry in crop.seed.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(crop.seed.id, int.MaxValue, false, entry.PurchasePrice, 0),
                        });
                    }
                }
            }

            // Duplicate check
            if (this.dupCrops.ContainsKey(crop.Name))
                Log.error($"Duplicate crop: {crop.Name} just added by {source.Name}, already added by {this.dupCrops[crop.Name].Name}!");
            else
                this.dupCrops[crop.Name] = source;

            this.objects.Add(crop.seed);

            if (!this.cropsByContentPack.ContainsKey(source))
                this.cropsByContentPack.Add(source, new List<string>());
            this.cropsByContentPack[source].Add(crop.Name);

            if (!this.objectsByContentPack.ContainsKey(source))
                this.objectsByContentPack.Add(source, new List<string>());
            this.objectsByContentPack[source].Add(crop.seed.Name);
        }

        public void RegisterFruitTree(IManifest source, FruitTreeData tree, Texture2D saplingTex)
        {
            this.fruitTrees.Add(tree);

            // save seed
            tree.sapling = new ObjectData
            {
                texture = saplingTex,
                Name = tree.SaplingName,
                Description = tree.SaplingDescription,
                Category = ObjectData.Category_.Seeds,
                Price = tree.SaplingPurchasePrice,
                CanPurchase = true,
                PurchaseRequirements = tree.SaplingPurchaseRequirements,
                PurchaseFrom = tree.SaplingPurchaseFrom,
                PurchasePrice = tree.SaplingPurchasePrice,
                AdditionalPurchaseData = tree.SaplingAdditionalPurchaseData,
                NameLocalization = tree.SaplingNameLocalization,
                DescriptionLocalization = tree.SaplingDescriptionLocalization
            };
            this.objects.Add(tree.sapling);

            if (tree.sapling.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = tree.sapling.PurchaseFrom,
                    Price = tree.sapling.PurchasePrice,
                    PurchaseRequirements = tree.sapling.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", tree.sapling.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(Vector2.Zero, tree.sapling.id, int.MaxValue),
                });
                if (tree.sapling.AdditionalPurchaseData != null)
                {
                    foreach (var entry in tree.sapling.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(tree.sapling.id, 1, true, tree.sapling.PurchasePrice, 0),
                        });
                    }
                }
            }

            // Duplicate check
            if (this.dupFruitTrees.ContainsKey(tree.Name))
                Log.error($"Duplicate fruit tree: {tree.Name} just added by {source.Name}, already added by {this.dupFruitTrees[tree.Name].Name}!");
            else
                this.dupFruitTrees[tree.Name] = source;

            if (!this.fruitTreesByContentPack.ContainsKey(source))
                this.fruitTreesByContentPack.Add(source, new List<string>());
            this.fruitTreesByContentPack[source].Add(tree.Name);
        }

        public void RegisterBigCraftable(IManifest source, BigCraftableData craftable)
        {
            this.bigCraftables.Add(craftable);

            if (craftable.Recipe != null && craftable.Recipe.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = craftable.Recipe.PurchaseFrom,
                    Price = craftable.Recipe.PurchasePrice,
                    PurchaseRequirements = craftable.Recipe.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", craftable.Recipe.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(Vector2.Zero, craftable.id, true),
                });
                if (craftable.Recipe.AdditionalPurchaseData != null)
                {
                    foreach (var entry in craftable.Recipe.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(Vector2.Zero, craftable.id, true),
                        });
                    }
                }
            }
            if (craftable.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = craftable.PurchaseFrom,
                    Price = craftable.PurchasePrice,
                    PurchaseRequirements = craftable.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", craftable.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(Vector2.Zero, craftable.id, false),
                });
                if (craftable.AdditionalPurchaseData != null)
                {
                    foreach (var entry in craftable.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(Vector2.Zero, craftable.id, false),
                        });
                    }
                }
            }

            // Duplicate check
            if (this.dupBigCraftables.ContainsKey(craftable.Name))
                Log.error($"Duplicate big craftable: {craftable.Name} just added by {source.Name}, already added by {this.dupBigCraftables[craftable.Name].Name}!");
            else
                this.dupBigCraftables[craftable.Name] = source;

            if (!this.bigCraftablesByContentPack.ContainsKey(source))
                this.bigCraftablesByContentPack.Add(source, new List<string>());
            this.bigCraftablesByContentPack[source].Add(craftable.Name);
        }

        public void RegisterHat(IManifest source, HatData hat)
        {
            this.hats.Add(hat);

            if (hat.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = "HatMouse",
                    Price = hat.PurchasePrice,
                    PurchaseRequirements = new string[0],
                    Object = () => new Hat(hat.id),
                });
            }

            // Duplicate check
            if (this.dupHats.ContainsKey(hat.Name))
                Log.error($"Duplicate hat: {hat.Name} just added by {source.Name}, already added by {this.dupHats[hat.Name].Name}!");
            else
                this.dupHats[hat.Name] = source;

            if (!this.hatsByContentPack.ContainsKey(source))
                this.hatsByContentPack.Add(source, new List<string>());
            this.hatsByContentPack[source].Add(hat.Name);
        }

        public void RegisterWeapon(IManifest source, WeaponData weapon)
        {
            this.weapons.Add(weapon);

            if (weapon.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = weapon.PurchaseFrom,
                    Price = weapon.PurchasePrice,
                    PurchaseRequirements = weapon.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", weapon.PurchaseRequirements?.ToArray()) },
                    Object = () => new MeleeWeapon(weapon.id)
                });
                if (weapon.AdditionalPurchaseData != null)
                {
                    foreach (var entry in weapon.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new MeleeWeapon(weapon.id)
                        });
                    }
                }
            }

            // Duplicate check
            if (this.dupWeapons.ContainsKey(weapon.Name))
                Log.error($"Duplicate weapon: {weapon.Name} just added by {source.Name}, already added by {this.dupWeapons[weapon.Name].Name}!");
            else
                this.dupWeapons[weapon.Name] = source;

            if (!this.weaponsByContentPack.ContainsKey(source))
                this.weaponsByContentPack.Add(source, new List<string>());
            this.weaponsByContentPack[source].Add(weapon.Name);
        }

        public void RegisterShirt(IManifest source, ShirtData shirt)
        {
            this.shirts.Add(shirt);

            // Duplicate check
            if (this.dupShirts.ContainsKey(shirt.Name))
                Log.error($"Duplicate shirt: {shirt.Name} just added by {source.Name}, already added by {this.dupShirts[shirt.Name].Name}!");
            else
                this.dupShirts[shirt.Name] = source;

            if (!this.clothingByContentPack.ContainsKey(source))
                this.clothingByContentPack.Add(source, new List<string>());
            this.clothingByContentPack[source].Add(shirt.Name);
        }

        public void RegisterPants(IManifest source, PantsData pants)
        {
            this.pantss.Add(pants);

            // Duplicate check
            if (this.dupPants.ContainsKey(pants.Name))
                Log.error($"Duplicate pants: {pants.Name} just added by {source.Name}, already added by {this.dupPants[pants.Name].Name}!");
            else
                this.dupPants[pants.Name] = source;

            if (!this.clothingByContentPack.ContainsKey(source))
                this.clothingByContentPack.Add(source, new List<string>());
            this.clothingByContentPack[source].Add(pants.Name);
        }

        public void RegisterTailoringRecipe(IManifest source, TailoringRecipeData recipe)
        {
            this.tailoring.Add(recipe);
        }

        public void RegisterBoots(IManifest source, BootsData boots)
        {
            this.bootss.Add(boots);

            if (boots.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry()
                {
                    PurchaseFrom = boots.PurchaseFrom,
                    Price = boots.PurchasePrice,
                    PurchaseRequirements = boots.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", boots.PurchaseRequirements?.ToArray()) },
                    Object = () => new Boots(boots.id)
                });

                if (boots.AdditionalPurchaseData != null)
                {
                    foreach (var entry in boots.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry()
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new Boots(boots.id)
                        });
                    }
                }
            }

            // Duplicate check
            if (this.dupBoots.ContainsKey(boots.Name))
                Log.error($"Duplicate boots: {boots.Name} just added by {source.Name}, already added by {this.dupBoots[boots.Name].Name}!");
            else
                this.dupBoots[boots.Name] = source;

            if (!this.bootsByContentPack.ContainsKey(source))
                this.bootsByContentPack.Add(source, new List<string>());
            this.bootsByContentPack[source].Add(boots.Name);
        }

        public void RegisterForgeRecipe(IManifest source, ForgeRecipeData recipe)
        {
            this.forge.Add(recipe);
        }

        public void RegisterFence(IManifest source, FenceData fence)
        {
            this.fences.Add(fence);

            Func<IList<FenceData.Recipe_.Ingredient>, IList<ObjectData.Recipe_.Ingredient>> convertIngredients = (ingredients) =>
            {
                var ret = new List<ObjectData.Recipe_.Ingredient>();
                foreach (var ingred in ingredients)
                {
                    ret.Add(new ObjectData.Recipe_.Ingredient()
                    {
                        Object = ingred.Object,
                        Count = ingred.Count,
                    });
                }
                return ret;
            };

            this.RegisterObject(source, fence.correspondingObject = new ObjectData()
            {
                texture = fence.objectTexture,
                Name = fence.Name,
                Description = fence.Description,
                Category = ObjectData.Category_.Crafting,
                Price = fence.Price,
                Recipe = fence.Recipe == null ? null : new ObjectData.Recipe_()
                {
                    SkillUnlockName = fence.Recipe.SkillUnlockName,
                    SkillUnlockLevel = fence.Recipe.SkillUnlockLevel,
                    ResultCount = fence.Recipe.ResultCount,
                    Ingredients = convertIngredients(fence.Recipe.Ingredients),
                    IsDefault = fence.Recipe.IsDefault,
                    CanPurchase = fence.Recipe.CanPurchase,
                    PurchasePrice = fence.Recipe.PurchasePrice,
                    PurchaseFrom = fence.Recipe.PurchaseFrom,
                    PurchaseRequirements = fence.Recipe.PurchaseRequirements,
                    AdditionalPurchaseData = fence.Recipe.AdditionalPurchaseData,
                },
                CanPurchase = fence.CanPurchase,
                PurchasePrice = fence.PurchasePrice,
                PurchaseFrom = fence.PurchaseFrom,
                PurchaseRequirements = fence.PurchaseRequirements,
                AdditionalPurchaseData = fence.AdditionalPurchaseData,
                NameLocalization = fence.NameLocalization,
                DescriptionLocalization = fence.DescriptionLocalization,
            });
        }

        private Dictionary<string, IManifest> dupObjects = new();
        private Dictionary<string, IManifest> dupCrops = new();
        private Dictionary<string, IManifest> dupFruitTrees = new();
        private Dictionary<string, IManifest> dupBigCraftables = new();
        private Dictionary<string, IManifest> dupHats = new();
        private Dictionary<string, IManifest> dupWeapons = new();
        private Dictionary<string, IManifest> dupShirts = new();
        private Dictionary<string, IManifest> dupPants = new();
        private Dictionary<string, IManifest> dupBoots = new();

        private readonly Regex SeasonLimiter = new("(z(?: spring| summer| fall| winter){2,4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private void loadData(IContentPack contentPack)
        {
            Log.info($"\t{contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author} - {contentPack.Manifest.Description}");

            // load objects
            DirectoryInfo objectsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Objects"));
            if (objectsDir.Exists)
            {
                foreach (DirectoryInfo dir in objectsDir.EnumerateDirectories())
                {
                    string relativePath = $"Objects/{dir.Name}";

                    // load data
                    ObjectData obj = contentPack.ReadJsonFile<ObjectData>($"{relativePath}/object.json");
                    if (obj == null || (obj.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(obj.DisableWithMod)) || (obj.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(obj.EnableWithMod)))
                        continue;

                    // save object
                    obj.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/object.png");
                    if (obj.IsColored)
                        obj.textureColor = contentPack.LoadAsset<Texture2D>($"{relativePath}/color.png");

                    this.RegisterObject(contentPack.Manifest, obj);
                }
            }

            // load crops
            DirectoryInfo cropsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Crops"));
            if (cropsDir.Exists)
            {
                foreach (DirectoryInfo dir in cropsDir.EnumerateDirectories())
                {
                    string relativePath = $"Crops/{dir.Name}";

                    // load data
                    CropData crop = contentPack.ReadJsonFile<CropData>($"{relativePath}/crop.json");
                    if (crop == null || (crop.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(crop.DisableWithMod)) || (crop.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(crop.EnableWithMod)))
                        continue;

                    // save crop
                    crop.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/crop.png");
                    if (contentPack.HasFile($"{relativePath}/giant.png"))
                        crop.giantTex = contentPack.LoadAsset<Texture2D>($"{relativePath}/giant.png");

                    this.RegisterCrop(contentPack.Manifest, crop, contentPack.LoadAsset<Texture2D>($"{relativePath}/seeds.png"));
                }
            }

            // load fruit trees
            DirectoryInfo fruitTreesDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "FruitTrees"));
            if (fruitTreesDir.Exists)
            {
                foreach (DirectoryInfo dir in fruitTreesDir.EnumerateDirectories())
                {
                    string relativePath = $"FruitTrees/{dir.Name}";

                    // load data
                    FruitTreeData tree = contentPack.ReadJsonFile<FruitTreeData>($"{relativePath}/tree.json");
                    if (tree == null || (tree.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(tree.DisableWithMod)) || (tree.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(tree.EnableWithMod)))
                        continue;

                    // save fruit tree
                    tree.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/tree.png");
                    this.RegisterFruitTree(contentPack.Manifest, tree, contentPack.LoadAsset<Texture2D>($"{relativePath}/sapling.png"));
                }
            }

            // load big craftables
            DirectoryInfo bigCraftablesDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "BigCraftables"));
            if (bigCraftablesDir.Exists)
            {
                foreach (DirectoryInfo dir in bigCraftablesDir.EnumerateDirectories())
                {
                    string relativePath = $"BigCraftables/{dir.Name}";

                    // load data
                    BigCraftableData craftable = contentPack.ReadJsonFile<BigCraftableData>($"{relativePath}/big-craftable.json");
                    if (craftable == null || (craftable.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(craftable.DisableWithMod)) || (craftable.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(craftable.EnableWithMod)))
                        continue;

                    // save craftable
                    craftable.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/big-craftable.png");
                    if (craftable.ReserveNextIndex && craftable.ReserveExtraIndexCount == 0)
                        craftable.ReserveExtraIndexCount = 1;
                    if (craftable.ReserveExtraIndexCount > 0)
                    {
                        craftable.extraTextures = new Texture2D[craftable.ReserveExtraIndexCount];
                        for (int i = 0; i < craftable.ReserveExtraIndexCount; ++i)
                            craftable.extraTextures[i] = contentPack.LoadAsset<Texture2D>($"{relativePath}/big-craftable-{i + 2}.png");
                    }
                    this.RegisterBigCraftable(contentPack.Manifest, craftable);
                }
            }

            // load hats
            DirectoryInfo hatsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Hats"));
            if (hatsDir.Exists)
            {
                foreach (DirectoryInfo dir in hatsDir.EnumerateDirectories())
                {
                    string relativePath = $"Hats/{dir.Name}";

                    // load data
                    HatData hat = contentPack.ReadJsonFile<HatData>($"{relativePath}/hat.json");
                    if (hat == null || (hat.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(hat.DisableWithMod)) || (hat.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(hat.EnableWithMod)))
                        continue;

                    // save object
                    hat.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/hat.png");
                    this.RegisterHat(contentPack.Manifest, hat);
                }
            }

            // Load weapons
            DirectoryInfo weaponsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Weapons"));
            if (weaponsDir.Exists)
            {
                foreach (DirectoryInfo dir in weaponsDir.EnumerateDirectories())
                {
                    string relativePath = $"Weapons/{dir.Name}";

                    // load data
                    WeaponData weapon = contentPack.ReadJsonFile<WeaponData>($"{relativePath}/weapon.json");
                    if (weapon == null || (weapon.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(weapon.DisableWithMod)) || (weapon.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(weapon.EnableWithMod)))
                        continue;

                    // save object
                    weapon.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/weapon.png");
                    this.RegisterWeapon(contentPack.Manifest, weapon);
                }
            }

            // Load shirts
            DirectoryInfo shirtsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Shirts"));
            if (shirtsDir.Exists)
            {
                foreach (DirectoryInfo dir in shirtsDir.EnumerateDirectories())
                {
                    string relativePath = $"Shirts/{dir.Name}";

                    // load data
                    ShirtData shirt = contentPack.ReadJsonFile<ShirtData>($"{relativePath}/shirt.json");
                    if (shirt == null || (shirt.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(shirt.DisableWithMod)) || (shirt.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(shirt.EnableWithMod)))
                        continue;

                    // save shirt
                    shirt.textureMale = contentPack.LoadAsset<Texture2D>($"{relativePath}/male.png");
                    if (shirt.Dyeable)
                        shirt.textureMaleColor = contentPack.LoadAsset<Texture2D>($"{relativePath}/male-color.png");
                    if (shirt.HasFemaleVariant)
                    {
                        shirt.textureFemale = contentPack.LoadAsset<Texture2D>($"{relativePath}/female.png");
                        if (shirt.Dyeable)
                            shirt.textureFemaleColor = contentPack.LoadAsset<Texture2D>($"{relativePath}/female-color.png");
                    }
                    this.RegisterShirt(contentPack.Manifest, shirt);
                }
            }

            // Load pants
            DirectoryInfo pantsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Pants"));
            if (pantsDir.Exists)
            {
                foreach (DirectoryInfo dir in pantsDir.EnumerateDirectories())
                {
                    string relativePath = $"Pants/{dir.Name}";

                    // load data
                    PantsData pants = contentPack.ReadJsonFile<PantsData>($"{relativePath}/pants.json");
                    if (pants == null || (pants.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(pants.DisableWithMod)) || (pants.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(pants.EnableWithMod)))
                        continue;

                    // save pants
                    pants.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/pants.png");
                    this.RegisterPants(contentPack.Manifest, pants);
                }
            }

            // Load tailoring
            DirectoryInfo tailoringDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Tailoring"));
            if (tailoringDir.Exists)
            {
                foreach (DirectoryInfo dir in tailoringDir.EnumerateDirectories())
                {
                    string relativePath = $"Tailoring/{dir.Name}";

                    // load data
                    TailoringRecipeData recipe = contentPack.ReadJsonFile<TailoringRecipeData>($"{relativePath}/recipe.json");
                    if (recipe == null || (recipe.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(recipe.DisableWithMod)) || (recipe.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(recipe.EnableWithMod)))
                        continue;

                    this.RegisterTailoringRecipe(contentPack.Manifest, recipe);
                }
            }

            // Load boots
            DirectoryInfo bootsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Boots"));
            if (bootsDir.Exists)
            {
                foreach (DirectoryInfo dir in bootsDir.EnumerateDirectories())
                {
                    string relativePath = $"Boots/{dir.Name}";

                    // load data
                    BootsData boots = contentPack.ReadJsonFile<BootsData>($"{relativePath}/boots.json");
                    if (boots == null || (boots.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(boots.DisableWithMod)) || (boots.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(boots.EnableWithMod)))
                        continue;

                    boots.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/boots.png");
                    boots.textureColor = contentPack.LoadAsset<Texture2D>($"{relativePath}/color.png");
                    this.RegisterBoots(contentPack.Manifest, boots);
                }
            }

            // Load boots
            DirectoryInfo fencesDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Fences"));
            if (fencesDir.Exists)
            {
                foreach (DirectoryInfo dir in fencesDir.EnumerateDirectories())
                {
                    string relativePath = $"Fences/{dir.Name}";

                    // load data
                    FenceData fence = contentPack.ReadJsonFile<FenceData>($"{relativePath}/fence.json");
                    if (fence == null || (fence.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(fence.DisableWithMod)) || (fence.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(fence.EnableWithMod)))
                        continue;

                    fence.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/fence.png");
                    fence.objectTexture = contentPack.LoadAsset<Texture2D>($"{relativePath}/object.png");
                    this.RegisterFence(contentPack.Manifest, fence);
                }
            }

            // Load tailoring
            DirectoryInfo forgeDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Forge"));
            if (forgeDir.Exists)
            {
                foreach (DirectoryInfo dir in forgeDir.EnumerateDirectories())
                {
                    string relativePath = $"Forge/{dir.Name}";

                    // load data
                    ForgeRecipeData recipe = contentPack.ReadJsonFile<ForgeRecipeData>($"{relativePath}/recipe.json");
                    if (recipe == null || (recipe.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(recipe.DisableWithMod)) || (recipe.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(recipe.EnableWithMod)))
                        continue;

                    this.RegisterForgeRecipe(contentPack.Manifest, recipe);
                }
            }
        }

        private void resetAtTitle()
        {
            this.didInit = false;
            // When we go back to the title menu we need to reset things so things don't break when
            // going back to a save.
            this.clearIds(out this.objectIds, this.objects.ToList<DataNeedsId>());
            this.clearIds(out this.cropIds, this.crops.ToList<DataNeedsId>());
            this.clearIds(out this.fruitTreeIds, this.fruitTrees.ToList<DataNeedsId>());
            this.clearIds(out this.bigCraftableIds, this.bigCraftables.ToList<DataNeedsId>());
            this.clearIds(out this.hatIds, this.hats.ToList<DataNeedsId>());
            this.clearIds(out this.weaponIds, this.weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new List<DataNeedsId>();
            clothing.AddRange(this.shirts);
            clothing.AddRange(this.pantss);
            this.clearIds(out this.clothingIds, clothing.ToList<DataNeedsId>());

            this.content1.InvalidateUsed();
            this.Helper.Content.AssetEditors.Remove(this.content2);

            this.locationsFixedAlready.Clear();
        }

        internal void onBlankSave()
        {
            Log.debug("Loading stuff early (really super early)");
            if (string.IsNullOrEmpty(Constants.CurrentSavePath))
            {
                this.initStuff(loadIdFiles: false);
            }
        }

        private void onCreated(object sender, SaveCreatedEventArgs e)
        {
            Log.debug("Loading stuff early (creation)");
            //initStuff(loadIdFiles: false);
        }

        private void onLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveParsed)
            {
                //Log.debug("Loading stuff early (loading)");
                this.initStuff(loadIdFiles: true);
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveLoadedLocations)
            {
                Log.debug("Fixing IDs");
                this.fixIdsEverywhere();
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.Loaded)
            {
                Log.debug("Adding default/leveled recipes");
                foreach (var obj in this.objects)
                {
                    if (obj.Recipe != null)
                    {
                        bool unlockedByLevel = false;
                        if (obj.Recipe.SkillUnlockName?.Length > 0 && obj.Recipe.SkillUnlockLevel > 0)
                        {
                            int level = 0;
                            switch (obj.Recipe.SkillUnlockName)
                            {
                                case "Farming": level = Game1.player.farmingLevel.Value; break;
                                case "Fishing": level = Game1.player.fishingLevel.Value; break;
                                case "Foraging": level = Game1.player.foragingLevel.Value; break;
                                case "Mining": level = Game1.player.miningLevel.Value; break;
                                case "Combat": level = Game1.player.combatLevel.Value; break;
                                case "Luck": level = Game1.player.luckLevel.Value; break;
                                default: level = Game1.player.GetCustomSkillLevel(obj.Recipe.SkillUnlockName); break;
                            }

                            if (level >= obj.Recipe.SkillUnlockLevel)
                            {
                                unlockedByLevel = true;
                            }
                        }
                        if ((obj.Recipe.IsDefault || unlockedByLevel) && !Game1.player.knowsRecipe(obj.Name))
                        {
                            if (obj.Category == ObjectData.Category_.Cooking)
                            {
                                Game1.player.cookingRecipes.Add(obj.Name, 0);
                            }
                            else
                            {
                                Game1.player.craftingRecipes.Add(obj.Name, 0);
                            }
                        }
                    }
                }
                foreach (var big in this.bigCraftables)
                {
                    if (big.Recipe != null)
                    {
                        bool unlockedByLevel = false;
                        if (big.Recipe.SkillUnlockName?.Length > 0 && big.Recipe.SkillUnlockLevel > 0)
                        {
                            int level = 0;
                            switch (big.Recipe.SkillUnlockName)
                            {
                                case "Farming": level = Game1.player.farmingLevel.Value; break;
                                case "Fishing": level = Game1.player.fishingLevel.Value; break;
                                case "Foraging": level = Game1.player.foragingLevel.Value; break;
                                case "Mining": level = Game1.player.miningLevel.Value; break;
                                case "Combat": level = Game1.player.combatLevel.Value; break;
                                case "Luck": level = Game1.player.luckLevel.Value; break;
                                default: level = Game1.player.GetCustomSkillLevel(big.Recipe.SkillUnlockName); break;
                            }

                            if (level >= big.Recipe.SkillUnlockLevel)
                            {
                                unlockedByLevel = true;
                            }
                        }
                        if ((big.Recipe.IsDefault || unlockedByLevel) && !Game1.player.knowsRecipe(big.Name))
                            Game1.player.craftingRecipes.Add(big.Name, 0);
                    }
                }
            }
        }


        private void clientConnected(object sender, PeerContextReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer && !this.didInit)
            {
                Log.debug("Loading stuff early (MP client)");
                this.initStuff(loadIdFiles: false);
            }
        }

        public List<ShopDataEntry> shopData = new();

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null)
                return;

            if (e.NewMenu is TitleMenu)
            {
                this.resetAtTitle();
                return;
            }

            var menu = e.NewMenu as ShopMenu;
            bool hatMouse = menu != null && menu?.potraitPersonDialogue?.Replace("\n", "") == Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11494"), Game1.dialogueFont, 304).Replace("\n", "");
            bool qiGemShop = menu?.storeContext == "QiGemShop";
            string portraitPerson = menu?.portraitPerson?.Name;
            if (portraitPerson == null && Game1.currentLocation?.Name == "Hospital")
                portraitPerson = "Harvey";
            if (menu == null || (portraitPerson == null || portraitPerson == "") && !hatMouse && !qiGemShop)
                return;
            bool doAllSeeds = Game1.player.hasOrWillReceiveMail("PierreStocklist");

            Log.trace($"Adding objects to {portraitPerson}'s shop");
            var forSale = menu.forSale;
            var itemPriceAndStock = menu.itemPriceAndStock;

            foreach (var entry in this.shopData)
            {
                if (!(entry.PurchaseFrom == portraitPerson || (entry.PurchaseFrom == "HatMouse" && hatMouse) || (entry.PurchaseFrom == "QiGemShop" && qiGemShop)))
                    continue;

                bool normalCond = true;
                if (entry.PurchaseRequirements != null && entry.PurchaseRequirements.Length > 0 && entry.PurchaseRequirements[0] != "")
                {
                    normalCond = this.epu.CheckConditions(entry.PurchaseRequirements);
                }
                if (entry.Price == 0 || !normalCond && !(doAllSeeds && entry.ShowWithStocklist && portraitPerson == "Pierre"))
                    continue;

                var item = entry.Object();
                int price = entry.Price;
                if (!normalCond)
                    price = (int)(price * 1.5);
                if (item is StardewValley.Object obj && obj.Category == StardewValley.Object.SeedsCategory)
                {
                    price = (int)(price * Game1.MasterPlayer.difficultyModifier);
                }
                if (item is StardewValley.Object obj2 && obj2.IsRecipe && Game1.player.knowsRecipe(obj2.Name))
                    continue;
                forSale.Add(item);
                if (qiGemShop)
                {
                    itemPriceAndStock.Add(item, new[] { 0, (item is StardewValley.Object obj3 && obj3.IsRecipe) ? 1 : int.MaxValue, 858, price });
                }
                else
                {
                    itemPriceAndStock.Add(item, new[] { price, (item is StardewValley.Object obj3 && obj3.IsRecipe) ? 1 : int.MaxValue });
                }
            }

            this.api.InvokeAddedItemsToShop();
        }

        internal bool didInit;
        private void initStuff(bool loadIdFiles)
        {
            if (this.didInit)
                return;
            this.didInit = true;

            // load object ID mappings from save folder
            // If loadIdFiles is "maybe" (null), check the current save path
            if (loadIdFiles)
            {
                IDictionary<TKey, TValue> LoadDictionary<TKey, TValue>(string filename)
                {
                    string path = Path.Combine(Constants.CurrentSavePath, "JsonAssets", filename);
                    return File.Exists(path)
                        ? JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(File.ReadAllText(path))
                        : new Dictionary<TKey, TValue>();
                }
                Directory.CreateDirectory(Path.Combine(Constants.CurrentSavePath, "JsonAssets"));
                this.oldObjectIds = LoadDictionary<string, int>("ids-objects.json") ?? new Dictionary<string, int>();
                this.oldCropIds = LoadDictionary<string, int>("ids-crops.json") ?? new Dictionary<string, int>();
                this.oldFruitTreeIds = LoadDictionary<string, int>("ids-fruittrees.json") ?? new Dictionary<string, int>();
                this.oldBigCraftableIds = LoadDictionary<string, int>("ids-big-craftables.json") ?? new Dictionary<string, int>();
                this.oldHatIds = LoadDictionary<string, int>("ids-hats.json") ?? new Dictionary<string, int>();
                this.oldWeaponIds = LoadDictionary<string, int>("ids-weapons.json") ?? new Dictionary<string, int>();
                this.oldClothingIds = LoadDictionary<string, int>("ids-clothing.json") ?? new Dictionary<string, int>();
                this.oldBootsIds = LoadDictionary<string, int>("ids-boots.json") ?? new Dictionary<string, int>();

                Log.verbose("OLD IDS START");
                foreach (var id in this.oldObjectIds)
                    Log.verbose("\tObject " + id.Key + " = " + id.Value);
                foreach (var id in this.oldCropIds)
                    Log.verbose("\tCrop " + id.Key + " = " + id.Value);
                foreach (var id in this.oldFruitTreeIds)
                    Log.verbose("\tFruit Tree " + id.Key + " = " + id.Value);
                foreach (var id in this.oldBigCraftableIds)
                    Log.verbose("\tBigCraftable " + id.Key + " = " + id.Value);
                foreach (var id in this.oldHatIds)
                    Log.verbose("\tHat " + id.Key + " = " + id.Value);
                foreach (var id in this.oldWeaponIds)
                    Log.verbose("\tWeapon " + id.Key + " = " + id.Value);
                foreach (var id in this.oldClothingIds)
                    Log.verbose("\tClothing " + id.Key + " = " + id.Value);
                foreach (var id in this.oldBootsIds)
                    Log.verbose("\tBoots " + id.Key + " = " + id.Value);
                Log.verbose("OLD IDS END");
            }

            // assign IDs
            var objList = new List<DataNeedsId>();
            objList.AddRange(this.objects.ToList<DataNeedsId>());
            objList.AddRange(this.bootss.ToList<DataNeedsId>());
            this.objectIds = this.AssignIds("objects", Mod.StartingObjectId, objList);
            this.cropIds = this.AssignIds("crops", Mod.StartingCropId, this.crops.ToList<DataNeedsId>());
            this.fruitTreeIds = this.AssignIds("fruittrees", Mod.StartingFruitTreeId, this.fruitTrees.ToList<DataNeedsId>());
            this.bigCraftableIds = this.AssignIds("big-craftables", Mod.StartingBigCraftableId, this.bigCraftables.ToList<DataNeedsId>());
            this.hatIds = this.AssignIds("hats", Mod.StartingHatId, this.hats.ToList<DataNeedsId>());
            this.weaponIds = this.AssignIds("weapons", Mod.StartingWeaponId, this.weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new List<DataNeedsId>();
            clothing.AddRange(this.shirts);
            clothing.AddRange(this.pantss);
            this.clothingIds = this.AssignIds("clothing", Mod.StartingClothingId, clothing.ToList<DataNeedsId>());

            this.AssignTextureIndices("shirts", Mod.StartingShirtTextureIndex, this.shirts.ToList<DataSeparateTextureIndex>());
            this.AssignTextureIndices("pants", Mod.StartingPantsTextureIndex, this.pantss.ToList<DataSeparateTextureIndex>());
            this.AssignTextureIndices("boots", Mod.StartingBootsId, this.bootss.ToList<DataSeparateTextureIndex>());

            Log.trace("Resetting max shirt/pants value");
            this.Helper.Reflection.GetField<int>(typeof(Clothing), "_maxShirtValue").SetValue(-1);
            this.Helper.Reflection.GetField<int>(typeof(Clothing), "_maxPantsValue").SetValue(-1);

            this.api.InvokeIdsAssigned();

            this.content1.InvalidateUsed();
            this.Helper.Content.AssetEditors.Add(this.content2 = new ContentInjector2());

            // This happens here instead of with ID fixing because TMXL apparently
            // uses the ID fixing API before ID fixing happens everywhere.
            // Doing this here prevents some NREs (that don't show up unless you're
            // debugging for some reason????)
            this.origObjects = this.cloneIdDictAndRemoveOurs(Game1.objectInformation, this.objectIds);
            this.origCrops = this.cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\Crops"), this.cropIds);
            this.origFruitTrees = this.cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\fruitTrees"), this.fruitTreeIds);
            this.origBigCraftables = this.cloneIdDictAndRemoveOurs(Game1.bigCraftablesInformation, this.bigCraftableIds);
            this.origHats = this.cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\hats"), this.hatIds);
            this.origWeapons = this.cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\weapons"), this.weaponIds);
            this.origClothing = this.cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"), this.clothingIds);
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            if (!Directory.Exists(Path.Combine(Constants.CurrentSavePath, "JsonAssets")))
                Directory.CreateDirectory(Path.Combine(Constants.CurrentSavePath, "JsonAssets"));

            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-objects.json"), JsonConvert.SerializeObject(this.objectIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-crops.json"), JsonConvert.SerializeObject(this.cropIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-fruittrees.json"), JsonConvert.SerializeObject(this.fruitTreeIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-big-craftables.json"), JsonConvert.SerializeObject(this.bigCraftableIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-hats.json"), JsonConvert.SerializeObject(this.hatIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-weapons.json"), JsonConvert.SerializeObject(this.weaponIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-clothing.json"), JsonConvert.SerializeObject(this.clothingIds));
        }

        internal IList<ObjectData> myRings = new List<ObjectData>();

        /// <summary>Raised after items are added or removed to a player's inventory. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            IList<int> ringIds = new List<int>();
            foreach (var ring in this.myRings)
                ringIds.Add(ring.id);

            for (int i = 0; i < Game1.player.Items.Count; ++i)
            {
                var item = Game1.player.Items[i];
                if (item is SObject obj && ringIds.Contains(obj.ParentSheetIndex))
                {
                    Log.trace($"Turning a ring-object of {obj.ParentSheetIndex} into a proper ring");
                    Game1.player.Items[i] = new Ring(obj.ParentSheetIndex);
                }
            }
        }

        internal const int StartingObjectId = 3000;
        internal const int StartingCropId = 100;
        internal const int StartingFruitTreeId = 10;
        internal const int StartingBigCraftableId = 300;
        internal const int StartingHatId = 160;
        internal const int StartingWeaponId = 128;
        internal const int StartingClothingId = 3000;
        internal const int StartingShirtTextureIndex = 750;
        internal const int StartingPantsTextureIndex = 20;
        internal const int StartingBootsId = 100;

        internal IList<ObjectData> objects = new List<ObjectData>();
        internal IList<CropData> crops = new List<CropData>();
        internal IList<FruitTreeData> fruitTrees = new List<FruitTreeData>();
        internal IList<BigCraftableData> bigCraftables = new List<BigCraftableData>();
        internal IList<HatData> hats = new List<HatData>();
        internal IList<WeaponData> weapons = new List<WeaponData>();
        internal IList<ShirtData> shirts = new List<ShirtData>();
        internal IList<PantsData> pantss = new List<PantsData>();
        internal IList<TailoringRecipeData> tailoring = new List<TailoringRecipeData>();
        internal IList<BootsData> bootss = new List<BootsData>();
        internal List<FenceData> fences = new();
        internal IList<ForgeRecipeData> forge = new List<ForgeRecipeData>();

        internal IDictionary<string, int> objectIds;
        internal IDictionary<string, int> cropIds;
        internal IDictionary<string, int> fruitTreeIds;
        internal IDictionary<string, int> bigCraftableIds;
        internal IDictionary<string, int> hatIds;
        internal IDictionary<string, int> weaponIds;
        internal IDictionary<string, int> clothingIds;

        internal IDictionary<string, int> oldObjectIds;
        internal IDictionary<string, int> oldCropIds;
        internal IDictionary<string, int> oldFruitTreeIds;
        internal IDictionary<string, int> oldBigCraftableIds;
        internal IDictionary<string, int> oldHatIds;
        internal IDictionary<string, int> oldWeaponIds;
        internal IDictionary<string, int> oldClothingIds;
        internal IDictionary<string, int> oldBootsIds;

        internal IDictionary<int, string> origObjects;
        internal IDictionary<int, string> origCrops;
        internal IDictionary<int, string> origFruitTrees;
        internal IDictionary<int, string> origBigCraftables;
        internal IDictionary<int, string> origHats;
        internal IDictionary<int, string> origWeapons;
        internal IDictionary<int, string> origClothing;
        internal IDictionary<int, string> origBoots;

        public int ResolveObjectId(object data)
        {
            if (data.GetType() == typeof(long))
                return (int)(long)data;
            else
            {
                if (this.objectIds.ContainsKey((string)data))
                    return this.objectIds[(string)data];

                foreach (var obj in Game1.objectInformation)
                {
                    if (obj.Value.Split('/')[0] == (string)data)
                        return obj.Key;
                }

                Log.warn($"No idea what '{data}' is!");
                return 0;
            }
        }

        public int ResolveClothingId(object data)
        {
            if (data.GetType() == typeof(long))
                return (int)(long)data;
            else
            {
                if (this.clothingIds.ContainsKey((string)data))
                    return this.clothingIds[(string)data];

                foreach (var obj in Game1.clothingInformation)
                {
                    if (obj.Value.Split('/')[0] == (string)data)
                        return obj.Key;
                }

                Log.warn($"No idea what '{data}' is!");
                return 0;
            }
        }

        private Dictionary<string, int> AssignIds(string type, int starting, List<DataNeedsId> data)
        {
            data.Sort((dni1, dni2) => dni1.Name.CompareTo(dni2.Name));

            Dictionary<string, int> ids = new Dictionary<string, int>();

            int[] bigSkip = new[] { 309, 310, 311, 326, 340, 434, 447, 459, 599, 621, 628, 629, 630, 631, 632, 633, 645, 812 };

            int currId = starting;
            foreach (var d in data)
            {
                if (d.id == -1)
                {
                    Log.verbose($"New ID: {d.Name} = {currId}");
                    int id = currId++;
                    if (type == "big-craftables")
                    {
                        while (bigSkip.Contains(id))
                        {
                            id = currId++;
                        }
                    }

                    ids.Add(d.Name, id);
                    if (type == "objects" && d is ObjectData objd && objd.IsColored)
                        ++currId;
                    else if (type == "big-craftables" && ((BigCraftableData)d).ReserveExtraIndexCount > 0)
                        currId += ((BigCraftableData)d).ReserveExtraIndexCount;
                    d.id = ids[d.Name];
                }
            }

            return ids;
        }

        private void AssignTextureIndices(string type, int starting, List<DataSeparateTextureIndex> data)
        {
            data.Sort((dni1, dni2) => dni1.Name.CompareTo(dni2.Name));

            Dictionary<string, int> idxs = new Dictionary<string, int>();

            int currIdx = starting;
            foreach (var d in data)
            {
                if (d.textureIndex == -1)
                {
                    Log.verbose($"New texture index: {d.Name} = {currIdx}");
                    idxs.Add(d.Name, currIdx++);
                    if (type == "shirts" && ((ClothingData)d).HasFemaleVariant)
                        ++currIdx;
                    d.textureIndex = idxs[d.Name];
                }
            }
        }

        private void clearIds(out IDictionary<string, int> ids, List<DataNeedsId> objs)
        {
            ids = null;
            foreach (DataNeedsId obj in objs)
            {
                obj.id = -1;
            }
        }

        private IDictionary<int, string> cloneIdDictAndRemoveOurs(IDictionary<int, string> full, IDictionary<string, int> ours)
        {
            var ret = new Dictionary<int, string>(full);
            foreach (var obj in ours)
                ret.Remove(obj.Value);
            return ret;
        }

        private bool reverseFixing;
        private HashSet<string> locationsFixedAlready = new();
        private void fixIdsEverywhere(bool reverse = false)
        {
            this.reverseFixing = reverse;
            if (this.reverseFixing)
            {
                Log.info("Reversing!");
            }

            this.fixItemList(Game1.player.Items);
            this.fixItemList(Game1.player.team.junimoChest);
#pragma warning disable AvoidNetField
            if (Game1.player.leftRing.Value != null && this.fixId(this.oldObjectIds, this.objectIds, Game1.player.leftRing.Value.indexInTileSheet, this.origObjects))
                Game1.player.leftRing.Value = null;
            if (Game1.player.leftRing.Value is CombinedRing cring)
            {
                var toRemoveRing = new List<Ring>();
                foreach (var ring2 in cring.combinedRings)
                {
                    if (this.fixId(this.oldObjectIds, this.objectIds, ring2.indexInTileSheet, this.origObjects))
                        toRemoveRing.Add(ring2);
                }
                foreach (var removeRing in toRemoveRing)
                    cring.combinedRings.Remove(removeRing);
            }
            if (Game1.player.rightRing.Value != null && this.fixId(this.oldObjectIds, this.objectIds, Game1.player.rightRing.Value.indexInTileSheet, this.origObjects))
                Game1.player.rightRing.Value = null;
            if (Game1.player.rightRing.Value is CombinedRing cring2)
            {
                var toRemoveRing = new List<Ring>();
                foreach (var ring2 in cring2.combinedRings)
                {
                    if (this.fixId(this.oldObjectIds, this.objectIds, ring2.indexInTileSheet, this.origObjects))
                        toRemoveRing.Add(ring2);
                }
                foreach (var removeRing in toRemoveRing)
                    cring2.combinedRings.Remove(removeRing);
            }
            if (Game1.player.hat.Value != null && this.fixId(this.oldHatIds, this.hatIds, Game1.player.hat.Value.which, this.origHats))
                Game1.player.hat.Value = null;
            if (Game1.player.shirtItem.Value != null && this.fixId(this.oldClothingIds, this.clothingIds, Game1.player.shirtItem.Value.parentSheetIndex, this.origClothing))
                Game1.player.shirtItem.Value = null;
            if (Game1.player.pantsItem.Value != null && this.fixId(this.oldClothingIds, this.clothingIds, Game1.player.pantsItem.Value.parentSheetIndex, this.origClothing))
                Game1.player.pantsItem.Value = null;
            if (Game1.player.boots.Value != null && this.fixId(this.oldObjectIds, this.objectIds, Game1.player.boots.Value.indexInTileSheet, this.origObjects))
                Game1.player.boots.Value = null;
            /*else if (Game1.player.boots.Value != null)
                Game1.player.boots.Value.reloadData();*/
#pragma warning restore AvoidNetField
            foreach (var loc in Game1.locations)
                this.fixLocation(loc);

            this.fixIdDict(Game1.player.basicShipped, removeUnshippable: true);
            this.fixIdDict(Game1.player.mineralsFound);
            this.fixIdDict(Game1.player.recipesCooked);
            this.fixIdDict2(Game1.player.archaeologyFound);
            this.fixIdDict2(Game1.player.fishCaught);

            var bundleData = Game1.netWorldState.Value.GetUnlocalizedBundleData();
            var bundleData_ = new Dictionary<string, string>(Game1.netWorldState.Value.GetUnlocalizedBundleData());

            foreach (var entry in bundleData_)
            {
                List<string> toks = new List<string>(entry.Value.Split('/'));

                // First, fix some stuff we broke in an earlier build by using .BundleData instead of the unlocalized version
                // Copied from Game1.applySaveFix (case FixBotchedBundleData)
                int temp = 0;
                while (toks.Count > 4 && !int.TryParse(toks[toks.Count - 1], out temp))
                {
                    string last_value = toks[toks.Count - 1];
                    if (char.IsDigit(last_value[last_value.Length - 1]) && last_value.Contains(":") && last_value.Contains("\\"))
                    {
                        break;
                    }
                    toks.RemoveAt(toks.Count - 1);
                }

                // Then actually fix IDs
                string[] toks1 = toks[1].Split(' ');
                if (toks1[0] == "O")
                {
                    int oldId = int.Parse(toks1[1]);
                    if (oldId != -1)
                    {
                        if (this.fixId(this.oldObjectIds, this.objectIds, ref oldId, this.origObjects))
                        {
                            Log.warn($"Bundle reward item missing ({entry.Key}, {oldId})! Probably broken now!");
                            oldId = -1;
                        }
                        else
                        {
                            toks1[1] = oldId.ToString();
                        }
                    }
                }
                else if (toks1[0] == "BO")
                {
                    int oldId = int.Parse(toks1[1]);
                    if (oldId != -1)
                    {
                        if (this.fixId(this.oldBigCraftableIds, this.bigCraftableIds, ref oldId, this.origBigCraftables))
                        {
                            Log.warn($"Bundle reward item missing ({entry.Key}, {oldId})! Probably broken now!");
                            oldId = -1;
                        }
                        else
                        {
                            toks1[1] = oldId.ToString();
                        }
                    }
                }
                toks[1] = string.Join(" ", toks1);
                string[] toks2 = toks[2].Split(' ');
                for (int i = 0; i < toks2.Length; i += 3)
                {
                    int oldId = int.Parse(toks2[i]);
                    if (oldId != -1)
                    {
                        if (this.fixId(this.oldObjectIds, this.objectIds, ref oldId, this.origObjects))
                        {
                            Log.warn($"Bundle item missing ({entry.Key}, {oldId})! Probably broken now!");
                            oldId = -1;
                        }
                        else
                        {
                            toks2[i] = oldId.ToString();
                        }
                    }
                }
                toks[2] = string.Join(" ", toks2);
                bundleData[entry.Key] = string.Join("/", toks);
            }
            // Fix bad bundle data

            Game1.netWorldState.Value.SetBundleData(bundleData);

            if (!this.reverseFixing)
                this.api.InvokeIdsFixed();
            this.reverseFixing = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal bool fixItem(Item item)
        {
            if (item is Hat hat)
            {
                if (this.fixId(this.oldHatIds, this.hatIds, hat.which, this.origHats))
                    return true;
            }
            else if (item is MeleeWeapon weapon)
            {
                if (this.fixId(this.oldWeaponIds, this.weaponIds, weapon.initialParentTileIndex, this.origWeapons))
                    return true;
                else if (this.fixId(this.oldWeaponIds, this.weaponIds, weapon.currentParentTileIndex, this.origWeapons))
                    return true;
                else if (this.fixId(this.oldWeaponIds, this.weaponIds, weapon.indexOfMenuItemView, this.origWeapons))
                    return true;
            }
            else if (item is Ring ring)
            {
                if (this.fixId(this.oldObjectIds, this.objectIds, ring.indexInTileSheet, this.origObjects))
                    return true;
            }
            else if (item is Clothing clothing)
            {
                if (this.fixId(this.oldClothingIds, this.clothingIds, clothing.parentSheetIndex, this.origClothing))
                    return true;
            }
            else if (item is Boots boots)
            {
                if (this.fixId(this.oldObjectIds, this.objectIds, boots.indexInTileSheet, this.origObjects))
                    return true;
                /*else
                    boots.reloadData();*/
            }
            else if (!(item is SObject))
                return false;
            var obj = item as StardewValley.Object;

            if (obj is Chest chest)
            {
                if (this.fixId(this.oldBigCraftableIds, this.bigCraftableIds, chest.parentSheetIndex, this.origBigCraftables))
                    chest.ParentSheetIndex = 130;
                else
                {
                    chest.startingLidFrame.Value = chest.ParentSheetIndex + 1;
                }
                this.fixItemList(chest.items);
            }
            else if (obj is IndoorPot pot)
            {
                var hd = pot.hoeDirt.Value;
                if (hd == null || hd.crop == null)
                    return false;

                int oldId = hd.crop.rowInSpriteSheet.Value;
                if (this.fixId(this.oldCropIds, this.cropIds, hd.crop.rowInSpriteSheet, this.origCrops))
                    hd.crop = null;
                else
                {
                    string key = this.cropIds.FirstOrDefault(x => x.Value == hd.crop.rowInSpriteSheet.Value).Key;
                    var c = this.crops.FirstOrDefault(x => x.Name == key);
                    if (c != null) // Non-JA crop
                    {
                        Log.verbose("Fixing crop product: From " + hd.crop.indexOfHarvest.Value + " to " + c.Product + "=" + this.ResolveObjectId(c.Product));
                        hd.crop.indexOfHarvest.Value = this.ResolveObjectId(c.Product);
                        this.fixId(this.oldObjectIds, this.objectIds, hd.crop.netSeedIndex, this.origObjects);
                    }
                }
            }
            else if (obj is Fence fence)
            {
                if (this.fixId(this.oldObjectIds, this.objectIds, fence.whichType, this.origObjects))
                    return true;
                else
                    fence.ParentSheetIndex = -fence.whichType.Value;
            }
            else if (obj.GetType() == typeof(SObject) || obj.GetType() == typeof(Cask))
            {
                if (!obj.bigCraftable.Value)
                {
                    if (this.fixId(this.oldObjectIds, this.objectIds, obj.preservedParentSheetIndex, this.origObjects))
                        obj.preservedParentSheetIndex.Value = -1;
                    if (this.fixId(this.oldObjectIds, this.objectIds, obj.parentSheetIndex, this.origObjects))
                        return true;
                }
                else
                {
                    if (this.fixId(this.oldBigCraftableIds, this.bigCraftableIds, obj.parentSheetIndex, this.origBigCraftables))
                        return true;
                }
            }

            if (obj.heldObject.Value != null)
            {
                if (this.fixId(this.oldObjectIds, this.objectIds, obj.heldObject.Value.parentSheetIndex, this.origObjects))
                    obj.heldObject.Value = null;

                if (obj.heldObject.Value is Chest chest2)
                {
                    this.fixItemList(chest2.items);
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal void fixLocation(GameLocation loc)
        {
            // TMXL fixes things before the main ID fixing, then adds them to the main location list
            // So things would get double fixed without this.
            if (this.locationsFixedAlready.Contains(loc.NameOrUniqueName))
                return;
            this.locationsFixedAlready.Add(loc.NameOrUniqueName);

            if (loc is FarmHouse fh)
            {
#pragma warning disable AvoidImplicitNetFieldCast
                if (fh.fridge.Value?.items != null)
#pragma warning restore AvoidImplicitNetFieldCast
                    this.fixItemList(fh.fridge.Value.items);
            }
            if (loc is IslandFarmHouse ifh)
            {
#pragma warning disable AvoidImplicitNetFieldCast
                if (ifh.fridge.Value?.items != null)
#pragma warning restore AvoidImplicitNetFieldCast
                    this.fixItemList(ifh.fridge.Value.items);
            }
            if (loc is Cabin cabin)
            {
                var player = cabin.farmhand.Value;
                if (player != null)
                {
                    this.fixItemList(player.Items);
                    //fixItemList( player.team.junimoChest );
#pragma warning disable AvoidNetField
                    if (player.leftRing.Value != null && this.fixId(this.oldObjectIds, this.objectIds, player.leftRing.Value.parentSheetIndex, this.origObjects))
                        player.leftRing.Value = null;
                    if (player.leftRing.Value is CombinedRing cring)
                    {
                        var toRemoveRing = new List<Ring>();
                        foreach (var ring2 in cring.combinedRings)
                        {
                            if (this.fixId(this.oldObjectIds, this.objectIds, ring2.indexInTileSheet, this.origObjects))
                                toRemoveRing.Add(ring2);
                        }
                        foreach (var removeRing in toRemoveRing)
                            cring.combinedRings.Remove(removeRing);
                    }
                    if (player.rightRing.Value != null && this.fixId(this.oldObjectIds, this.objectIds, player.rightRing.Value.parentSheetIndex, this.origObjects))
                        player.rightRing.Value = null;
                    if (player.rightRing.Value is CombinedRing cring2)
                    {
                        var toRemoveRing = new List<Ring>();
                        foreach (var ring2 in cring2.combinedRings)
                        {
                            if (this.fixId(this.oldObjectIds, this.objectIds, ring2.indexInTileSheet, this.origObjects))
                                toRemoveRing.Add(ring2);
                        }
                        foreach (var removeRing in toRemoveRing)
                            cring2.combinedRings.Remove(removeRing);
                    }
                    if (player.hat.Value != null && this.fixId(this.oldHatIds, this.hatIds, player.hat.Value.which, this.origHats))
                        player.hat.Value = null;
                    if (player.shirtItem.Value != null && this.fixId(this.oldClothingIds, this.clothingIds, player.shirtItem.Value.parentSheetIndex, this.origClothing))
                        player.shirtItem.Value = null;
                    if (player.pantsItem.Value != null && this.fixId(this.oldClothingIds, this.clothingIds, player.pantsItem.Value.parentSheetIndex, this.origClothing))
                        player.pantsItem.Value = null;
                    if (player.boots.Value != null && this.fixId(this.oldObjectIds, this.objectIds, player.boots.Value.parentSheetIndex, this.origObjects))
                        player.boots.Value = null;
                    /*else if (player.boots.Value != null)
                        player.boots.Value.reloadData();*/
#pragma warning restore AvoidNetField
                }
            }

            foreach (var npc in loc.characters)
            {
                if (npc is Horse horse)
                {
                    if (horse.hat.Value != null && this.fixId(this.oldHatIds, this.hatIds, horse.hat.Value.which, this.origHats))
                        horse.hat.Value = null;
                }
                else if (npc is Child child)
                {
                    if (child.hat.Value != null && this.fixId(this.oldHatIds, this.hatIds, child.hat.Value.which, this.origHats))
                        child.hat.Value = null;
                }
            }

            IList<Vector2> toRemove = new List<Vector2>();
            foreach (var tfk in loc.terrainFeatures.Keys)
            {
                var tf = loc.terrainFeatures[tfk];
                if (tf is HoeDirt hd)
                {
                    if (hd.crop == null)
                        continue;

                    int oldId = hd.crop.rowInSpriteSheet.Value;
                    if (this.fixId(this.oldCropIds, this.cropIds, hd.crop.rowInSpriteSheet, this.origCrops))
                        hd.crop = null;
                    else
                    {
                        string key = this.cropIds.FirstOrDefault(x => x.Value == hd.crop.rowInSpriteSheet.Value).Key;
                        var c = this.crops.FirstOrDefault(x => x.Name == key);
                        if (c != null) // Non-JA crop
                        {
                            Log.verbose("Fixing crop product: From " + hd.crop.indexOfHarvest.Value + " to " + c.Product + "=" + this.ResolveObjectId(c.Product));
                            hd.crop.indexOfHarvest.Value = this.ResolveObjectId(c.Product);
                            this.fixId(this.oldObjectIds, this.objectIds, hd.crop.netSeedIndex, this.origObjects);
                        }
                    }
                }
                else if (tf is FruitTree ft)
                {
                    int oldId = ft.treeType.Value;
                    if (this.fixId(this.oldFruitTreeIds, this.fruitTreeIds, ft.treeType, this.origFruitTrees))
                        toRemove.Add(tfk);
                    else
                    {
                        string key = this.fruitTreeIds.FirstOrDefault(x => x.Value == ft.treeType.Value).Key;
                        var ftt = this.fruitTrees.FirstOrDefault(x => x.Name == key);
                        if (ftt != null) // Non-JA fruit tree
                        {
                            Log.verbose("Fixing fruit tree product: From " + ft.indexOfFruit.Value + " to " + ftt.Product + "=" + this.ResolveObjectId(ftt.Product));
                            ft.indexOfFruit.Value = this.ResolveObjectId(ftt.Product);
                        }
                    }
                }
            }
            foreach (var rem in toRemove)
                loc.terrainFeatures.Remove(rem);

            toRemove.Clear();
            foreach (var objk in loc.netObjects.Keys)
            {
                var obj = loc.netObjects[objk];
                if (this.fixItem(obj))
                {
                    toRemove.Add(objk);
                }
            }
            foreach (var rem in toRemove)
                loc.objects.Remove(rem);

            toRemove.Clear();
            foreach (var objk in loc.overlayObjects.Keys)
            {
                var obj = loc.overlayObjects[objk];
                if (obj is Chest chest)
                {
                    this.fixItemList(chest.items);
                }
                else if (obj is Sign sign)
                {
                    if (!this.fixItem(sign.displayItem.Value))
                        sign.displayItem.Value = null;
                }
                else if (obj.GetType() == typeof(SObject))
                {
                    if (!obj.bigCraftable.Value)
                    {
                        if (this.fixId(this.oldObjectIds, this.objectIds, obj.parentSheetIndex, this.origObjects))
                            toRemove.Add(objk);
                    }
                    else
                    {
                        if (this.fixId(this.oldBigCraftableIds, this.bigCraftableIds, obj.parentSheetIndex, this.origBigCraftables))
                            toRemove.Add(objk);
                        else if (obj.ParentSheetIndex == 126 && obj.Quality != 0) // Alien rarecrow stores what ID is it is wearing here
                        {
                            obj.Quality--;
                            if (this.fixId(this.oldHatIds, this.hatIds, obj.quality, this.origHats))
                                obj.Quality = 0;
                            else obj.Quality++;
                        }
                    }
                }

                if (obj.heldObject.Value != null)
                {
                    if (this.fixId(this.oldObjectIds, this.objectIds, obj.heldObject.Value.parentSheetIndex, this.origObjects))
                        obj.heldObject.Value = null;

                    if (obj.heldObject.Value is Chest chest2)
                    {
                        this.fixItemList(chest2.items);
                    }
                }
            }
            foreach (var rem in toRemove)
                loc.overlayObjects.Remove(rem);

            if (loc is BuildableGameLocation buildLoc)
                foreach (var building in buildLoc.buildings)
                {
                    if (building.indoors.Value != null)
                        this.fixLocation(building.indoors.Value);
                    if (building is Mill mill)
                    {
                        this.fixItemList(mill.input.Value.items);
                        this.fixItemList(mill.output.Value.items);
                    }
                    else if (building is FishPond pond)
                    {
                        if (pond.fishType.Value == -1)
                        {
                            this.Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                            continue;
                        }

                        if (this.fixId(this.oldObjectIds, this.objectIds, pond.fishType, this.origObjects))
                        {
                            pond.fishType.Value = -1;
                            pond.currentOccupants.Value = 0;
                            pond.maxOccupants.Value = 0;
                            this.Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                        }
                        if (pond.sign.Value != null && this.fixId(this.oldObjectIds, this.objectIds, pond.sign.Value.parentSheetIndex, this.origObjects))
                            pond.sign.Value = null;
                        if (pond.output.Value != null && this.fixId(this.oldObjectIds, this.objectIds, pond.output.Value.parentSheetIndex, this.origObjects))
                            pond.output.Value = null;
                        if (pond.neededItem.Value != null && this.fixId(this.oldObjectIds, this.objectIds, pond.neededItem.Value.parentSheetIndex, this.origObjects))
                            pond.neededItem.Value = null;
                    }
                }

            //if (loc is DecoratableLocation decoLoc)
            foreach (var furniture in loc.furniture)
            {
                if (furniture.heldObject.Value != null)
                {
                    if (!furniture.heldObject.Value.bigCraftable.Value)
                    {
                        if (this.fixId(this.oldObjectIds, this.objectIds, furniture.heldObject.Value.parentSheetIndex, this.origObjects))
                            furniture.heldObject.Value = null;
                    }
                    else
                    {
                        if (this.fixId(this.oldBigCraftableIds, this.bigCraftableIds, furniture.heldObject.Value.parentSheetIndex, this.origBigCraftables))
                            furniture.heldObject.Value = null;
                    }
                }
                if (furniture is StorageFurniture storage)
                    this.fixItemList(storage.heldItems);
            }

            if (loc is Farm farm)
            {
                foreach (var animal in farm.Animals.Values)
                {
                    if (animal.currentProduce.Value != -1)
                        if (this.fixId(this.oldObjectIds, this.objectIds, animal.currentProduce, this.origObjects))
                            animal.currentProduce.Value = -1;
                    if (animal.defaultProduceIndex.Value != -1)
                        if (this.fixId(this.oldObjectIds, this.objectIds, animal.defaultProduceIndex, this.origObjects))
                            animal.defaultProduceIndex.Value = 0;
                    if (animal.deluxeProduceIndex.Value != -1)
                        if (this.fixId(this.oldObjectIds, this.objectIds, animal.deluxeProduceIndex, this.origObjects))
                            animal.deluxeProduceIndex.Value = 0;
                }

                var clumpsToRemove = new List<ResourceClump>();
                foreach (var clump in farm.resourceClumps)
                {
                    if (this.fixId(this.oldObjectIds, this.objectIds, clump.parentSheetIndex, this.origObjects))
                        clumpsToRemove.Add(clump);
                }
                foreach (var clump in clumpsToRemove)
                {
                    farm.resourceClumps.Remove(clump);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal void fixItemList(IList<Item> items)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                var item = items[i];
                if (item == null)
                    continue;
                if (item.GetType() == typeof(SObject))
                {
                    var obj = item as SObject;
                    if (!obj.bigCraftable.Value)
                    {
                        if (this.fixId(this.oldObjectIds, this.objectIds, obj.parentSheetIndex, this.origObjects))
                            items[i] = null;
                    }
                    else
                    {
                        if (this.fixId(this.oldBigCraftableIds, this.bigCraftableIds, obj.parentSheetIndex, this.origBigCraftables))
                            items[i] = null;
                    }
                }
                else if (item is Hat hat)
                {
                    if (this.fixId(this.oldHatIds, this.hatIds, hat.which, this.origHats))
                        items[i] = null;
                }
                else if (item is Tool tool)
                {
                    for (int a = 0; a < tool.attachments?.Count; ++a)
                    {
                        var attached = tool.attachments[a];
                        if (attached == null)
                            continue;

                        if (attached.GetType() != typeof(StardewValley.Object) || attached.bigCraftable)
                        {
                            Log.warn("Unsupported attachment types! Let spacechase0 know he needs to support " + attached.bigCraftable.Value + " " + attached);
                        }
                        else
                        {
                            if (this.fixId(this.oldObjectIds, this.objectIds, attached.parentSheetIndex, this.origObjects))
                            {
                                tool.attachments[a] = null;
                            }
                        }
                    }
                    if (item is MeleeWeapon weapon)
                    {
                        if (this.fixId(this.oldWeaponIds, this.weaponIds, weapon.initialParentTileIndex, this.origWeapons))
                            items[i] = null;
                        else if (this.fixId(this.oldWeaponIds, this.weaponIds, weapon.currentParentTileIndex, this.origWeapons))
                            items[i] = null;
                        else if (this.fixId(this.oldWeaponIds, this.weaponIds, weapon.currentParentTileIndex, this.origWeapons))
                            items[i] = null;
                    }
                }
                else if (item is Ring ring)
                {
                    if (this.fixId(this.oldObjectIds, this.objectIds, ring.indexInTileSheet, this.origObjects))
                        items[i] = null;
                    if (ring is CombinedRing cring)
                    {
                        var toRemove = new List<Ring>();
                        foreach (var ring2 in cring.combinedRings)
                        {
                            if (this.fixId(this.oldObjectIds, this.objectIds, ring2.indexInTileSheet, this.origObjects))
                                toRemove.Add(ring2);
                        }
                        foreach (var removeRing in toRemove)
                            cring.combinedRings.Remove(removeRing);
                    }
                }
                else if (item is Clothing clothing)
                {
                    if (this.fixId(this.oldClothingIds, this.clothingIds, clothing.parentSheetIndex, this.origClothing))
                        items[i] = null;
                }
                else if (item is Boots boots)
                {
                    if (this.fixId(this.oldObjectIds, this.objectIds, boots.indexInTileSheet, this.origObjects))
                        items[i] = null;
                    /*else
                        boots.reloadData();*/
                }
            }
        }

        private void fixIdDict(NetIntDictionary<int, NetInt> dict, bool removeUnshippable = false)
        {
            var toRemove = new List<int>();
            var toAdd = new Dictionary<int, int>();
            foreach (int entry in dict.Keys)
            {
                if (this.origObjects.ContainsKey(entry))
                    continue;
                else if (this.oldObjectIds.Values.Contains(entry))
                {
                    string key = this.oldObjectIds.FirstOrDefault(x => x.Value == entry).Key;
                    bool isRing = this.myRings.FirstOrDefault(r => r.id == entry) != null;
                    bool canShip = this.objects.FirstOrDefault(o => o.id == entry)?.CanSell ?? true;
                    bool hideShippable = this.objects.FirstOrDefault(o => o.id == entry)?.HideFromShippingCollection ?? true;

                    toRemove.Add(entry);
                    if (this.objectIds.ContainsKey(key))
                    {
                        if (removeUnshippable && (!canShip || hideShippable || isRing))
                            ;// Log.warn("Found unshippable");
                        else
                            toAdd.Add(this.objectIds[key], dict[entry]);
                    }
                }
            }
            foreach (int entry in toRemove)
                dict.Remove(entry);
            foreach (var entry in toAdd)
            {
                if (dict.ContainsKey(entry.Key))
                {
                    Log.error("Dict already has value for " + entry.Key + "!");
                    foreach (var obj in this.objects)
                    {
                        if (obj.id == entry.Key)
                            Log.error("\tobj = " + obj.Name);
                    }
                }
                dict.Add(entry.Key, entry.Value);
            }
        }

        private void fixIdDict2(NetIntIntArrayDictionary dict)
        {
            var toRemove = new List<int>();
            var toAdd = new Dictionary<int, int[]>();
            foreach (int entry in dict.Keys)
            {
                if (this.origObjects.ContainsKey(entry))
                    continue;
                else if (this.oldObjectIds.Values.Contains(entry))
                {
                    string key = this.oldObjectIds.FirstOrDefault(x => x.Value == entry).Key;

                    toRemove.Add(entry);
                    if (this.objectIds.ContainsKey(key))
                    {
                        toAdd.Add(this.objectIds[key], dict[entry]);
                    }
                }
            }
            foreach (int entry in toRemove)
                dict.Remove(entry);
            foreach (var entry in toAdd)
                dict.Add(entry.Key, entry.Value);
        }

        // Return true if the item should be deleted, false otherwise.
        // Only remove something if old has it but not new
        private bool fixId(IDictionary<string, int> oldIds, IDictionary<string, int> newIds, NetInt id, IDictionary<int, string> origData)
        {
            if (origData.ContainsKey(id.Value))
                return false;

            if (this.reverseFixing)
            {
                if (newIds.Values.Contains(id.Value))
                {
                    int id_ = id.Value;
                    string key = newIds.FirstOrDefault(x => x.Value == id_).Key;

                    if (oldIds.ContainsKey(key))
                    {
                        id.Value = oldIds[key];
                        Log.verbose("Changing ID: " + key + " from ID " + id_ + " to " + id.Value);
                        return false;
                    }
                    else
                    {
                        Log.warn("New item " + key + " with ID " + id_ + "!");
                        return false;
                    }
                }
                else return false;
            }
            else
            {
                if (oldIds.Values.Contains(id.Value))
                {
                    int id_ = id.Value;
                    string key = oldIds.FirstOrDefault(x => x.Value == id_).Key;

                    if (newIds.ContainsKey(key))
                    {
                        id.Value = newIds[key];
                        Log.trace("Changing ID: " + key + " from ID " + id_ + " to " + id.Value);
                        return false;
                    }
                    else
                    {
                        Log.trace("Deleting missing item " + key + " with old ID " + id_);
                        return true;
                    }
                }
                else return false;
            }
        }

        // Return true if the item should be deleted, false otherwise.
        // Only remove something if old has it but not new
        private bool fixId(IDictionary<string, int> oldIds, IDictionary<string, int> newIds, ref int id, IDictionary<int, string> origData)
        {
            if (origData.ContainsKey(id))
                return false;

            if (this.reverseFixing)
            {
                if (newIds.Values.Contains(id))
                {
                    int id_ = id;
                    string key = newIds.FirstOrDefault(xTile => xTile.Value == id_).Key;

                    if (oldIds.ContainsKey(key))
                    {
                        id = oldIds[key];
                        Log.trace("Changing ID: " + key + " from ID " + id_ + " to " + id);
                        return false;
                    }
                    else
                    {
                        Log.warn("New item " + key + " with ID " + id_ + "!");
                        return false;
                    }
                }
                else return false;
            }
            else
            {
                if (oldIds.Values.Contains(id))
                {
                    int id_ = id;
                    string key = oldIds.FirstOrDefault(x => x.Value == id_).Key;

                    if (newIds.ContainsKey(key))
                    {
                        id = newIds[key];
                        Log.verbose("Changing ID: " + key + " from ID " + id_ + " to " + id);
                        return false;
                    }
                    else
                    {
                        Log.trace("Deleting missing item " + key + " with old ID " + id_);
                        return true;
                    }
                }
                else return false;
            }
        }
    }
}
