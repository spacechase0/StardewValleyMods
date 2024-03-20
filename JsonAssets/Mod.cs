using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JsonAssets.Data;
using JsonAssets.Framework;
using JsonAssets.Framework.ContentPatcher;
using JsonAssets.Patches;
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
    public static class Extensions
    {
        // Doing this as an extension method isn't more efficient
        // It just saves me a miniscule amount of time finding/replacing the old code

        private static Regex NameFixer = new("[^a-zA-Z0-9_]", RegexOptions.Compiled);
        public static string FixIdJA(this string before)
        {
            return NameFixer.Replace(before, "_");
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
        public static Mod instance;

        private ContentInjector1 Content1;
        private ContentInjector2 Content2;

        /// <summary>The Expanded Preconditions Utility API, if that mod is loaded.</summary>
        private IExpandedPreconditionsUtilityApi ExpandedPreconditionsUtility;

        /// <summary>The last shop menu Json Assets added items to.</summary>
        /// <remarks>This is used to avoid adding items again if the menu was stashed and restored (e.g. by Lookup Anything).</remarks>
        private ShopMenu LastShopMenu;

        private List<CustomForgeRecipe> myForgeRecipes = new();

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

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnTick;
            helper.Events.Specialized.LoadStageChanged += this.OnLoadStageChanged;
            helper.Events.Multiplayer.PeerContextReceived += this.ClientConnected;

            HarmonyPatcher.Apply(this,
                new CropPatcher(),
                //new FencePatcher(),
                new Game1Patcher(),
                new GiantCropPatcher(),
                new HoeDirtPatcher(),
                new ShopMenuPatcher()
            );
        }

        private Api Api;
        public override object GetApi()
        {
            return this.Api ??= new Api(this.LoadData);
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
            //var t = Game1.currentLocation.terrainFeatures;
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


                new ContentInjector1();
                new ContentInjector2();

                this.Api.InvokeItemsRegistered();
            }

        }

        private static readonly Regex NameToId = new("[^a-zA-Z0-9_.]");

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
                    Object = () => new SObject(obj.Name, 1, true, obj.Recipe.PurchasePrice)
                });

                foreach (var entry in obj.Recipe.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(obj.Name, 1, true, entry.PurchasePrice)
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
                    Object = () => new SObject(obj.Name, int.MaxValue, false, obj.Price)
                });
                foreach (var entry in obj.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(obj.Name, int.MaxValue, false, obj.Price)
                    });
                }
            }

            // check for duplicates
            if (this.DupObjects.TryGetValue(obj.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate object: {obj.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupObjects[obj.Name] = source;

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
                    Object = () => new SObject(crop.Seed.Name, int.MaxValue, false, crop.Seed.Price),
                    ShowWithStocklist = true
                });
                foreach (var entry in crop.Seed.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(crop.Seed.Name, int.MaxValue, false, crop.Seed.Price)
                    });
                }
            }

            // check for duplicates
            if (this.DupCrops.TryGetValue(crop.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate crop: {crop.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupCrops[crop.Name] = source;

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
                    Object = () => new SObject(tree.Sapling.Name, int.MaxValue)
                });
                foreach (var entry in tree.Sapling.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(tree.Sapling.Name, int.MaxValue)
                    });
                }
            }

            // check for duplicates
            if (this.DupFruitTrees.TryGetValue(tree.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate fruit tree: {tree.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupFruitTrees[tree.Name] = source;

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
                    Object = () => new SObject(Vector2.Zero, craftable.Name, true)
                });
                foreach (var entry in craftable.Recipe.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(Vector2.Zero, craftable.Name, true)
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
                    Object = () => new SObject(Vector2.Zero, craftable.Name)
                });
                foreach (var entry in craftable.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new SObject(Vector2.Zero, craftable.Name)
                    });
                }
            }

            // check for duplicates
            if (this.DupBigCraftables.TryGetValue(craftable.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate big craftable: {craftable.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupBigCraftables[craftable.Name] = source;

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

            // save data
            this.Hats.Add(hat);

            // add to shops
            if (hat.CanPurchase)
            {
                this.shopData.Add(new ShopDataEntry
                {
                    PurchaseFrom = hat.PurchaseFrom,
                    Price = hat.PurchasePrice,
                    PurchaseRequirements = ParsedConditions.AlwaysTrue,
                    Object = () => new Hat(hat.Name)
                });
            }

            // check for duplicates
            if (this.DupHats.TryGetValue(hat.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate hat: {hat.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupHats[hat.Name] = source;

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
                    Object = () => new MeleeWeapon(weapon.Name)
                });
                foreach (var entry in weapon.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new MeleeWeapon(weapon.Name)
                    });
                }
            }

            // check for duplicates
            if (this.DupWeapons.TryGetValue(weapon.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate weapon: {weapon.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupWeapons[weapon.Name] = source;

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

            // save data
            this.Shirts.Add(shirt);

            // check for duplicates
            if (this.DupShirts.TryGetValue(shirt.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate shirt: {shirt.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupShirts[shirt.Name] = source;

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

            // save data
            this.Pants.Add(pants);

            // check for duplicates
            if (this.DupPants.TryGetValue(pants.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate pants: {pants.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupPants[pants.Name] = source;

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
                    Object = () => new Boots(boots.Name)
                });

                foreach (var entry in boots.AdditionalPurchaseData)
                {
                    this.shopData.Add(new ShopDataEntry
                    {
                        PurchaseFrom = entry.PurchaseFrom,
                        Price = entry.PurchasePrice,
                        PurchaseRequirements = this.ParseAndValidateRequirements(source, entry.PurchaseRequirements),
                        Object = () => new Boots(boots.Name)
                    });
                }
            }

            // check for duplicates
            if (this.DupBoots.TryGetValue(boots.Name, out IManifest prevManifest))
            {
                Log.Error($"Duplicate boots: {boots.Name} just added by {source.Name}, already added by {prevManifest.Name}!");
                return;
            }
            else
                this.DupBoots[boots.Name] = source;

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
                    string relativePath = $"Crops/{dir.Name}";

                    // load data
                    CropData crop = contentPack.ReadJsonFile<CropData>($"{relativePath}/crop.json");
                    if (crop == null || (crop.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(crop.DisableWithMod)) || (crop.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(crop.EnableWithMod)))
                        continue;

                    // save crop
                    crop.Texture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/crop.png");
                    if (contentPack.HasFile($"{relativePath}/giant.png"))
                        crop.GiantTexture = contentPack.ModContent.Load<Texture2D>($"{relativePath}/giant.png");

                    this.RegisterCrop(contentPack.Manifest, crop, contentPack.ModContent.Load<Texture2D>($"{relativePath}/seeds.png"), translations);
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
                    string relativePath = $"Forge/{dir.Name}";

                    // load data
                    ForgeRecipeData recipe = contentPack.ReadJsonFile<ForgeRecipeData>($"{relativePath}/recipe.json");
                    if (recipe == null || (recipe.DisableWithMod != null && this.Helper.ModRegistry.IsLoaded(recipe.DisableWithMod)) || (recipe.EnableWithMod != null && !this.Helper.ModRegistry.IsLoaded(recipe.EnableWithMod)))
                        continue;

                    this.RegisterForgeRecipe(contentPack.Manifest, recipe);
                }
            }
        }

        internal void OnBlankSave()
        {
            Log.Trace("Loading stuff early (really super early)");
            if (string.IsNullOrEmpty(Constants.CurrentSavePath))
            {
                this.InitStuff(loadIdFiles: false);
            }
        }

        [EventPriority(EventPriority.High)]
        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveParsed)
            {
                //Log.debug("Loading stuff early (loading)");
                this.InitStuff(loadIdFiles: true);
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveLoadedLocations)
            {
                Log.Trace("Fixing IDs");
                this.FixIdsEverywhere();
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.Loaded)
            {
                Log.Trace("Adding default recipes");
                foreach (var obj in this.Objects)
                {
                    if (obj.Recipe != null)
                    {
                        if (obj.Recipe.IsDefault && !Game1.player.knowsRecipe(obj.Name))
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
                        if (big.Recipe.IsDefault && !Game1.player.knowsRecipe(big.Name))
                            Game1.player.craftingRecipes.Add(big.Name, 0);
                    }
                }

                foreach (var frecipe in Forge)
                {
                    CustomForgeRecipe recipe = new JAForgeRecipe(frecipe);
                    myForgeRecipes.Add(recipe);
                    CustomForgeRecipe.Recipes.Add(recipe);
                }
            }
        }

        // Terrible place to put these, TODO move later
        private class JAForgeRecipe : CustomForgeRecipe
        {
            public IngredientMatcher baseItem;
            public override IngredientMatcher BaseItem => baseItem;

            public IngredientMatcher ingredientItem;
            public override IngredientMatcher IngredientItem => ingredientItem;

            public int shards;
            public override int CinderShardCost => shards;

            public override Item CreateResult(Item baseItem, Item ingredItem)
            {
                return Utility.fuzzyItemSearch(data.ResultItemName);
            }

            private ForgeRecipeData data;

            public JAForgeRecipe(ForgeRecipeData frecipe)
            {
                data = frecipe;
                baseItem = new JAForgeIngredientMatcher(frecipe, baseItem: true);
                ingredientItem = new JAForgeIngredientMatcher(frecipe, baseItem: false);
                shards = frecipe.CinderShardCost;
            }
        }
        private class JAForgeIngredientMatcher : CustomForgeRecipe.IngredientMatcher
        {
            private bool isBaseItem;
            private ForgeRecipeData recipe;

            public JAForgeIngredientMatcher(ForgeRecipeData data, bool baseItem)
            {
                isBaseItem = baseItem;
                recipe = data;
            }

            public override void Consume(ref Item item)
            {
                if (item.Stack > 1)
                    item.Stack--;
                else
                    item = null;
            }

            public override bool HasEnoughFor(Item item)
            {
                if (recipe.AbleToForgeConditions != null && recipe.AbleToForgeConditions.Length > 0)
                {
                    if (JsonAssets.Mod.instance.CheckEpuCondition(recipe.AbleToForgeConditions))
                    {
                        return false;
                    }
                }

                if (isBaseItem)
                {
                    return item.Name == recipe.BaseItemName;
                }
                else
                {
                    return item.GetContextTags().Contains(recipe.IngredientContextTag);
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

                bool isPierre = shopIds.Contains("Pierre");
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
                    var isi = isQiGemShop
                        ? new ItemStockInformation(0, isRecipe ? 1 : int.MaxValue, "858", price)
                        : new ItemStockInformation(price, isRecipe ? 1 : int.MaxValue);
                    itemPriceAndStock.Add(item, isi);
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
                // TODO: How to reproduce this in 1.6?
                /*
                string portraitName = !string.IsNullOrWhiteSpace(menu.portraitPerson?.Name) ? menu.portraitPerson.Name : null;
                if (portraitName != null)
                    yield return portraitName;
                */

                // shop context
                string context = !string.IsNullOrWhiteSpace(menu.ShopId) ? menu.ShopId : null;
                if (context != null)
                    yield return context;
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
                this.OldObjectIds = (LoadDictionary<string, int>("ids-objects.json") ?? new Dictionary<string, int>()).ToDictionary( x => x.Value.ToString(), x => x.Key );
                this.OldCropIds = (LoadDictionary<string, int>("ids-crops.json") ?? new Dictionary<string, int>()).ToDictionary(x => x.Value.ToString(), x => x.Key);
                this.OldFruitTreeIds = (LoadDictionary<string, int>("ids-fruittrees.json") ?? new Dictionary<string, int>()).ToDictionary(x => x.Value.ToString(), x => x.Key);
                this.OldBigCraftableIds = (LoadDictionary<string, int>("ids-big-craftables.json") ?? new Dictionary<string, int>()).ToDictionary(x => x.Value.ToString(), x => x.Key);
                this.OldHatIds = (LoadDictionary<string, int>("ids-hats.json") ?? new Dictionary<string, int>()).ToDictionary(x => x.Value.ToString(), x => x.Key);
                this.OldWeaponIds = (LoadDictionary<string, int>("ids-weapons.json") ?? new Dictionary<string, int>()).ToDictionary(x => x.Value.ToString(), x => x.Key);
                this.OldClothingIds = (LoadDictionary<string, int>("ids-clothing.json") ?? new Dictionary<string, int>()).ToDictionary(x => x.Value.ToString(), x => x.Key);
                this.OldBootsIds = (LoadDictionary<string, int>("ids-boots.json") ?? new Dictionary<string, int>()).ToDictionary(x => x.Value.ToString(), x => x.Key);

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

                Log.Verbose("(Removing ones that aren't loaded)");

                // We need to remove IDs they aren't loaded so they don't get "fixed" so that
                // other mods can migrate their own items if they migrate away from JA.
                // The Dup* containers weren't originally intended for this, but hey, it works
                var objs = LoadDictionary<string, int>("ids-objects.json");
                foreach (string key in objs.Keys)
                {
                    if (!DupObjects.ContainsKey(key))
                        OldObjectIds.Remove(objs[key].ToString());
                }
                var crops = LoadDictionary<string, int>("ids-crops.json");
                foreach (string key in crops.Keys)
                {
                    if (!DupCrops.ContainsKey(key))
                        OldCropIds.Remove(crops[key].ToString());
                }
                var ftrees = LoadDictionary<string, int>("ids-fruittrees.json");
                foreach (string key in ftrees.Keys)
                {
                    if (!DupFruitTrees.ContainsKey(key))
                        OldFruitTreeIds.Remove(ftrees[key].ToString());
                }
                var bigs = LoadDictionary<string, int>("ids-big-craftables.json");
                foreach (string key in bigs.Keys)
                {
                    if (!DupBigCraftables.ContainsKey(key))
                        OldBigCraftableIds.Remove(bigs[key].ToString());
                }
                var hats = LoadDictionary<string, int>("ids-hats.json");
                foreach (string key in hats.Keys)
                {
                    if (!DupHats.ContainsKey(key))
                        OldHatIds.Remove(hats[key].ToString());
                }
                var weapons = LoadDictionary<string, int>("ids-weapons.json");
                foreach (string key in weapons.Keys)
                {
                    if (!DupWeapons.ContainsKey(key))
                        OldWeaponIds.Remove(weapons[key].ToString());
                }
                var clothing = LoadDictionary<string, int>("ids-clothing.json");
                foreach (string key in clothing.Keys)
                {
                    if (!DupShirts.ContainsKey(key) && !DupPants.ContainsKey(key))
                        OldClothingIds.Remove(clothing[key].ToString());
                }
                var boots = LoadDictionary<string, int>("ids-boots.json");
                foreach (string key in boots.Keys)
                {
                    if (!DupBoots.ContainsKey(key))
                        OldBootsIds.Remove(boots[key].ToString());
                }
            }
        }

        internal List<ObjectData> Objects = new List<ObjectData>();
        internal List<CropData> Crops = new List<CropData>();
        internal List<FruitTreeData> FruitTrees = new List<FruitTreeData>();
        internal List<BigCraftableData> BigCraftables = new List<BigCraftableData>();
        internal List<HatData> Hats = new List<HatData>();
        internal List<WeaponData> Weapons = new List<WeaponData>();
        internal List<ShirtData> Shirts = new List<ShirtData>();
        internal List<PantsData> Pants = new List<PantsData>();
        internal List<TailoringRecipeData> Tailoring = new List<TailoringRecipeData>();
        internal List<BootsData> Boots = new List<BootsData>();
        internal List<FenceData> Fences = new();
        internal List<ForgeRecipeData> Forge = new List<ForgeRecipeData>();

        // In this version of JA, these are int-ID-string to name, not name to int ID
        internal Dictionary<string, string> OldObjectIds = new();
        internal Dictionary<string, string> OldCropIds = new();
        internal Dictionary<string, string> OldFruitTreeIds = new();
        internal Dictionary<string, string> OldBigCraftableIds = new();
        internal Dictionary<string, string> OldHatIds = new();
        internal Dictionary<string, string> OldWeaponIds = new();
        internal Dictionary<string, string> OldClothingIds = new();
        internal Dictionary<string, string> OldBootsIds = new();

        /// <summary>Populate an item's localization fields based on the <see cref="ITranslatableItem.TranslationKey"/> property, if defined.</summary>
        /// <param name="item">The item for which to populate translations.</param>
        /// <param name="translations">The translation helper from which to fetch translations.</param>
        private void PopulateTranslations(ITranslatableItem item, ITranslationHelper translations)
        {
            if (translations == null || string.IsNullOrWhiteSpace(item?.TranslationKey))
                return;

            foreach (var pair in translations.GetInAllLocales($"{item.TranslationKey}.name"))
            {
                string locale = pair.Key;
                string text = pair.Value;

                item.NameLocalization[locale] = text;
            }

            foreach (var pair in translations.GetInAllLocales($"{item.TranslationKey}.description"))
            {
                string locale = pair.Key;
                string text = pair.Value;

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


        private readonly HashSet<string> LocationsFixedAlready = new();
        private void FixIdsEverywhere(bool reverse = false)
        {
            Utility.ForEachItem(i => { FixItem(i); return true; });
            SpaceUtility.iterateAllTerrainFeatures(this.FixTerrainFeature);
            foreach ( var loc in Game1.locations )
            {
                foreach (var building in loc.buildings)
                    FixBuilding(building);
            }

            this.FixIdDict(Game1.player.basicShipped, removeUnshippable: true);
            this.FixIdDict(Game1.player.mineralsFound);
            this.FixIdDict(Game1.player.recipesCooked);
            this.FixIdDict2(Game1.player.archaeologyFound);
            this.FixIdDict2(Game1.player.fishCaught);

            // Fix this if anyone complains it isn't working
            /*
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
                toks[1] = string.Join(" ", toks1);
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
                toks[2] = string.Join(" ", toks2);
                bundleData[entry.Key] = string.Join("/", toks);
            }

            Game1.netWorldState.Value.SetBundleData(bundleData);
            */
            // Fix bad bundle data
        }

        /// <summary>Fix item IDs contained by an item, including the item itself.</summary>
        /// <param name="item">The item to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal Item FixItem(Item item)
        {
            switch (item)
            {
                case Hat hat:
                    if (hat.obsolete_which.HasValue && this.OldHatIds.ContainsKey(hat.obsolete_which.Value.ToString()))
                        hat.ItemId = this.OldHatIds[hat.obsolete_which.Value.ToString()].FixIdJA();
                    break;

                case MeleeWeapon weapon:
                    if (this.OldWeaponIds.ContainsKey(weapon.ItemId))
                        weapon.ItemId = this.OldWeaponIds[weapon.ItemId].FixIdJA();
                    if (weapon.appearance.Value != null && this.OldWeaponIds.ContainsKey(weapon.appearance.Value))
                        weapon.appearance.Value = this.OldWeaponIds[weapon.appearance.Value].FixIdJA();
                    break;

                case Ring ring:
                    if (this.OldObjectIds.ContainsKey(ring.ItemId))
                        ring.ItemId = this.OldObjectIds[ring.ItemId].FixIdJA();

                    if (ring is CombinedRing combinedRing)
                    {
                        for (int i = combinedRing.combinedRings.Count - 1; i >= 0; i--)
                        {
                            combinedRing.combinedRings[i] = FixItem(combinedRing.combinedRings[i]) as Ring;
                        }
                    }
                    break;

                case Clothing clothing:
                    if (this.OldClothingIds.ContainsKey(clothing.ItemId))
                        clothing.ItemId = this.OldClothingIds[clothing.ItemId].FixIdJA();
                    break;

                case Boots boots:
                    if (this.OldBootsIds.ContainsKey(boots.ItemId))
                        boots.ItemId = this.OldBootsIds[boots.ItemId].FixIdJA();
                    // TODO: what to do about tailored boots...
                    break;

                case SObject obj:
                    if (obj is Chest chest)
                    {
                        if (this.OldBigCraftableIds.ContainsKey(chest.ItemId))
                            chest.ItemId = this.OldBigCraftableIds[chest.ItemId].FixIdJA();
                        else
                            chest.startingLidFrame.Value = chest.ParentSheetIndex + 1;
                        this.FixItemList(chest.Items);
                    }
                    else if (obj is IndoorPot pot)
                    {
                        if (pot.hoeDirt.Value != null && pot.hoeDirt.Value.crop != null)
                            this.FixCrop(pot.hoeDirt.Value.crop);
                    }
                    else if (obj is Fence fence)
                    {
                        // TODO: Do this once custom fences are in
                    }
                    else if (obj.GetType() == typeof(SObject) || obj.GetType() == typeof(Cask))
                    {
                        if (!obj.bigCraftable.Value)
                        {
                            /*
                            if (this.FixId(this.OldObjectIds, this.ObjectIds, obj.preservedParentSheetIndex, this.VanillaObjectIds))
                                obj.preservedParentSheetIndex.Value = -1;
                            */
                            if (this.OldObjectIds.ContainsKey(obj.ItemId))
                                obj.ItemId = this.OldObjectIds[obj.ItemId].FixIdJA();
                        }
                        else
                        {
                            if (this.OldBigCraftableIds.ContainsKey(obj.ItemId))
                                obj.ItemId = this.OldBigCraftableIds[obj.ItemId].FixIdJA();
                        }
                    }

                    if (obj.heldObject.Value != null)
                    {
                        if (this.OldObjectIds.ContainsKey(obj.heldObject.Value.ItemId))
                            obj.heldObject.Value.ItemId = this.OldObjectIds[obj.heldObject.Value.ItemId].Replace(' ', '_');

                        if (obj.heldObject.Value is Chest innerChest)
                            this.FixItemList(innerChest.Items);
                    }
                    break;
            }

            item?.ResetParentSheetIndex();
            return item;
        }

        /// <summary>Fix item IDs contained by a character.</summary>
        /// <param name="character">The character to fix.</param>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        private void FixCharacter(Character character)
        {
            switch (character)
            {
                case Horse horse:
                    horse.hat.Value = FixItem(horse.hat.Value) as Hat;
                    break;

                case Child child:
                    child.hat.Value = FixItem(child.hat.Value) as Hat;
                    break;

                case Farmer player:
                    this.FixItemList(player.Items);
                    player.leftRing.Value = FixItem(player.leftRing.Value) as Ring;
                    player.rightRing.Value = FixItem(player.rightRing.Value) as Ring;
                    player.hat.Value = FixItem(player.hat.Value) as Hat;
                    player.shirtItem.Value = FixItem(player.shirtItem.Value) as Clothing;
                    player.pantsItem.Value = FixItem(player.pantsItem.Value) as Clothing;
                    player.boots.Value = FixItem(player.boots.Value) as Boots;
                    break;
            }
        }

        /// <summary>Fix item IDs contained by a building.</summary>
        /// <param name="building">The building to fix.</param>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        private void FixBuilding(Building building)
        {
            if (building is null)
                return;

            switch (building)
            {
                default:
                    foreach ( var chest in building.buildingChests.ToList() )
                        this.FixItemList(chest.Items);
                    break;

                case FishPond pond:
                    if (pond.fishType.Value == "-1")
                    {
                        this.Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                        break;
                    }

                    if (pond.fishType.Value != null && this.OldObjectIds.ContainsKey(pond.fishType.Value))
                        pond.fishType.Value = this.OldObjectIds[pond.fishType.Value].FixIdJA();
                    pond.sign.Value = FixItem(pond.sign.Value) as SObject;
                    pond.output.Value = FixItem(pond.output.Value);
                    pond.neededItem.Value = FixItem(pond.neededItem.Value) as SObject;
                    break;
            }
        }

        /// <summary>Fix item IDs contained by a crop, including the crop itself.</summary>
        /// <param name="crop">The crop to fix.</param>
        /// <returns>Returns whether the crop should be removed.</returns>
        private void FixCrop(Crop crop)
        {
            if (crop is null || crop.indexOfHarvest.Value == null)
                return;

            if (this.OldObjectIds.ContainsKey(crop.indexOfHarvest.Value))
                crop.indexOfHarvest.Value = this.OldObjectIds[crop.indexOfHarvest.Value].FixIdJA();
            if (crop.netSeedIndex.Value != null && this.OldObjectIds.ContainsKey(crop.netSeedIndex.Value))
                crop.netSeedIndex.Value = this.OldObjectIds[crop.netSeedIndex.Value].FixIdJA();
            if (crop.netSeedIndex.Value == null)
            {
                foreach (var data in Game1.cropData)
                {
                    if (data.Value.HarvestItemId == crop.indexOfHarvest.Value)
                    {
                        crop.netSeedIndex.Value = data.Key;
                        break;
                    }
                }
            }

            if (this.OldCropIds.ContainsKey(crop.rowInSpriteSheet.Value.ToString()))
            {
                crop.overrideTexturePath.Value = "JA/Crop/" + this.OldCropIds[crop.rowInSpriteSheet.Value.ToString()].FixIdJA();
                crop.rowInSpriteSheet.Value = 0;
            }
        }

        /// <summary>Fix item IDs contained by a terrain feature, including the terrain feature itself.</summary>
        /// <param name="feature">The terrain feature to fix.</param>
        /// <returns>Returns whether the item should be removed.</returns>
        private TerrainFeature FixTerrainFeature(TerrainFeature feature)
        {
            switch (feature)
            {
                case HoeDirt dirt:
                    this.FixCrop(dirt.crop);
                    break;

                case FruitTree ftree:
                    {
                        if (this.OldFruitTreeIds.ContainsKey(ftree.treeId.Value))
                        {
                            ftree.treeId.Value = this.OldFruitTreeIds[ftree.treeId.Value].FixIdJA();
                        }
                    }
                    break;

                    // TODO: How to do in 1.6?
                    /*
                case ResourceClump rclump:
                    if ( this.OldObjectIds.ContainsKey( rclump.parentSheetIndex.Value.ToString() ) )
                    {
                        rclump.ItemId = this.OldObjectIds[rclump.parentSheetIndex.Value.ToString()].FixIdJA();
                        rclump.parentSheetIndex.Value = 1720;
                    }
                    */

                    break;
            }

            return feature;
        }

        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal void FixItemList(IList<Item> items)
        {
            if (items is null)
                return;

            for (int i = 0; i < items.Count; ++i)
            {
                items[i] = FixItem(items[i]);
                var item = items[i];
                if (item == null)
                    continue;
            }
        }

        private void FixIdDict(NetStringDictionary<int, NetInt> dict, bool removeUnshippable = false)
        {
            var toRemove = new List<string>();
            var toAdd = new Dictionary<string, int>();
            foreach (string entry in dict.Keys)
            {
                if (this.OldObjectIds.ContainsKey(entry))
                {
                    toRemove.Add(entry);
                    toAdd.Add(this.OldObjectIds[entry].FixIdJA(), dict[entry]);
                }
            }
            foreach (string entry in toRemove)
                dict.Remove(entry);
            foreach (var entry in toAdd)
            {
                if (dict.ContainsKey(entry.Key))
                {
                    Log.Error("Dict already has value for " + entry.Key + "!");
                    foreach (var obj in this.Objects)
                    {
                        if (obj.Name.FixIdJA() == entry.Key)
                            Log.Error("\tobj = " + obj.Name);
                    }
                }
                dict.Add(entry.Key, entry.Value);
            }
        }

        private void FixIdDict2(NetStringIntArrayDictionary dict)
        {
            var toRemove = new List<string>();
            var toAdd = new Dictionary<string, int[]>();
            foreach (string entry in dict.Keys)
            {
                if (this.OldObjectIds.ContainsKey(entry))
                {
                    toRemove.Add(entry);
                    toAdd.Add(this.OldObjectIds[entry].FixIdJA(), dict[entry]);
                }
            }
            foreach (string entry in toRemove)
                dict.Remove(entry);
            foreach (var entry in toAdd)
                dict.Add(entry.Key, entry.Value);
        }
    }
}
