using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Harmony;
using JsonAssets.Data;
using JsonAssets.Other.ContentPatcher;
using JsonAssets.Overrides;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
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
        private HarmonyInstance harmony;
        private ContentInjector1 content1;
        private ContentInjector2 content2;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.Display.MenuChanged += onMenuChanged;
            helper.Events.GameLoop.Saved += onSaved;
            helper.Events.Player.InventoryChanged += onInventoryChanged;
            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.SaveCreated += onCreated;
            helper.Events.GameLoop.UpdateTicked += onTick;
            helper.Events.Specialized.LoadStageChanged += onLoadStageChanged;
            helper.Events.Multiplayer.PeerContextReceived += clientConnected;

            helper.Content.AssetEditors.Add(content1 = new ContentInjector1());

            SpaceCore.TileSheetExtensions.RegisterExtendedTileSheet("TileSheets\\crops", 32);
            SpaceCore.TileSheetExtensions.RegisterExtendedTileSheet("TileSheets\\fruitTrees", 80);
            SpaceCore.TileSheetExtensions.RegisterExtendedTileSheet("Characters\\Farmer\\shirts", 32);
            SpaceCore.TileSheetExtensions.RegisterExtendedTileSheet("Characters\\Farmer\\pants", 688);

            try
            {
                harmony = HarmonyInstance.Create("spacechase0.JsonAssets");

                // object patches
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.canBePlacedHere)),
                    prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.CanBePlacedHere_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.CheckForAction_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), "loadDisplayName"),
                    prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.LoadDisplayName_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.getCategoryName)),
                    prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.GetCategoryName_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.isIndexOkForBasicShippedCategory)),
                    postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.IsIndexOkForBasicShippedCategory_Postfix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.getCategoryColor)),
                    prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.GetCategoryColor_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(SObject), nameof(SObject.canBeGivenAsGift)),
                    postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.CanBeGivenAsGift_Postfix))
                );

                // ring patches
                harmony.Patch(
                    original: AccessTools.Method(typeof(Ring), "loadDisplayFields"),
                    prefix: new HarmonyMethod(typeof(RingPatches), nameof(RingPatches.LoadDisplayFields_Prefix))
                );

                // crop patches
                harmony.Patch(
                    original: AccessTools.Method(typeof(Crop), nameof(Crop.isPaddyCrop)),
                    prefix: new HarmonyMethod(typeof(CropPatches), nameof(CropPatches.IsPaddyCrop_Prefix))
                );

                harmony.Patch(
                    original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                    transpiler: new HarmonyMethod(typeof(CropPatches), nameof(CropPatches.NewDay_Transpiler))
                );

                // GiantCrop patches
                harmony.Patch(
                    original: AccessTools.Method(typeof(GiantCrop), nameof(GiantCrop.draw)),
                    prefix: new HarmonyMethod(typeof(GiantCropPatches), nameof(GiantCropPatches.Draw_Prefix))
                );

                // item patches
                harmony.Patch(
                    original: AccessTools.Method(typeof(Item), nameof(Item.canBeDropped)),
                    postfix: new HarmonyMethod(typeof(ItemPatches), nameof(ItemPatches.CanBeDropped_Postfix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(Item), nameof(Item.canBeTrashed)),
                    postfix: new HarmonyMethod(typeof(ItemPatches), nameof(ItemPatches.CanBeTrashed_Postfix))
                );
            }
            catch (Exception e)
            {
                Log.error($"Exception doing harmony stuff: {e}");
            }
        }

        private Api api;
        public override object GetApi()
        {
            return api ?? (api = new Api(this.loadData));
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ContentPatcherIntegration.Initialize();
        }

        bool firstTick = true;
        private void onTick(object sender, UpdateTickedEventArgs e)
        {
            // This needs to run after GameLaunched, because of the event 
            if (firstTick)
            {
                firstTick = false;

                Log.info("Loading content packs...");
                foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
                    try
                    {
                        loadData(contentPack);
                    }
                    catch (Exception e1)
                    {
                        Log.error("Exception loading content pack: " + e1);
                    }
                if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
                {
                    foreach (string dir in Directory.EnumerateDirectories(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
                        try
                        {
                            loadData(dir);
                        }
                        catch (Exception e2)
                        {
                            Log.error("Exception loading content pack: " + e2);
                        }
                }
                api.InvokeItemsRegistered();

                resetAtTitle();
            }

        }

        private static readonly Regex nameToId = new Regex("[^a-zA-Z0-9_.]");
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
            string id = nameToId.Replace(info.Name, "");
            IContentPack contentPack = this.Helper.ContentPacks.CreateTemporary(dir, id: id, name: info.Name, description: info.Description, author: info.Author, version: new SemanticVersion(info.Version));
            this.loadData(contentPack);
        }

        internal Dictionary<IManifest, List<string>> objectsByContentPack = new Dictionary<IManifest, List<string>>();
        internal Dictionary<IManifest, List<string>> cropsByContentPack = new Dictionary<IManifest, List<string>>();
        internal Dictionary<IManifest, List<string>> fruitTreesByContentPack = new Dictionary<IManifest, List<string>>();
        internal Dictionary<IManifest, List<string>> bigCraftablesByContentPack = new Dictionary<IManifest, List<string>>();
        internal Dictionary<IManifest, List<string>> hatsByContentPack = new Dictionary<IManifest, List<string>>();
        internal Dictionary<IManifest, List<string>> weaponsByContentPack = new Dictionary<IManifest, List<string>>();
        internal Dictionary<IManifest, List<string>> clothingByContentPack = new Dictionary<IManifest, List<string>>();
        internal Dictionary<IManifest, List<string>> bootsByContentPack = new Dictionary<IManifest, List<string>>();

        public void RegisterObject(IManifest source, ObjectData obj)
        {
            objects.Add(obj);

            // save ring
            if (obj.Category == ObjectData.Category_.Ring)
                this.myRings.Add(obj);

            // Duplicate check
            if (dupObjects.ContainsKey(obj.Name))
                Log.error($"Duplicate object: {obj.Name} just added by {source.Name}, already added by {dupObjects[obj.Name].Name}!");
            else
                dupObjects[obj.Name] = source;

            if (!objectsByContentPack.ContainsKey(source))
                objectsByContentPack.Add(source, new List<string>());
            objectsByContentPack[source].Add(obj.Name);
        }

        public void RegisterCrop(IManifest source, CropData crop, Texture2D seedTex)
        {
            crops.Add(crop);

            // save seeds
            crop.seed = new ObjectData
            {
                texture = seedTex,
                Name = crop.SeedName,
                Description = crop.SeedDescription,
                Category = ObjectData.Category_.Seeds,
                Price = crop.SeedSellPrice == -1 ? crop.SeedPurchasePrice : crop.SeedSellPrice,
                CanPurchase = true,
                PurchaseFrom = crop.SeedPurchaseFrom,
                PurchasePrice = crop.SeedPurchasePrice,
                PurchaseRequirements = crop.SeedPurchaseRequirements ?? new List<string>(),
                NameLocalization = crop.SeedNameLocalization,
                DescriptionLocalization = crop.SeedDescriptionLocalization
            };

            // TODO: Clean up this chunk
            // I copy/pasted it from the unofficial update decompiled
            string str = "";
            string[] array = new[] { "spring", "summer", "fall", "winter" }
                .Except(crop.Seasons)
                .ToArray();
            foreach (var season in array)
            {
                str += $"/z {season}";
            }
            if (str != "")
            {
                string strtrimstart = str.TrimStart(new char[] { '/' });
                if (crop.SeedPurchaseRequirements != null && crop.SeedPurchaseRequirements.Count > 0)
                {
                    for (int index = 0; index < crop.SeedPurchaseRequirements.Count; index++)
                    {
                        if (SeasonLimiter.IsMatch(crop.SeedPurchaseRequirements[index]))
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

            // Duplicate check
            if (dupCrops.ContainsKey(crop.Name))
                Log.error($"Duplicate crop: {crop.Name} just added by {source.Name}, already added by {dupCrops[crop.Name].Name}!");
            else
                dupCrops[crop.Name] = source;

            objects.Add(crop.seed);

            if (!cropsByContentPack.ContainsKey(source))
                cropsByContentPack.Add(source, new List<string>());
            cropsByContentPack[source].Add(crop.Name);

            if (!objectsByContentPack.ContainsKey(source))
                objectsByContentPack.Add(source, new List<string>());
            objectsByContentPack[source].Add(crop.seed.Name);
        }

        public void RegisterFruitTree(IManifest source, FruitTreeData tree, Texture2D saplingTex)
        {
            fruitTrees.Add(tree);

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
                NameLocalization = tree.SaplingNameLocalization,
                DescriptionLocalization = tree.SaplingDescriptionLocalization
            };
            objects.Add(tree.sapling);

            // Duplicate check
            if (dupFruitTrees.ContainsKey(tree.Name))
                Log.error($"Duplicate fruit tree: {tree.Name} just added by {source.Name}, already added by {dupFruitTrees[tree.Name].Name}!");
            else
                dupFruitTrees[tree.Name] = source;

            if (!fruitTreesByContentPack.ContainsKey(source))
                fruitTreesByContentPack.Add(source, new List<string>());
            fruitTreesByContentPack[source].Add(tree.Name);
        }

        public void RegisterBigCraftable(IManifest source, BigCraftableData craftable)
        {
            bigCraftables.Add(craftable);

            // Duplicate check
            if (dupBigCraftables.ContainsKey(craftable.Name))
                Log.error($"Duplicate big craftable: {craftable.Name} just added by {source.Name}, already added by {dupBigCraftables[craftable.Name].Name}!");
            else
                dupBigCraftables[craftable.Name] = source;

            if (!bigCraftablesByContentPack.ContainsKey(source))
                bigCraftablesByContentPack.Add(source, new List<string>());
            bigCraftablesByContentPack[source].Add(craftable.Name);
        }

        public void RegisterHat(IManifest source, HatData hat)
        {
            hats.Add(hat);

            // Duplicate check
            if (dupHats.ContainsKey(hat.Name))
                Log.error($"Duplicate hat: {hat.Name} just added by {source.Name}, already added by {dupHats[hat.Name].Name}!");
            else
                dupHats[hat.Name] = source;

            if (!hatsByContentPack.ContainsKey(source))
                hatsByContentPack.Add(source, new List<string>());
            hatsByContentPack[source].Add(hat.Name);
        }

        public void RegisterWeapon(IManifest source, WeaponData weapon)
        {
            weapons.Add(weapon);

            // Duplicate check
            if (dupWeapons.ContainsKey(weapon.Name))
                Log.error($"Duplicate weapon: {weapon.Name} just added by {source.Name}, already added by {dupWeapons[weapon.Name].Name}!");
            else
                dupWeapons[weapon.Name] = source;

            if (!weaponsByContentPack.ContainsKey(source))
                weaponsByContentPack.Add(source, new List<string>());
            weaponsByContentPack[source].Add(weapon.Name);
        }

        public void RegisterShirt(IManifest source, ShirtData shirt)
        {
            shirts.Add(shirt);

            // Duplicate check
            if (dupShirts.ContainsKey(shirt.Name))
                Log.error($"Duplicate shirt: {shirt.Name} just added by {source.Name}, already added by {dupShirts[shirt.Name].Name}!");
            else
                dupShirts[shirt.Name] = source;

            if (!clothingByContentPack.ContainsKey(source))
                clothingByContentPack.Add(source, new List<string>());
            clothingByContentPack[source].Add(shirt.Name);
        }

        public void RegisterPants(IManifest source, PantsData pants)
        {
            pantss.Add(pants);

            // Duplicate check
            if (dupPants.ContainsKey(pants.Name))
                Log.error($"Duplicate pants: {pants.Name} just added by {source.Name}, already added by {dupPants[pants.Name].Name}!");
            else
                dupPants[pants.Name] = source;

            if (!clothingByContentPack.ContainsKey(source))
                clothingByContentPack.Add(source, new List<string>());
            clothingByContentPack[source].Add(pants.Name);
        }

        public void RegisterTailoringRecipe(IManifest source, TailoringRecipeData recipe)
        {
            tailoring.Add(recipe);
        }

        public void RegisterBoots(IManifest source, BootsData boots)
        {
            bootss.Add(boots);

            // Duplicate check
            if (dupBoots.ContainsKey(boots.Name))
                Log.error($"Duplicate boots: {boots.Name} just added by {source.Name}, already added by {dupBoots[boots.Name].Name}!");
            else
                dupBoots[boots.Name] = source;

            if (!bootsByContentPack.ContainsKey(source))
                bootsByContentPack.Add(source, new List<string>());
            bootsByContentPack[source].Add(boots.Name);
        }

        private Dictionary<string, IManifest> dupObjects = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupCrops = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupFruitTrees = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupBigCraftables = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupHats = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupWeapons = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupShirts = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupPants = new Dictionary<string, IManifest>();
        private Dictionary<string, IManifest> dupBoots = new Dictionary<string, IManifest>();

        private readonly Regex SeasonLimiter = new Regex("(z(?: spring| summer| fall| winter){2,4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
                    if (obj == null || (obj.DisableWithMod != null && Helper.ModRegistry.IsLoaded(obj.DisableWithMod)) || (obj.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(obj.EnableWithMod)))
                        continue;

                    // save object
                    obj.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/object.png");
                    if (obj.IsColored)
                        obj.textureColor = contentPack.LoadAsset<Texture2D>($"{relativePath}/color.png");

                    RegisterObject(contentPack.Manifest, obj);
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
                    if (crop == null || (crop.DisableWithMod != null && Helper.ModRegistry.IsLoaded(crop.DisableWithMod)) || (crop.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(crop.EnableWithMod)))
                        continue;

                    // save crop
                    crop.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/crop.png");
                    if (contentPack.HasFile($"{relativePath}/giant.png"))
                        crop.giantTex = contentPack.LoadAsset<Texture2D>($"{relativePath}/giant.png");

                    RegisterCrop(contentPack.Manifest, crop, contentPack.LoadAsset<Texture2D>($"{relativePath}/seeds.png"));
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
                    if (tree == null || (tree.DisableWithMod != null && Helper.ModRegistry.IsLoaded(tree.DisableWithMod)) || (tree.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(tree.EnableWithMod)))
                        continue;

                    // save fruit tree
                    tree.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/tree.png");
                    RegisterFruitTree(contentPack.Manifest, tree, contentPack.LoadAsset<Texture2D>($"{relativePath}/sapling.png"));
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
                    if (craftable == null || (craftable.DisableWithMod != null && Helper.ModRegistry.IsLoaded(craftable.DisableWithMod)) || (craftable.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(craftable.EnableWithMod)))
                        continue;
                    
                    // save craftable
                    craftable.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/big-craftable.png");
                    if (craftable.ReserveNextIndex && craftable.ReserveExtraIndexCount == 0)
                        craftable.ReserveExtraIndexCount = 1;
                    if (craftable.ReserveExtraIndexCount > 0)
                    {
                        craftable.extraTextures = new Texture2D[craftable.ReserveExtraIndexCount];
                        for ( int i = 0; i < craftable.ReserveExtraIndexCount; ++i )
                            craftable.extraTextures[i] = contentPack.LoadAsset<Texture2D>($"{relativePath}/big-craftable-{i + 2}.png");
                    }
                    RegisterBigCraftable(contentPack.Manifest, craftable);
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
                    if (hat == null || (hat.DisableWithMod != null && Helper.ModRegistry.IsLoaded(hat.DisableWithMod)) || (hat.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(hat.EnableWithMod)))
                        continue;

                    // save object
                    hat.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/hat.png");
                    RegisterHat(contentPack.Manifest, hat);
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
                    if (weapon == null || (weapon.DisableWithMod != null && Helper.ModRegistry.IsLoaded(weapon.DisableWithMod)) || (weapon.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(weapon.EnableWithMod)))
                        continue;

                    // save object
                    weapon.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/weapon.png");
                    RegisterWeapon(contentPack.Manifest, weapon);
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
                    if (shirt == null || (shirt.DisableWithMod != null && Helper.ModRegistry.IsLoaded(shirt.DisableWithMod)) || (shirt.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(shirt.EnableWithMod)))
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
                    RegisterShirt(contentPack.Manifest, shirt);
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
                    if (pants == null || (pants.DisableWithMod != null && Helper.ModRegistry.IsLoaded(pants.DisableWithMod)) || (pants.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(pants.EnableWithMod)))
                        continue;

                    // save pants
                    pants.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/pants.png");
                    RegisterPants(contentPack.Manifest, pants);
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
                    if (recipe == null || (recipe.DisableWithMod != null && Helper.ModRegistry.IsLoaded(recipe.DisableWithMod)) || (recipe.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(recipe.EnableWithMod)))
                        continue;

                    RegisterTailoringRecipe(contentPack.Manifest, recipe);
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
                    if (boots == null || (boots.DisableWithMod != null && Helper.ModRegistry.IsLoaded(boots.DisableWithMod)) || (boots.EnableWithMod != null && !Helper.ModRegistry.IsLoaded(boots.EnableWithMod)))
                        continue;

                    boots.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/boots.png");
                    boots.textureColor = contentPack.LoadAsset<Texture2D>($"{relativePath}/color.png");
                    RegisterBoots(contentPack.Manifest, boots);
                }
            }
        }

        private void resetAtTitle()
        {
            didInit = false;
            // When we go back to the title menu we need to reset things so things don't break when
            // going back to a save.
            clearIds(out objectIds, objects.ToList<DataNeedsId>());
            clearIds(out cropIds, crops.ToList<DataNeedsId>());
            clearIds(out fruitTreeIds, fruitTrees.ToList<DataNeedsId>());
            clearIds(out bigCraftableIds, bigCraftables.ToList<DataNeedsId>());
            clearIds(out hatIds, hats.ToList<DataNeedsId>());
            clearIds(out weaponIds, weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new List<DataNeedsId>();
            clothing.AddRange(shirts);
            clothing.AddRange(pantss);
            clearIds(out clothingIds, clothing.ToList<DataNeedsId>());

            content1.InvalidateUsed();
            Helper.Content.AssetEditors.Remove(content2);
        }

        private void onCreated(object sender, SaveCreatedEventArgs e)
        {
            Log.debug("Loading stuff early (creation)");
            initStuff(loadIdFiles: false);

            api.InvokeIdsFixed();
        }

        private void onLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveParsed)
            {
                Log.debug("Loading stuff early (loading)");
                initStuff(loadIdFiles: true);
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveLoadedLocations)
            {
                Log.debug("Fixing IDs");
                fixIdsEverywhere();
            }
            else if (e.NewStage == StardewModdingAPI.Enums.LoadStage.Loaded)
            {
                Log.debug("Adding default/leveled recipes");
                foreach (var obj in objects)
                {
                    if (obj.Recipe != null)
                    {
                        bool unlockedByLevel = false;
                        if ( obj.Recipe.SkillUnlockName?.Length > 0 && obj.Recipe.SkillUnlockLevel > 0 )
                        {
                            int level = 0;
                            switch ( obj.Recipe.SkillUnlockName )
                            {
                                case "Farming": level = Game1.player.farmingLevel.Value; break;
                                case "Fishing": level = Game1.player.fishingLevel.Value; break;
                                case "Foraging": level = Game1.player.foragingLevel.Value; break;
                                case "Mining": level = Game1.player.miningLevel.Value; break;
                                case "Combat": level = Game1.player.combatLevel.Value; break;
                                case "Luck": level = Game1.player.luckLevel.Value; break;
                                default: level = Game1.player.GetCustomSkillLevel(obj.Recipe.SkillUnlockName); break;
                            }

                            if ( level >= obj.Recipe.SkillUnlockLevel )
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
                foreach (var big in bigCraftables)
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
            if (!Context.IsMainPlayer && !didInit)
            {
                Log.debug("Loading stuff early (MP client)");
                initStuff(loadIdFiles: false);
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null)
                return;

            if (e.NewMenu is TitleMenu)
            {
                resetAtTitle();
                return;
            }

            var menu = e.NewMenu as ShopMenu;
            bool hatMouse = menu != null && menu.potraitPersonDialogue == Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11494"), Game1.dialogueFont, Game1.tileSize * 5 - Game1.pixelZoom * 4);
            string portraitPerson = menu?.portraitPerson?.Name;
            if (portraitPerson == null && Game1.currentLocation?.Name == "Hospital")
                portraitPerson = "Harvey";
            if (menu == null || (portraitPerson == null || portraitPerson == "") && !hatMouse)
                return;

            //if (menu.portraitPerson.name == "Pierre")
            {
                Log.trace($"Adding objects to {portraitPerson}'s shop");

                var forSale = Helper.Reflection.GetField<List<ISalable>>(menu, "forSale").GetValue();
                var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<ISalable, int[]>>(menu, "itemPriceAndStock").GetValue();

                var precondMeth = Helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition");
                foreach (var obj in objects)
                {
                    if (obj.Recipe != null && obj.Recipe.CanPurchase)
                    {
                        bool add = true;
                        // Can't use continue here or the item might not sell
                        if (obj.Recipe.PurchaseFrom != portraitPerson || (obj.Recipe.PurchaseFrom == "HatMouse" && hatMouse))
                            add = false;
                        if (Game1.player.craftingRecipes.ContainsKey(obj.Name) || Game1.player.cookingRecipes.ContainsKey(obj.Name))
                            add = false;
                        if (obj.Recipe.PurchaseRequirements != null && obj.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { obj.Recipe.GetPurchaseRequirementString() }) == -1)
                            add = false;
                        if (add)
                        {
                            var recipeObj = new SObject(obj.id, 1, true, obj.Recipe.PurchasePrice, 0);
                            forSale.Add(recipeObj);
                            itemPriceAndStock.Add(recipeObj, new int[] { obj.Recipe.PurchasePrice, 1 });
                            Log.trace($"\tAdding recipe for {obj.Name}");
                        }
                    }
                    if (!obj.CanPurchase)
                        continue;
                    if (obj.PurchaseFrom != portraitPerson || (obj.PurchaseFrom == "HatMouse" && hatMouse))
                        continue;
                    if (obj.PurchaseRequirements != null && obj.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { obj.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new SObject(Vector2.Zero, obj.id, int.MaxValue);
                    forSale.Add(item);
                    int price = obj.PurchasePrice;
                    if (obj.Category == ObjectData.Category_.Seeds)
                    {
                        price = (int)(price * Game1.MasterPlayer.difficultyModifier);
                    }
                    itemPriceAndStock.Add(item, new int[] { price, int.MaxValue });
                    Log.trace($"\tAdding {obj.Name}");
                }
                foreach (var big in bigCraftables)
                {
                    if (big.Recipe != null && big.Recipe.CanPurchase)
                    {
                        bool add = true;
                        // Can't use continue here or the item might not sell
                        if (big.Recipe.PurchaseFrom != portraitPerson || (big.Recipe.PurchaseFrom == "HatMouse" && hatMouse))
                            add = false;
                        if (Game1.player.craftingRecipes.ContainsKey(big.Name) || Game1.player.cookingRecipes.ContainsKey(big.Name))
                            add = false;
                        if (big.Recipe.PurchaseRequirements != null && big.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { big.Recipe.GetPurchaseRequirementString() }) == -1)
                            add = false;
                        if (add)
                        {
                            var recipeObj = new SObject(new Vector2(0, 0), big.id, true);
                            forSale.Add(recipeObj);
                            itemPriceAndStock.Add(recipeObj, new int[] { big.Recipe.PurchasePrice, 1 });
                            Log.trace($"\tAdding recipe for {big.Name}");
                        }
                    }
                    if (!big.CanPurchase)
                        continue;
                    if (big.PurchaseFrom != portraitPerson || (big.PurchaseFrom == "HatMouse" && hatMouse))
                        continue;
                    if (big.PurchaseRequirements != null && big.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { big.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Log.trace($"\tAdding {big.Name}");
                    Item item = new SObject(Vector2.Zero, big.id, false);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { big.PurchasePrice, int.MaxValue });
                }
                if (hatMouse)
                {
                    foreach (var hat in hats)
                    {
                        Item item = new Hat(hat.GetHatId());
                        forSale.Add(item);
                        itemPriceAndStock.Add(item, new int[] { hat.PurchasePrice, int.MaxValue });
                        Log.trace($"\tAdding {hat.Name}");
                    }
                }
                foreach (var weapon in weapons)
                {
                    if (!weapon.CanPurchase)
                        continue;
                    if (weapon.PurchaseFrom != portraitPerson || (weapon.PurchaseFrom == "HatMouse" && hatMouse))
                        continue;
                    if (weapon.PurchaseRequirements != null && weapon.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { weapon.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new StardewValley.Tools.MeleeWeapon(weapon.id);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { weapon.PurchasePrice, int.MaxValue });
                    Log.trace($"\tAdding {weapon.Name}");
                }
                foreach (var boots in bootss)
                {
                    if (!boots.CanPurchase)
                        continue;
                    if (boots.PurchaseFrom != portraitPerson || (boots.PurchaseFrom == "HatMouse" && hatMouse))
                        continue;
                    if (boots.PurchaseRequirements != null && boots.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { boots.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new Boots(boots.id);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { boots.PurchasePrice, int.MaxValue });
                    Log.trace($"\tAdding {boots.Name}");
                }
            }

            ((Api)api).InvokeAddedItemsToShop();
        }

        internal bool didInit = false;
        private void initStuff(bool loadIdFiles)
        {
            if (didInit)
                return;
            didInit = true;

            // load object ID mappings from save folder
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
                oldObjectIds = LoadDictionary<string, int>("ids-objects.json") ?? new Dictionary<string, int>();
                oldCropIds = LoadDictionary<string, int>("ids-crops.json") ?? new Dictionary<string, int>();
                oldFruitTreeIds = LoadDictionary<string, int>("ids-fruittrees.json") ?? new Dictionary<string, int>();
                oldBigCraftableIds = LoadDictionary<string, int>("ids-big-craftables.json") ?? new Dictionary<string, int>();
                oldHatIds = LoadDictionary<string, int>("ids-hats.json") ?? new Dictionary<string, int>();
                oldWeaponIds = LoadDictionary<string, int>("ids-weapons.json") ?? new Dictionary<string, int>();
                oldClothingIds = LoadDictionary<string, int>("ids-clothing.json") ?? new Dictionary<string, int>();
                oldBootsIds = LoadDictionary<string, int>("ids-boots.json") ?? new Dictionary<string, int>();

                Log.verbose("OLD IDS START");
                foreach (var id in oldObjectIds)
                    Log.verbose("\tObject " + id.Key + " = " + id.Value);
                foreach (var id in oldCropIds)
                    Log.verbose("\tCrop " + id.Key + " = " + id.Value);
                foreach (var id in oldFruitTreeIds)
                    Log.verbose("\tFruit Tree " + id.Key + " = " + id.Value);
                foreach (var id in oldBigCraftableIds)
                    Log.verbose("\tBigCraftable " + id.Key + " = " + id.Value);
                foreach (var id in oldHatIds)
                    Log.verbose("\tHat " + id.Key + " = " + id.Value);
                foreach (var id in oldWeaponIds)
                    Log.verbose("\tWeapon " + id.Key + " = " + id.Value);
                foreach (var id in oldClothingIds)
                    Log.verbose("\tClothing " + id.Key + " = " + id.Value);
                foreach (var id in oldBootsIds)
                    Log.verbose("\tBoots " + id.Key + " = " + id.Value);
                Log.verbose("OLD IDS END");
            }

            // assign IDs
            var objList = new List<DataNeedsId>();
            objList.AddRange(objects.ToList<DataNeedsId>());
            objList.AddRange(bootss.ToList<DataNeedsId>());
            objectIds = AssignIds("objects", StartingObjectId, objList);
            cropIds = AssignIds("crops", StartingCropId, crops.ToList<DataNeedsId>());
            fruitTreeIds = AssignIds("fruittrees", StartingFruitTreeId, fruitTrees.ToList<DataNeedsId>());
            bigCraftableIds = AssignIds("big-craftables", StartingBigCraftableId, bigCraftables.ToList<DataNeedsId>());
            hatIds = AssignIds("hats", StartingHatId, hats.ToList<DataNeedsId>());
            weaponIds = AssignIds("weapons", StartingWeaponId, weapons.ToList<DataNeedsId>());
            List<DataNeedsId> clothing = new List<DataNeedsId>();
            clothing.AddRange(shirts);
            clothing.AddRange(pantss);
            clothingIds = AssignIds("clothing", StartingClothingId, clothing.ToList<DataNeedsId>());

            AssignTextureIndices("shirts", StartingShirtTextureIndex, shirts.ToList<DataSeparateTextureIndex>());
            AssignTextureIndices("pants", StartingPantsTextureIndex, pantss.ToList<DataSeparateTextureIndex>());
            AssignTextureIndices("boots", StartingBootsId, bootss.ToList<DataSeparateTextureIndex>());

            Log.trace("Resetting max shirt/pants value");
            Helper.Reflection.GetField<int>(typeof(Clothing), "_maxShirtValue").SetValue(-1);
            Helper.Reflection.GetField<int>(typeof(Clothing), "_maxPantsValue").SetValue(-1);

            api.InvokeIdsAssigned();

            content1.InvalidateUsed();
            Helper.Content.AssetEditors.Add(content2 = new ContentInjector2());
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaved(object sender, SavedEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            if (!Directory.Exists(Path.Combine(Constants.CurrentSavePath, "JsonAssets")))
                Directory.CreateDirectory(Path.Combine(Constants.CurrentSavePath, "JsonAssets"));

            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-objects.json"), JsonConvert.SerializeObject(objectIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-crops.json"), JsonConvert.SerializeObject(cropIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-fruittrees.json"), JsonConvert.SerializeObject(fruitTreeIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-big-craftables.json"), JsonConvert.SerializeObject(bigCraftableIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-hats.json"), JsonConvert.SerializeObject(hatIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-weapons.json"), JsonConvert.SerializeObject(weaponIds));
            File.WriteAllText(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-clothing.json"), JsonConvert.SerializeObject(clothingIds));
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
            foreach (var ring in myRings)
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

        internal const int StartingObjectId = 2000;
        internal const int StartingCropId = 100;
        internal const int StartingFruitTreeId = 10;
        internal const int StartingBigCraftableId = 300;
        internal const int StartingHatId = 80;
        internal const int StartingWeaponId = 64;
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
                if (objectIds.ContainsKey((string)data))
                    return objectIds[(string)data];

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
                if (clothingIds.ContainsKey((string)data))
                    return clothingIds[(string)data];

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

            int[] bigSkip = new int[] { 309, 310, 311, 326, 340, 434, 599, 621, 628, 629, 630, 631, 632, 633, 645 };

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

        private void AssignTextureIndices(string type, int starting, IList<DataSeparateTextureIndex> data)
        {
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

        private void fixIdsEverywhere()
        {
            origObjects = cloneIdDictAndRemoveOurs(Game1.objectInformation, objectIds);
            origCrops = cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\Crops"), cropIds);
            origFruitTrees = cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\fruitTrees"), fruitTreeIds);
            origBigCraftables = cloneIdDictAndRemoveOurs(Game1.bigCraftablesInformation, bigCraftableIds);
            origHats = cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\hats"), hatIds);
            origWeapons = cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\weapons"), weaponIds);
            origClothing = cloneIdDictAndRemoveOurs(Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"), clothingIds);

            fixItemList(Game1.player.Items);
#pragma warning disable AvoidNetField
            if (Game1.player.leftRing.Value != null && fixId(oldObjectIds, objectIds, Game1.player.leftRing.Value.parentSheetIndex, origObjects))
                Game1.player.leftRing.Value = null;
            if (Game1.player.rightRing.Value != null && fixId(oldObjectIds, objectIds, Game1.player.rightRing.Value.parentSheetIndex, origObjects))
                Game1.player.rightRing.Value = null;
            if (Game1.player.hat.Value != null && fixId(oldHatIds, hatIds, Game1.player.hat.Value.which, origHats))
                Game1.player.hat.Value = null;
            if (Game1.player.shirtItem.Value != null && fixId(oldClothingIds, clothingIds, Game1.player.shirtItem.Value.parentSheetIndex, origClothing))
                Game1.player.shirtItem.Value = null;
            if (Game1.player.pantsItem.Value != null && fixId(oldClothingIds, clothingIds, Game1.player.pantsItem.Value.parentSheetIndex, origClothing))
                Game1.player.pantsItem.Value = null;
            if (Game1.player.boots.Value != null && fixId(oldObjectIds, objectIds, Game1.player.boots.Value.indexInTileSheet, origObjects))
                Game1.player.boots.Value = null;
            /*else if (Game1.player.boots.Value != null)
                Game1.player.boots.Value.reloadData();*/
#pragma warning restore AvoidNetField
            foreach (var loc in Game1.locations)
                fixLocation(loc);

            fixIdDict(Game1.player.basicShipped, removeUnshippable: true);
            fixIdDict(Game1.player.mineralsFound);
            fixIdDict(Game1.player.recipesCooked);
            fixIdDict2(Game1.player.archaeologyFound);
            fixIdDict2(Game1.player.fishCaught);

            api.InvokeIdsFixed();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal bool fixItem(Item item)
        {
            if (item is Hat hat)
            {
                if (fixId(oldHatIds, hatIds, hat.which, origHats))
                    return true;
            }
            else if (item is MeleeWeapon weapon)
            {
                if (fixId(oldWeaponIds, weaponIds, weapon.initialParentTileIndex, origWeapons))
                    return true;
                else if (fixId(oldWeaponIds, weaponIds, weapon.currentParentTileIndex, origWeapons))
                    return true;
                else if (fixId(oldWeaponIds, weaponIds, weapon.currentParentTileIndex, origWeapons))
                    return true;
            }
            else if (item is Ring ring)
            {
                if (fixId(oldObjectIds, objectIds, ring.indexInTileSheet, origObjects))
                    return true;
            }
            else if (item is Clothing clothing)
            {
                if (fixId(oldClothingIds, clothingIds, clothing.parentSheetIndex, origClothing))
                    return true;
            }
            else if (item is Boots boots)
            {
                if (fixId(oldObjectIds, objectIds, boots.parentSheetIndex, origObjects))
                    return true;
                /*else
                    boots.reloadData();*/
            }
            else if (!(item is StardewValley.Object))
                return false;
            var obj = item as StardewValley.Object;

            if (obj is Chest chest)
            {
                fixItemList(chest.items);
            }
            else if (obj is IndoorPot pot)
            {
                var hd = pot.hoeDirt.Value;
                if (hd == null || hd.crop == null)
                    return false;

                var oldId = hd.crop.rowInSpriteSheet.Value;
                if (fixId(oldCropIds, cropIds, hd.crop.rowInSpriteSheet, origCrops))
                    hd.crop = null;
                else
                {
                    var key = cropIds.FirstOrDefault(x => x.Value == hd.crop.rowInSpriteSheet.Value).Key;
                    var c = crops.FirstOrDefault(x => x.Name == key);
                    if (c != null) // Non-JA crop
                    {
                        Log.verbose("Fixing crop product: From " + hd.crop.indexOfHarvest.Value + " to " + c.Product + "=" + ResolveObjectId(c.Product));
                        hd.crop.indexOfHarvest.Value = ResolveObjectId(c.Product);
                        fixId(oldObjectIds, objectIds, hd.crop.netSeedIndex, origObjects);
                    }
                }
            }
            else
            {
                if (!obj.bigCraftable.Value)
                {
                    if (fixId(oldObjectIds, objectIds, obj.preservedParentSheetIndex, origObjects))
                        obj.preservedParentSheetIndex.Value = -1;
                    if (fixId(oldObjectIds, objectIds, obj.parentSheetIndex, origObjects))
                        return true;
                }
                else
                {
                    if (fixId(oldBigCraftableIds, bigCraftableIds, obj.parentSheetIndex, origBigCraftables))
                        return true;
                }
            }

            if (obj.heldObject.Value != null)
            {
                if (fixId(oldObjectIds, objectIds, obj.heldObject.Value.parentSheetIndex, origObjects))
                    obj.heldObject.Value = null;

                if (obj.heldObject.Value is Chest chest2)
                {
                    fixItemList(chest2.items);
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        internal void fixLocation(GameLocation loc)
        {
            if (loc is FarmHouse fh)
            {
#pragma warning disable AvoidImplicitNetFieldCast
                if (fh.fridge.Value?.items != null)
#pragma warning restore AvoidImplicitNetFieldCast
                    fixItemList(fh.fridge.Value.items);
            }
            if ( loc is Cabin cabin )
            {
                var player = cabin.farmhand.Value;
                fixItemList(player.Items);
#pragma warning disable AvoidNetField
                if (player.leftRing.Value != null && fixId(oldObjectIds, objectIds, player.leftRing.Value.parentSheetIndex, origObjects))
                    player.leftRing.Value = null;
                if (player.rightRing.Value != null && fixId(oldObjectIds, objectIds, player.rightRing.Value.parentSheetIndex, origObjects))
                    player.rightRing.Value = null;
                if (player.hat.Value != null && fixId(oldHatIds, hatIds, player.hat.Value.which, origHats))
                    player.hat.Value = null;
                if (player.shirtItem.Value != null && fixId(oldClothingIds, clothingIds, player.shirtItem.Value.parentSheetIndex, origClothing))
                    player.shirtItem.Value = null;
                if (player.pantsItem.Value != null && fixId(oldClothingIds, clothingIds, player.pantsItem.Value.parentSheetIndex, origClothing))
                    player.pantsItem.Value = null;
                if (player.boots.Value != null && fixId(oldObjectIds, objectIds, player.boots.Value.parentSheetIndex, origObjects))
                    player.boots.Value = null;
                /*else if (player.boots.Value != null)
                    player.boots.Value.reloadData();*/
#pragma warning restore AvoidNetField
            }

            IList<Vector2> toRemove = new List<Vector2>();
            foreach (var tfk in loc.terrainFeatures.Keys)
            {
                var tf = loc.terrainFeatures[tfk];
                if (tf is HoeDirt hd)
                {
                    if (hd.crop == null)
                        continue;

                    var oldId = hd.crop.rowInSpriteSheet.Value;
                    if (fixId(oldCropIds, cropIds, hd.crop.rowInSpriteSheet, origCrops))
                        hd.crop = null;
                    else
                    {
                        var key = cropIds.FirstOrDefault(x => x.Value == hd.crop.rowInSpriteSheet.Value).Key;
                        var c = crops.FirstOrDefault(x => x.Name == key);
                        if (c != null) // Non-JA crop
                        {
                            Log.verbose("Fixing crop product: From " + hd.crop.indexOfHarvest.Value + " to " + c.Product + "=" + ResolveObjectId(c.Product));
                            hd.crop.indexOfHarvest.Value = ResolveObjectId(c.Product);
                            fixId(oldObjectIds, objectIds, hd.crop.netSeedIndex, origObjects);
                        }
                    }
                }
                else if (tf is FruitTree ft)
                {
                    var oldId = ft.treeType.Value;
                    if (fixId(oldFruitTreeIds, fruitTreeIds, ft.treeType, origFruitTrees))
                        toRemove.Add(tfk);
                    else
                    {
                        var key = fruitTreeIds.FirstOrDefault(x => x.Value == ft.treeType.Value).Key;
                        var ftt = fruitTrees.FirstOrDefault(x => x.Name == key);
                        if (ftt != null) // Non-JA fruit tree
                        {
                            Log.verbose("Fixing fruit tree product: From " + ft.indexOfFruit.Value + " to " + ftt.Product + "=" + ResolveObjectId(ftt.Product));
                            ft.indexOfFruit.Value = ResolveObjectId(ftt.Product);
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
                if ( fixItem(obj) )
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
                    fixItemList(chest.items);
                }
                else if (obj is Sign sign)
                {
                    if (!fixItem(sign.displayItem.Value))
                        sign.displayItem.Value = null;
                }
                else
                {
                    if (!obj.bigCraftable.Value)
                    {
                        if (fixId(oldObjectIds, objectIds, obj.parentSheetIndex, origObjects))
                            toRemove.Add(objk);
                    }
                    else
                    {
                        if (fixId(oldBigCraftableIds, bigCraftableIds, obj.parentSheetIndex, origBigCraftables))
                            toRemove.Add(objk);
                    }
                }

                if (obj.heldObject.Value != null)
                {
                    if (fixId(oldObjectIds, objectIds, obj.heldObject.Value.parentSheetIndex, origObjects))
                        obj.heldObject.Value = null;

                    if (obj.heldObject.Value is Chest chest2)
                    {
                        fixItemList(chest2.items);
                    }
                }
            }
            foreach (var rem in toRemove)
                loc.overlayObjects.Remove(rem);

            if (loc is BuildableGameLocation buildLoc)
                foreach (var building in buildLoc.buildings)
                {
                    if (building.indoors.Value != null)
                        fixLocation(building.indoors.Value);
                    if (building is Mill mill)
                    {
                        fixItemList(mill.input.Value.items);
                        fixItemList(mill.output.Value.items);
                    }
                    else if (building is FishPond pond)
                    {
                        if (pond.fishType.Value == -1)
                        {
                            Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                            continue;
                        }

                        if (fixId(oldObjectIds, objectIds, pond.fishType, origObjects))
                        {
                            pond.fishType.Value = -1;
                            pond.currentOccupants.Value = 0;
                            pond.maxOccupants.Value = 0;
                            Helper.Reflection.GetField<SObject>(pond, "_fishObject").SetValue(null);
                        }
                        if (pond.sign.Value != null && fixId(oldObjectIds, objectIds, pond.sign.Value.parentSheetIndex, origObjects))
                            pond.sign.Value = null;
                        if (pond.output.Value != null && fixId(oldObjectIds, objectIds, pond.output.Value.parentSheetIndex, origObjects))
                            pond.output.Value = null;
                        if (pond.neededItem.Value != null && fixId(oldObjectIds, objectIds, pond.neededItem.Value.parentSheetIndex, origObjects))
                            pond.neededItem.Value = null;
                    }
                }

            if (loc is DecoratableLocation decoLoc)
                foreach (var furniture in decoLoc.furniture)
                {
                    if (furniture.heldObject.Value != null)
                    {
                        if (!furniture.heldObject.Value.bigCraftable.Value)
                        {
                            if (fixId(oldObjectIds, objectIds, furniture.heldObject.Value.parentSheetIndex, origObjects))
                                furniture.heldObject.Value = null;
                        }
                        else
                        {
                            if (fixId(oldBigCraftableIds, bigCraftableIds, furniture.heldObject.Value.parentSheetIndex, origBigCraftables))
                                furniture.heldObject.Value = null;
                        }
                    }
                    if (furniture is StorageFurniture storage)
                        fixItemList(storage.heldItems);
                }

            if (loc is Farm farm)
            {
                foreach (var animal in farm.Animals.Values)
                {
                    if (animal.currentProduce.Value != -1)
                        if (fixId(oldObjectIds, objectIds, animal.currentProduce, origObjects))
                            animal.currentProduce.Value = -1;
                    if (animal.defaultProduceIndex.Value != -1)
                        if (fixId(oldObjectIds, objectIds, animal.defaultProduceIndex, origObjects))
                            animal.defaultProduceIndex.Value = 0;
                    if (animal.deluxeProduceIndex.Value != -1)
                        if (fixId(oldObjectIds, objectIds, animal.deluxeProduceIndex, origObjects))
                            animal.deluxeProduceIndex.Value = 0;
                }

                var clumpsToRemove = new List<ResourceClump>();
                foreach ( var clump in farm.resourceClumps )
                {
                    if (fixId(oldObjectIds, objectIds, clump.parentSheetIndex, origObjects))
                        clumpsToRemove.Add(clump);
                }
                foreach ( var clump in clumpsToRemove )
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
                if (item is SObject obj)
                {
                    if (!obj.bigCraftable.Value)
                    {
                        if (fixId(oldObjectIds, objectIds, obj.parentSheetIndex, origObjects))
                            items[i] = null;
                    }
                    else
                    {
                        if (fixId(oldBigCraftableIds, bigCraftableIds, obj.parentSheetIndex, origBigCraftables))
                            items[i] = null;
                    }
                }
                else if (item is Hat hat)
                {
                    if (fixId(oldHatIds, hatIds, hat.which, origHats))
                        items[i] = null;
                }
                else if (item is MeleeWeapon weapon)
                {
                    if (fixId(oldWeaponIds, weaponIds, weapon.initialParentTileIndex, origWeapons))
                        items[i] = null;
                    else if (fixId(oldWeaponIds, weaponIds, weapon.currentParentTileIndex, origWeapons))
                        items[i] = null;
                    else if (fixId(oldWeaponIds, weaponIds, weapon.currentParentTileIndex, origWeapons))
                        items[i] = null;
                }
                else if (item is Ring ring)
                {
                    if (fixId(oldObjectIds, objectIds, ring.indexInTileSheet, origObjects))
                        items[i] = null;
                }
                else if (item is Clothing clothing)
                {
                    if (fixId(oldClothingIds, clothingIds, clothing.parentSheetIndex, origClothing))
                        items[i] = null;
                }
                else if (item is Boots boots)
                {
                    if (fixId(oldObjectIds, objectIds, boots.parentSheetIndex, origObjects))
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
            foreach (var entry in dict.Keys)
            {
                if (origObjects.ContainsKey(entry))
                    continue;
                else if (oldObjectIds.Values.Contains(entry))
                {
                    var key = oldObjectIds.FirstOrDefault(x => x.Value == entry).Key;
                    bool isRing = myRings.FirstOrDefault(r => r.id == entry) != null;
                    bool canShip = objects.FirstOrDefault(o => o.id == entry)?.CanSell ?? true;

                    toRemove.Add(entry);
                    if (objectIds.ContainsKey(key))
                    {
                        if (removeUnshippable && (!canShip || isRing))
                            ;// Log.warn("Found unshippable");
                        else
                            toAdd.Add(objectIds[key], dict[entry]);
                    }
                }
            }
            foreach (var entry in toRemove)
                dict.Remove(entry);
            foreach (var entry in toAdd)
                dict.Add(entry.Key, entry.Value);
        }

        private void fixIdDict2(NetIntIntArrayDictionary dict)
        {
            var toRemove = new List<int>();
            var toAdd = new Dictionary<int, int[]>();
            foreach (var entry in dict.Keys)
            {
                if (origObjects.ContainsKey(entry))
                    continue;
                else if (oldObjectIds.Values.Contains(entry))
                {
                    var key = oldObjectIds.FirstOrDefault(x => x.Value == entry).Key;

                    toRemove.Add(entry);
                    if (objectIds.ContainsKey(key))
                    {
                        toAdd.Add(objectIds[key], dict[entry]);
                    }
                }
            }
            foreach (var entry in toRemove)
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

            if (oldIds.Values.Contains(id.Value))
            {
                int id_ = id.Value;
                var key = oldIds.FirstOrDefault(x => x.Value == id_).Key;

                if (newIds.ContainsKey(key))
                {
                    id.Value = newIds[key];
                    Log.verbose("Changing ID: " + key + " from ID " + id_ + " to " + id.Value);
                    return false;
                }
                else
                {
                    Log.verbose("Deleting missing item " + key + " with old ID " + id_);
                    return true;
                }
            }
            else return false;
        }
    }
}
