using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using JsonAssets.Data;
using Microsoft.Xna.Framework;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Crafting;

namespace JsonAssets.Framework
{
    internal static class ContentInjector1
    {
        private static readonly Dictionary<string, Action<IAssetData>> Files = new();

        private static readonly Dictionary<int, FenceData> FenceIndexes = new();

        /// <summary>
        /// Call after assigning IDs. Populate the content injector's dictionary
        /// with only the assets that need editing.
        /// </summary>
        /// <param name="helper">Game content helper.</param>
        internal static void Initialize(IGameContentHelper helper)
        {
            lock (Files)
            {
                Files.Clear();
                if (Mod.instance.Objects.Count > 0 || Mod.instance.Boots.Count > 0)
                { // boots are objects too.
                    Files[helper.ParseAssetName(@"Data\ObjectInformation").BaseName] = InjectDataObjectInformation;
                    Files[helper.ParseAssetName(@"Data\ObjectContextTags").BaseName] = InjectDataObjectContextTags;
                    Files[helper.ParseAssetName(@"Data\CookingRecipes").BaseName] = InjectDataCookingRecipes;
                    Files[helper.ParseAssetName(@"Maps\springobjects").BaseName] = InjectMapsSpringobjects;
                }
                if (Mod.instance.Objects.Count > 0 || Mod.instance.Boots.Count > 0 || Mod.instance.BigCraftables.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\CraftingRecipes").BaseName] = InjectDataCraftingRecipes;
                }
                if (Mod.instance.BigCraftables.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\BigCraftablesInformation").BaseName] = InjectDataBigCraftablesInformation;
                    Files[helper.ParseAssetName(@"TileSheets\Craftables").BaseName] = InjectTileSheetsCraftables;
                }
                if (Mod.instance.Crops.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\Crops").BaseName] = InjectDataCrops;
                    Files[helper.ParseAssetName(@"TileSheets\crops").BaseName] = InjectTileSheetsCrops;
                }
                if (Mod.instance.FruitTrees.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\fruitTrees").BaseName] = InjectDataFruitTrees;
                    Files[helper.ParseAssetName(@"TileSheets\fruitTrees").BaseName] = InjectTileSheetsFruitTrees;
                }
                if (Mod.instance.Hats.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\hats").BaseName] = InjectDataHats;
                    Files[helper.ParseAssetName(@"Characters\Farmer\hats").BaseName] = InjectCharactersFarmerHats;
                }
                if (Mod.instance.Weapons.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\weapons").BaseName] = InjectDataWeapons;
                    Files[helper.ParseAssetName(@"TileSheets\weapons").BaseName] = InjectTileSheetsWeapons;
                }
                if (Mod.instance.Shirts.Count > 0 || Mod.instance.Pants.Count > 0 )
                {
                    Files[helper.ParseAssetName(@"Data\ClothingInformation").BaseName] = InjectDataClothingInformation;
                }
                if (Mod.instance.Shirts.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Characters\Farmer\shirts").BaseName] = InjectCharactersFarmerShirts;
                }
                if (Mod.instance.Pants.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Characters\Farmer\pants").BaseName] = InjectCharactersFarmerPants;
                }
                if (Mod.instance.Tailoring.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\TailoringRecipes").BaseName] = InjectDataTailoringRecipes;
                }
                if (Mod.instance.Boots.Count > 0)
                {
                    Files[helper.ParseAssetName(@"Data\Boots").BaseName] = InjectDataBoots;
                    Files[helper.ParseAssetName(@"Characters\Farmer\shoeColors").BaseName] = InjectCharactersFarmerShoeColors;
                }

                Log.Trace($"Content Injector 1 initialized with {Files.Count} assets.");
            }

            lock (FenceIndexes)
            {
                FenceIndexes.Clear();
                foreach (FenceData fence in Mod.instance.Fences)
                    if (fence?.CorrespondingObject?.GetObjectId() is int index)
                        FenceIndexes[index] = fence;
            }
        }

        internal static void Clear()
        {
            lock (Files)
            {
                Files.Clear();
            }
            lock (FenceIndexes)
            {
                FenceIndexes.Clear();
            }
        }

        public static void InvalidateUsed()
        {
            if (Files.Count > 0)
                Mod.instance.Helper.GameContent.InvalidateCache(asset => Files.ContainsKey(asset.NameWithoutLocale.BaseName));
            if (FenceIndexes.Count > 0)
            {
                foreach (int fence in FenceIndexes.Keys)
                    Mod.instance.Helper.GameContent.InvalidateCache($@"LooseSprites\Fence{fence}");
            }    
        }

        public static void OnAssetRequested(AssetRequestedEventArgs e)
        {
            if (!Mod.instance.DidInit)
                return;
            if (FenceIndexes.Count > 0 && e.NameWithoutLocale.StartsWith(@"LooseSprites\Fence")
                && int.TryParse(e.NameWithoutLocale.BaseName[@"LooseSprites\Fence".Length..], out int index)
                && FenceIndexes.TryGetValue(index, out var fenceData))
                e.LoadFrom(() => fenceData.Texture, AssetLoadPriority.Low);
            else if (Files.TryGetValue(e.NameWithoutLocale.BaseName, out var injector))
                e.Edit(injector, (AssetEditPriority)int.MinValue); // insist on editing first.
        }

        private static void InjectDataObjectInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    string objinfo = obj.GetObjectInformation().ToString();
                    if (Log.IsVerbose)
                        Log.Trace( $"Injecting to objects: {obj.GetObjectId()}: {objinfo}");
                    if (!data.TryAdd(obj.GetObjectId(), objinfo))
                        Log.Error($"Object {obj.GetObjectId()} is a duplicate???");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {obj.Name}: {e}");
                }
            }
        }
        private static void InjectDataObjectContextTags(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    string tags = string.Join(',', obj.ContextTags);
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to object context tags: {obj.Name}: {tags}");
                    if (!data.TryGetValue(obj.Name, out string prevTags) || string.IsNullOrWhiteSpace(prevTags))
                        data[obj.Name] = tags;
                    else
                        data[obj.Name] += (", " + tags);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object context tags for {obj.Name}: {e}");
                }
            }
        }
        private static void InjectDataCrops(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    string cropinfo = crop.GetCropInformation().ToString();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to crops: {crop.GetSeedId()}: {cropinfo}");
                    if (!data.TryAdd(crop.GetSeedId(), cropinfo))
                        Log.Error($"Crop {crop.GetSeedId()} already exists!");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop for {crop.Name}: {e}");
                }
            }
        }
        private static void InjectDataFruitTrees(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    string treeinfo = fruitTree.GetFruitTreeInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to fruit trees: {fruitTree.GetSaplingId()}: {treeinfo}");
                    if (!data.TryAdd(fruitTree.GetSaplingId(), treeinfo))
                        Log.Error($"Fruit tree {fruitTree.Name} is a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree for {fruitTree.Name}: {e}");
                }
            }
        }
        private static void InjectDataCookingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null || obj.Category != ObjectCategory.Cooking)
                        continue;
                    string recipestring = obj.Recipe.GetRecipeString(obj).ToString();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to cooking recipes: {obj.Name}: {recipestring}");
                    if (!data.TryAdd(obj.Name, recipestring))
                        Log.Error($"Recipe for {obj.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting cooking recipe for {obj.Name}: {e}");
                }
            }
        }
        private static void InjectDataCraftingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null || obj.Category == ObjectCategory.Cooking)
                        continue;
                    string recipestring = obj.Recipe.GetRecipeString(obj).ToString();
                    if (Log.IsVerbose)
                        Log.Trace( $"Injecting to crafting recipes: {obj.Name}: {recipestring}");
                    if (!data.TryAdd(obj.Name, recipestring))
                        Log.Error($"Recipe for {obj.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {obj.Name}: {e}");
                }
            }
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    if (big.Recipe == null)
                        continue;
                    string recipestring = big.Recipe.GetRecipeString(big).ToString();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to crafting recipes: {big.Name}: {recipestring}");
                    if (!data.TryAdd(big.Name, recipestring))
                        Log.Error($"Recipe for {big.Name} already seems to exist?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {big.Name}: {e}");
                }
            }
        }
        private static void InjectDataBigCraftablesInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    string bigcraftableinfo = big.GetCraftableInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to big craftables: {big.GetCraftableId()}: {bigcraftableinfo}");
                    if (!data.TryAdd(big.GetCraftableId(), big.GetCraftableInformation()))
                        Log.Error($"{big.Name} already seems to exist!");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {big.Name}: {e}");
                }
            }
        }
        private static void InjectDataHats(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    string hatinfo = hat.GetHatInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to hats: {hat.GetHatId()}: {hatinfo}");
                    if (!data.TryAdd(hat.GetHatId(), hat.GetHatInformation()))
                        Log.Error($"Hat {hat.GetHatId()} appears to be a duplicate???");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting hat information for {hat.Name}: {e}");
                }
            }
        }
        private static void InjectDataWeapons(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    string weaponData = weapon.GetWeaponInformation();
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to weapons: {weapon.GetWeaponId()}: {weaponData}");
                    if (!data.TryAdd(weapon.GetWeaponId(), weaponData))
                        Log.Error($"{weapon.GetWeaponId()} appears to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting weapon information for {weapon.Name}: {e}");
                }
            }
        }
        private static void InjectDataClothingInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to clothing information: {shirt.GetClothingId()}: {shirt.GetClothingInformation()}");
                    if (!data.TryAdd(shirt.GetClothingId(), shirt.GetClothingInformation()))
                        Log.Error($"Shirt {shirt.GetClothingId()} appears to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting clothing information for {shirt.Name}: {e}");
                }
            }
            foreach (var pants in Mod.instance.Pants)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to clothing information: {pants.GetClothingId()}: {pants.GetClothingInformation()}");
                    if (!data.TryAdd(pants.GetClothingId(), pants.GetClothingInformation()))
                        Log.Error($"Pants {pants.GetClothingId()} appears to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting clothing information for {pants.Name}: {e}");
                }
            }
        }
        private static void InjectDataTailoringRecipes(IAssetData asset)
        {
            var data = asset.GetData<List<TailorItemRecipe>>();
            foreach (var recipe in Mod.instance.Tailoring)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to tailoring recipe: {recipe.ToGameData()}");
                    data.Add(recipe.ToGameData());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting tailoring recipe: {e}");
                }
            }
        }
        private static void InjectDataBoots(IAssetData asset)
        {
            var data = asset.AsDictionary<int, string>().Data;
            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting to boots: {boots.GetObjectId()}: {boots.GetBootsInformation()}");
                    if (!data.TryAdd(boots.GetObjectId(), boots.GetBootsInformation()))
                        Log.Error($"Boots {boots.Name} appear to be a duplicate?");
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting boots information for {boots.Name}: {e}");
                }
            }
        }
        private static void InjectMapsSpringobjects(IAssetData asset)
        {
            if (Mod.instance.Objects.Count == 0 && Mod.instance.Boots.Count == 0)
                return;

            var tex= asset.AsImage();
            if (tex.ExtendImage(tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"SpringObjects are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (Log.IsVerbose)
                        Log.Trace($"Injecting {obj.Name} sprites @ {ContentInjector1.ObjectRect(obj.GetObjectId())}");
                    tex.PatchExtendedTileSheet(obj.Texture, null, ContentInjector1.ObjectRect(obj.GetObjectId()));
                    if (obj.IsColored)
                    {
                        if (Log.IsVerbose)
                            Log.Trace($"Injecting {obj.Name} color sprites @ {ContentInjector1.ObjectRect(obj.GetObjectId() + 1)}");
                        tex.PatchExtendedTileSheet(obj.TextureColor, null, ContentInjector1.ObjectRect(obj.GetObjectId() + 1));
                    }

                    var rect = ContentInjector1.ObjectRect(obj.GetObjectId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    obj.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    obj.TilesheetX = rect.X;
                    obj.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {obj.Name}: {e}");
                }
            }

            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    Log.Verbose($"Injecting {boots.Name} sprites @ {ContentInjector1.ObjectRect(boots.GetObjectId())}");
                    tex.PatchExtendedTileSheet(boots.Texture, null, ContentInjector1.ObjectRect(boots.GetObjectId()));

                    var rect = ContentInjector1.ObjectRect(boots.GetObjectId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    boots.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    boots.TilesheetX = rect.X;
                    boots.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {boots.Name}: {e}");
                }
            }
        }

        private static void InjectTileSheetsCrops(IAssetData asset)
        {
            var tex = asset.AsImage();
            if (tex.ExtendImage(tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"Crops are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    Log.Verbose($"Injecting {crop.Name} crop images @ {ContentInjector1.CropRect(crop.GetCropSpriteIndex())}");
                    tex.PatchExtendedTileSheet(crop.Texture, null, ContentInjector1.CropRect(crop.GetCropSpriteIndex()));

                    var rect = ContentInjector1.CropRect(crop.GetCropSpriteIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    crop.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    crop.TilesheetX = rect.X;
                    crop.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop sprite for {crop.Name}: {e}");
                }
            }
        }

        private static void InjectTileSheetsFruitTrees(IAssetData asset)
        {
            var tex = asset.AsImage();
            if (tex.ExtendImage(tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"FruitTrees are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    Log.Verbose($"Injecting {fruitTree.Name} fruit tree images @ {ContentInjector1.FruitTreeRect(fruitTree.GetFruitTreeIndex())}");
                    tex.PatchExtendedTileSheet(fruitTree.Texture, null, ContentInjector1.FruitTreeRect(fruitTree.GetFruitTreeIndex()));

                    var rect = ContentInjector1.FruitTreeRect(fruitTree.GetFruitTreeIndex());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    fruitTree.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    fruitTree.TilesheetX = rect.X;
                    fruitTree.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree sprite for {fruitTree.Name}: {e}");
                }
            }
        }

        private static void InjectTileSheetsCraftables(IAssetData asset)
        {
            var tex = asset.AsImage();
            if (tex.ExtendImage(tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"Big craftables are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    Log.Verbose($"Injecting {big.Name} sprites @ {ContentInjector1.BigCraftableRect(big.GetCraftableId())}");
                    tex.PatchExtendedTileSheet(big.Texture, null, ContentInjector1.BigCraftableRect(big.GetCraftableId()));
                    if (big.ReserveExtraIndexCount > 0)
                    {
                        for (int i = 0; i < big.ReserveExtraIndexCount; ++i)
                        {
                            Log.Verbose($"Injecting {big.Name} reserved extra sprite {i + 1} @ {ContentInjector1.BigCraftableRect(big.GetCraftableId() + i + 1)}");
                            asset.AsImage().PatchExtendedTileSheet(big.ExtraTextures[i], null, ContentInjector1.BigCraftableRect(big.GetCraftableId() + i + 1));
                        }
                    }

                    var rect = ContentInjector1.BigCraftableRect(big.GetCraftableId());
                    int ts = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect).TileSheet;
                    big.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    big.TilesheetX = rect.X;
                    big.TilesheetY = rect.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {big.Name}: {e}");
                }
            }
        }

        private static void InjectCharactersFarmerHats(IAssetData asset)
        {
            var image = asset.AsImage();
            if (image.ExtendImage(image.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"Hats are now ({image.Data.Width}, {image.Data.Height})");

            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    Log.Verbose($"Injecting {hat.Name} sprites @ {ContentInjector1.HatRect(hat.GetHatId())}");
                    asset.AsImage().PatchExtendedTileSheet(hat.Texture, null, ContentInjector1.HatRect(hat.GetHatId()));

                    var rect = ContentInjector1.HatRect(hat.GetHatId());
                    var target = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.NameWithoutLocale.BaseName, rect);
                    int ts = target.TileSheet;
                    hat.Tilesheet = asset.NameWithoutLocale.BaseName.GetTilesheetName(ts);
                    hat.TilesheetX = rect.X;
                    hat.TilesheetY = target.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {hat.Name}: {e}");
                }
            }
        }

        private static void InjectTileSheetsWeapons(IAssetData asset)
        {
            var tex = asset.AsImage();
            if (tex.ExtendImage(tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"Weapons are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    Log.Verbose($"Injecting {weapon.Name} sprites @ {ContentInjector1.WeaponRect(weapon.GetWeaponId())}");
                    tex.PatchImage(weapon.Texture, null, ContentInjector1.WeaponRect(weapon.GetWeaponId()));

                    var rect = ContentInjector1.WeaponRect(weapon.GetWeaponId());
                    int ts = 0;// TileSheetExtensions.GetAdjustedTileSheetTarget(asset.AssetName, rect).TileSheet;
                    weapon.Tilesheet = asset.NameWithoutLocale.BaseName;
                    weapon.TilesheetX = rect.X;
                    weapon.TilesheetY = rect.Y;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {weapon.Name}: {e}");
                }
            }
        }
        private static void InjectCharactersFarmerShirts(IAssetData asset)
        {
            if (Mod.instance.Shirts.Count == 0)
                return;

            var tex = asset.AsImage();

            if (tex.ExtendImage(tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"Shirts are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    if (Log.IsVerbose)
                    {
                        List<Rectangle> rects = new(4) { ShirtRectPlain(shirt.GetMaleIndex()) };
                        if (shirt.Dyeable)
                            rects.Add(ShirtRectDye(shirt.GetMaleIndex()));
                        if (shirt.HasFemaleVariant)
                        {
                            rects.Add(ShirtRectPlain(shirt.GetFemaleIndex()));
                            if (shirt.Dyeable)
                                rects.Add(ShirtRectDye(shirt.GetFemaleIndex()));
                        }

                        Log.Trace($"Injecting {shirt.Name} sprites @ {string.Join(',', rects)}");
                    }
                    tex.PatchExtendedTileSheet(shirt.TextureMale, null, ContentInjector1.ShirtRectPlain(shirt.GetMaleIndex()));
                    if (shirt.Dyeable)
                        tex.PatchExtendedTileSheet(shirt.TextureMaleColor, null, ContentInjector1.ShirtRectDye(shirt.GetMaleIndex()));
                    if (shirt.HasFemaleVariant)
                    {
                        tex.PatchExtendedTileSheet(shirt.TextureFemale, null, ContentInjector1.ShirtRectPlain(shirt.GetFemaleIndex()));
                        if (shirt.Dyeable)
                            tex.PatchExtendedTileSheet(shirt.TextureFemaleColor, null, ContentInjector1.ShirtRectDye(shirt.GetFemaleIndex()));
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {shirt.Name}: {e}");
                }
            }
        }

        private static void InjectCharactersFarmerPants(IAssetData asset)
        {
            var tex = asset.AsImage();
            if (tex.ExtendImage(tex.Data.Width, TileSheetExtensions.MAXTILESHEETHEIGHT))
                Log.Trace($"Pants are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var pants in Mod.instance.Pants)
            {
                try
                {
                    Log.Verbose($"Injecting {pants.Name} sprites @ {ContentInjector1.PantsRect(pants.GetTextureIndex())}");
                    tex.PatchExtendedTileSheet(pants.Texture, null, ContentInjector1.PantsRect(pants.GetTextureIndex()));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {pants.Name}: {e}");
                }
            }
        }

        private static void InjectCharactersFarmerShoeColors(IAssetData asset)
        {
            if (Mod.instance.Boots.Count == 0)
                return;

            var tex = asset.AsImage();
            tex.ExtendImage(tex.Data.Width, 4096);
            Log.Trace($"Boots are now ({tex.Data.Width}, {tex.Data.Height})");

            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    Log.Verbose($"Injecting {boots.Name} sprites @ {ContentInjector1.BootsRect(boots.GetTextureIndex())}");
                    tex.PatchExtendedTileSheet(boots.TextureColor, null, ContentInjector1.BootsRect(boots.GetTextureIndex()));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting sprite for {boots.Name}: {e}");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetTilesheetName(this string assetName, int ts)
            => ts == 0 ? assetName : $"{assetName}{ts + 1}";

        #region rectangles
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle ObjectRect(int index)
        {
            int div = Math.DivRem(index, 24, out int rem);
            return new(rem * 16, div * 16, 16, 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle CropRect(int index)
        {
            int div = Math.DivRem(index, 2, out int rem);
            return new(rem * 128, div * 32, 128, 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle FruitTreeRect(int index)
        {
            return new(0, index * 80, 432, 80);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle BigCraftableRect(int index)
        {
            int div = Math.DivRem(index, 8, out int rem);
            return new(rem * 16, div * 32, 16, 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle HatRect(int index)
        {
            int div = Math.DivRem(index, 12, out int rem);
            return new(rem * 20, div * 80, 20, 80);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle WeaponRect(int index)
        {
            int div = Math.DivRem(index, 8, out int rem);
            return new(rem * 16, div * 16, 16, 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle ShirtRectPlain(int index)
        {
            int div = Math.DivRem(index, 16, out int rem);
            return new(rem * 8, div * 32, 8, 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle ShirtRectDye(int index)
        {
            var rect = ContentInjector1.ShirtRectPlain(index);
            rect.X += 16 * 8;
            return rect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle PantsRect(int index)
        {
            int div = Math.DivRem(index, 10, out int rem);
            return new(rem * 192, div * 688, 192, 688);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rectangle BootsRect(int index)
        {
            return new(0, index, 4, 1);
        }

        #endregion
    }
}
