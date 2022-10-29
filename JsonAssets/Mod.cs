using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using JsonAssets.Data;
using JsonAssets.Framework;
using JsonAssets.Framework.ContentPatcher;
using JsonAssets.Patches;
using JsonAssets.Utilities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using Spacechase.Shared.Patching;
using SpaceCore;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
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

        /// <summary>The Expanded Preconditions Utility API, if that mod is loaded.</summary>
        private IExpandedPreconditionsUtilityApi ExpandedPreconditionsUtility;

        /// <summary>The last shop menu Json Assets added items to.</summary>
        /// <remarks>This is used to avoid adding items again if the menu was stashed and restored (e.g. by Lookup Anything).</remarks>
        private ShopMenu LastShopMenu;

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

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("ja_summary", "Summary of JA ids", this.DoCommands);
            helper.ConsoleCommands.Add("ja_unfix", "Unfix IDs once, in case IDs were double fixed.", this.DoCommands);
            helper.ConsoleCommands.Add("ja_fix", "Fix IDs once.", this.DoCommands);

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveCreated += this.OnCreated;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.UpdateTicked += this.OnTick;
            helper.Events.Specialized.LoadStageChanged += this.OnLoadStageChanged;
            helper.Events.Multiplayer.PeerContextReceived += this.ClientConnected;

            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            TileSheetExtensions.RegisterExtendedTileSheet(PathUtilities.NormalizeAssetName(@"Maps\springobjects"), 16);
            TileSheetExtensions.RegisterExtendedTileSheet(PathUtilities.NormalizeAssetName(@"TileSheets\Craftables"), 32);
            TileSheetExtensions.RegisterExtendedTileSheet(PathUtilities.NormalizeAssetName(@"TileSheets\crops"), 32);
            TileSheetExtensions.RegisterExtendedTileSheet(PathUtilities.NormalizeAssetName(@"TileSheets\fruitTrees"), 80);
            TileSheetExtensions.RegisterExtendedTileSheet(PathUtilities.NormalizeAssetName(@"Characters\Farmer\shirts"), 32);
            TileSheetExtensions.RegisterExtendedTileSheet(PathUtilities.NormalizeAssetName(@"Characters\Farmer\pants"), 688);
            TileSheetExtensions.RegisterExtendedTileSheet(PathUtilities.NormalizeAssetName(@"Characters\Farmer\hats"), 80);

            HarmonyPatcher.Apply(this,
                new CropPatcher(),
                new FencePatcher(),
                new ForgeMenuPatcher(),
                new Game1Patcher(),
                new GiantCropPatcher(),
                new ItemPatcher(),
                new ObjectPatcher(),
                new RingPatcher(),
                new ShopMenuPatcher(),
                new BootPatcher(),
                new CraftingRecipePatcher()
            );

            ItemResolver.Initialize(helper.GameContent);
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            if (this.DidInit)
                this.ResetAtTitle();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            ContentInjector1.OnAssetRequested(e);
            ContentInjector2.OnAssetRequested(e);
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
                if (ret.TryGetValue(newId.Key, out KeyValuePair<int, int> pair))
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
                if (!Context.IsMainPlayer)
                {
                    Log.Warn("Only the main player can use this command!");
                    return;
                }
                this.LocationsFixedAlready.Clear();
                this.FixIdsEverywhere(reverse: true);
            }
            else if (cmd is "ja_fix")
            {
                if (!Context.IsMainPlayer)
                {
                    Log.Warn("Only the main player can use this command!");
                    return;
                }
                this.LocationsFixedAlready.Clear();
                this.FixIdsEverywhere(reverse: false);
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.ExpandedPreconditionsUtility = this.Helper.ModRegistry.GetApi<IExpandedPreconditionsUtilityApi>("Cherry.ExpandedPreconditionsUtility");
            this.ExpandedPreconditionsUtility?.Initialize(false, this.ModManifest.UniqueID);

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

        private static readonly Regex NameToId = new("[^a-zA-Z0-9_.]", RegexOptions.Compiled|RegexOptions.ECMAScript);

        /// <summary>Load a folder as a Json Assets content pack.</summary>
        /// <param name="path">The absolute path to the content pack folder.</param>
        /// <param name="translations">The translations to use for <c>TranslationKey</c> fields, or <c>null</c> to load the content pack's <c>i18n</c> folder if present.</param>
        private void LoadData(string path, ITranslationHelper translations = null)
        {
            // read initial info
            IContentPack temp = this.Helper.ContentPacks.CreateFake(path);
            ContentPackData info = temp.ReadJsonFile<ContentPackData>("content-pack.json");
            if (info == null)
            {
                Log.Warn($"\tNo {path}/content-pack.json!");
                return;
            }

            // load content pack
            string id = Mod.NameToId.Replace(info.Name, "");
            IContentPack contentPack = this.Helper.ContentPacks.CreateTemporary(path, id: id, name: info.Name, description: info.Description, author: info.Author, version: new SemanticVersion(info.Version));
            this.LoadData(contentPack, translations);
        }

        internal Dictionary<IManifest, List<string>> ObjectsByContentPack = new();
        internal Dictionary<IManifest, List<string>> CropsByContentPack = new();
        internal Dictionary<IManifest, List<string>> FruitTreesByContentPack = new();
        internal Dictionary<IManifest, List<string>> BigCraftablesByContentPack = new();
        internal Dictionary<IManifest, List<string>> HatsByContentPack = new();
        internal Dictionary<IManifest, List<string>> WeaponsByContentPack = new();
        internal Dictionary<IManifest, List<string>> ClothingByContentPack = new();
        internal Dictionary<IManifest, List<string>> BootsByContentPack = new();

        /// <summary>Register a custom object with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the object.</param>
        /// <param name="obj">The object data.</param>
        public void RegisterObject(IManifest source, ObjectData obj)
        {
            this.RegisterObject(source, obj, null);
        }

        /// <summary>Register a custom object with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the object.</param>
        /// <param name="obj">The object data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="ObjectData.TranslationKey"/> is used.</param>
        public void RegisterObject(IManifest source, ObjectData obj, ITranslationHelper translations)
        {
            // load data
            obj.InvokeOnDeserialized();
            this.PopulateTranslations(obj, translations);

            // validate
            if (!this.AssertHasName(obj, "object", source, translations))
                return;

            // check for duplicates
            if (this.DupObjects.TryGetValue(obj.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate object: {obj.Name} from {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupObjects[obj.Name] = source;

            // save data
            this.Objects.Add(obj);

            // add recipe to shops
            if (obj.Recipe is { CanPurchase: true })
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = obj.Recipe.PurchaseFrom,
                    Price = obj.Recipe.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, obj.Recipe.PurchaseRequirements),
                    Object = () => new SObject(obj.Id, 1, true, obj.Recipe.PurchasePrice)
                });

                foreach (var entry in obj.Recipe.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(obj.Id, 1, true, entry.PurchasePrice)
                    });
                }
            }

            // add object to shops
            if (obj.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = obj.PurchaseFrom,
                    Price = obj.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, obj.PurchaseRequirements),
                    Object = () => new SObject(obj.Id, int.MaxValue, false, obj.Price)
                });
                foreach (var entry in obj.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(obj.Id, int.MaxValue, false, obj.Price)
                    });
                }
            }

            // save ring
            if (obj.Category == ObjectCategory.Ring)
                this.MyRings.Add(obj);

            // track added
            if (!this.ObjectsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.ObjectsByContentPack[source] = new();
            addedNames.Add(obj.Name);
        }

        /// <summary>Register a custom crop with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the crop.</param>
        /// <param name="crop">The crop data.</param>
        /// <param name="seedTex">The crop's seed texture.</param>
        public void RegisterCrop(IManifest source, CropData crop, Texture2D seedTex)
        {
            this.RegisterCrop(source, crop, seedTex, null);
        }

        /// <summary>Register a custom crop with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the crop.</param>
        /// <param name="crop">The crop data.</param>
        /// <param name="seedTexture">The crop's seed texture.</param>
        /// <param name="translations">The translations from which to get text if <see cref="CropData.SeedTranslationKey"/> is used.</param>
        public void RegisterCrop(IManifest source, CropData crop, Texture2D seedTexture, ITranslationHelper translations)
        {
            // load data
            crop.InvokeOnDeserialized();
            crop.Seed = new ObjectData
            {
                Texture = seedTexture,
                Name = crop.SeedName,
                Description = crop.SeedDescription,
                Category = ObjectCategory.Seeds,
                Price = crop.SeedSellPrice == -1 ? crop.SeedPurchasePrice : crop.SeedSellPrice,
                CanPurchase = crop.SeedPurchasePrice > 0,
                PurchaseFrom = crop.SeedPurchaseFrom,
                PurchasePrice = crop.SeedPurchasePrice,
                PurchaseRequirements = crop.SeedPurchaseRequirements,
                AdditionalPurchaseData = crop.SeedAdditionalPurchaseData,
                NameLocalization = crop.SeedNameLocalization,
                DescriptionLocalization = crop.SeedDescriptionLocalization,
                TranslationKey = crop.SeedTranslationKey
            };

            this.PopulateTranslations(crop.Seed, translations);

            // validate
            if (!this.AssertHasName(crop, "crop", source, translations))
                return;
            if (!this.AssertHasName(crop.Seed, "crop seed", source, translations, discriminator: $"crop: {crop.Name}", fieldName: nameof(crop.SeedName)))
                return;

            // check for duplicates
            if (this.DupCrops.TryGetValue(crop.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate crop: {crop.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupCrops[crop.Name] = source;

            if (this.DupObjects.TryGetValue(crop.Seed.Name, out var oldmanifest))
            {
                Log.Error($"{crop.Seed.Name} previously added by {oldmanifest.UniqueID}, this may cause errors. Crop {crop.Name} by {source.Name} will not be added");
                return;
            }
            else
                this.DupObjects[crop.Seed.Name] = source;

            // save crop data
            this.Crops.Add(crop);

            // add purchase requirement for crop seasons
            {
                string seasonReq = "";
                foreach (string season in new[] { "spring", "summer", "fall", "winter" }.Except(crop.Seasons))
                    seasonReq += $"/z {season}";
                if (seasonReq != "")
                {
                    seasonReq = seasonReq.TrimStart('/');
                    if (crop.SeedPurchaseRequirements.Any())
                    {
                        for (int index = 0; index < crop.SeedPurchaseRequirements.Count; index++)
                        {
                            if (this.SeasonLimiter.IsMatch(crop.SeedPurchaseRequirements[index]))
                            {
                                crop.SeedPurchaseRequirements[index] = seasonReq;
                                Log.Warn($"        Faulty season requirements for {crop.SeedName}!\n        Fixed season requirements: {crop.SeedPurchaseRequirements[index]}");
                            }
                        }
                        if (!crop.SeedPurchaseRequirements.Contains(seasonReq))
                        {
                            Log.Trace($"        Adding season requirements for {crop.SeedName}:\n        New season requirements: {seasonReq}");
                            crop.Seed.PurchaseRequirements.Add(seasonReq);
                        }
                    }
                    else
                    {
                        Log.Trace($"        Adding season requirements for {crop.SeedName}:\n        New season requirements: {seasonReq}");
                        crop.Seed.PurchaseRequirements.Add(seasonReq);
                    }
                }
            }

            // add seed to shops
            if (crop.Seed.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = crop.Seed.PurchaseFrom,
                    Price = crop.Seed.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, crop.Seed.PurchaseRequirements),
                    Object = () => new SObject(crop.Seed.Id, int.MaxValue, false, crop.Seed.Price),
                    ShowWithStocklist = true
                });
                foreach (var entry in crop.Seed.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(crop.Seed.Id, int.MaxValue, false, crop.Seed.Price)
                    });
                }
            }

            // save seed data
            this.Objects.Add(crop.Seed);

            if (!this.CropsByContentPack.TryGetValue(source, out List<string> addedCrops))
                addedCrops = this.CropsByContentPack[source] = new();
            addedCrops.Add(crop.Name);

            if (!this.ObjectsByContentPack.TryGetValue(source, out List<string> addedSeeds))
                addedSeeds = this.ObjectsByContentPack[source] = new();
            addedSeeds.Add(crop.Seed.Name);
        }

        /// <summary>Register a custom fruit tree with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the fruit tree.</param>
        /// <param name="tree">The fruit tree data.</param>
        /// <param name="saplingTex">The fruit tree's sapling texture.</param>
        public void RegisterFruitTree(IManifest source, FruitTreeData tree, Texture2D saplingTex)
        {
            this.RegisterFruitTree(source, tree, saplingTex, null);
        }

        /// <summary>Register a custom fruit tree with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the fruit tree.</param>
        /// <param name="tree">The fruit tree data.</param>
        /// <param name="saplingTexture">The fruit tree's sapling texture.</param>
        /// <param name="translations">The translations from which to get text if <see cref="FruitTreeData.SaplingTranslationKey"/> is used.</param>
        public void RegisterFruitTree(IManifest source, FruitTreeData tree, Texture2D saplingTexture, ITranslationHelper translations)
        {
            // load data
            tree.InvokeOnDeserialized();
            tree.Sapling = new ObjectData
            {
                Texture = saplingTexture,
                Name = tree.SaplingName,
                Description = tree.SaplingDescription,
                Category = ObjectCategory.Seeds,
                Price = tree.SaplingPurchasePrice,
                CanPurchase = true,
                PurchaseRequirements = tree.SaplingPurchaseRequirements,
                PurchaseFrom = tree.SaplingPurchaseFrom,
                PurchasePrice = tree.SaplingPurchasePrice,
                AdditionalPurchaseData = tree.SaplingAdditionalPurchaseData,
                NameLocalization = tree.SaplingNameLocalization,
                DescriptionLocalization = tree.SaplingDescriptionLocalization,
                TranslationKey = tree.SaplingTranslationKey
            };
            this.PopulateTranslations(tree.Sapling, translations);

            // check for duplicates
            if (this.DupFruitTrees.TryGetValue(tree.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate fruit tree: {tree.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupFruitTrees[tree.Name] = source;

            // validate
            if (!this.AssertHasName(tree, "fruit tree", source, translations))
                return;
            if (!this.AssertHasName(tree.Sapling, "fruit tree sapling", source, translations, discriminator: $"fruit tree: {tree.Name}", fieldName: nameof(tree.SaplingName)))
                return;

            // save data
            this.FruitTrees.Add(tree);
            this.Objects.Add(tree.Sapling);

            // add sapling to shops
            if (tree.Sapling.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = tree.Sapling.PurchaseFrom,
                    Price = tree.Sapling.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, tree.Sapling.PurchaseRequirements),
                    Object = () => new SObject(Vector2.Zero, tree.Sapling.Id, int.MaxValue)
                });
                foreach (var entry in tree.Sapling.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(Vector2.Zero, tree.Sapling.Id, int.MaxValue)
                    });
                }
            }

            if (!this.FruitTreesByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.FruitTreesByContentPack[source] = new List<string>();
            addedNames.Add(tree.Name);
        }

        /// <summary>Register a custom big craftable with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the big craftable.</param>
        /// <param name="craftable">The big craftable data.</param>
        public void RegisterBigCraftable(IManifest source, BigCraftableData craftable)
        {
            this.RegisterBigCraftable(source, craftable, null);
        }

        /// <summary>Register a custom big craftable with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the big craftable.</param>
        /// <param name="craftable">The big craftable data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="BigCraftableData.TranslationKey"/> is used.</param>
        public void RegisterBigCraftable(IManifest source, BigCraftableData craftable, ITranslationHelper translations)
        {
            // load data
            craftable.InvokeOnDeserialized();
            this.PopulateTranslations(craftable, translations);

            // validate
            if (!this.AssertHasName(craftable, "craftable", source, translations))
                return;

            // check for duplicates
            if (this.DupBigCraftables.TryGetValue(craftable.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate big craftable: {craftable.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupBigCraftables[craftable.Name] = source;

            // save data
            this.BigCraftables.Add(craftable);

            // add recipe shop data
            if (craftable.Recipe?.CanPurchase == true)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = craftable.Recipe.PurchaseFrom,
                    Price = craftable.Recipe.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, craftable.Recipe.PurchaseRequirements),
                    Object = () => new SObject(Vector2.Zero, craftable.Id, true)
                });
                foreach (var entry in craftable.Recipe.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(Vector2.Zero, craftable.Id, true)
                    });
                }
            }

            // add item shop data
            if (craftable.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = craftable.PurchaseFrom,
                    Price = craftable.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, craftable.PurchaseRequirements),
                    Object = () => new SObject(Vector2.Zero, craftable.Id)
                });
                foreach (var entry in craftable.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(Vector2.Zero, craftable.Id)
                    });
                }
            }

            if (!this.BigCraftablesByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.BigCraftablesByContentPack[source] = new();
            addedNames.Add(craftable.Name);
        }

        /// <summary>Register a custom hat with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the hat.</param>
        /// <param name="hat">The shirt data.</param>
        public void RegisterHat(IManifest source, HatData hat)
        {
            this.RegisterHat(source, hat, null);
        }

        /// <summary>Register a custom hat with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the hat.</param>
        /// <param name="hat">The shirt data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="BigCraftableData.TranslationKey"/> is used.</param>
        public void RegisterHat(IManifest source, HatData hat, ITranslationHelper translations)
        {
            // load data
            hat.InvokeOnDeserialized();
            this.PopulateTranslations(hat, translations);

            // validate
            if (!this.AssertHasName(hat, "hat", source, translations))
                return;

            // check for duplicates
            if (this.DupHats.TryGetValue(hat.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate hat: {hat.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupHats[hat.Name] = source;

            // save data
            this.Hats.Add(hat);

            // add to shops
            if (hat.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = "HatMouse",
                    Price = hat.PurchasePrice,
                    PurchaseRequirements = ParsedConditions.AlwaysTrue,
                    Object = () => new Hat(hat.Id)
                });
            }

            if (!this.HatsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.HatsByContentPack[source] = new();
            addedNames.Add(hat.Name);
        }

        /// <summary>Register a custom weapon with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the weapon.</param>
        /// <param name="weapon">The weapon data.</param>
        public void RegisterWeapon(IManifest source, WeaponData weapon)
        {
            this.RegisterWeapon(source, weapon, null);
        }

        /// <summary>Register a custom weapon with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the weapon.</param>
        /// <param name="weapon">The weapon data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="WeaponData.TranslationKey"/> is used.</param>
        public void RegisterWeapon(IManifest source, WeaponData weapon, ITranslationHelper translations)
        {
            // load data
            weapon.InvokeOnDeserialized();
            this.PopulateTranslations(weapon, translations);

            // validate
            if (!this.AssertHasName(weapon, "weapon", source, translations))
                return;

            // check for duplicates
            if (this.DupWeapons.TryGetValue(weapon.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate weapon: {weapon.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupWeapons[weapon.Name] = source;

            // save data
            this.Weapons.Add(weapon);

            // add to shops
            if (weapon.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = weapon.PurchaseFrom,
                    Price = weapon.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, weapon.PurchaseRequirements),
                    Object = () => new MeleeWeapon(weapon.Id)
                });
                foreach (var entry in weapon.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new MeleeWeapon(weapon.Id)
                    });
                }
            }

            if (!this.WeaponsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.WeaponsByContentPack[source] = new();
            addedNames.Add(weapon.Name);
        }

        /// <summary>Register a custom shirt with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the shirt.</param>
        /// <param name="shirt">The shirt data.</param>
        public void RegisterShirt(IManifest source, ShirtData shirt)
        {
            this.RegisterShirt(source, shirt, null);
        }

        /// <summary>Register a custom shirt with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the shirt.</param>
        /// <param name="shirt">The shirt data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="ShirtData.TranslationKey"/> is used.</param>
        public void RegisterShirt(IManifest source, ShirtData shirt, ITranslationHelper translations)
        {
            // load data
            shirt.InvokeOnDeserialized();
            this.PopulateTranslations(shirt, translations);

            // validate
            if (!this.AssertHasName(shirt, "shirt", source, translations))
                return;

            // check for duplicates
            if (this.DupShirts.TryGetValue(shirt.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate shirt: {shirt.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupShirts[shirt.Name] = source;

            // save data
            this.Shirts.Add(shirt);

            if (!this.ClothingByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.ClothingByContentPack[source] = new();
            addedNames.Add(shirt.Name);
        }

        /// <summary>Register custom pants with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the pants.</param>
        /// <param name="pants">The pants data.</param>
        public void RegisterPants(IManifest source, PantsData pants)
        {
            this.RegisterPants(source, pants, null);
        }

        /// <summary>Register custom pants with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the pants.</param>
        /// <param name="pants">The pants data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="PantsData.TranslationKey"/> is used.</param>
        public void RegisterPants(IManifest source, PantsData pants, ITranslationHelper translations)
        {
            // load data
            pants.InvokeOnDeserialized();
            this.PopulateTranslations(pants, translations);

            // validate
            if (!this.AssertHasName(pants, "pants", source, translations))
                return;

            // check for duplicates
            if (this.DupPants.TryGetValue(pants.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate pants: {pants.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupPants[pants.Name] = source;

            // save data
            this.Pants.Add(pants);

            if (!this.ClothingByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.ClothingByContentPack[source] = new();
            addedNames.Add(pants.Name);
        }

        /// <summary>Register a custom tailoring recipe with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the pants.</param>
        /// <param name="recipe">The pants data.</param>
        public void RegisterTailoringRecipe(IManifest source, TailoringRecipeData recipe)
        {
            recipe.InvokeOnDeserialized();

            this.Tailoring.Add(recipe);
        }

        /// <summary>Register custom boots with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the boots.</param>
        /// <param name="boots">The boots data.</param>
        public void RegisterBoots(IManifest source, BootsData boots)
        {
            this.RegisterBoots(source, boots, null);
        }

        /// <summary>Register custom boots with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the boots.</param>
        /// <param name="boots">The boots data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="BootsData.TranslationKey"/> is used.</param>
        public void RegisterBoots(IManifest source, BootsData boots, ITranslationHelper translations)
        {
            // load data
            boots.InvokeOnDeserialized();
            this.PopulateTranslations(boots, translations);

            // validate
            if (!this.AssertHasName(boots, "boots", source, translations))
                return;

            // check for duplicates
            if (this.DupBoots.TryGetValue(boots.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate boots: {boots.Name} by {source.Name} will not be added, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupBoots[boots.Name] = source;

            // save data
            this.Boots.Add(boots);

            // add to shops
            if (boots.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = boots.PurchaseFrom,
                    Price = boots.PurchasePrice,
                    PurchaseRequirements = this.ParseAndValidateRequirements(source, boots.PurchaseRequirements),
                    Object = () => new Boots(boots.Id)
                });

                foreach (var entry in boots.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new Boots(boots.Id)
                    });
                }
            }

            if (!this.BootsByContentPack.TryGetValue(source, out List<string> addedNames))
                addedNames = this.BootsByContentPack[source] = new();
            addedNames.Add(boots.Name);
        }

        /// <summary>Register a custom forge recipe with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the forge recipe.</param>
        /// <param name="recipe">The forge recipe data.</param>
        public void RegisterForgeRecipe(IManifest source, ForgeRecipeData recipe)
        {
            recipe.InvokeOnDeserialized();

            this.Forge.Add(recipe);
        }

        /// <summary>Register a custom fence with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the fence.</param>
        /// <param name="fence">The fence data.</param>
        public void RegisterFence(IManifest source, FenceData fence)
        {
            this.RegisterFence(source, fence, null);
        }

        /// <summary>Register a custom fence with Json Assets.</summary>
        /// <param name="source">The manifest for the mod registering the fence.</param>
        /// <param name="fence">The fence data.</param>
        /// <param name="translations">The translations from which to get text if <see cref="FenceData.TranslationKey"/> is used.</param>
        public void RegisterFence(IManifest source, FenceData fence, ITranslationHelper translations)
        {
            // load data
            fence.InvokeOnDeserialized();
            this.PopulateTranslations(fence, translations);
            fence.CorrespondingObject = new ObjectData
            {
                Texture = fence.ObjectTexture,
                Name = fence.Name,
                Description = fence.Description,
                Category = ObjectCategory.Crafting,
                Price = fence.Price,
                Recipe = fence.Recipe == null ? null : new ObjectRecipe
                {
                    SkillUnlockName = fence.Recipe.SkillUnlockName,
                    SkillUnlockLevel = fence.Recipe.SkillUnlockLevel,
                    ResultCount = fence.Recipe.ResultCount,
                    Ingredients = fence.Recipe.Ingredients
                        .Select(ingredient => new ObjectIngredient { Object = ingredient.Object, Count = ingredient.Count })
                        .ToList(),
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
                DescriptionLocalization = fence.DescriptionLocalization,
                TranslationKey = fence.TranslationKey
            };

            // validate data
            if (!this.AssertHasName(fence, "fence", source, translations))
                return;
            if (!this.AssertHasName(fence.CorrespondingObject, "fence object", source, translations, discriminator: $"fence: {fence.Name}"))
                return;

            // save data
            this.Fences.Add(fence);
            this.RegisterObject(source, fence.CorrespondingObject, translations);
        }

        /// <summary>Get whether conditions in the Expanded Preconditions Utility (EPU) format match the current context.</summary>
        /// <param name="conditions">The EPU conditions to check.</param>
        /// <returns>This always returns false if EPU isn't installed.</returns>
        internal bool CheckEpuCondition(string[] conditions)
        {
            // not conditional
            if (conditions?.Any() != true)
                return true;

            // If EPU isn't installed, all EPU conditions automatically fail.
            // Json Assets will show a separate error/warning about this.
            if (this.ExpandedPreconditionsUtility == null)
                return false;

            // check conditions
            return this.ExpandedPreconditionsUtility.CheckConditions(conditions);
        }

        /// <summary>Parse individual requirements for the <see cref="ShopDataEntry.PurchaseRequirements"/> property, and log an error if a dependency is required but missing.</summary>
        /// <param name="source">The mod registering the content.</param>
        /// <param name="requirementFields">The purchase requirements.</param>
        private IParsedConditions ParseAndValidateRequirements(IManifest source, IList<string> requirementFields)
        {
            IParsedConditions parsed = new ParsedConditions(requirementFields, this.ExpandedPreconditionsUtility);

            if (parsed.NeedsExpandedPreconditionsUtility && this.ExpandedPreconditionsUtility == null)
                this.Monitor.LogOnce($"{source.Name} uses conditions from Expanded Preconditions Utility, but you don't have that mod installed. Some of its content might not work correctly.", LogLevel.Error);

            return parsed;
        }

        /// <summary>Load a content pack.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="translations">The translations to use for <c>TranslationKey</c> fields, or <c>null</c> to use the content pack's translations.</param>
        private void LoadData(IContentPack contentPack, ITranslationHelper translations = null)
        {
            Log.Info($"\t{contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author} - {contentPack.Manifest.Description}");

            translations ??= contentPack.Translation;

            // load objects
            DirectoryInfo objectsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Objects"));
            if (objectsDir.Exists)
            {
                foreach (DirectoryInfo dir in objectsDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Objects/{dir.Name}";

                    // load data
                    ObjectData obj = contentPack.ReadJsonFile<ObjectData>($"{relativePath}/object.json");
                    if (obj == null || (obj.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(obj.DisableWithMod)) || (obj.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(obj.EnableWithMod)))
                        continue;

                    // save object
                    obj.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/object.png");
                    if (obj.IsColored)
                        obj.TextureColor = contentPack.ModContent.Load<Texture2D>($"{relativePath}/color.png");

                    this.RegisterObject(contentPack.Manifest, obj, translations);
                }
            }

            // load crops
            DirectoryInfo cropsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Crops"));
            if (cropsDir.Exists)
            {
                foreach (DirectoryInfo dir in cropsDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Crops/{dir.Name}";

                    // load data
                    CropData crop = contentPack.ReadJsonFile<CropData>($"{relativePath}/crop.json");
                    if (crop == null || (crop.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(crop.DisableWithMod)) || (crop.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(crop.EnableWithMod)))
                        continue;

                    // save crop
                    crop.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/crop.png");
                    if (contentPack.HasFile($"{relativePath}/giant.png"))
                        crop.GiantTexture = new (() => contentPack.ModContent.Load<Texture2D>($"{relativePath}/giant.png"));

                    this.RegisterCrop(contentPack.Manifest, crop, contentPack.ModContent.Load<Texture2D>($"{relativePath}/seeds.png"), translations);
                }
            }

            // load fruit trees
            DirectoryInfo fruitTreesDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "FruitTrees"));
            if (fruitTreesDir.Exists)
            {
                foreach (DirectoryInfo dir in fruitTreesDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"FruitTrees/{dir.Name}";

                    // load data
                    FruitTreeData tree = contentPack.ReadJsonFile<FruitTreeData>($"{relativePath}/tree.json");
                    if (tree == null || (tree.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(tree.DisableWithMod)) || (tree.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(tree.EnableWithMod)))
                        continue;

                    // save fruit tree
                    tree.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/tree.png");
                    this.RegisterFruitTree(contentPack.Manifest, tree, contentPack.ModContent.Load<Texture2D>($"{relativePath}/sapling.png"), translations);
                }
            }

            // load big craftables
            DirectoryInfo bigCraftablesDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "BigCraftables"));
            if (bigCraftablesDir.Exists)
            {
                foreach (DirectoryInfo dir in bigCraftablesDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"BigCraftables/{dir.Name}";

                    // load data
                    BigCraftableData craftable = contentPack.ReadJsonFile<BigCraftableData>($"{relativePath}/big-craftable.json");
                    if (craftable == null || (craftable.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(craftable.DisableWithMod)) || (craftable.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(craftable.EnableWithMod)))
                        continue;

                    // save craftable
                    craftable.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/big-craftable.png");
                    if (craftable.ReserveNextIndex && craftable.ReserveExtraIndexCount == 0)
                        craftable.ReserveExtraIndexCount = 1;
                    if (craftable.ReserveExtraIndexCount > 0)
                    {
                        craftable.ExtraTextures = new Texture2D[craftable.ReserveExtraIndexCount];
                        for (int i = 0; i < craftable.ReserveExtraIndexCount; ++i)
                            craftable.ExtraTextures[i] = contentPack.ModContent.Load<Texture2D>($"{relativePath}/big-craftable-{i + 2}.png");
                    }
                    this.RegisterBigCraftable(contentPack.Manifest, craftable, translations);
                }
            }

            // load hats
            DirectoryInfo hatsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Hats"));
            if (hatsDir.Exists)
            {
                foreach (DirectoryInfo dir in hatsDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Hats/{dir.Name}";

                    // load data
                    HatData hat = contentPack.ReadJsonFile<HatData>($"{relativePath}/hat.json");
                    if (hat == null || (hat.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(hat.DisableWithMod)) || (hat.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(hat.EnableWithMod)))
                        continue;

                    // save object
                    hat.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/hat.png");
                    this.RegisterHat(contentPack.Manifest, hat, translations);
                }
            }

            // Load weapons
            DirectoryInfo weaponsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Weapons"));
            if (weaponsDir.Exists)
            {
                foreach (DirectoryInfo dir in weaponsDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Weapons/{dir.Name}";

                    // load data
                    WeaponData weapon = contentPack.ReadJsonFile<WeaponData>($"{relativePath}/weapon.json");
                    if (weapon == null || (weapon.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(weapon.DisableWithMod)) || (weapon.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(weapon.EnableWithMod)))
                        continue;

                    // save object
                    weapon.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/weapon.png");
                    this.RegisterWeapon(contentPack.Manifest, weapon, translations);
                }
            }

            // Load shirts
            DirectoryInfo shirtsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Shirts"));
            if (shirtsDir.Exists)
            {
                foreach (DirectoryInfo dir in shirtsDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Shirts/{dir.Name}";

                    // load data
                    ShirtData shirt = contentPack.ReadJsonFile<ShirtData>($"{relativePath}/shirt.json");
                    if (shirt == null || (shirt.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(shirt.DisableWithMod)) || (shirt.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(shirt.EnableWithMod)))
                        continue;

                    // save shirt
                    shirt.TextureMale = contentPack.ModContent.Load<Texture2D>($"{relativePath}/male.png");
                    if (shirt.Dyeable)
                        shirt.TextureMaleColor = contentPack.ModContent.Load<Texture2D>($"{relativePath}/male-color.png");
                    if (shirt.HasFemaleVariant)
                    {
                        shirt.TextureFemale = contentPack.ModContent.Load<Texture2D>($"{relativePath}/female.png");
                        if (shirt.Dyeable)
                            shirt.TextureFemaleColor = contentPack.ModContent.Load<Texture2D>($"{relativePath}/female-color.png");
                    }
                    this.RegisterShirt(contentPack.Manifest, shirt, translations);
                }
            }

            // Load pants
            DirectoryInfo pantsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Pants"));
            if (pantsDir.Exists)
            {
                foreach (DirectoryInfo dir in pantsDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Pants/{dir.Name}";

                    // load data
                    PantsData pants = contentPack.ReadJsonFile<PantsData>($"{relativePath}/pants.json");
                    if (pants == null || (pants.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(pants.DisableWithMod)) || (pants.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(pants.EnableWithMod)))
                        continue;

                    // save pants
                    pants.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/pants.png");
                    this.RegisterPants(contentPack.Manifest, pants, translations);
                }
            }

            // Load tailoring
            DirectoryInfo tailoringDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Tailoring"));
            if (tailoringDir.Exists)
            {
                foreach (DirectoryInfo dir in tailoringDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
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
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Boots/{dir.Name}";

                    // load data
                    BootsData boots = contentPack.ReadJsonFile<BootsData>($"{relativePath}/boots.json");
                    if (boots == null || (boots.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(boots.DisableWithMod)) || (boots.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(boots.EnableWithMod)))
                        continue;

                    boots.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/boots.png");
                    boots.TextureColor = contentPack.ModContent.Load<Texture2D>($"{relativePath}/color.png");
                    this.RegisterBoots(contentPack.Manifest, boots, translations);
                }
            }

            // Load boots
            DirectoryInfo fencesDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Fences"));
            if (fencesDir.Exists)
            {
                foreach (DirectoryInfo dir in fencesDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
                    string relativePath = $"Fences/{dir.Name}";

                    // load data
                    FenceData fence = contentPack.ReadJsonFile<FenceData>($"{relativePath}/fence.json");
                    if (fence == null || (fence.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(fence.DisableWithMod)) || (fence.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(fence.EnableWithMod)))
                        continue;

                    fence.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/fence.png");
                    fence.ObjectTexture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/object.png");
                    this.RegisterFence(contentPack.Manifest, fence, translations);
                }
            }

            // Load tailoring
            DirectoryInfo forgeDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Forge"));
            if (forgeDir.Exists)
            {
                foreach (DirectoryInfo dir in forgeDir.EnumerateDirectories())
                {
                    if (dir.Name.StartsWith('.'))
                        continue;
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
            var objects = new List<DataNeedsId>(this.Objects);
            objects.AddRange(this.Boots); // boots are also in objects.
            this.ClearIds(this.ObjectIds, objects);
            this.ClearIds(this.CropIds, this.Crops.ToList<DataNeedsId>());
            this.ClearIds(this.FruitTreeIds, this.FruitTrees.ToList<DataNeedsId>());
            this.ClearIds(this.BigCraftableIds, this.BigCraftables.ToList<DataNeedsId>());
            this.ClearIds(this.HatIds, this.Hats.ToList<DataNeedsId>());
            this.ClearIds(this.WeaponIds, this.Weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new List<DataNeedsId>(this.Shirts);
            clothing.AddRange(this.Pants);
            this.ClearIds(this.ClothingIds, clothing.ToList());

            ContentInjector1.InvalidateUsed();
            ContentInjector1.Clear();
            ContentInjector2.ResetGiftTastes();

            this.LocationsFixedAlready.Clear();
        }

        internal void OnBlankSave()
        {
            if (string.IsNullOrEmpty(Constants.CurrentSavePath))
            {
                Log.Trace("Loading stuff early (for blank save)");
                this.InitStuff(loadIdFiles: false);
            }
        }

        private void OnCreated(object sender, SaveCreatedEventArgs e)
        {
            Log.Trace("Loading stuff early (creation)");
            //initStuff(loadIdFiles: false);
        }

        private bool DoesntNeedDeshuffling(IDictionary<string, int> oldIds, IDictionary<string, int> newIds)
            => oldIds.Count == 0
                || (oldIds.Count == newIds.Count 
                    && oldIds.All((kvp) => newIds.TryGetValue(kvp.Key, out int val) && val == kvp.Value));

        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveParsed)
            {
                //Log.debug("Loading stuff early (loading)");
                this.InitStuff(loadIdFiles: true);
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveLoadedLocations)
            {
                if (this.DoesntNeedDeshuffling(this.OldObjectIds, this.ObjectIds)
                    && this.DoesntNeedDeshuffling(this.OldCropIds, this.OldCropIds)
                    && this.DoesntNeedDeshuffling(this.OldFruitTreeIds, this.FruitTreeIds)
                    && this.DoesntNeedDeshuffling(this.OldHatIds, this.HatIds)
                    && this.DoesntNeedDeshuffling(this.OldBigCraftableIds, this.BigCraftableIds)
                    && this.DoesntNeedDeshuffling(this.OldWeaponIds, this.WeaponIds)
                    && this.DoesntNeedDeshuffling(this.OldClothingIds, this.ClothingIds))
                {
                    Log.Trace("Nothing has changed, deshuffling unnecessary.");
                }
                else
                {
                    Log.Trace("Fixing IDs");
                    this.FixIdsEverywhere();
                }

                sfapi = this.Helper.ModRegistry.GetApi<ISolidFoundationsAPI>("PeacefulEnd.SolidFoundations");
                if (sfapi is not null)
                    sfapi.AfterBuildingRestoration += this.FixSFBuildings;
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.Loaded)
            {
                Log.Trace("Adding default/leveled recipes");
                foreach (var obj in this.Objects)
                {
                    if (obj.Recipe != null)
                    {
                        bool unlockedByLevel = false;
                        if (obj.Recipe.SkillUnlockName?.Length > 0 && obj.Recipe.SkillUnlockLevel > 0)
                        {
                            int level = obj.Recipe.SkillUnlockName switch
                            {
                                "Farming" => Game1.player.farmingLevel.Value,
                                "Fishing" => Game1.player.fishingLevel.Value,
                                "Foraging" => Game1.player.foragingLevel.Value,
                                "Mining" => Game1.player.miningLevel.Value,
                                "Combat" => Game1.player.combatLevel.Value,
                                "Luck" => Game1.player.luckLevel.Value,
                                _ => Game1.player.GetCustomSkillLevel(obj.Recipe.SkillUnlockName)
                            };

                            if (level >= obj.Recipe.SkillUnlockLevel)
                            {
                                unlockedByLevel = true;
                            }
                        }
                        if ((obj.Recipe.IsDefault || unlockedByLevel) && !Game1.player.knowsRecipe(obj.Name))
                        {
                            if (obj.Category == ObjectCategory.Cooking)
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
                            int level = big.Recipe.SkillUnlockName switch
                            {
                                "Farming" => Game1.player.farmingLevel.Value,
                                "Fishing" => Game1.player.fishingLevel.Value,
                                "Foraging" => Game1.player.foragingLevel.Value,
                                "Mining" => Game1.player.miningLevel.Value,
                                "Combat" => Game1.player.combatLevel.Value,
                                "Luck" => Game1.player.luckLevel.Value,
                                _ => Game1.player.GetCustomSkillLevel(big.Recipe.SkillUnlockName)
                            };

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
                Log.Trace("Loading stuff early (MP client)");
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

            // handle shop menu
            if (e.NewMenu is ShopMenu { source: not StorageFurniture } menu && !object.ReferenceEquals(e.NewMenu, this.LastShopMenu))
            {
                this.LastShopMenu = menu;

                ISet<string> shopIds = this.GetShopIds(menu);
                if (!shopIds.Any())
                {
                    Log.Trace("Ignored shop with no ID.");
                    return;
                }
                Log.Trace($"Adding objects for shop IDs '{string.Join("', '", shopIds)}'.");

                bool isPierre = shopIds.Contains("Pierre") || shopIds.Contains("SeedShop");
                bool isQiGemShop = shopIds.Contains("QiGemShop");

                bool doAllSeeds = Game1.player.hasOrWillReceiveMail("PierreStocklist");
                var forSale = menu.forSale;
                var itemPriceAndStock = menu.itemPriceAndStock;

                foreach (var entry in this.shopData)
                {
                    if (!shopIds.Contains(entry.PurchaseFrom))
                        continue;

                    bool normalCond = entry.PurchaseRequirements.CurrentlyMatch();
                    if (entry.Price == 0 || !normalCond && !(doAllSeeds && entry.ShowWithStocklist && isPierre))
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
                    int[] values = isQiGemShop
                        ? new[] { 0, isRecipe ? 1 : int.MaxValue, 858, price }
                        : new[] { price, isRecipe ? 1 : int.MaxValue };
                    itemPriceAndStock.Add(item, values);
                }

                this.Api.InvokeAddedItemsToShop();
            }
        }

        /// <summary>Get the valid shop IDs recognized for a given shop menu.</summary>
        /// <param name="menu">The shop menu to check.</param>
        private ISet<string> GetShopIds(ShopMenu menu)
        {
            IEnumerable<string> GetAll()
            {
                // owner ID
                if (!string.IsNullOrWhiteSpace(ShopMenuPatcher.LastShopOwner))
                    yield return ShopMenuPatcher.LastShopOwner;

                // portrait name
                string portraitName = !string.IsNullOrWhiteSpace(menu.portraitPerson?.Name) ? menu.portraitPerson.Name : null;
                if (portraitName != null)
                    yield return portraitName;

                // shop context
                string context = !string.IsNullOrWhiteSpace(menu.storeContext) ? menu.storeContext : null;
                if (context != null)
                    yield return context;

                // special cases
                if (ShopMenuPatcher.LastShopOwner == null && portraitName == null && context == "Hospital")
                    yield return "Harvey";
                if (ShopMenuPatcher.LastShopOwner == "KrobusGone")
                    yield return "Krobus";
            }

            return new HashSet<string>(GetAll(), StringComparer.OrdinalIgnoreCase);
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

                if (this.Monitor.IsVerbose)
                {
                    Log.Trace("OLD IDS START");
                    foreach (var id in this.OldObjectIds)
                        Log.Trace("\tObject " + id.Key + " = " + id.Value);
                    foreach (var id in this.OldCropIds)
                        Log.Trace("\tCrop " + id.Key + " = " + id.Value);
                    foreach (var id in this.OldFruitTreeIds)
                        Log.Trace("\tFruit Tree " + id.Key + " = " + id.Value);
                    foreach (var id in this.OldBigCraftableIds)
                        Log.Trace("\tBigCraftable " + id.Key + " = " + id.Value);
                    foreach (var id in this.OldHatIds)
                        Log.Trace("\tHat " + id.Key + " = " + id.Value);
                    foreach (var id in this.OldWeaponIds)
                        Log.Trace("\tWeapon " + id.Key + " = " + id.Value);
                    foreach (var id in this.OldClothingIds)
                        Log.Trace("\tClothing " + id.Key + " = " + id.Value);
                    Log.Trace("OLD IDS END");
                }
            }

            // assign IDs
            var objList = new List<DataNeedsId>(this.Objects);
            objList.AddRange(this.Boots); // boots are objects too.
            this.ObjectIds = this.AssignIds("objects", Mod.StartingObjectId, objList);
            this.CropIds = this.AssignIds("crops", Mod.StartingCropId, this.Crops.ToList<DataNeedsId>());
            this.FruitTreeIds = this.AssignIds("fruittrees", Mod.StartingFruitTreeId, this.FruitTrees.ToList<DataNeedsId>());
            this.BigCraftableIds = this.AssignIds("big-craftables", Mod.StartingBigCraftableId, this.BigCraftables.ToList<DataNeedsId>());
            this.HatIds = this.AssignIds("hats", Mod.StartingHatId, this.Hats.ToList<DataNeedsId>());
            this.WeaponIds = this.AssignIds("weapons", Mod.StartingWeaponId, this.Weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new(this.Shirts);
            clothing.AddRange(this.Pants);
            this.ClothingIds = this.AssignIds("clothing", Mod.StartingClothingId, clothing);

            this.AssignTextureIndices("shirts", Mod.StartingShirtTextureIndex, this.Shirts.ToList<DataSeparateTextureIndex>());
            this.AssignTextureIndices("pants", Mod.StartingPantsTextureIndex, this.Pants.ToList<DataSeparateTextureIndex>());
            this.AssignTextureIndices("boots", Mod.StartingBootsId, this.Boots.ToList<DataSeparateTextureIndex>());

            Log.Trace("Resetting max shirt/pants value");
            this.Helper.Reflection.GetField<int>(typeof(Clothing), "_maxShirtValue").SetValue(-1);
            this.Helper.Reflection.GetField<int>(typeof(Clothing), "_maxPantsValue").SetValue(-1);

            // Call before invoking Ids Assigned since clients may want to edit after.
            ContentInjector1.Initialize(this.Helper.GameContent);
            
            Log.Trace("Resolving Crop and Tree product Ids");
            CropData.giantCropMap.Clear();
            foreach (var crop in this.Crops)
            {
                crop.ProductId = ItemResolver.GetObjectID(crop.Product);
                if (crop.GiantTexture is not null)
                    CropData.giantCropMap[crop.ProductId] = crop.GiantTexture;
            }
            foreach (var fruitTree in this.FruitTrees)
            {
                fruitTree.ProductId = ItemResolver.GetObjectID(fruitTree.Product);
                FruitTreeData.SaplingIds.Add(fruitTree.GetSaplingId());
            }

            if (this.MyRings.Count > 0)
            {
                Log.Trace("Indexing rings");
                ObjectData.TrackedRings.Clear();
                foreach (var ring in this.MyRings)
                    ObjectData.TrackedRings.Add(ring.GetObjectId());

                this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
                this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            }

            // the game rewrites the display names of anything with honey in the name.
            BigCraftableData.HasHoneyInName.Clear();
            ObjectData.HasHoneyInName.Clear();

            foreach (var obj in this.Objects)
                if (obj.Name.Contains("Honey"))
                    ObjectData.HasHoneyInName.Add(obj.GetObjectId());

            foreach (var big in this.BigCraftables)
                if (big.Name.Contains("Honey"))
                    BigCraftableData.HasHoneyInName.Add(big.GetCraftableId());

            this.Api.InvokeIdsAssigned();

            ContentInjector1.InvalidateUsed();
            ContentInjector2.ResetGiftTastes();
            this.Helper.GameContent.InvalidateCache("Data/NPCGiftTastes");
            if (this.Helper.GameContent.CurrentLocaleConstant != LocalizedContentManager.LanguageCode.en)
                this.Helper.GameContent.InvalidateCache($"Data/NPCGiftTastes.{this.Helper.GameContent.CurrentLocale}");

            // This happens here instead of with ID fixing because TMXL apparently
            // uses the ID fixing API before ID fixing happens everywhere.
            // Doing this here prevents some NREs (that don't show up unless you're
            // debugging for some reason????)
            this.VanillaObjectIds = this.GetVanillaIds(Game1.objectInformation, this.ObjectIds);
            this.VanillaCropIds = this.GetVanillaIds(Game1.content.Load<Dictionary<int, string>>("Data\\Crops"), this.CropIds);
            this.VanillaFruitTreeIds = this.GetVanillaIds(Game1.content.Load<Dictionary<int, string>>("Data\\fruitTrees"), this.FruitTreeIds);
            this.VanillaBigCraftableIds = this.GetVanillaIds(Game1.bigCraftablesInformation, this.BigCraftableIds);
            this.VanillaHatIds = this.GetVanillaIds(Game1.content.Load<Dictionary<int, string>>("Data\\hats"), this.HatIds);
            this.VanillaWeaponIds = this.GetVanillaIds(Game1.content.Load<Dictionary<int, string>>("Data\\weapons"), this.WeaponIds);
            this.VanillaClothingIds = this.GetVanillaIds(Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"), this.ClothingIds);
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

            // NOTE: Have to save the file even if it's empty - maybe the user removed a JA pack?
            Task objects = Task.Run(() => File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-objects.json"), JsonConvert.SerializeObject(this.ObjectIds)));
            Task crops = Task.Run(() => File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-crops.json"), JsonConvert.SerializeObject(this.CropIds)));
            Task fruitTrees = Task.Run(() => File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-fruittrees.json"), JsonConvert.SerializeObject(this.FruitTreeIds)));
            Task bigs = Task.Run(() => File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-big-craftables.json"), JsonConvert.SerializeObject(this.BigCraftableIds)));
            Task hats = Task.Run(() => File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-hats.json"), JsonConvert.SerializeObject(this.HatIds)));
            Task weapons = Task.Run(() => File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-weapons.json"), JsonConvert.SerializeObject(this.WeaponIds)));
            Task clothing = Task.Run(() => File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-clothing.json"), JsonConvert.SerializeObject(this.ClothingIds)));

            this.Helper.Events.GameLoop.Saving -= this.OnSaving;
        }

        internal IList<ObjectData> MyRings = new List<ObjectData>();

        /// <summary>Raised after items are added or removed to a player's inventory. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            for (int i = 0; i < Game1.player.Items.Count; ++i)
            {
                var item = Game1.player.Items[i];
                if (item is SObject obj && ObjectData.TrackedRings.Contains(obj.ParentSheetIndex))
                { // NOTE: Rings are not SObjects, so duplicate conversions do not occur.
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
        //internal IDictionary<string, int> OldBootsIds;

        /// <summary>The vanilla object IDs.</summary>
        internal ISet<int> VanillaObjectIds;

        /// <summary>The vanilla crop IDs.</summary>
        internal ISet<int> VanillaCropIds;

        /// <summary>The vanilla fruit tree IDs.</summary>
        internal ISet<int> VanillaFruitTreeIds;

        /// <summary>The vanilla big craftable IDs.</summary>
        internal ISet<int> VanillaBigCraftableIds;

        /// <summary>The vanilla hat IDs.</summary>
        internal ISet<int> VanillaHatIds;

        /// <summary>The vanilla weapon IDs.</summary>
        internal ISet<int> VanillaWeaponIds;

        /// <summary>The vanilla clothing IDs.</summary>
        internal ISet<int> VanillaClothingIds;

        /// <summary>The vanilla boot IDs.</summary>
        internal ISet<int> VanillaBootIds;

        /// <summary>Populate an item's localization fields based on the <see cref="ITranslatableItem.TranslationKey"/> property, if defined.</summary>
        /// <param name="item">The item for which to populate translations.</param>
        /// <param name="translations">The translation helper from which to fetch translations.</param>
        private void PopulateTranslations(ITranslatableItem item, ITranslationHelper translations)
        {
            if (translations is null || string.IsNullOrWhiteSpace(item?.TranslationKey))
                return;

            foreach (var (locale, text) in translations.GetInAllLocales($"{item.TranslationKey}.name"))
            {
                item.NameLocalization[locale] = text;
            }

            foreach (var (locale, text) in translations.GetInAllLocales($"{item.TranslationKey}.description"))
            {
                item.DescriptionLocalization[locale] = text;
                if (locale == "default" && string.IsNullOrWhiteSpace(item.Description))
                    item.Description = text;
            }
        }

        /// <summary>Assert that an item has a name set, and log a descriptive error if it doesn't.</summary>
        /// <param name="item">The item whose name to validate.</param>
        /// <param name="typeLabel">The type label shown in the error message.</param>
        /// <param name="source">The mod which registered the item.</param>
        /// <param name="translations">The translations which have been applied to the name field, if any.</param>
        /// <param name="discriminator">A human-readable parenthetical phrase which provide more details in the error message, if any.</param>
        /// <param name="fieldName">The field name to show in error messages.</param>
        private bool AssertHasName(DataNeedsId item, string typeLabel, IManifest source, ITranslationHelper translations, string discriminator = null, string fieldName = nameof(DataNeedsId.Name))
        {
            if (!string.IsNullOrWhiteSpace(item.Name))
                return true;

            // add translation key to error
            if (item is ITranslatableItem translatable && !string.IsNullOrWhiteSpace(translatable.TranslationKey))
            {
                discriminator = string.Join(
                    ", ",
                    new[] { discriminator, $"translation key: {translatable.TranslationKey}" }.Where(p => !string.IsNullOrWhiteSpace(p))
                );
            }

            // log error
            this.Monitor.Log($"Ignored invalid content: {source.Name} added {typeLabel} with no {fieldName} field{(discriminator is not null ? $" ({discriminator})" : "")}.", LogLevel.Error);
            return false;
        }

        private Dictionary<string, int> AssignIds(string type, int starting, List<DataNeedsId> data)
        {
            data.Sort((dni1, dni2) => string.Compare(dni1.Name, dni2.Name, StringComparison.InvariantCulture));

            Log.Trace($"Assiging {type} ids starting at {starting}: {data.Count} items");

            Dictionary<string, int> ids = new();

            // some places the game doesn't distinguish between normal SObjects and big craftables and just checks by ID. We'll skip these numbers because they may cause problems
            // ie, the preserves jar at least used to accept 812 as roe.
            int[] bigSkip = type == "big-craftables" ? new[] { 309, 310, 311, 326, 340, 434, 447, 459, 599, 621, 628, 629, 630, 631, 632, 633, 645, 812, 872, 928 } : Array.Empty<int>();

            int currId = starting;
            foreach (var d in data)
            {
                // handle name conflict
                if (ids.TryGetValue(d.Name, out int prevId))
                {
                    Log.Warn($"Found ID conflict: there are two custom '{type}' items with the name '{d.Name}'. This may have unintended consequences.");
                    d.Id = prevId;
                }

                // else assign new ID
                else
                {
                    Log.Verbose($"New ID: {d.Name} = {currId}");
                    int id = currId++;
                    if (bigSkip.Length != 0)
                    {
                        while (bigSkip.Contains(id))
                        {
                            id = currId++;
                        }
                    }

                    ids.Add(d.Name, id);
                    if (type == "objects" && d is ObjectData { IsColored: true })
                        currId++;
                    else if (type == "big-craftables" && ((BigCraftableData)d).ReserveExtraIndexCount > 0)
                        currId += ((BigCraftableData)d).ReserveExtraIndexCount;
                    d.Id = ids[d.Name];
                }
            }

            return ids;
        }

        private void AssignTextureIndices(string type, int starting, List<DataSeparateTextureIndex> data)
        {
            data.Sort((dni1, dni2) => string.Compare(dni1.Name, dni2.Name, StringComparison.InvariantCulture));

            Dictionary<string, int> idxs = new Dictionary<string, int>();

            int currIdx = starting;
            foreach (var d in data)
            {
                if (d.TextureIndex == -1)
                {
                    Log.Verbose($"New texture index: {d.Name} = {currIdx}");
                    idxs.Add(d.Name, currIdx++);
                    if (type == "shirts" && ((ClothingData)d).HasFemaleVariant)
                        ++currIdx;
                    d.TextureIndex = idxs[d.Name];
                }
            }
        }

        private void ClearIds(IDictionary<string, int> ids, List<DataNeedsId> objs)
        {
            ids?.Clear();
            foreach (DataNeedsId obj in objs)
            {
                obj.Id = -1;
            }
        }

        /// <summary>Get the vanilla IDs from the game data.</summary>
        /// <param name="full">The full list of items, including both vanilla and custom IDs.</param>
        /// <param name="customIds">The custom IDs.</param>
        private ISet<int> GetVanillaIds(IDictionary<int, string> full, IDictionary<string, int> customIds)
        {
            return new HashSet<int>(
                full.Keys.Except(customIds.Values)
            );
        }

        private static readonly Regex TailoringStandardDescription = new("(?<type>[a-zA-Z]) (?<id>[0-9]+)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

        private static readonly Regex ItemContextTag = new("item_(?<type>[a-zA-Z]+)_(?<id>[0-9]+)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
        private static readonly Regex PreservesContextTag = new("preserve_sheet_index_(?<id>[0-9]+)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

        private static string AdjustPreservesContextTag(Match m)
        {
            if (m.Success && m.Groups["id"] is Group group && group.Success)
            {
                int oldId = int.Parse(group.Value);
                if (Mod.instance.VanillaObjectIds.Contains(oldId))
                    return m.Value;
                KeyValuePair<string, int> item = Mod.instance.OldObjectIds.FirstOrDefault(x => x.Value == oldId);
                if (item.Key is not null && Mod.instance.ObjectIds.TryGetValue(item.Key, out int newID))
                    return m.Value.Replace(group.Value, newID.ToString());
            }
            return m.Value;
        }

        private static readonly MatchEvaluator PreservesEvaluator = new(AdjustPreservesContextTag);

        private static string AdjustContextTagOrStandardDescription(Match m)
        {
            if (m.Success && m.Groups["id"] is Group id && id.Success
                && m.Groups["type"] is Group type && type.Success)
            {
                int oldId = int.Parse(id.Value);
                switch (type.Value)
                {
                    case "B": //boots are in objects
                    case "b":
                    case "O":
                    case "o":
                    case "R": //rings are in objects too.
                    case "r": 
                    {
                        if (Mod.instance.VanillaObjectIds.Contains(oldId))
                            return m.Value;
                        KeyValuePair<string, int> item = Mod.instance.OldObjectIds.FirstOrDefault(x => x.Value == oldId);
                        if (item.Key is not null && Mod.instance.ObjectIds.TryGetValue(item.Key, out int newID))
                            return m.Value.Replace(id.Value, newID.ToString());
                        break;
                    }
                    case "BO":
                    case "bo":
                    {
                        if (Mod.instance.VanillaBigCraftableIds.Contains(oldId))
                            return m.Value;
                        KeyValuePair<string, int> item = Mod.instance.OldBigCraftableIds.FirstOrDefault(x => x.Value == oldId);
                        if (item.Key is not null && Mod.instance.BigCraftableIds.TryGetValue(item.Key, out int newID))
                            return m.Value.Replace(id.Value, newID.ToString());
                        break;
                    }
                    case "C":
                    case "c":
                    {
                        if (Mod.instance.VanillaClothingIds.Contains(oldId))
                            return m.Value;
                        KeyValuePair<string, int> item = Mod.instance.OldClothingIds.FirstOrDefault(x => x.Value == oldId);
                        if (item.Key is not null && Mod.instance.ClothingIds.TryGetValue(item.Key, out int newID))
                            return m.Value.Replace(id.Value, newID.ToString());
                        break;
                    }
                    case "h":
                    case "H":
                    {
                        if (Mod.instance.VanillaHatIds.Contains(oldId))
                            return m.Value;
                        KeyValuePair<string, int> item = Mod.instance.OldHatIds.FirstOrDefault(x => x.Value == oldId);
                        if (item.Key is not null && Mod.instance.HatIds.TryGetValue(item.Key, out int newID))
                            return m.Value.Replace(id.Value, newID.ToString());
                        break;
                    }
                    case "w":
                    case "W":
                    {
                        if (Mod.instance.VanillaWeaponIds.Contains(oldId))
                            return m.Value;
                        KeyValuePair<string, int> item = Mod.instance.OldWeaponIds.FirstOrDefault(x => x.Value == oldId);
                        if (item.Key is not null && Mod.instance.WeaponIds.TryGetValue(item.Key, out int newID))
                            return m.Value.Replace(id.Value, newID.ToString());
                        break;
                    }
                    // JA doesn't do furniture, yes?
                }
            }
            return m.Value;
        }

        private static readonly MatchEvaluator ItemEvaluator = new(AdjustContextTagOrStandardDescription);

        // this ID marks SF buildings.
        private const string SFID = "SolidFoundations.GenericBuilding.Id";

        private bool ReverseFixing;
        private ISolidFoundationsAPI sfapi;
        private readonly HashSet<string> LocationsFixedAlready = new();
        private void FixIdsEverywhere(bool reverse = false)
        {
            this.ReverseFixing = reverse;
            if (this.ReverseFixing)
            {
                Log.Info("Reversing!");
            }

            this.FixCharacter(Game1.player);

            foreach (SpecialOrder order in Game1.player.team.specialOrders)
                this.FixSpecialOrder(order);

            this.FixItemList(Game1.player.team.junimoChest);
            this.RemoveNulls(Game1.player.team.junimoChest);

            foreach (var loc in Game1.locations)
                this.FixLocation(loc);

            // fix museum donations
            this.FixVector2Dictionary(Game1.netWorldState.Value.MuseumPieces);

            //fix returned items
            this.FixItemList(Game1.player.team.returnedDonations);
            this.RemoveNulls(Game1.player.team.returnedDonations);

            var bundleData = Game1.netWorldState.Value.GetUnlocalizedBundleData();
            var bundleDataCopy = new Dictionary<string, string>(Game1.netWorldState.Value.GetUnlocalizedBundleData());

            foreach (var entry in bundleDataCopy)
            {
                List<string> toks = new List<string>(entry.Value.Split('/'));

                // First, fix some stuff we broke in an earlier build by using .BundleData instead of the unlocalized version
                // Copied from Game1.applySaveFix (case FixBotchedBundleData)
                while (toks.Count > 4 && !int.TryParse(toks[^1], out _))
                {
                    string lastValue = toks[^1];
                    if (char.IsDigit(lastValue[^1]) && lastValue.Contains(':') && lastValue.Contains('\\'))
                    {
                        break;
                    }
                    toks.RemoveAt(toks.Count - 1);
                }

                // Then actually fix IDs
                string[] toks1 = toks[1].Split(' ');
                if (toks1[0] == "O")
                {
                    if (int.TryParse(toks1[1], out int oldId) && oldId != -1)
                    {
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, ref oldId, this.VanillaObjectIds))
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
                    if (int.TryParse(toks1[1], out int oldId) && oldId != -1)
                    {
                        if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, ref oldId, this.VanillaBigCraftableIds))
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
                toks[1] = string.Join(' ', toks1);
                string[] toks2 = toks[2].Split(' ');
                for (int i = 0; i < toks2.Length; i += 3)
                {
                    if (int.TryParse(toks2[i], out int oldId) && oldId != -1)
                    {
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, ref oldId, this.VanillaObjectIds))
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
                toks[2] = string.Join(' ', toks2);
                bundleData[entry.Key] = string.Join('/', toks);
            }
            // Fix bad bundle data
            Game1.netWorldState.Value.SetBundleData(bundleData);

            if (!this.ReverseFixing)
                this.Api.InvokeIdsFixed();
            this.ReverseFixing = false;
        }

        /// <summary>
        /// Fixes IDs related to quests.
        /// </summary>
        /// <param name="quest"></param>
        /// <returns>Inverse of whether or not the quest was fixed.</returns>
        private bool FixQuest(Quest quest)
        {
            return quest switch
            {
                CraftingQuest cq => cq.isBigCraftable.Value
                                        ? this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, cq.indexToCraft, this.VanillaBigCraftableIds)
                                        : this.FixId(this.OldObjectIds, this.ObjectIds, cq.indexToCraft, this.VanillaObjectIds),
                FishingQuest fq => this.FixId(this.OldObjectIds, this.ObjectIds, fq.whichFish, this.VanillaObjectIds)
                                        || this.FixItem(fq.fish.Value),
                ItemDeliveryQuest idq => this.FixId(this.OldObjectIds, this.ObjectIds, idq.item, this.VanillaObjectIds)
                                        || this.FixItem(idq.deliveryItem.Value),
                ItemHarvestQuest ihq => this.FixId(this.OldObjectIds, this.ObjectIds, ihq.itemIndex, this.VanillaObjectIds),
                LostItemQuest liq => this.FixId(this.OldObjectIds, this.ObjectIds, liq.itemIndex, this.VanillaObjectIds),
                _ => false,
            };
        }

        private void FixSpecialOrder(SpecialOrder order)
        {
            foreach (var objective in order.objectives)
                this.FixSpecialOrderObjective(objective);
            this.FixItemList(order.donatedItems);
            this.RemoveNulls(order.donatedItems);
        }

        private void FixSpecialOrderObjective(OrderObjective objective)
        {
            switch (objective)
            {
                case CollectObjective collect:
                {
                    this.FixContextList(collect.acceptableContextTagSets);
                    break;
                }
                case DonateObjective donate:
                {
                    this.FixContextList(donate.acceptableContextTagSets);
                    break;
                }
                case ShipObjective ship:
                {
                    this.FixContextList(ship.acceptableContextTagSets);
                    break;
                }
                case FishObjective fish:
                {
                    this.FixContextList(fish.acceptableContextTagSets);
                    break;
                }
                case GiftObjective gift:
                {
                    this.FixContextList(gift.acceptableContextTagSets);
                    break;
                }
            }
        }

        private void FixContextList(NetStringList tags)
        {
            for (int i = tags.Count - 1; i >=0; i--)
            {
                tags[i] = this.FixContextTagString(tags[i]);
            }
        }

        private string FixContextTagString(string contextstring)
        {
            contextstring = PreservesContextTag.Replace(contextstring, PreservesEvaluator);
            contextstring = ItemContextTag.Replace(contextstring, ItemEvaluator);
            return contextstring;
        }

        private void FixTailoringDict(NetStringDictionary<int, NetInt> dictionary)
        {
            Dictionary<string, int> newValues = new();
            foreach (var (k, v) in dictionary.Pairs)
                newValues[TailoringStandardDescription.Replace(k, ItemEvaluator)] = v;

            dictionary.Clear();
            foreach (var (k, v) in newValues)
                dictionary[k] = v;
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
                {
                    if (this.VanillaHatIds.Contains(hat.which.Value))
                        return false;
                    if (this.HatIds.TryGetValue(hat.Name, out int val))
                    {
                        if (val != hat.which.Value)
                        {
                            Log.Trace($"Fixing hat {hat.Name} with new id {val} by name");
                            hat.which.Value = val;
                        }
                        return false;
                    }
                    return this.FixId(this.OldHatIds, this.HatIds, hat.which, this.VanillaHatIds);
                }
                case MeleeWeapon weapon:
                {
                    if (this.VanillaWeaponIds.Contains(weapon.InitialParentTileIndex))
                        return false;
                    if (this.WeaponIds.TryGetValue(weapon.Name, out int val))
                    {
                        if (val != weapon.InitialParentTileIndex)
                        {
                            Log.Trace($"Fixing weapon {weapon.Name} with new id {val} by name");
                            weapon.InitialParentTileIndex = val;
                            weapon.CurrentParentTileIndex = val;
                            weapon.IndexOfMenuItemView = val;
                        }
                        return false;
                    }
                    return
                        this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.initialParentTileIndex, this.VanillaWeaponIds)
                        || this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.currentParentTileIndex, this.VanillaWeaponIds)
                        || this.FixId(this.OldWeaponIds, this.WeaponIds, weapon.indexOfMenuItemView, this.VanillaWeaponIds);
                }
                case Ring ring:
                    return this.FixRing(ring);

                case Clothing clothing:
                {
                    if (this.VanillaClothingIds.Contains(clothing.ParentSheetIndex))
                        return false;
                    if (this.ClothingIds.TryGetValue(clothing.Name, out int val))
                    {
                        if (val != clothing.ParentSheetIndex)
                        {
                            Log.Trace($"Fixing clothing {clothing.Name} with new id {val} by name");
                            clothing.ParentSheetIndex = val;
                            this.Helper.Reflection.GetField<bool>(clothing, "_LoadedData").SetValue(false);
                            clothing.LoadData();
                        }
                        return false;
                    }
                    else
                        return this.FixId(this.OldClothingIds, this.ClothingIds, clothing.parentSheetIndex, this.VanillaClothingIds);
                }
                case Boots boots:
                {
                    if (this.VanillaObjectIds.Contains(boots.indexInTileSheet.Value))
                        return false;
                    if (this.ObjectIds.TryGetValue(boots.Name, out int val))
                    {
                        if (val != boots.indexInTileSheet.Value)
                        {
                            Log.Trace($"Fixing boots {boots.Name} with new id {val} by name");
                            boots.indexInTileSheet.Value = val;
                        }
                    }
                    else if (this.FixId(this.OldObjectIds, this.ObjectIds, boots.indexInTileSheet, this.VanillaObjectIds))
                        return true;
                    var bootdata = this.Boots.FirstOrDefault((boot) => boot.GetObjectId() == boots.indexInTileSheet.Value);
                    boots.indexInColorSheet.Value = bootdata is null ? 0 : bootdata.GetTextureIndex();
                    return false;
                }
                case Tool tool:
                {
                    for (int a = 0; a < tool.attachments?.Count; ++a)
                    {
                        var attached = tool.attachments[a];
                        if (attached is not null && this.FixItem(attached))
                                tool.attachments[a] = null;
                    }
                    return false;
                }
                case SObject obj:
                    if (obj is Chest chest)
                    {
                        Log.Trace($"Fixing chest at {chest.TileLocation}");
                        if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, chest.parentSheetIndex, this.VanillaBigCraftableIds))
                            chest.ParentSheetIndex = 130;
                        else
                            chest.startingLidFrame.Value = chest.ParentSheetIndex + 1;
                        this.FixItemList(chest.items);
                        chest.clearNulls();
                    }
                    else if (obj is IndoorPot pot)
                    {
                        if (pot.hoeDirt.Value is not null && this.FixCrop(pot.hoeDirt.Value.crop))
                            pot.hoeDirt.Value.crop = null;
                    }
                    else if (obj is Fence fence)
                    {
                        if (this.FixId(this.OldObjectIds, this.ObjectIds, fence.whichType, this.VanillaObjectIds))
                            return true;
                        fence.ParentSheetIndex = -fence.whichType.Value;
                    }
                    else if (obj.GetType() == typeof(SObject) || obj.GetType() == typeof(Cask) || obj.GetType() == typeof(ColoredObject))
                    {
                        if (!obj.bigCraftable.Value)
                        {
                            // preserves index.
                            if (obj.Name != "Drum Block" && obj.Name != "Flute Block"
                                && this.FixId(this.OldObjectIds, this.ObjectIds, obj.preservedParentSheetIndex, this.VanillaObjectIds))
                                obj.preservedParentSheetIndex.Value = -1;

                            if (!this.VanillaObjectIds.Contains(obj.ParentSheetIndex)
                                && this.ObjectIds.TryGetValue(obj.Name, out int val))
                            {
                                if (val != obj.ParentSheetIndex)
                                {
                                    Log.Trace($"Fixing object {obj.Name} with new id {val} by name");
                                    obj.ParentSheetIndex = val;
                                }
                            }
                            else if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.parentSheetIndex, this.VanillaObjectIds))
                                return true;
                        }
                        else if (!this.VanillaBigCraftableIds.Contains(obj.ParentSheetIndex)
                            && this.BigCraftableIds.TryGetValue(obj.Name, out int id))
                        {
                            if (id != obj.ParentSheetIndex)
                            {
                                Log.Trace($"Fixing big craftable {obj.Name} with new id {id} by name");
                                obj.ParentSheetIndex = id;
                            }
                        }
                        else if (this.FixId(this.OldBigCraftableIds, this.BigCraftableIds, obj.parentSheetIndex, this.VanillaBigCraftableIds))
                            return true;
                    }

                    if (obj.heldObject.Value is SObject heldObject)
                    {
                        if (this.FixItem(heldObject))
                            obj.heldObject.Value = null;
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
                    Log.Trace($"Fixing horse {horse.Name}");
                    if (this.FixItem(horse.hat.Value))
                        horse.hat.Value = null;
                break;

                case Child child:
                    Log.Trace($"Fixing child {child.Name}");
                    if (this.FixItem(child.hat.Value))
                        child.hat.Value = null;
                    break;

                case Farmer player:
                    Log.Trace($"Fixing player {player.Name} - {player.UniqueMultiplayerID}");

                    // inventory and equipment
                    this.FixItemList(player.Items);

                    //fix inventory size that atra broke.
                    if (player.items.Count != player.MaxItems)
                    {
                        for (int i = 0; i < player.MaxItems - player.items.Count; i++)
                        {
                            player.items.Add(null);
                        }
                    }


                    if (this.FixRing(player.leftRing.Value))
                        player.leftRing.Value = null;
                    if (this.FixRing(player.rightRing.Value))
                        player.rightRing.Value = null;


                    if (this.FixItem(player.hat.Value))
                        player.hat.Value = null;
                    if (this.FixItem(player.shirtItem.Value))
                        player.shirtItem.Value = null;
                    if (this.FixItem(player.pantsItem.Value))
                        player.pantsItem.Value = null;
                    if (this.FixItem(player.boots.Value))
                        player.boots.Value = null;

                    // items lost to death;
                    this.FixItemList(player.itemsLostLastDeath);
                    this.RemoveNulls(player.itemsLostLastDeath);


                    if (player.recoveredItem is not null && this.FixItem(player.recoveredItem))
                    {
                        player.recoveredItem = null;
                        player.mailbox.Remove("MarlonRecovery");
                        player.mailForTomorrow.Remove("MarlonRecovery");
                    }

                    try
                    {
                        // completion metadata
                        this.FixIdDict(player.basicShipped, removeUnshippable: true);
                        this.FixIdDict(player.mineralsFound);
                        this.FixIdDict(player.recipesCooked);
                        this.FixIdDict2(player.archaeologyFound);
                        this.FixIdDict2(player.fishCaught);
                        foreach (var dict in player.giftedItems.Values)
                            this.FixIdDict3(dict);

                        this.FixTailoringDict(player.tailoredItems);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in fixing player metadata:\n\n{ex}");
                    }

                    foreach (var quest in player.questLog)
                    {
                        if (!this.FixQuest(quest))
                        {
                            try
                            {
                                quest.reloadObjective();
                                quest.reloadDescription();
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"Failed refreshing quest objectives:\n\n{ex}");
                            }
                        }
                    }
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

            // fix main ring
            if (!this.VanillaObjectIds.Contains(ring.indexInTileSheet.Value)
                && this.ObjectIds.TryGetValue(ring.Name, out int index))
            {
                if (ring.indexInTileSheet.Value != index)
                {
                    Log.Trace($"Fixing ring {ring.Name} with new id {index} by name");
                    ring.indexInTileSheet.Value = index;
                }
            }
            else if (this.FixId(this.OldObjectIds, this.ObjectIds, ring.indexInTileSheet, this.VanillaObjectIds))
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
            if (!this.LocationsFixedAlready.Add(loc.NameOrUniqueName))
                return;

            Log.Trace($"Fixing {loc.NameOrUniqueName}");

            switch (loc)
            {
                case FarmHouse house:
                    this.FixItemList(house.fridge.Value?.items);
                    house.fridge.Value?.clearNulls();
                    if (house is Cabin cabin)
                        this.FixCharacter(cabin.farmhand.Value);
                    break;

                case IslandFarmHouse house:
                    this.FixItemList(house.fridge.Value?.items);
                    house.fridge.Value?.clearNulls();
                    break;
                case ShopLocation shop:
                    this.FixItemList(shop.itemsFromPlayerToSell);
                    this.RemoveNulls(shop.itemsFromPlayerToSell);
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
            foreach (var (key, obj) in loc.objects.Pairs)
            {
                if (this.FixItem(obj))
                    toRemove.Add(key);
            }
            foreach (var rem in toRemove)
                loc.objects.Remove(rem);

            toRemove.Clear();
            foreach (var (key, obj) in loc.overlayObjects)
            {
                if (obj is Chest chest)
                {
                    this.FixItemList(chest.items);
                    chest.clearNulls();
                }
                else if (obj is Sign sign)
                {
                    if (!this.FixItem(sign.displayItem.Value))
                        sign.displayItem.Value = null;
                }
                else if (obj.GetType() == typeof(SObject) || obj.GetType() == typeof(ColoredObject))
                {
                    if (this.FixItem(obj))
                        toRemove.Add(key);
                    else if (obj.ParentSheetIndex == 126 && obj.Quality != 0 && obj.bigCraftable.Value) // Alien rarecrow stores what ID is it is wearing here
                    {
                        obj.Quality--;
                        if (this.FixId(this.OldHatIds, this.HatIds, obj.quality, this.VanillaHatIds))
                            obj.Quality = 0;
                        else obj.Quality++;
                    }
                }

                if (obj.heldObject.Value != null)
                {
                    if (this.FixItem(obj.heldObject.Value))
                        obj.heldObject.Value = null;

                    if (obj.heldObject.Value is Chest chest2)
                    {
                        this.FixItemList(chest2.items);
                        chest2.clearNulls();
                    }
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
                if (furniture.heldObject.Value != null && this.FixItem(furniture.heldObject.Value))
                    furniture.heldObject.Value = null;

                if (furniture is StorageFurniture storage)
                {
                    this.FixItemList(storage.heldItems);
                    storage.ClearNulls();

                    if (storage is FishTankFurniture fishTank)
                        fishTank.ResetFish();
                }
            }

            if (loc is Farm farm)
            {
                foreach (var animal in farm.Animals.Values)
                    this.FixFarmAnimal(animal);
            }

            foreach (var clump in loc.resourceClumps.Where(this.FixResourceClump).ToArray())
                loc.resourceClumps.Remove(clump);
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
                    mill.input.Value.clearNulls();
                    this.FixItemList(mill.output.Value.items);
                    mill.output.Value.clearNulls();
                    break;

                case FishPond pond:
                    if (pond.fishType.Value == -1)
                    {
                        this.Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                        break;
                    }

                    if (this.FixId(this.OldObjectIds, this.ObjectIds, pond.fishType, this.VanillaObjectIds))
                    {
                        pond.fishType.Value = -1;
                        pond.currentOccupants.Value = 0;
                        pond.maxOccupants.Value = 0;
                        this.Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                    }
                    if (this.FixItem(pond.sign.Value))
                        pond.sign.Value = null;
                    if (this.FixItem(pond.output.Value))
                        pond.output.Value = null;
                    if (this.FixItem(pond.neededItem.Value))
                        pond.neededItem.Value = null;
                    break;
                case JunimoHut hut:
                    this.FixItemList(hut.output.Value.items);
                    hut.output.Value.clearNulls();
                    break;
            }

            if (building.modData.ContainsKey(SFID))
            {
                var chests = this.Helper.Reflection.GetField<NetList<Chest, NetRef<Chest>>>(building, "buildingChests", required: false)?.GetValue();
                if (chests?.Count > 0)
                {
                    Log.Trace($"Fixing SF building's chests: {chests.Count} chests.");
                    try
                    {
                        foreach (var chest in chests)
                        {
                            this.FixItemList(chest.items);
                            this.RemoveNulls(chest.items);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error while deshuffling {building.modData[SFID]}:\n\n{ex}");
                    }
                }
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
            if (this.FixId(this.OldCropIds, this.CropIds, crop.rowInSpriteSheet, this.VanillaCropIds))
                return true;

            // fix index of harvest and netSeedIndex.
            CropData cropData = this.Crops.FirstOrDefault(x => crop.rowInSpriteSheet.Value == x.GetCropSpriteIndex());
            if (cropData is not null) // JA-managed crop
            {
                if (cropData.ProductId != crop.indexOfHarvest.Value)
                {
                    Log.Trace($"Fixing crop product: From {crop.indexOfHarvest.Value} to {cropData.Product}={cropData.ProductId}");
                    crop.indexOfHarvest.Value = cropData.ProductId;
                }
                if (this.FixId(this.OldObjectIds, this.ObjectIds, crop.netSeedIndex, this.VanillaObjectIds))
                    crop.netSeedIndex.Value = -1; // game will try to infer it again if it's used.
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
                    if (this.FixId(this.OldObjectIds, this.ObjectIds, id, this.VanillaObjectIds))
                        id.Value = -1;
                }
            }
        }

        /// <summary>Fix item IDs contained by a terrain feature, including the clump itself.</summary>
        /// <param name="clump">The resource clump to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        private bool FixResourceClump(ResourceClump clump)
        {
            return this.FixId(this.OldObjectIds, this.ObjectIds, clump.parentSheetIndex, this.VanillaObjectIds);
        }

        /// <summary>Fix item IDs contained by a terrain feature, including the terrain feature itself.</summary>
        /// <param name="feature">The terrain feature to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        private bool FixTerrainFeature(TerrainFeature feature)
        {
            switch (feature)
            {
                case HoeDirt dirt:
                {
                    if (this.FixCrop(dirt.crop))
                        dirt.crop = null;
                    return false;
                }

                case FruitTree tree:
                    {
                        if (this.FixId(this.OldFruitTreeIds, this.FruitTreeIds, tree.treeType, this.VanillaFruitTreeIds))
                            return true;

                    string key = this.FruitTreeIds.FirstOrDefault(x => x.Value == tree.treeType.Value).Key;
                    FruitTreeData treeData = this.FruitTrees.FirstOrDefault(x => x.Name == key);
                    if (treeData is not null && treeData.ProductId != tree.indexOfFruit.Value) // JA managed fruit tree.
                    {
                        Log.Trace($"Fixing fruit tree product: From {tree.indexOfFruit.Value} to {treeData.Product}={treeData.ProductId}");
                        tree.indexOfFruit.Value = treeData.ProductId;
                    }

                    return false;
                }

                default:
                    return false;
            }
        }

        /// <summary>
        /// Fixes the items in an item list. Sets items that it cannot be found to null.
        /// NOTE: Will need to remove nulls for chests later!
        /// </summary>
        /// <param name="items">A list of items.</param>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal void FixItemList(IList<Item> items)
        {
            if (items is null)
                return;

            int count = 0;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (item is not null)
                {
                    count++;
                    if (this.FixItem(item))
                        items[i] = null;
                }
            }

            Log.Verbose($"Found {count} items in list");
        }

        /// <summary>
        /// Removes the nulls from an item list.
        /// (This is required to prevent chests from causing crashes).
        /// </summary>
        /// <param name="items"></param>
        internal void RemoveNulls(IList<Item> items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] is null)
                    items.RemoveAt(i);
            }
        }

        private void FixIdDict(NetIntDictionary<int, NetInt> dict, bool removeUnshippable = false)
        {
            var toRemove = new List<int>();
            var toAdd = new Dictionary<int, int>();
            foreach (int entry in dict.Keys)
            {
                if (this.VanillaObjectIds.Contains(entry))
                    continue;

                if (this.OldObjectIds.FirstOrDefault(x => x.Value == entry).Key is string name)
                {
                    toRemove.Add(entry);

                    if (this.ObjectIds.TryGetValue(name, out int id))
                    {
                        var obj = this.Objects.First(o => o.Name == name);
                        if (!removeUnshippable || (obj.CanSell && !obj.HideFromShippingCollection && !this.MyRings.Any(r => r.Name == name)))
                            toAdd.Add(id, dict[entry]);
                    }
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
                else
                {
                    dict.Add(entry.Key, entry.Value);
                }
            }
        }

        private void FixIdDict2(NetIntIntArrayDictionary dict)
        {
            var toRemove = new List<int>();
            var toAdd = new Dictionary<int, int[]>();
            foreach (int entry in dict.Keys)
            {
                if (this.VanillaObjectIds.Contains(entry))
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

        private void FixIdDict3(SerializableDictionary<int, int> dict)
        {
            var toRemove = new List<int>();
            var toAddOrUpdate = new Dictionary<int, int>();

            foreach ((int key, int val) in dict)
            {
                if (this.VanillaObjectIds.Contains(key))
                    continue;

                if (this.ReverseFixing)
                {
                    KeyValuePair<string, int> item = this.ObjectIds.FirstOrDefault(x => x.Value == key);
                    if (item.Key is not null)
                    {
                        if (this.OldObjectIds.TryGetValue(item.Key, out int oldindex))
                        {
                            if (oldindex != item.Value)
                            {
                                toRemove.Add(key);
                                toAddOrUpdate.Add(oldindex, val);
                            }
                        }
                        else
                        {
                            toRemove.Add(key);
                        }
                    }
                }
                else
                {
                    KeyValuePair<string, int> item = this.OldObjectIds.FirstOrDefault(x => x.Value == key);
                    if (item.Key is not null) // default(kvp(string,int)) is (null,0)
                    {
                        if (this.ObjectIds.TryGetValue(item.Key, out int newindex))
                        {
                            if (newindex != item.Value)
                            {
                                toRemove.Add(key);
                                toAddOrUpdate.Add(newindex, val);
                            }
                        }
                        else
                        {
                            toRemove.Add(key);
                        }
                    }
                }
            }
            foreach (int entry in toRemove)
                dict.Remove(entry);
            foreach ((int entry, int val) in toAddOrUpdate)
                dict[entry] = val;
        }

        private void FixVector2Dictionary(NetVector2Dictionary<int, NetInt> dict)
        {
            var toRemove = new List<Vector2>();
            var addOrUpdate = new Dictionary<Vector2, int>();
            foreach (var (loc, index) in dict.Pairs)
            {

                if (this.VanillaObjectIds.Contains(index))
                    continue;

                if (this.ReverseFixing)
                {
                    KeyValuePair<string, int> item = this.ObjectIds.FirstOrDefault(x => x.Value == index);
                    if (item.Key is not null)
                    {
                        if (this.OldObjectIds.TryGetValue(item.Key, out int oldindex))
                        {
                            if (oldindex != item.Value)
                                addOrUpdate.Add(loc, oldindex);
                        }
                        else
                        {
                            toRemove.Add(loc);
                        }
                    }
                }
                else
                {
                    KeyValuePair<string, int> item = this.OldObjectIds.FirstOrDefault(x => x.Value == index);
                    if (item.Key is not null) // default(kvp(string,int)) is (null,0)
                    {
                        if (this.ObjectIds.TryGetValue(item.Key, out int newindex))
                        {
                            if (newindex != item.Value)
                                addOrUpdate.Add(loc, newindex);
                        }
                        else
                        {
                            toRemove.Add(loc);
                        }
                    }
                }
            }

            foreach (var entry in toRemove)
                dict.Remove(entry);
            foreach ((var entry, int val) in addOrUpdate)
                dict[entry] = val;
        }

        /// <summary>Fix item IDs contained by an item, including the item itself.</summary>
        /// <param name="oldIds">The custom items' previously assigned IDs from the save data, indexed by item name.</param>
        /// <param name="newIds">The custom items' currently assigned IDs, indexed by item name.</param>
        /// <param name="id">The current item ID.</param>
        /// <param name="vanillaIds">The vanilla items' IDs, indexed by item name.</param>
        /// <returns>Returns whether the item should be removed. Items should only be removed if they no longer exist in the new data.</returns>
        private bool FixId(IDictionary<string, int> oldIds, IDictionary<string, int> newIds, NetInt id, ISet<int> vanillaIds)
        {
            if (id is null)
                return false;

            if (vanillaIds.Contains(id.Value))
                return false;

            if (this.ReverseFixing)
            {
                if (newIds.Values.Contains(id.Value))
                {
                    int curId = id.Value;
                    string key = newIds.FirstOrDefault(x => x.Value == curId).Key;

                    if (key is not null && oldIds.TryGetValue(key, out int oldId))
                    {
                        id.Value = oldId;
                        if (curId != id.Value)
                            Log.Trace("Changing ID: " + key + " from ID " + curId + " to " + id.Value);
                        return false;
                    }
                    else
                    {
                        Log.Warn("New item " + key + " with ID " + curId + "!");
                        return false;
                    }
                }
                else
                    return false;
            }
            else
            {
                if (oldIds.Values.Contains(id.Value))
                {
                    int curId = id.Value;
                    string key = oldIds.FirstOrDefault(x => x.Value == curId).Key;

                    if (key is not null && newIds.TryGetValue(key, out int newId))
                    {
                        id.Value = newId;
                        if (curId != newId)
                            Log.Trace("Changing ID: " + key + " from ID " + curId + " to " + id.Value);
                        return false;
                    }
                    else
                    {
                        Log.Trace("Deleting missing item " + key + " with old ID " + curId);
                        return true;
                    }
                }
                else
                    return false;
            }
        }

        // Return true if the item should be deleted, false otherwise.
        // Only remove something if old has it but not new
        private bool FixId(IDictionary<string, int> oldIds, IDictionary<string, int> newIds, ref int id, ISet<int> vanillaIds)
        {
            if (vanillaIds.Contains(id))
                return false;

            if (this.ReverseFixing)
            {
                if (newIds.Values.Contains(id))
                {
                    int curId = id;
                    string key = newIds.FirstOrDefault(xTile => xTile.Value == curId).Key;

                    if (key is not null && oldIds.TryGetValue(key, out int oldId))
                    {
                        id = oldId;
                        if (id != curId)
                            Log.Trace("Changing ID: " + key + " from ID " + curId + " to " + id);
                        return false;
                    }
                    else
                    {
                        Log.Warn("New item " + key + " with ID " + curId + "!");
                        return false;
                    }
                }
                else
                    return false;
            }
            else
            {
                if (oldIds.Values.Contains(id))
                {
                    int curId = id;
                    string key = oldIds.FirstOrDefault(x => x.Value == curId).Key;

                    if (key is not null && newIds.TryGetValue(key, out int newId))
                    {
                        id = newId;
                        if (curId != id)
                            Log.Trace("Changing ID: " + key + " from ID " + curId + " to " + id);
                        return false;
                    }
                    else
                    {
                        Log.Trace("Deleting missing item " + key + " with old ID " + curId);
                        return true;
                    }
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets all the buildings.
        /// </summary>
        /// <returns>IEnumerable of all buildings.</returns>
        public static IEnumerable<Building> GetBuildings()
        {
            foreach (GameLocation? loc in Game1.locations)
            {
                if (loc is BuildableGameLocation buildable)
                {
                    foreach (Building? building in GetBuildings(buildable))
                    {
                        yield return building;
                    }
                }
            }
        }

        private static IEnumerable<Building> GetBuildings(BuildableGameLocation loc)
        {
            foreach (Building building in loc.buildings)
            {
                yield return building;
                if (building.indoors?.Value is BuildableGameLocation buildable)
                {
                    foreach (Building interiorBuilding in GetBuildings(buildable))
                    {
                        yield return interiorBuilding;
                    }
                }
            }
        }

        private void FixSFBuildings(object sender, EventArgs e)
        {
            this.sfapi.AfterBuildingRestoration -= this.FixSFBuildings;

            try
            {
                foreach (var building in GetBuildings())
                {
                    if (building.modData.ContainsKey(SFID))
                    {
                        this.FixBuilding(building);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed while trying to deshuffle SF buildings {ex}");
            }
        }
    }
}
