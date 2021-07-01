using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JsonAssets.Data;
using JsonAssets.Framework;
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
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
        public static Mod instance;

        private ContentInjector1 Content1;
        private ContentInjector2 Content2;
        internal IExpandedPreconditionsUtilityApi Epu;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("ja_summary", "Summary of JA ids", this.DoCommands);
            helper.ConsoleCommands.Add("ja_unfix", "Unfix IDs once, in case IDs were double fixed.", this.DoCommands);

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveCreated += this.OnCreated;
            helper.Events.GameLoop.UpdateTicked += this.OnTick;
            helper.Events.Specialized.LoadStageChanged += this.OnLoadStageChanged;
            helper.Events.Multiplayer.PeerContextReceived += this.ClientConnected;

            helper.Content.AssetEditors.Add(this.Content1 = new ContentInjector1());
            helper.Content.AssetLoaders.Add(this.Content1);

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
                new HoeDirtPatcher(),
                new ItemPatcher(),
                new ObjectPatcher(),
                new RingPatcher()
            );
        }

        private Api Api;
        public override object GetApi()
        {
            return this.Api ??= new Api(this.LoadData);
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
                if (ret.TryGetValue(newId.Key, out var pair))
                    ret[newId.Key] = new KeyValuePair<int, int>(pair.Key, newId.Value);
                else
                    ret.Add(newId.Key, new KeyValuePair<int, int>(-1, newId.Value));
            }
            return ret;
        }

        private void PrintIdMapping(string header, Dictionary<string, KeyValuePair<int, int>> mapping)
        {
            Log.Info(header);
            Log.Info("-------------------------");

            int len = 0;
            foreach (var entry in mapping)
                len = Math.Max(len, entry.Key.Length);

            foreach (var entry in mapping)
            {
                Log.Info(string.Format("{0,-" + len + "} | {1,5} -> {2,-5}",
                                          entry.Key,
                                          entry.Value.Key == -1 ? "" : entry.Value.Key.ToString(),
                                          entry.Value.Value == -1 ? "" : entry.Value.Value.ToString()));
            }
            Log.Info("");
        }

        private void DoCommands(string cmd, string[] args)
        {
            if (!this.DidInit)
            {
                Log.Info("A save must be loaded first.");
                return;
            }

            if (cmd == "ja_summary")
            {
                var objs = this.MakeIdMapping(this.OldObjectIds, this.ObjectIds);
                var crops = this.MakeIdMapping(this.OldCropIds, this.CropIds);
                var ftrees = this.MakeIdMapping(this.OldFruitTreeIds, this.FruitTreeIds);
                var bigs = this.MakeIdMapping(this.OldBigCraftableIds, this.BigCraftableIds);
                var hats = this.MakeIdMapping(this.OldHatIds, this.HatIds);
                var weapons = this.MakeIdMapping(this.OldWeaponIds, this.WeaponIds);
                var clothings = this.MakeIdMapping(this.OldClothingIds, this.ClothingIds);

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
                this.LocationsFixedAlready.Clear();
                this.FixIdsEverywhere(reverse: true);
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Epu = this.Helper.ModRegistry.GetApi<IExpandedPreconditionsUtilityApi>("Cherry.ExpandedPreconditionsUtility");
            this.Epu.Initialize(false, this.ModManifest.UniqueID);

            ContentPatcherIntegration.Initialize();
        }

        private bool FirstTick = true;
        private void OnTick(object sender, UpdateTickedEventArgs e)
        {
            // This needs to run after GameLaunched, because of the event 
            if (this.FirstTick)
            {
                this.FirstTick = false;

                Log.Info("Loading content packs...");
                foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
                    try
                    {
                        this.LoadData(contentPack);
                    }
                    catch (Exception e1)
                    {
                        Log.Error("Exception loading content pack: " + e1);
                    }
                if (Directory.Exists(Path.Combine(this.Helper.DirectoryPath, "ContentPacks")))
                {
                    foreach (string dir in Directory.EnumerateDirectories(Path.Combine(this.Helper.DirectoryPath, "ContentPacks")))
                        try
                        {
                            this.LoadData(dir);
                        }
                        catch (Exception e2)
                        {
                            Log.Error("Exception loading content pack: " + e2);
                        }
                }
                this.Api.InvokeItemsRegistered();

                this.ResetAtTitle();
            }

        }

        private static readonly Regex NameToId = new("[^a-zA-Z0-9_.]");
        private void LoadData(string dir)
        {
            // read initial info
            IContentPack temp = this.Helper.ContentPacks.CreateFake(dir);
            ContentPackData info = temp.ReadJsonFile<ContentPackData>("content-pack.json");
            if (info == null)
            {
                Log.Warn($"\tNo {dir}/content-pack.json!");
                return;
            }

            // load content pack
            string id = Mod.NameToId.Replace(info.Name, "");
            IContentPack contentPack = this.Helper.ContentPacks.CreateTemporary(dir, id: id, name: info.Name, description: info.Description, author: info.Author, version: new SemanticVersion(info.Version));
            this.LoadData(contentPack);
        }

        internal Dictionary<IManifest, List<string>> ObjectsByContentPack = new();
        internal Dictionary<IManifest, List<string>> CropsByContentPack = new();
        internal Dictionary<IManifest, List<string>> FruitTreesByContentPack = new();
        internal Dictionary<IManifest, List<string>> BigCraftablesByContentPack = new();
        internal Dictionary<IManifest, List<string>> HatsByContentPack = new();
        internal Dictionary<IManifest, List<string>> WeaponsByContentPack = new();
        internal Dictionary<IManifest, List<string>> ClothingByContentPack = new();
        internal Dictionary<IManifest, List<string>> BootsByContentPack = new();

        public void RegisterObject(IManifest source, ObjectData obj)
        {
            this.Objects.Add(obj);

            if (obj.Recipe is { CanPurchase: true })
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = obj.Recipe.PurchaseFrom,
                    Price = obj.Recipe.PurchasePrice,
                    PurchaseRequirements = obj.Recipe.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", obj.Recipe.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(obj.Id, 1, true, obj.Recipe.PurchasePrice)
                });
                if (obj.Recipe.AdditionalPurchaseData != null)
                {
                    foreach (var entry in obj.Recipe.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(obj.Id, 1, true, entry.PurchasePrice)
                        });
                    }
                }
            }
            if (obj.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = obj.PurchaseFrom,
                    Price = obj.PurchasePrice,
                    PurchaseRequirements = obj.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", obj.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(obj.Id, int.MaxValue, false, obj.PurchasePrice)
                });
                if (obj.AdditionalPurchaseData != null)
                {
                    foreach (var entry in obj.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(obj.Id, int.MaxValue, false, entry.PurchasePrice)
                        });
                    }
                }
            }

            // save ring
            if (obj.Category == ObjectData.Category_.Ring)
                this.MyRings.Add(obj);

            // Duplicate check
            if (this.DupObjects.TryGetValue(obj.Name, out IManifest prevManifest))
                Log.Error($"Duplicate object: {obj.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupObjects[obj.Name] = source;

            if (!this.ObjectsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.ObjectsByContentPack[source] = new();
            addedNames.Add(obj.Name);
        }

        public void RegisterCrop(IManifest source, CropData crop, Texture2D seedTex)
        {
            this.Crops.Add(crop);

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
                            Log.Warn($"        Faulty season requirements for {crop.SeedName}!\n        Fixed season requirements: {crop.SeedPurchaseRequirements[index]}");
                        }
                    }
                    if (!crop.SeedPurchaseRequirements.Contains(str.TrimStart('/')))
                    {
                        Log.Trace($"        Adding season requirements for {crop.SeedName}:\n        New season requirements: {strtrimstart}");
                        crop.seed.PurchaseRequirements.Add(strtrimstart);
                    }
                }
                else
                {
                    Log.Trace($"        Adding season requirements for {crop.SeedName}:\n        New season requirements: {strtrimstart}");
                    crop.seed.PurchaseRequirements.Add(strtrimstart);
                }
            }

            if (crop.seed.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = crop.seed.PurchaseFrom,
                    Price = crop.seed.PurchasePrice,
                    PurchaseRequirements = crop.seed.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", crop.seed.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(crop.seed.Id, int.MaxValue, false, crop.seed.PurchasePrice),
                    ShowWithStocklist = true
                });
                if (crop.seed.AdditionalPurchaseData != null)
                {
                    foreach (var entry in crop.seed.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(crop.seed.Id, int.MaxValue, false, entry.PurchasePrice)
                        });
                    }
                }
            }

            // Duplicate check
            if (this.DupCrops.TryGetValue(crop.Name, out IManifest prevManifest))
                Log.Error($"Duplicate crop: {crop.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupCrops[crop.Name] = source;

            this.Objects.Add(crop.seed);

            if (!this.CropsByContentPack.TryGetValue(source, out List<string> addedCrops))
                addedCrops = this.CropsByContentPack[source] = new();
            addedCrops.Add(crop.Name);

            if (!this.ObjectsByContentPack.TryGetValue(source, out List<string> addedSeeds))
                addedSeeds = this.ObjectsByContentPack[source] = new();
            addedSeeds.Add(crop.seed.Name);
        }

        public void RegisterFruitTree(IManifest source, FruitTreeData tree, Texture2D saplingTex)
        {
            this.FruitTrees.Add(tree);

            // save seed
            tree.Sapling = new ObjectData
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
            this.Objects.Add(tree.Sapling);

            if (tree.Sapling.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = tree.Sapling.PurchaseFrom,
                    Price = tree.Sapling.PurchasePrice,
                    PurchaseRequirements = tree.Sapling.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", tree.Sapling.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(Vector2.Zero, tree.Sapling.Id, int.MaxValue)
                });
                if (tree.Sapling.AdditionalPurchaseData != null)
                {
                    foreach (var entry in tree.Sapling.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(Vector2.Zero, tree.Sapling.Id, int.MaxValue)
                        });
                    }
                }
            }

            // Duplicate check
            if (this.DupFruitTrees.TryGetValue(tree.Name, out IManifest prevManifest))
                Log.Error($"Duplicate fruit tree: {tree.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupFruitTrees[tree.Name] = source;

            if (!this.FruitTreesByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.FruitTreesByContentPack[source] = new List<string>();
            addedNames.Add(tree.Name);
        }

        public void RegisterBigCraftable(IManifest source, BigCraftableData craftable)
        {
            this.BigCraftables.Add(craftable);

            if (craftable.Recipe != null && craftable.Recipe.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = craftable.Recipe.PurchaseFrom,
                    Price = craftable.Recipe.PurchasePrice,
                    PurchaseRequirements = craftable.Recipe.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", craftable.Recipe.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(Vector2.Zero, craftable.Id, true)
                });
                if (craftable.Recipe.AdditionalPurchaseData != null)
                {
                    foreach (var entry in craftable.Recipe.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(Vector2.Zero, craftable.Id, true)
                        });
                    }
                }
            }
            if (craftable.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = craftable.PurchaseFrom,
                    Price = craftable.PurchasePrice,
                    PurchaseRequirements = craftable.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", craftable.PurchaseRequirements?.ToArray()) },
                    Object = () => new StardewValley.Object(Vector2.Zero, craftable.Id)
                });
                if (craftable.AdditionalPurchaseData != null)
                {
                    foreach (var entry in craftable.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new StardewValley.Object(Vector2.Zero, craftable.Id)
                        });
                    }
                }
            }

            // Duplicate check
            if (this.DupBigCraftables.TryGetValue(craftable.Name, out IManifest prevManifest))
                Log.Error($"Duplicate big craftable: {craftable.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupBigCraftables[craftable.Name] = source;

            if (!this.BigCraftablesByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.BigCraftablesByContentPack[source] = new();
            addedNames.Add(craftable.Name);
        }

        public void RegisterHat(IManifest source, HatData hat)
        {
            this.Hats.Add(hat);

            if (hat.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = "HatMouse",
                    Price = hat.PurchasePrice,
                    PurchaseRequirements = new string[0],
                    Object = () => new Hat(hat.Id)
                });
            }

            // Duplicate check
            if (this.DupHats.TryGetValue(hat.Name, out IManifest prevManifest))
                Log.Error($"Duplicate hat: {hat.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupHats[hat.Name] = source;

            if (!this.HatsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.HatsByContentPack[source] = new();
            addedNames.Add(hat.Name);
        }

        public void RegisterWeapon(IManifest source, WeaponData weapon)
        {
            this.Weapons.Add(weapon);

            if (weapon.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = weapon.PurchaseFrom,
                    Price = weapon.PurchasePrice,
                    PurchaseRequirements = weapon.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", weapon.PurchaseRequirements?.ToArray()) },
                    Object = () => new MeleeWeapon(weapon.Id)
                });
                if (weapon.AdditionalPurchaseData != null)
                {
                    foreach (var entry in weapon.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new MeleeWeapon(weapon.Id)
                        });
                    }
                }
            }

            // Duplicate check
            if (this.DupWeapons.TryGetValue(weapon.Name, out IManifest prevManifest))
                Log.Error($"Duplicate weapon: {weapon.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupWeapons[weapon.Name] = source;

            if (!this.WeaponsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.WeaponsByContentPack[source] = new();
            addedNames.Add(weapon.Name);
        }

        public void RegisterShirt(IManifest source, ShirtData shirt)
        {
            this.Shirts.Add(shirt);

            // Duplicate check
            if (this.DupShirts.TryGetValue(shirt.Name, out IManifest prevManifest))
                Log.Error($"Duplicate shirt: {shirt.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupShirts[shirt.Name] = source;

            if (!this.ClothingByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.ClothingByContentPack[source] = new();
            addedNames.Add(shirt.Name);
        }

        public void RegisterPants(IManifest source, PantsData pants)
        {
            this.Pants.Add(pants);

            // Duplicate check
            if (this.DupPants.TryGetValue(pants.Name, out IManifest prevManifest))
                Log.Error($"Duplicate pants: {pants.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupPants[pants.Name] = source;

            if (!this.ClothingByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.ClothingByContentPack[source] = new();
            addedNames.Add(pants.Name);
        }

        public void RegisterTailoringRecipe(IManifest source, TailoringRecipeData recipe)
        {
            this.Tailoring.Add(recipe);
        }

        public void RegisterBoots(IManifest source, BootsData boots)
        {
            this.Boots.Add(boots);

            if (boots.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = boots.PurchaseFrom,
                    Price = boots.PurchasePrice,
                    PurchaseRequirements = boots.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", boots.PurchaseRequirements?.ToArray()) },
                    Object = () => new Boots(boots.Id)
                });

                if (boots.AdditionalPurchaseData != null)
                {
                    foreach (var entry in boots.AdditionalPurchaseData)
                    {
                        this.shopData.Add(new ShopDataEntry
                        {
                            PurchaseFrom = entry.PurchaseFrom,
                            Price = entry.PurchasePrice,
                            PurchaseRequirements = entry.PurchaseRequirements == null ? new string[0] : new[] { string.Join("/", entry.PurchaseRequirements?.ToArray()) },
                            Object = () => new Boots(boots.Id)
                        });
                    }
                }
            }

            // Duplicate check
            if (this.DupBoots.TryGetValue(boots.Name, out IManifest prevManifest))
                Log.Error($"Duplicate boots: {boots.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
            else
                this.DupBoots[boots.Name] = source;

            if (!this.BootsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.BootsByContentPack[source] = new();
            addedNames.Add(boots.Name);
        }

        public void RegisterForgeRecipe(IManifest source, ForgeRecipeData recipe)
        {
            this.Forge.Add(recipe);
        }

        public void RegisterFence(IManifest source, FenceData fence)
        {
            this.Fences.Add(fence);

            IList<ObjectData.Recipe_.Ingredient> ConvertIngredients(IList<FenceData.Recipe_.Ingredient> ingredients)
            {
                return ingredients
                    .Select(ingredient => new ObjectData.Recipe_.Ingredient { Object = ingredient.Object, Count = ingredient.Count })
                    .ToList();
            }

            this.RegisterObject(source, fence.correspondingObject = new ObjectData
            {
                texture = fence.objectTexture,
                Name = fence.Name,
                Description = fence.Description,
                Category = ObjectData.Category_.Crafting,
                Price = fence.Price,
                Recipe = fence.Recipe == null ? null : new ObjectData.Recipe_
                {
                    SkillUnlockName = fence.Recipe.SkillUnlockName,
                    SkillUnlockLevel = fence.Recipe.SkillUnlockLevel,
                    ResultCount = fence.Recipe.ResultCount,
                    Ingredients = ConvertIngredients(fence.Recipe.Ingredients),
                    IsDefault = fence.Recipe.IsDefault,
                    CanPurchase = fence.Recipe.CanPurchase,
                    PurchasePrice = fence.Recipe.PurchasePrice,
                    PurchaseFrom = fence.Recipe.PurchaseFrom,
                    PurchaseRequirements = fence.Recipe.PurchaseRequirements,
                    AdditionalPurchaseData = fence.Recipe.AdditionalPurchaseData
                },
                CanPurchase = fence.CanPurchase,
                PurchasePrice = fence.PurchasePrice,
                PurchaseFrom = fence.PurchaseFrom,
                PurchaseRequirements = fence.PurchaseRequirements,
                AdditionalPurchaseData = fence.AdditionalPurchaseData,
                NameLocalization = fence.NameLocalization,
                DescriptionLocalization = fence.DescriptionLocalization
            });
        }

        private readonly Dictionary<string, IManifest> DupObjects = new();
        private readonly Dictionary<string, IManifest> DupCrops = new();
        private readonly Dictionary<string, IManifest> DupFruitTrees = new();
        private readonly Dictionary<string, IManifest> DupBigCraftables = new();
        private readonly Dictionary<string, IManifest> DupHats = new();
        private readonly Dictionary<string, IManifest> DupWeapons = new();
        private readonly Dictionary<string, IManifest> DupShirts = new();
        private readonly Dictionary<string, IManifest> DupPants = new();
        private readonly Dictionary<string, IManifest> DupBoots = new();

        private readonly Regex SeasonLimiter = new("(z(?: spring| summer| fall| winter){2,4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private void LoadData(IContentPack contentPack)
        {
            Log.Info($"\t{contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author} - {contentPack.Manifest.Description}");

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

        private void ResetAtTitle()
        {
            this.DidInit = false;
            // When we go back to the title menu we need to reset things so things don't break when
            // going back to a save.
            this.ClearIds(out this.ObjectIds, this.Objects.ToList<DataNeedsId>());
            this.ClearIds(out this.CropIds, this.Crops.ToList<DataNeedsId>());
            this.ClearIds(out this.FruitTreeIds, this.FruitTrees.ToList<DataNeedsId>());
            this.ClearIds(out this.BigCraftableIds, this.BigCraftables.ToList<DataNeedsId>());
            this.ClearIds(out this.HatIds, this.Hats.ToList<DataNeedsId>());
            this.ClearIds(out this.WeaponIds, this.Weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new List<DataNeedsId>();
            clothing.AddRange(this.Shirts);
            clothing.AddRange(this.Pants);
            this.ClearIds(out this.ClothingIds, clothing.ToList<DataNeedsId>());

            this.Content1.InvalidateUsed();
            this.Helper.Content.AssetEditors.Remove(this.Content2);

            this.LocationsFixedAlready.Clear();
        }

        internal void OnBlankSave()
        {
            Log.Debug("Loading stuff early (really super early)");
            if (string.IsNullOrEmpty(Constants.CurrentSavePath))
            {
                this.InitStuff(loadIdFiles: false);
            }
        }

        private void OnCreated(object sender, SaveCreatedEventArgs e)
        {
            Log.Debug("Loading stuff early (creation)");
            //initStuff(loadIdFiles: false);
        }

        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveParsed)
            {
                //Log.debug("Loading stuff early (loading)");
                this.InitStuff(loadIdFiles: true);
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveLoadedLocations)
            {
                Log.Debug("Fixing IDs");
                this.FixIdsEverywhere();
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.Loaded)
            {
                Log.Debug("Adding default/leveled recipes");
                foreach (var obj in this.Objects)
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
                foreach (var big in this.BigCraftables)
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


        private void ClientConnected(object sender, PeerContextReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer && !this.DidInit)
            {
                Log.Debug("Loading stuff early (MP client)");
                this.InitStuff(loadIdFiles: false);
            }
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
        public List<ShopDataEntry> shopData = new();

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null)
                return;

            if (e.NewMenu is TitleMenu)
            {
                this.ResetAtTitle();
                return;
            }

            var menu = e.NewMenu as ShopMenu;
            bool hatMouse = menu != null && menu?.potraitPersonDialogue?.Replace("\n", "") == Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11494"), Game1.dialogueFont, 304).Replace("\n", "");
            bool qiGemShop = menu?.storeContext == "QiGemShop";
            string portraitPerson = menu?.portraitPerson?.Name;
            if (portraitPerson == null && Game1.currentLocation?.Name == "Hospital")
                portraitPerson = "Harvey";
            if (menu == null || string.IsNullOrEmpty(portraitPerson) && !hatMouse && !qiGemShop)
                return;
            bool doAllSeeds = Game1.player.hasOrWillReceiveMail("PierreStocklist");

            Log.Trace($"Adding objects to {portraitPerson}'s shop");
            var forSale = menu.forSale;
            var itemPriceAndStock = menu.itemPriceAndStock;

            foreach (var entry in this.shopData)
            {
                if (!(entry.PurchaseFrom == portraitPerson || (entry.PurchaseFrom == "HatMouse" && hatMouse) || (entry.PurchaseFrom == "QiGemShop" && qiGemShop)))
                    continue;

                bool normalCond = true;
                if (entry.PurchaseRequirements != null && entry.PurchaseRequirements.Length > 0 && entry.PurchaseRequirements[0] != "")
                {
                    normalCond = this.Epu.CheckConditions(entry.PurchaseRequirements);
                }
                if (entry.Price == 0 || !normalCond && !(doAllSeeds && entry.ShowWithStocklist && portraitPerson == "Pierre"))
                    continue;

                var item = entry.Object();
                int price = entry.Price;
                if (!normalCond)
                    price = (int)(price * 1.5);
                if (item is SObject { Category: SObject.SeedsCategory })
                {
                    price = (int)(price * Game1.MasterPlayer.difficultyModifier);
                }
                if (item is SObject { IsRecipe: true } obj2 && Game1.player.knowsRecipe(obj2.Name))
                    continue;
                forSale.Add(item);

                bool isRecipe = (item as SObject)?.IsRecipe == true;
                int[] values = qiGemShop
                    ? new[] { 0, isRecipe ? 1 : int.MaxValue, 858, price }
                    : new[] { price, isRecipe ? 1 : int.MaxValue };
                itemPriceAndStock.Add(item, values);
            }

            this.Api.InvokeAddedItemsToShop();
        }

        internal bool DidInit;
        private void InitStuff(bool loadIdFiles)
        {
            if (this.DidInit)
                return;
            this.DidInit = true;

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
                this.OldObjectIds = LoadDictionary<string, int>("ids-objects.json") ?? new Dictionary<string, int>();
                this.OldCropIds = LoadDictionary<string, int>("ids-crops.json") ?? new Dictionary<string, int>();
                this.OldFruitTreeIds = LoadDictionary<string, int>("ids-fruittrees.json") ?? new Dictionary<string, int>();
                this.OldBigCraftableIds = LoadDictionary<string, int>("ids-big-craftables.json") ?? new Dictionary<string, int>();
                this.OldHatIds = LoadDictionary<string, int>("ids-hats.json") ?? new Dictionary<string, int>();
                this.OldWeaponIds = LoadDictionary<string, int>("ids-weapons.json") ?? new Dictionary<string, int>();
                this.OldClothingIds = LoadDictionary<string, int>("ids-clothing.json") ?? new Dictionary<string, int>();
                this.OldBootsIds = LoadDictionary<string, int>("ids-boots.json") ?? new Dictionary<string, int>();

                Log.Verbose("OLD IDS START");
                foreach (var id in this.OldObjectIds)
                    Log.Verbose("\tObject " + id.Key + " = " + id.Value);
                foreach (var id in this.OldCropIds)
                    Log.Verbose("\tCrop " + id.Key + " = " + id.Value);
                foreach (var id in this.OldFruitTreeIds)
                    Log.Verbose("\tFruit Tree " + id.Key + " = " + id.Value);
                foreach (var id in this.OldBigCraftableIds)
                    Log.Verbose("\tBigCraftable " + id.Key + " = " + id.Value);
                foreach (var id in this.OldHatIds)
                    Log.Verbose("\tHat " + id.Key + " = " + id.Value);
                foreach (var id in this.OldWeaponIds)
                    Log.Verbose("\tWeapon " + id.Key + " = " + id.Value);
                foreach (var id in this.OldClothingIds)
                    Log.Verbose("\tClothing " + id.Key + " = " + id.Value);
                foreach (var id in this.OldBootsIds)
                    Log.Verbose("\tBoots " + id.Key + " = " + id.Value);
                Log.Verbose("OLD IDS END");
            }

            // assign IDs
            var objList = new List<DataNeedsId>();
            objList.AddRange(this.Objects.ToList<DataNeedsId>());
            objList.AddRange(this.Boots.ToList<DataNeedsId>());
            this.ObjectIds = this.AssignIds("objects", Mod.StartingObjectId, objList);
            this.CropIds = this.AssignIds("crops", Mod.StartingCropId, this.Crops.ToList<DataNeedsId>());
            this.FruitTreeIds = this.AssignIds("fruittrees", Mod.StartingFruitTreeId, this.FruitTrees.ToList<DataNeedsId>());
            this.BigCraftableIds = this.AssignIds("big-craftables", Mod.StartingBigCraftableId, this.BigCraftables.ToList<DataNeedsId>());
            this.HatIds = this.AssignIds("hats", Mod.StartingHatId, this.Hats.ToList<DataNeedsId>());
            this.WeaponIds = this.AssignIds("weapons", Mod.StartingWeaponId, this.Weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new List<DataNeedsId>();
            clothing.AddRange(this.Shirts);
            clothing.AddRange(this.Pants);
            this.ClothingIds = this.AssignIds("clothing", Mod.StartingClothingId, clothing.ToList<DataNeedsId>());

            this.AssignTextureIndices("shirts", Mod.StartingShirtTextureIndex, this.Shirts.ToList<DataSeparateTextureIndex>());
            this.AssignTextureIndices("pants", Mod.StartingPantsTextureIndex, this.Pants.ToList<DataSeparateTextureIndex>());
            this.AssignTextureIndices("boots", Mod.StartingBootsId, this.Boots.ToList<DataSeparateTextureIndex>());

            Log.Trace("Resetting max shirt/pants value");
            this.Helper.Reflection.GetField<int>(typeof(Clothing), "_maxShirtValue").SetValue(-1);
            this.Helper.Reflection.GetField<int>(typeof(Clothing), "_maxPantsValue").SetValue(-1);

            this.Api.InvokeIdsAssigned();

            this.Content1.InvalidateUsed();
            this.Helper.Content.AssetEditors.Add(this.Content2 = new ContentInjector2());

            // This happens here instead of with ID fixing because TMXL apparently
            // uses the ID fixing API before ID fixing happens everywhere.
            // Doing this here prevents some NREs (that don't show up unless you're
            // debugging for some reason????)
            this.OrigObjects = this.CloneIdDictAndRemoveOurs(Game1.objectInformation, this.ObjectIds);
            this.OrigCrops = this.CloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\Crops"), this.CropIds);
            this.OrigFruitTrees = this.CloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\fruitTrees"), this.FruitTreeIds);
            this.OrigBigCraftables = this.CloneIdDictAndRemoveOurs(Game1.bigCraftablesInformation, this.BigCraftableIds);
            this.OrigHats = this.CloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\hats"), this.HatIds);
            this.OrigWeapons = this.CloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\weapons"), this.WeaponIds);
            this.OrigClothing = this.CloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"), this.ClothingIds);
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            if (!Directory.Exists(Path.Combine(Constants.CurrentSavePath, "JsonAssets")))
                Directory.CreateDirectory(Path.Combine(Constants.CurrentSavePath, "JsonAssets"));

            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-objects.json"), JsonConvert.SerializeObject(this.ObjectIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-crops.json"), JsonConvert.SerializeObject(this.CropIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-fruittrees.json"), JsonConvert.SerializeObject(this.FruitTreeIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-big-craftables.json"), JsonConvert.SerializeObject(this.BigCraftableIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-hats.json"), JsonConvert.SerializeObject(this.HatIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-weapons.json"), JsonConvert.SerializeObject(this.WeaponIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-clothing.json"), JsonConvert.SerializeObject(this.ClothingIds));
        }

        internal IList<ObjectData> MyRings = new List<ObjectData>();

        /// <summary>Raised after items are added or removed to a player's inventory. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            IList<int> ringIds = new List<int>();
            foreach (var ring in this.MyRings)
                ringIds.Add(ring.Id);

            for (int i = 0; i < Game1.player.Items.Count; ++i)
            {
                var item = Game1.player.Items[i];
                if (item is SObject obj && ringIds.Contains(obj.ParentSheetIndex))
                {
                    Log.Trace($"Turning a ring-object of {obj.ParentSheetIndex} into a proper ring");
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

        internal IList<ObjectData> Objects = new List<ObjectData>();
        internal IList<CropData> Crops = new List<CropData>();
        internal IList<FruitTreeData> FruitTrees = new List<FruitTreeData>();
        internal IList<BigCraftableData> BigCraftables = new List<BigCraftableData>();
        internal IList<HatData> Hats = new List<HatData>();
        internal IList<WeaponData> Weapons = new List<WeaponData>();
        internal IList<ShirtData> Shirts = new List<ShirtData>();
        internal IList<PantsData> Pants = new List<PantsData>();
        internal IList<TailoringRecipeData> Tailoring = new List<TailoringRecipeData>();
        internal IList<BootsData> Boots = new List<BootsData>();
        internal List<FenceData> Fences = new();
        internal IList<ForgeRecipeData> Forge = new List<ForgeRecipeData>();

        /// <summary>The custom objects' currently assigned IDs, indexed by item name.</summary>
        internal IDictionary<string, int> ObjectIds;

        /// <summary>The custom objects' currently assigned IDs, indexed by item name.</summary>
        internal IDictionary<string, int> CropIds;

        /// <summary>The custom fruit trees' currently assigned IDs, indexed by item name.</summary>
        internal IDictionary<string, int> FruitTreeIds;

        /// <summary>The custom big craftables' currently assigned IDs, indexed by item name.</summary>
        internal IDictionary<string, int> BigCraftableIds;

        /// <summary>The custom hats' currently assigned IDs, indexed by item name.</summary>
        internal IDictionary<string, int> HatIds;

        /// <summary>The custom weapons' currently assigned IDs, indexed by item name.</summary>
        internal IDictionary<string, int> WeaponIds;

        /// <summary>The custom clothing's currently assigned IDs, indexed by item name.</summary>
        internal IDictionary<string, int> ClothingIds;

        /// <summary>The custom objects' previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldObjectIds;

        /// <summary>The custom crops' previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldCropIds;

        /// <summary>The custom fruit trees' previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldFruitTreeIds;

        /// <summary>The custom big craftables' previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldBigCraftableIds;

        /// <summary>The custom hats' previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldHatIds;

        /// <summary>The custom weapons' previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldWeaponIds;

        /// <summary>The custom clothing's previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldClothingIds;

        /// <summary>The custom boots' previously assigned IDs from the save data, indexed by item name.</summary>
        internal IDictionary<string, int> OldBootsIds;

        /// <summary>The vanilla objects' IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigObjects;

        /// <summary>The vanilla objects' IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigCrops;

        /// <summary>The vanilla fruit trees' IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigFruitTrees;

        /// <summary>The vanilla big craftables' IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigBigCraftables;

        /// <summary>The vanilla hats' IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigHats;

        /// <summary>The vanilla weapons' IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigWeapons;

        /// <summary>The vanilla clothing's IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigClothing;

        /// <summary>The vanilla boots' IDs, indexed by item name.</summary>
        internal IDictionary<int, string> OrigBoots;

        public int ResolveObjectId(object data)
        {
            if (data is long inputId)
                return (int)inputId;

            if (this.ObjectIds.TryGetValue((string)data, out int id))
                return id;

            foreach (var obj in Game1.objectInformation)
            {
                if (obj.Value.Split('/')[0] == (string)data)
                    return obj.Key;
            }

            Log.Warn($"No idea what '{data}' is!");
            return 0;
        }

        public int ResolveClothingId(object data)
        {
            if (data is long inputId)
                return (int)inputId;

            if (this.ClothingIds.TryGetValue((string)data, out int id))
                return id;

            foreach (var obj in Game1.clothingInformation)
            {
                if (obj.Value.Split('/')[0] == (string)data)
                    return obj.Key;
            }

            Log.Warn($"No idea what '{data}' is!");
            return 0;
        }

        private Dictionary<string, int> AssignIds(string type, int starting, List<DataNeedsId> data)
        {
            data.Sort((dni1, dni2) => dni1.Name.CompareTo(dni2.Name));

            Dictionary<string, int> ids = new Dictionary<string, int>();

            int[] bigSkip = new[] { 309, 310, 311, 326, 340, 434, 447, 459, 599, 621, 628, 629, 630, 631, 632, 633, 645, 812 };

            int currId = starting;
            foreach (var d in data)
            {
                if (d.Id == -1)
                {
                    Log.Verbose($"New ID: {d.Name} = {currId}");
                    int id = currId++;
                    if (type == "big-craftables")
                    {
                        while (bigSkip.Contains(id))
                        {
                            id = currId++;
                        }
                    }

                    ids.Add(d.Name, id);
                    if (type == "objects" && d is ObjectData { IsColored: true })
                        ++currId;
                    else if (type == "big-craftables" && ((BigCraftableData)d).ReserveExtraIndexCount > 0)
                        currId += ((BigCraftableData)d).ReserveExtraIndexCount;
                    d.Id = ids[d.Name];
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
                    Log.Verbose($"New texture index: {d.Name} = {currIdx}");
                    idxs.Add(d.Name, currIdx++);
                    if (type == "shirts" && ((ClothingData)d).HasFemaleVariant)
                        ++currIdx;
                    d.textureIndex = idxs[d.Name];
                }
            }
        }

        private void ClearIds(out IDictionary<string, int> ids, List<DataNeedsId> objs)
        {
            ids = null;
            foreach (DataNeedsId obj in objs)
            {
                obj.Id = -1;
            }
        }

        private IDictionary<int, string> CloneIdDictAndRemoveOurs(IDictionary<int, string> full, IDictionary<string, int> ours)
        {
            var ret = new Dictionary<int, string>(full);
            foreach (var obj in ours)
                ret.Remove(obj.Value);
            return ret;
        }

        private bool ReverseFixing;
        private readonly HashSet<string> LocationsFixedAlready = new();
        private void FixIdsEverywhere(bool reverse = false)
        {
            this.ReverseFixing = reverse;
            if (this.ReverseFixing)
            {
                Log.Info("Reversing!");
            }

            this.FixItemList(Game1.player.team.junimoChest);
            this.FixCharacter(Game1.player);
            foreach (var loc in Game1.locations)
                this.FixLocation(loc);

            this.FixIdDict(Game1.player.basicShipped, removeUnshippable: true);
            this.FixIdDict(Game1.player.mineralsFound);
            this.FixIdDict(Game1.player.recipesCooked);
            this.FixIdDict2(Game1.player.archaeologyFound);
            this.FixIdDict2(Game1.player.fishCaught);

            var bundleData = Game1.netWorldState.Value.GetUnlocalizedBundleData();
            var bundleDataCopy = new Dictionary<string, string>(Game1.netWorldState.Value.GetUnlocalizedBundleData());

            foreach (var entry in bundleDataCopy)
            {
                List<string> toks = new List<string>(entry.Value.Split('/'));

                // First, fix some stuff we broke in an earlier build by using .BundleData instead of the unlocalized version
                // Copied from Game1.applySaveFix (case FixBotchedBundleData)
                while (toks.Count > 4 && !int.TryParse(toks[toks.Count - 1], out _))
                {
                    string lastValue = toks[toks.Count - 1];
                    if (char.IsDigit(lastValue[lastValue.Length - 1]) && lastValue.Contains(":") && lastValue.Contains("\\"))
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
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, ref oldId, this.OrigObjects))
                        {
                            Log.Warn($"Bundle reward item missing ({entry.Key}, {oldId})! Probably broken now!");
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
                        if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, ref oldId, this.OrigBigCraftables))
                        {
                            Log.Warn($"Bundle reward item missing ({entry.Key}, {oldId})! Probably broken now!");
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
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, ref oldId, this.OrigObjects))
                        {
                            Log.Warn($"Bundle item missing ({entry.Key}, {oldId})! Probably broken now!");
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

            if (!this.ReverseFixing)
                this.Api.InvokeIdsFixed();
            this.ReverseFixing = false;
        }

        /// <summary>Fix item IDs contained by an item, including the item itself.</summary>
        /// <param name="item">The item to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal bool FixItem(Item item)
        {
            switch (item)
            {
                case Hat hat:
                    return this.FixId(this.OldHatIds, this.HatIds, hat.which, this.OrigHats);

                case MeleeWeapon weapon:
                    return
                        this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.initialParentTileIndex, this.OrigWeapons)
                        || this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.currentParentTileIndex, this.OrigWeapons)
                        || this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.indexOfMenuItemView, this.OrigWeapons);

                case Ring ring:
                    return this.FixRing(ring);

                case Clothing clothing:
                    return this.FixId(this.OldClothingIds, this.ClothingIds, clothing.parentSheetIndex, this.OrigClothing);

                case Boots boots:
                    return this.FixId(this.OldObjectIds, this.ObjectIds, boots.indexInTileSheet, this.OrigObjects);


                case SObject obj:
                    if (obj is Chest chest)
                    {
                        if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, chest.parentSheetIndex, this.OrigBigCraftables))
                            chest.ParentSheetIndex = 130;
                        else
                            chest.startingLidFrame.Value = chest.ParentSheetIndex + 1;
                        this.FixItemList(chest.items);
                    }
                    else if (obj is IndoorPot pot)
                    {
                        if (this.FixCrop(pot.hoeDirt.Value?.crop))
                            pot.hoeDirt.Value = null;
                    }
                    else if (obj is Fence fence)
                    {
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, fence.whichType, this.OrigObjects))
                            return true;
                        fence.ParentSheetIndex = -fence.whichType.Value;
                    }
                    else if (obj.GetType() == typeof(SObject) || obj.GetType() == typeof(Cask))
                    {
                        if (!obj.bigCraftable.Value)
                        {
                            if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.preservedParentSheetIndex, this.OrigObjects))
                                obj.preservedParentSheetIndex.Value = -1;
                            if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.parentSheetIndex, this.OrigObjects))
                                return true;
                        }
                        else if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, obj.parentSheetIndex, this.OrigBigCraftables))
                            return true;
                    }

                    if (obj.heldObject.Value != null)
                    {
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.heldObject.Value.parentSheetIndex, this.OrigObjects))
                            obj.heldObject.Value = null;

                        if (obj.heldObject.Value is Chest innerChest)
                            this.FixItemList(innerChest.items);
                    }
                    break;
            }

            return false;
        }

        /// <summary>Fix item IDs contained by a character.</summary>
        /// <param name="character">The character to fix.</param>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        private void FixCharacter(Character character)
        {
            switch (character)
            {
                case Horse horse:
                    if (this.FixId(this.OldHatIds, this.HatIds, horse.hat.Value?.which, this.OrigHats))
                        horse.hat.Value = null;
                    break;

                case Child child:
                    if (this.FixId(this.OldHatIds, this.HatIds, child.hat.Value?.which, this.OrigHats))
                        child.hat.Value = null;
                    break;

                case Farmer player:
                    this.FixItemList(player.Items);
                    if (this.FixRing(player.leftRing.Value))
                        player.leftRing.Value = null;
                    if (this.FixRing(player.rightRing.Value))
                        player.rightRing.Value = null;
                    if (this.FixId(this.OldHatIds, this.HatIds, player.hat.Value?.which, this.OrigHats))
                        player.hat.Value = null;
                    if (this.FixId(this.OldClothingIds, this.ClothingIds, player.shirtItem.Value?.parentSheetIndex, this.OrigClothing))
                        player.shirtItem.Value = null;
                    if (this.FixId(this.OldClothingIds, this.ClothingIds, player.pantsItem.Value?.parentSheetIndex, this.OrigClothing))
                        player.pantsItem.Value = null;
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, player.boots.Value?.indexInTileSheet, this.OrigObjects))
                        player.boots.Value = null;
                    break;
            }
        }

        /// <summary>Fix item IDs contained by a ring, including the ring itself.</summary>
        /// <param name="ring">The ring to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        private bool FixRing(Ring ring)
        {
            if (ring is null)
                return false;

            // main ring
            if (this.FixId(this.OldObjectIds, this.ObjectIds, ring.indexInTileSheet, this.OrigObjects))
                return true;

            // inner rings
            if (ring is CombinedRing combinedRing)
            {
                for (int i = combinedRing.combinedRings.Count - 1; i >= 0; i--)
                {
                    Ring innerRing = combinedRing.combinedRings[i];
                    if (this.FixRing(innerRing))
                        combinedRing.combinedRings.RemoveAt(i);
                }
            }

            return false;
        }

        /// <summary>Fix item IDs contained by a location.</summary>
        /// <param name="loc">The location to fix.</param>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal void FixLocation(GameLocation loc)
        {
            if (loc is null)
                return;

            // TMXL fixes things before the main ID fixing, then adds them to the main location list
            // So things would get double fixed without this.
            if (this.LocationsFixedAlready.Contains(loc.NameOrUniqueName))
                return;
            this.LocationsFixedAlready.Add(loc.NameOrUniqueName);

            switch (loc)
            {
                case FarmHouse house:
                    this.FixItemList(house.fridge.Value?.items);
                    if (house is Cabin cabin)
                        this.FixCharacter(cabin.farmhand.Value);
                    break;

                case IslandFarmHouse house:
                    this.FixItemList(house.fridge.Value?.items);
                    break;
            }

            foreach (var npc in loc.characters)
                this.FixCharacter(npc);

            IList<Vector2> toRemove = new List<Vector2>();
            foreach (var pair in loc.terrainFeatures.Pairs)
            {
                if (this.FixTerrainFeature(pair.Value))
                    toRemove.Add(pair.Key);
            }
            foreach (Vector2 rem in toRemove)
                loc.terrainFeatures.Remove(rem);

            toRemove.Clear();
            foreach (var pair in loc.netObjects.Pairs)
            {
                SObject obj = pair.Value;
                if (this.FixItem(obj))
                    toRemove.Add(pair.Key);
            }
            foreach (var rem in toRemove)
                loc.objects.Remove(rem);

            toRemove.Clear();
            foreach (var pair in loc.overlayObjects)
            {
                SObject obj = pair.Value;
                if (obj is Chest chest)
                    this.FixItemList(chest.items);
                else if (obj is Sign sign)
                {
                    if (!this.FixItem(sign.displayItem.Value))
                        sign.displayItem.Value = null;
                }
                else if (obj.GetType() == typeof(SObject))
                {
                    if (!obj.bigCraftable.Value)
                    {
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.parentSheetIndex, this.OrigObjects))
                            toRemove.Add(pair.Key);
                    }
                    else
                    {
                        if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, obj.parentSheetIndex, this.OrigBigCraftables))
                            toRemove.Add(pair.Key);
                        else if (obj.ParentSheetIndex == 126 && obj.Quality != 0) // Alien rarecrow stores what ID is it is wearing here
                        {
                            obj.Quality--;
                            if (this.FixId(this.OldHatIds, this.HatIds, obj.quality, this.OrigHats))
                                obj.Quality = 0;
                            else obj.Quality++;
                        }
                    }
                }

                if (obj.heldObject.Value != null)
                {
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.heldObject.Value.parentSheetIndex, this.OrigObjects))
                        obj.heldObject.Value = null;

                    if (obj.heldObject.Value is Chest chest2)
                        this.FixItemList(chest2.items);
                }
            }
            foreach (var rem in toRemove)
                loc.overlayObjects.Remove(rem);

            if (loc is BuildableGameLocation buildLoc)
            {
                foreach (var building in buildLoc.buildings)
                    this.FixBuilding(building);
            }

            //if (loc is DecoratableLocation decoLoc)
            foreach (var furniture in loc.furniture)
            {
                if (furniture.heldObject.Value != null)
                {
                    if (!furniture.heldObject.Value.bigCraftable.Value)
                    {
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, furniture.heldObject.Value.parentSheetIndex, this.OrigObjects))
                            furniture.heldObject.Value = null;
                    }
                    else
                    {
                        if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, furniture.heldObject.Value.parentSheetIndex, this.OrigBigCraftables))
                            furniture.heldObject.Value = null;
                    }
                }
                if (furniture is StorageFurniture storage)
                    this.FixItemList(storage.heldItems);
            }

            if (loc is Farm farm)
            {
                foreach (var animal in farm.Animals.Values)
                    this.FixFarmAnimal(animal);

                foreach (var clump in farm.resourceClumps.Where(this.FixResourceClump).ToArray())
                    farm.resourceClumps.Remove(clump);
            }
        }

        /// <summary>Fix item IDs contained by a building.</summary>
        /// <param name="building">The building to fix.</param>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        private void FixBuilding(Building building)
        {
            if (building is null)
                return;

            this.FixLocation(building.indoors.Value);

            switch (building)
            {
                case Mill mill:
                    this.FixItemList(mill.input.Value.items);
                    this.FixItemList(mill.output.Value.items);
                    break;

                case FishPond pond:
                    if (pond.fishType.Value == -1)
                    {
                        this.Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                        break;
                    }

                    if (this.FixId(this.OldObjectIds, this.ObjectIds, pond.fishType, this.OrigObjects))
                    {
                        pond.fishType.Value = -1;
                        pond.currentOccupants.Value = 0;
                        pond.maxOccupants.Value = 0;
                        this.Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                    }
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, pond.sign.Value?.parentSheetIndex, this.OrigObjects))
                        pond.sign.Value = null;
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, pond.output.Value?.parentSheetIndex, this.OrigObjects))
                        pond.output.Value = null;
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, pond.neededItem.Value?.parentSheetIndex, this.OrigObjects))
                        pond.neededItem.Value = null;
                    break;
            }
        }

        /// <summary>Fix item IDs contained by a crop, including the crop itself.</summary>
        /// <param name="crop">The crop to fix.</param>
        /// <returns>Returns whether the crop should be removed.</returns>
        private bool FixCrop(Crop crop)
        {
            if (crop is null)
                return false;

            // fix crop
            if (this.FixId(this.OldCropIds, this.CropIds, crop.rowInSpriteSheet, this.OrigCrops))
                return true;

            // fix index of harvest
            string key = this.CropIds.FirstOrDefault(x => x.Value == crop.rowInSpriteSheet.Value).Key;
            CropData cropData = this.Crops.FirstOrDefault(x => x.Name == key);
            if (cropData != null) // Non-JA crop
            {
                Log.Verbose($"Fixing crop product: From {crop.indexOfHarvest.Value} to {cropData.Product}={this.ResolveObjectId(cropData.Product)}");
                crop.indexOfHarvest.Value = this.ResolveObjectId(cropData.Product);
                this.FixId(this.OldObjectIds, this.ObjectIds, crop.netSeedIndex, this.OrigObjects);
            }

            return false;
        }

        /// <summary>Fix item IDs contained by a farm animal.</summary>
        /// <param name="animal">The farm animal to fix.</param>
        private void FixFarmAnimal(FarmAnimal animal)
        {
            foreach (NetInt id in new[] { animal.currentProduce, animal.defaultProduceIndex, animal.deluxeProduceIndex })
            {
                if (id.Value != -1)
                {
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, id, this.OrigObjects))
                        id.Value = -1;
                }
            }
        }

        /// <summary>Fix item IDs contained by a terrain feature, including the clump itself.</summary>
        /// <param name="clump">The resource clump to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        private bool FixResourceClump(ResourceClump clump)
        {
            return this.FixId(this.OldObjectIds, this.ObjectIds, clump.parentSheetIndex, this.OrigObjects);
        }

        /// <summary>Fix item IDs contained by a terrain feature, including the terrain feature itself.</summary>
        /// <param name="feature">The terrain feature to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        private bool FixTerrainFeature(TerrainFeature feature)
        {
            switch (feature)
            {
                case HoeDirt dirt:
                    if (this.FixCrop(dirt.crop))
                        dirt.crop = null;
                    return false;

                case FruitTree tree:
                    {
                        if (this.FixId(this.OldFruitTreeIds, this.FruitTreeIds, tree.treeType, this.OrigFruitTrees))
                            return true;

                        string key = this.FruitTreeIds.FirstOrDefault(x => x.Value == tree.treeType.Value).Key;
                        FruitTreeData treeData = this.FruitTrees.FirstOrDefault(x => x.Name == key);
                        if (treeData != null) // Non-JA fruit tree
                        {
                            Log.Verbose($"Fixing fruit tree product: From {tree.indexOfFruit.Value} to {treeData.Product}={this.ResolveObjectId(treeData.Product)}");
                            tree.indexOfFruit.Value = this.ResolveObjectId(treeData.Product);
                        }

                        return false;
                    }

                default:
                    return false;
            }
        }

        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal void FixItemList(IList<Item> items)
        {
            if (items is null)
                return;

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
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.parentSheetIndex, this.OrigObjects))
                            items[i] = null;
                    }
                    else
                    {
                        if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, obj.parentSheetIndex, this.OrigBigCraftables))
                            items[i] = null;
                    }
                }
                else if (item is Hat hat)
                {
                    if (this.FixId(this.OldHatIds, this.HatIds, hat.which, this.OrigHats))
                        items[i] = null;
                }
                else if (item is Tool tool)
                {
                    for (int a = 0; a < tool.attachments?.Count; ++a)
                    {
                        var attached = tool.attachments[a];
                        if (attached == null)
                            continue;

                        if (attached.GetType() != typeof(SObject) || attached.bigCraftable.Value)
                        {
                            Log.Warn($"Unsupported attachment types! Let spacechase0 know he needs to support {attached.bigCraftable.Value} {attached}");
                        }
                        else
                        {
                            if (this.FixId(this.OldObjectIds, this.ObjectIds, attached.parentSheetIndex, this.OrigObjects))
                            {
                                tool.attachments[a] = null;
                            }
                        }
                    }
                    if (item is MeleeWeapon weapon)
                    {
                        if (this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.initialParentTileIndex, this.OrigWeapons))
                            items[i] = null;
                        else if (this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.currentParentTileIndex, this.OrigWeapons))
                            items[i] = null;
                        else if (this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.currentParentTileIndex, this.OrigWeapons))
                            items[i] = null;
                    }
                }
                else if (item is Ring ring)
                {
                    if (this.FixRing(ring))
                        items[i] = null;
                }
                else if (item is Clothing clothing)
                {
                    if (this.FixId(this.OldClothingIds, this.ClothingIds, clothing.parentSheetIndex, this.OrigClothing))
                        items[i] = null;
                }
                else if (item is Boots boots)
                {
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, boots.indexInTileSheet, this.OrigObjects))
                        items[i] = null;
                    /*else
                        boots.reloadData();*/
                }
            }
        }

        private void FixIdDict(NetIntDictionary<int, NetInt> dict, bool removeUnshippable = false)
        {
            var toRemove = new List<int>();
            var toAdd = new Dictionary<int, int>();
            foreach (int entry in dict.Keys)
            {
                if (this.OrigObjects.ContainsKey(entry))
                    continue;

                if (this.OldObjectIds.Values.Contains(entry))
                {
                    string key = this.OldObjectIds.FirstOrDefault(x => x.Value == entry).Key;
                    bool isRing = this.MyRings.FirstOrDefault(r => r.Id == entry) != null;
                    bool canShip = this.Objects.FirstOrDefault(o => o.Id == entry)?.CanSell ?? true;
                    bool hideShippable = this.Objects.FirstOrDefault(o => o.Id == entry)?.HideFromShippingCollection ?? true;

                    toRemove.Add(entry);
                    if (this.ObjectIds.TryGetValue(key, out int id) && (!removeUnshippable || (canShip && !hideShippable && !isRing)))
                        toAdd.Add(id, dict[entry]);
                }
            }
            foreach (int entry in toRemove)
                dict.Remove(entry);
            foreach (var entry in toAdd)
            {
                if (dict.ContainsKey(entry.Key))
                {
                    Log.Error("Dict already has value for " + entry.Key + "!");
                    foreach (var obj in this.Objects)
                    {
                        if (obj.Id == entry.Key)
                            Log.Error("\tobj = " + obj.Name);
                    }
                }
                dict.Add(entry.Key, entry.Value);
            }
        }

        private void FixIdDict2(NetIntIntArrayDictionary dict)
        {
            var toRemove = new List<int>();
            var toAdd = new Dictionary<int, int[]>();
            foreach (int entry in dict.Keys)
            {
                if (this.OrigObjects.ContainsKey(entry))
                    continue;

                if (this.OldObjectIds.Values.Contains(entry))
                {
                    string key = this.OldObjectIds.FirstOrDefault(x => x.Value == entry).Key;

                    toRemove.Add(entry);
                    if (this.ObjectIds.TryGetValue(key, out int id))
                        toAdd.Add(id, dict[entry]);
                }
            }
            foreach (int entry in toRemove)
                dict.Remove(entry);
            foreach (var entry in toAdd)
                dict.Add(entry.Key, entry.Value);
        }

        /// <summary>Fix item IDs contained by an item, including the item itself.</summary>
        /// <param name="oldIds">The custom items' previously assigned IDs from the save data, indexed by item name.</param>
        /// <param name="newIds">The custom items' currently assigned IDs, indexed by item name.</param>
        /// <param name="id">The current item ID.</param>
        /// <param name="origData">The vanilla items' IDs, indexed by item name.</param>
        /// <returns>Returns whether the item should be removed. Items should only be removed if they no longer exist in the new data.</returns>
        private bool FixId(IDictionary<string, int> oldIds, IDictionary<string, int> newIds, NetInt id, IDictionary<int, string> origData)
        {
            if (id is null)
                return false;

            if (origData.ContainsKey(id.Value))
                return false;

            if (this.ReverseFixing)
            {
                if (newIds.Values.Contains(id.Value))
                {
                    int curId = id.Value;
                    string key = newIds.FirstOrDefault(x => x.Value == curId).Key;

                    if (oldIds.TryGetValue(key, out int oldId))
                    {
                        id.Value = oldId;
                        Log.Verbose("Changing ID: " + key + " from ID " + curId + " to " + id.Value);
                        return false;
                    }
                    else
                    {
                        Log.Warn("New item " + key + " with ID " + curId + "!");
                        return false;
                    }
                }
                else return false;
            }
            else
            {
                if (oldIds.Values.Contains(id.Value))
                {
                    int curId = id.Value;
                    string key = oldIds.FirstOrDefault(x => x.Value == curId).Key;

                    if (newIds.TryGetValue(key, out int newId))
                    {
                        id.Value = newId;
                        Log.Trace("Changing ID: " + key + " from ID " + curId + " to " + id.Value);
                        return false;
                    }
                    else
                    {
                        Log.Trace("Deleting missing item " + key + " with old ID " + curId);
                        return true;
                    }
                }
                else return false;
            }
        }

        // Return true if the item should be deleted, false otherwise.
        // Only remove something if old has it but not new
        private bool FixId(IDictionary<string, int> oldIds, IDictionary<string, int> newIds, ref int id, IDictionary<int, string> origData)
        {
            if (origData.ContainsKey(id))
                return false;

            if (this.ReverseFixing)
            {
                if (newIds.Values.Contains(id))
                {
                    int curId = id;
                    string key = newIds.FirstOrDefault(xTile => xTile.Value == curId).Key;

                    if (oldIds.TryGetValue(key, out int oldId))
                    {
                        id = oldId;
                        Log.Trace("Changing ID: " + key + " from ID " + curId + " to " + id);
                        return false;
                    }
                    else
                    {
                        Log.Warn("New item " + key + " with ID " + curId + "!");
                        return false;
                    }
                }
                else return false;
            }
            else
            {
                if (oldIds.Values.Contains(id))
                {
                    int curId = id;
                    string key = oldIds.FirstOrDefault(x => x.Value == curId).Key;

                    if (newIds.TryGetValue(key, out int newId))
                    {
                        id = newId;
                        Log.Verbose("Changing ID: " + key + " from ID " + curId + " to " + id);
                        return false;
                    }
                    else
                    {
                        Log.Trace("Deleting missing item " + key + " with old ID " + curId);
                        return true;
                    }
                }
                else return false;
            }
        }
    }
}
