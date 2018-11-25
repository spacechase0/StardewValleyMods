using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using System.Reflection;
using Netcode;
using StardewValley.Buildings;
using Harmony;
using System.Text.RegularExpressions;
using JsonAssets.Overrides;

// TODO: Refactor recipes

namespace JsonAssets
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private HarmonyInstance harmony;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            MenuEvents.MenuChanged += menuChanged;
            //SaveEvents.AfterLoad += afterLoad;
            SaveEvents.AfterSave += afterSave;
            PlayerEvents.InventoryChanged += invChanged;
            //SpecialisedEvents.UnvalidatedUpdateTick += unsafeUpdate;

            Log.info("Loading content packs...");
            foreach (IContentPack contentPack in this.Helper.GetContentPacks())
                loadData(contentPack);
            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
            {
                foreach (string dir in Directory.EnumerateDirectories(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
                    loadData(dir);
            }

            resetAtTitle();

            try
            {
                harmony = HarmonyInstance.Create("spacechase0.JsonAssets");
                doPrefix(typeof(StardewValley.Object), "canBePlacedHere", typeof(ObjectCanPlantHereOverride));
                doPrefix(typeof(StardewValley.Object), "checkForAction", typeof(ObjectNoActionHook));
            }
            catch (Exception e)
            {
                Log.error("Exception doing harmony stuff: " + e);
            }
        }
        private void doPrefix(Type origType, string origMethod, Type newType)
        {
            doPrefix(origType.GetMethod(origMethod), newType.GetMethod("Prefix"));
        }
        private void doPrefix(MethodInfo orig, MethodInfo prefix)
        {
            try
            {
                Log.trace($"Doing prefix patch {orig}:{prefix}...");
                harmony.Patch(orig, new HarmonyMethod(prefix), null);
            }
            catch (Exception e)
            {
                Log.error($"Exception doing prefix patch {orig}:{prefix}: {e}");
            }
        }

        private IApi api;
        public override object GetApi()
        {
            if (api == null)
                api = new Api(this.loadData);

            return api;
        }

        private void loadData(string dir)
        {
            // read info
            if (!File.Exists(Path.Combine(dir, "content-pack.json")))
            {
                Log.warn($"\tNo {dir}/content-pack.json!");
                return;
            }
            ContentPackData info = this.Helper.ReadJsonFile<ContentPackData>(Path.Combine(dir, "content-pack.json"));

            // load content pack
            IContentPack contentPack = this.Helper.CreateTransitionalContentPack(dir, id: Guid.NewGuid().ToString("N"), name: info.Name, description: info.Description, author: info.Author, version: new SemanticVersion(info.Version));
            this.loadData(contentPack);
        }

        private Regex SeasonLimiter = new Regex("(z(?: spring| summer| fall| winter){2,4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
                    if (obj == null)
                        continue;

                    // save object
                    obj.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/object.png");
                    if (obj.IsColored)
                        obj.textureColor = contentPack.LoadAsset<Texture2D>($"{relativePath}/color.png");
                    this.objects.Add(obj);

                    // save ring
                    if (obj.Category == ObjectData.Category_.Ring)
                        this.myRings.Add(obj);
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
                    if (crop == null)
                        continue;

                    // save crop
                    crop.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/crop.png");
                    crops.Add(crop);

                    // save seeds
                    crop.seed = new ObjectData
                    {
                        texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/seeds.png"),
                        Name = crop.SeedName,
                        Description = crop.SeedDescription,
                        Category = ObjectData.Category_.Seeds,
                        Price = crop.SeedPurchasePrice,
                        CanPurchase = true,
                        PurchaseFrom = crop.SeedPurchaseFrom,
                        PurchasePrice = crop.SeedPurchasePrice,
                        PurchaseRequirements = crop.SeedPurchaseRequirements ?? new List<string>()
                    };

                    // TODO: Clean up this chunk
                    // I copy/pasted it from the unofficial update decompiled
                    string str = "";
                    string[] array = new string[]
                    {
                            "spring",
                            "summer",
                            "fall",
                            "winter"
                    }.Except(crop.Seasons).ToArray<string>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        string season = array[i];
                        str += string.Format("/z {0}", season);
                    }
                    string strtrimstart = str.TrimStart(new char[] { '/' });
                    if (crop.SeedPurchaseRequirements != null && crop.SeedPurchaseRequirements.Count > 0)
                    {
                        for (int index = 0; index < crop.SeedPurchaseRequirements.Count; index++)
                        {
                            if (SeasonLimiter.IsMatch(crop.SeedPurchaseRequirements[index]))
                            {
                                crop.SeedPurchaseRequirements[index] = strtrimstart;
                                Log.warn(string.Format("        Faulty season requirements for {0}!\n", crop.SeedName) + string.Format("        Fixed season requirements: {0}", crop.SeedPurchaseRequirements[index]));
                            }
                        }
                        if (!crop.SeedPurchaseRequirements.Contains(str.TrimStart(new char[] { '/' })))
                        {
                            Log.trace(string.Format("        Adding season requirements for {0}:\n", crop.SeedName) + string.Format("        New season requirements: {0}", strtrimstart));
                            crop.seed.PurchaseRequirements.Add(strtrimstart);
                        }
                    }
                    else
                    {
                        Log.trace(string.Format("        Adding season requirements for {0}:\n", crop.SeedName) + string.Format("        New season requirements: {0}", strtrimstart));
                        crop.seed.PurchaseRequirements.Add(strtrimstart);
                    }

                    objects.Add(crop.seed);
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
                    if (tree == null)
                        continue;

                    // save fruit tree
                    tree.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/tree.png");
                    fruitTrees.Add(tree);

                    // save seed
                    tree.sapling = new ObjectData
                    {
                        texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/sapling.png"),
                        Name = tree.SaplingName,
                        Description = tree.SaplingDescription,
                        Category = ObjectData.Category_.Seeds,
                        Price = tree.SaplingPurchasePrice,
                        CanPurchase = true,
                        PurchaseRequirements = tree.SaplingPurchaseRequirements,
                        PurchaseFrom = tree.SaplingPurchaseFrom,
                        PurchasePrice = tree.SaplingPurchasePrice
                    };
                    objects.Add(tree.sapling);
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
                    if (craftable == null)
                        continue;

                    // save craftable
                    craftable.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/big-craftable.png");
                    bigCraftables.Add(craftable);
                }
            }

            // load objects
            DirectoryInfo hatsDir = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Hats"));
            if (hatsDir.Exists)
            {
                foreach (DirectoryInfo dir in hatsDir.EnumerateDirectories())
                {
                    string relativePath = $"Hats/{dir.Name}";

                    // load data
                    HatData hat = contentPack.ReadJsonFile<HatData>($"{relativePath}/hat.json");
                    if (hat == null)
                        continue;

                    // save object
                    hat.texture = contentPack.LoadAsset<Texture2D>($"{relativePath}/hat.png");
                    hats.Add(hat);
                }
            }
        }

        private void unsafeUpdate(object sender, EventArgs args)
        {
            // I need the items to register before most other things do
            var saveLoaded = (bool) typeof(Context).GetProperty("IsSaveLoaded", BindingFlags.NonPublic | BindingFlags.Static).GetValue( null );
            if (saveLoaded && !SaveGame.IsProcessing)
            {
                Log.debug("Loading stuff early");
                afterLoad(sender, args);
                SpecialisedEvents.UnvalidatedUpdateTick -= unsafeUpdate;
            }
        }

        private void resetAtTitle()
        {
            // When we go back to the title menu we need to reset things so things don't break when
            // going back to a save. Also, this is where it is initially done, too.
            oldObjectIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath, $"ids-objects.json"));
            oldCropIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath, $"ids-crops.json"));
            oldFruitTreeIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath, $"ids-fruittrees.json"));
            oldBigCraftableIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath, $"ids-big-craftables.json"));
            oldHatIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath, $"ids-hats.json"));

            if (objectIds != null)
            {
                clearIds(ref objectIds, objects.ToList<DataNeedsId>());
                clearIds(ref cropIds, crops.ToList<DataNeedsId>());
                clearIds(ref fruitTreeIds, fruitTrees.ToList<DataNeedsId>());
                clearIds(ref bigCraftableIds, bigCraftables.ToList<DataNeedsId>());
                clearIds(ref hatIds, hats.ToList<DataNeedsId>());
            }

            var editor = Helper.Content.AssetEditors.Where(x => x is ContentInjector);
            if (editor.Count() > 0)
                Helper.Content.AssetEditors.Remove(editor.ElementAt(0));

            SpecialisedEvents.UnvalidatedUpdateTick += unsafeUpdate;
        }

        private void menuChanged(object sender, EventArgsClickableMenuChanged args)
        {
            if ( args.NewMenu is TitleMenu )
            {
                resetAtTitle();
                return;
            }

            var menu = args.NewMenu as ShopMenu;
            bool hatMouse = false;
            if (menu != null && menu.potraitPersonDialogue == Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11494"), Game1.dialogueFont, Game1.tileSize * 5 - Game1.pixelZoom * 4))
                hatMouse = true;
            if (menu == null || menu.portraitPerson == null && !hatMouse)
                return;

            //if (menu.portraitPerson.name == "Pierre")
            {
                Log.trace($"Adding objects to {menu.portraitPerson?.Name}'s shop");

                var forSale = Helper.Reflection.GetField<List<Item>>(menu, "forSale").GetValue();
                var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();

                var precondMeth = Helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition");
                foreach (var obj in objects)
                {
                    if (obj.Recipe != null && obj.Recipe.CanPurchase)
                    {
                        bool add = true;
                        // Can't use continue here or the item might not sell
                        if (obj.Recipe.PurchaseFrom != menu.portraitPerson?.Name || (obj.Recipe.PurchaseFrom == "HatMouse" && hatMouse) )
                            add = false;
                        if (Game1.player.craftingRecipes.ContainsKey(obj.Name) || Game1.player.cookingRecipes.ContainsKey(obj.Name))
                            add = false;
                        if (obj.Recipe.PurchaseRequirements != null && obj.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { obj.Recipe.GetPurchaseRequirementString() }) == -1)
                            add = false;
                        if (add)
                        {
                            var recipeObj = new StardewValley.Object(obj.id, 1, true, obj.Recipe.PurchasePrice, 0);
                            forSale.Add(recipeObj);
                            itemPriceAndStock.Add(recipeObj, new int[] { obj.Recipe.PurchasePrice, 1 });
                            Log.trace($"\tAdding recipe for {obj.Name}");
                        }
                    }
                    if (!obj.CanPurchase)
                        continue;
                    if (obj.PurchaseFrom != menu.portraitPerson?.Name || (obj.PurchaseFrom == "HatMouse" && hatMouse))
                        continue;
                    if (obj.PurchaseRequirements != null && obj.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { obj.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new StardewValley.Object(Vector2.Zero, obj.id, int.MaxValue);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { obj.PurchasePrice, int.MaxValue });
                    Log.trace($"\tAdding {obj.Name}");
                }
                foreach (var big in bigCraftables)
                {
                    if (big.Recipe != null && big.Recipe.CanPurchase)
                    {
                        bool add = true;
                        // Can't use continue here or the item might not sell
                        if (big.Recipe.PurchaseFrom != menu.portraitPerson?.Name || (big.Recipe.PurchaseFrom == "HatMouse" && hatMouse))
                            add = false;
                        if (Game1.player.craftingRecipes.ContainsKey(big.Name) || Game1.player.cookingRecipes.ContainsKey(big.Name))
                            add = false;
                        if (big.Recipe.PurchaseRequirements != null && big.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { big.Recipe.GetPurchaseRequirementString() }) == -1)
                            add = false;
                        if (add)
                        {
                            var recipeObj = new StardewValley.Object(new Vector2(0, 0), big.id, true);
                            forSale.Add(recipeObj);
                            itemPriceAndStock.Add(recipeObj, new int[] { big.Recipe.PurchasePrice, 1 });
                            Log.trace($"\tAdding recipe for {big.Name}");
                        }
                    }
                    if (!big.CanPurchase)
                        continue;
                    if (big.PurchaseFrom != menu.portraitPerson?.Name || (big.PurchaseFrom == "HatMouse" && hatMouse))
                        continue;
                    if (big.PurchaseRequirements != null && big.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { big.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new StardewValley.Object(Vector2.Zero, big.id, false);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { big.PurchasePrice, int.MaxValue });
                    Log.trace($"\tAdding {big.Name}");
                }
                if ( hatMouse )
                {
                    foreach ( var hat in hats )
                    {
                        Item item = new Hat(hat.GetHatId());
                        forSale.Add(item);
                        itemPriceAndStock.Add(item, new int[] { hat.PurchasePrice, int.MaxValue });
                        Log.trace($"\tAdding {hat.Name}");
                    }
                }
            }

            ( ( Api ) api ).InvokeAddedItemsToShop();
        }

        private void afterLoad(object sender, EventArgs args)
        {
            if (File.Exists(Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-objects.json")))
            {
                oldObjectIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-objects.json"));
                oldCropIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-crops.json"));
                oldFruitTreeIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-fruittrees.json"));
                oldBigCraftableIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-big-craftables.json"));
                oldHatIds = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-hats.json"));
            }
            else
                Directory.CreateDirectory(Path.Combine(Constants.CurrentSavePath, "JsonAssets"));

            if (oldObjectIds == null)
            {
                oldObjectIds = new Dictionary<string, int>();
                oldCropIds = new Dictionary<string, int>();
                oldFruitTreeIds = new Dictionary<string, int>();
                oldBigCraftableIds = new Dictionary<string, int>();
                oldHatIds = new Dictionary<string, int>();
            }

            objectIds = AssignIds("objects", StartingObjectId, objects.ToList<DataNeedsId>());
            cropIds = AssignIds("crops", StartingCropId, crops.ToList<DataNeedsId>());
            fruitTreeIds = AssignIds("fruittrees", StartingFruitTreeId, fruitTrees.ToList<DataNeedsId>());
            bigCraftableIds = AssignIds("big-craftables", StartingBigCraftableId, bigCraftables.ToList<DataNeedsId>());
            hatIds = AssignIds("hats", StartingHatId, hats.ToList<DataNeedsId>());

            fixIdsEverywhere();
            (api as Api).InvokeIdsAssigned();

            Helper.Content.AssetEditors.Add(new ContentInjector());

            foreach (var obj in objects)
            {
                if (obj.Recipe != null && obj.Recipe.IsDefault && !Game1.player.knowsRecipe(obj.Name))
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
            foreach (var big in bigCraftables)
            {
                if (big.Recipe != null && big.Recipe.IsDefault && !Game1.player.knowsRecipe(big.Name))
                {
                    Game1.player.craftingRecipes.Add(big.Name, 0);
                }
            }
        }

        private void afterSave(object sender, EventArgs args)
        {
            Helper.WriteJsonFile(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-objects.json"), objectIds);
            Helper.WriteJsonFile(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-crops.json"), cropIds);
            Helper.WriteJsonFile(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-fruittrees.json"), fruitTreeIds);
            Helper.WriteJsonFile(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-big-craftables.json"), bigCraftableIds);
            Helper.WriteJsonFile(Path.Combine(Constants.CurrentSavePath, "JsonAssets", $"ids-hats.json"), hatIds);
        }

        private IList<ObjectData> myRings = new List<ObjectData>();
        private void invChanged(object sender, EventArgsInventoryChanged args)
        {
            IList<int> ringIds = new List<int>();
            foreach (var ring in myRings)
                ringIds.Add(ring.id);

            for (int i = 0; i < Game1.player.Items.Count; ++i)
            {
                var item = Game1.player.Items[i];
                if (item is StardewValley.Object obj && ringIds.Contains(obj.ParentSheetIndex))
                {
                    Log.trace($"Turning a ring-object of {obj.ParentSheetIndex} into a proper ring");
                    Game1.player.Items[i] = new StardewValley.Objects.Ring(obj.ParentSheetIndex);
                }
            }
        }

        private const int StartingObjectId = 2000;
        private const int StartingCropId = 100;
        private const int StartingFruitTreeId = 10;
        private const int StartingBigCraftableId = 300;
        private const int StartingHatId = 50;

        internal IList<ObjectData> objects = new List<ObjectData>();
        internal IList<CropData> crops = new List<CropData>();
        internal IList<FruitTreeData> fruitTrees = new List<FruitTreeData>();
        internal IList<BigCraftableData> bigCraftables = new List<BigCraftableData>();
        internal IList<HatData> hats = new List<HatData>();

        internal IDictionary<string, int> objectIds;
        internal IDictionary<string, int> cropIds;
        internal IDictionary<string, int> fruitTreeIds;
        internal IDictionary<string, int> bigCraftableIds;
        internal IDictionary<string, int> hatIds;

        internal IDictionary<string, int> oldObjectIds;
        internal IDictionary<string, int> oldCropIds;
        internal IDictionary<string, int> oldFruitTreeIds;
        internal IDictionary<string, int> oldBigCraftableIds;
        internal IDictionary<string, int> oldHatIds;

        internal IDictionary<int, string> origObjects;
        internal IDictionary<int, string> origCrops;
        internal IDictionary<int, string> origFruitTrees;
        internal IDictionary<int, string> origBigCraftables;
        internal IDictionary<int, string> origHats;

        public int ResolveObjectId(object data)
        {
            if (data.GetType() == typeof(long))
                return (int)(long)data;
            else
            {
                if (objectIds.ContainsKey((string)data))
                    return objectIds[(string)data];

                foreach ( var obj in Game1.objectInformation )
                {
                    if (obj.Value.Split('/')[0] == (string)data)
                        return obj.Key;
                }

                Log.warn("No idea what '" + data + "' is!");
                return 0;
            }
        }

        private Dictionary<string, int> AssignIds(string type, int starting, IList<DataNeedsId> data)
        {
            Dictionary<string, int> ids = new Dictionary<string, int>();

            int currId = starting;
            foreach (var d in data)
            {
                if (d.id == -1)
                {
                    Log.trace("New ID: " + d.Name + " = " + currId);
                    ids.Add(d.Name, currId++);
                    if (type == "objects" && ((ObjectData)d).IsColored)
                        ++currId;
                    d.id = ids[d.Name];
                }
            }

            return ids;
        }

        private void clearIds(ref IDictionary<string, int> ids, List<DataNeedsId> objs)
        {
            ids = null;
            foreach ( DataNeedsId obj in objs )
            {
                obj.id = -1;
            }
        }

        private IDictionary<int, string> cloneIdDictAndRemoveOurs( IDictionary<int, string> full, IDictionary<string, int> ours )
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

            fixItemList(Game1.player.Items);
            foreach ( var loc in Game1.locations )
                fixLocation(loc);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "SMAPI.CommonErrors", "AvoidNetField") ]
        private void fixLocation( GameLocation loc )
        {
            if (loc is FarmHouse fh)
            {
#pragma warning disable AvoidImplicitNetFieldCast
                if (fh.fridge.Value != null && fh.fridge.Value.items != null)
#pragma warning restore AvoidImplicitNetFieldCast
                    fixItemList(fh.fridge.Value.items);
            }

            IList<Vector2> toRemove = new List<Vector2>();
            foreach ( var tfk in loc.terrainFeatures.Keys )
            {
                var tf = loc.terrainFeatures[tfk];
                if ( tf is HoeDirt hd )
                {
                    if (hd.crop == null)
                        continue;

                    if (fixId(oldCropIds, cropIds, hd.crop.rowInSpriteSheet, origCrops))
                        hd.crop = null;
                    else
                    {
                        var key = cropIds.FirstOrDefault(x => x.Value == hd.crop.rowInSpriteSheet.Value).Key;
                        var c = crops.FirstOrDefault(x => x.Name == key);
                        if ( c != null ) // Non-JA crop
                            hd.crop.indexOfHarvest.Value = ResolveObjectId(c.Product);
                    }
                }
                else if ( tf is FruitTree ft )
                {
                    if (fixId(oldFruitTreeIds, fruitTreeIds, ft.treeType, origFruitTrees))
                        toRemove.Add(tfk);
                    else
                    {
                        var key = oldFruitTreeIds.FirstOrDefault(x => x.Value == ft.treeType.Value).Key;
                        var ftt = fruitTrees.FirstOrDefault(x => x.Name == key);
                        if ( ftt != null ) // Non-JA fruit tree
                            ft.indexOfFruit.Value = ResolveObjectId(ftt.Product);
                    }
                }
            }
            foreach (var rem in toRemove)
                loc.terrainFeatures.Remove(rem);

            toRemove.Clear();
            foreach ( var objk in loc.netObjects.Keys )
            {
                var obj = loc.netObjects[objk];
                if ( obj is Chest chest )
                {
                    fixItemList(chest.items);
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
                
                if ( obj.heldObject.Value != null )
                {
                    if (fixId(oldObjectIds, objectIds, obj.heldObject.Value.parentSheetIndex, origObjects))
                        obj.heldObject.Value = null;

                    if ( obj.heldObject.Value is Chest chest2 )
                    {
                        fixItemList(chest2.items);
                    }
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
                    if ( building is Mill mill )
                    {
                        fixItemList(mill.input.Value.items);
                        fixItemList(mill.output.Value.items);
                    }
                }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField")]
        private void fixItemList( IList< Item > items )
        {
            for ( int i = 0; i < items.Count; ++i )
            {
                var item = items[i];
                if ( item is StardewValley.Object obj )
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
                else if ( item is Hat hat )
                {
                    if (fixId(oldHatIds, hatIds, hat.which, origHats))
                        items[i] = null;
                }
            }
        }

        // Return true if the item should be deleted, false otherwise.
        // Only remove something if old has it but not new
        private bool fixId(IDictionary<string, int> oldIds, IDictionary<string, int> newIds, NetInt id, IDictionary<int, string> origData )
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
                    return false;
                }
                else return true;
            }
            else return false;
        }
    }
}
