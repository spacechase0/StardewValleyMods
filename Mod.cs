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

// TODO: Refactor recipes
// TODO: Handle recipe.IsDefault

namespace JsonAssets
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            MenuEvents.MenuChanged += menuChanged;
            SaveEvents.AfterLoad += afterLoad;
            GameEvents.FirstUpdateTick += firstUpdate;
            PlayerEvents.InventoryChanged += invChanged;

            Log.info("Loading content packs...");
            foreach (IContentPack contentPack in this.Helper.GetContentPacks())
                loadData(contentPack);
            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
            {
                foreach (string dir in Directory.EnumerateDirectories(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
                    loadData(dir);
            }
        }

        private IApi api;
        public override object GetApi()
        {
            if (api == null)
                api = new Api(this.loadData);

            return api;
        }

        private void firstUpdate(object sender, EventArgs args)
        {
            GameEvents.UpdateTick += secondUpdate;
        }

        private void secondUpdate(object sender, EventArgs args)
        {
            GameEvents.UpdateTick -= secondUpdate;

            objectIds = AssignIds("objects", StartingObjectId, objects.ToList<DataNeedsId>());
            cropIds = AssignIds("crops", StartingCropId, crops.ToList<DataNeedsId>());
            fruitTreeIds = AssignIds("fruittrees", StartingFruitTreeId, fruitTrees.ToList<DataNeedsId>());
            bigCraftableIds = AssignIds("big-craftables", StartingBigCraftableId, bigCraftables.ToList<DataNeedsId>());

            Helper.Content.AssetEditors.Add(new ContentInjector());
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
                    string[] excludeSeasons = new[] { "spring", "summer", "fall", "winter" }.Except(crop.Seasons).ToArray();
                    crop.seed.PurchaseRequirements.Add($"z {string.Join(" ", excludeSeasons)}");
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
                        PurchaseFrom = tree.SsaplingPurchaseFrom,
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
        }

        private void menuChanged(object sender, EventArgsClickableMenuChanged args)
        {
            var menu = args.NewMenu as ShopMenu;
            if (menu == null || menu.portraitPerson == null)
                return;

            //if (menu.portraitPerson.name == "Pierre")
            {
                Log.trace($"Adding objects to {menu.portraitPerson.name}'s shop");

                var forSale = Helper.Reflection.GetPrivateValue<List<Item>>(menu, "forSale");
                var itemPriceAndStock = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(menu, "itemPriceAndStock");

                var precondMeth = Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "checkEventPrecondition");
                foreach (var obj in objects)
                {
                    if (obj.Recipe != null && obj.Recipe.CanPurchase)
                    {
                        if (obj.Recipe.PurchaseFrom != menu.portraitPerson.name)
                            continue;
                        if (Game1.player.craftingRecipes.ContainsKey(obj.Name) || Game1.player.cookingRecipes.ContainsKey(obj.Name))
                            continue;
                        if (obj.Recipe.PurchaseRequirements != null && obj.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { obj.Recipe.GetPurchaseRequirementString() }) == -1)
                            continue;
                        var recipeObj = new StardewValley.Object(obj.id, 1, true, obj.Recipe.PurchasePrice, 0);
                        forSale.Add(recipeObj);
                        itemPriceAndStock.Add(recipeObj, new int[] { obj.Recipe.PurchasePrice, 1 });
                        Log.trace($"\tAdding recipe for {obj.Name}");
                    }
                    if (!obj.CanPurchase)
                        continue;
                    if (obj.PurchaseFrom != menu.portraitPerson.name)
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
                        if (big.Recipe.PurchaseFrom != menu.portraitPerson.name)
                            continue;
                        if (Game1.player.craftingRecipes.ContainsKey(big.Name) || Game1.player.cookingRecipes.ContainsKey(big.Name))
                            continue;
                        if (big.Recipe.PurchaseRequirements != null && big.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { big.Recipe.GetPurchaseRequirementString() }) == -1)
                            continue;
                        var recipeObj = new StardewValley.Object(new Vector2(0, 0), big.id, true);
                        forSale.Add(recipeObj);
                        itemPriceAndStock.Add(recipeObj, new int[] { big.Recipe.PurchasePrice, 1 });
                        Log.trace($"\tAdding recipe for {big.Name}");
                    }
                    if (!big.CanPurchase)
                        continue;
                    if (big.PurchaseFrom != menu.portraitPerson.name)
                        continue;
                    if (big.PurchaseRequirements != null && big.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { big.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new StardewValley.Object(Vector2.Zero, big.id, false);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { big.PurchasePrice, int.MaxValue });
                    Log.trace($"\tAdding {big.Name}");
                }
            }

            ( ( Api ) api ).InvokeAddedItemsToShop();
        }

        private void afterLoad(object sender, EventArgs args)
        {
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

        private IList<ObjectData> myRings = new List<ObjectData>();
        private void invChanged(object sender, EventArgsInventoryChanged args)
        {
            IList<int> ringIds = new List<int>();
            foreach (var ring in myRings)
                ringIds.Add(ring.id);

            for (int i = 0; i < Game1.player.items.Count; ++i)
            {
                var item = Game1.player.items[i];
                if (item is StardewValley.Object obj && ringIds.Contains(obj.parentSheetIndex))
                {
                    Log.trace($"Turning a ring-object of {obj.parentSheetIndex} into a proper ring");
                    Game1.player.items[i] = new StardewValley.Objects.Ring(obj.parentSheetIndex);
                }
            }
        }

        private const int StartingObjectId = 2000;
        private const int StartingCropId = 100;
        private const int StartingFruitTreeId = 20;
        private const int StartingBigCraftableId = 300;
        internal IList<ObjectData> objects = new List<ObjectData>();
        internal IList<CropData> crops = new List<CropData>();
        internal IList<FruitTreeData> fruitTrees = new List<FruitTreeData>();
        internal IList<BigCraftableData> bigCraftables = new List<BigCraftableData>();
        internal IDictionary<string, int> objectIds;
        internal IDictionary<string, int> cropIds;
        internal IDictionary<string, int> fruitTreeIds;
        internal IDictionary<string, int> bigCraftableIds;

        public int ResolveObjectId(object data)
        {
            if (data.GetType() == typeof(long))
                return (int)(long)data;
            else
                return objectIds[(string)data];
        }

        private Dictionary<string, int> AssignIds(string type, int starting, IList<DataNeedsId> data)
        {
            var saved = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath, $"ids-{type}.json"));
            Dictionary<string, int> ids = new Dictionary<string, int>();

            int currId = starting;
            // First, populate saved IDs
            foreach (var d in data)
            {
                if (saved != null && saved.ContainsKey(d.Name))
                {
                    ids.Add(d.Name, saved[d.Name]);
                    currId = Math.Max(currId, saved[d.Name] + 1);
                    d.id = ids[d.Name];
                }
            }
            // Next, add in new IDs
            foreach (var d in data)
            {
                if (d.id == -1)
                {
                    ids.Add(d.Name, currId++);
                    if (type == "objects" && ((ObjectData)d).IsColored)
                        ++currId;
                    d.id = ids[d.Name];
                }
            }

            Helper.WriteJsonFile(Path.Combine(Helper.DirectoryPath, $"ids-{type}.json"), ids);
            return ids;
        }
    }
}
