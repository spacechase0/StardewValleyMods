using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crafting;

namespace JsonAssets.Framework
{
    internal class ContentInjector1 : IAssetEditor, IAssetLoader
    {
        private delegate void Injector(IAssetData asset);
        private readonly Dictionary<string, Injector> Files;
        private readonly Dictionary<string, object> ToLoad;
        public ContentInjector1()
        {
            Func<string, string> normalize = Mod.instance.Helper.Content.NormalizeAssetName;

            //normalize with 
            this.Files = new Dictionary<string, Injector>
            {
                {normalize("Data\\ObjectInformation"), this.InjectDataObjectInformation},
                {normalize("Data\\ObjectContextTags"), this.InjectDataObjectContextTags},
                {normalize("Data\\Crops"), this.InjectDataCrops},
                {normalize("Data\\fruitTrees"), this.InjectDataFruitTrees},
                {normalize("Data\\CookingRecipes"), this.InjectDataCookingRecipes},
                {normalize("Data\\CraftingRecipes"), this.InjectDataCraftingRecipes},
                {normalize("Data\\BigCraftablesInformation"), this.InjectDataBigCraftablesInformation},
                {normalize("Data\\hats"), this.InjectDataHats},
                {normalize("Data\\weapons"), this.InjectDataWeapons},
                {normalize("Data\\ClothingInformation"), this.InjectDataClothingInformation},
                {normalize("Data\\TailoringRecipes"), this.InjectDataTailoringRecipes},
                {normalize("Data\\Boots"), this.InjectDataBoots},
            };

            this.ToLoad = new();
            foreach (var obj in Mod.instance.Objects)
            {
                ToLoad.Add(normalize("JA/Object/" + obj.Name), obj.Texture);
                if (obj.TextureColor != null)
                    ToLoad.Add(normalize("JA/ObjectColor/" + obj.Name), obj.TextureColor);
            }
            foreach (var crop in Mod.instance.Crops)
            {
                ToLoad.Add(normalize("JA/Crop/" + crop.Name), crop.Texture);
                if (crop.GiantTexture != null)
                    ToLoad.Add(normalize("JA/CropGiant/" + crop.Name), crop.GiantTexture);
            }
            foreach (var ftree in Mod.instance.FruitTrees)
                ToLoad.Add(normalize("JA/FruitTree/" + ftree.Name), ftree.Texture);
            foreach (var big in Mod.instance.BigCraftables)
            {
                ToLoad.Add(normalize("JA/BigCraftable0/" + big.Name), big.Texture);
                for (int i = 0; i < big.ExtraTextures.Length; ++i)
                    ToLoad.Add(normalize("JA/BigCraftable" + (i + 1) + "/" + big.Name), big.ExtraTextures[i]);
            }
            foreach (var hat in Mod.instance.Hats)
                ToLoad.Add(normalize("JA/Hat/" + hat.Name), hat.Texture);
            foreach (var weapon in Mod.instance.Weapons)
                ToLoad.Add(normalize("JA/Weapon/" + weapon.Name), weapon.Texture);
            foreach ( var shirt in Mod.instance.Shirts )
            {
                ToLoad.Add(normalize("JA/ShirtMale/" + shirt.Name), shirt.TextureMale);
                if (shirt.TextureFemale != null)
                    ToLoad.Add(normalize("JA/ShirtFemale/" + shirt.Name), shirt.TextureFemale);
                if (shirt.TextureMaleColor != null)
                    ToLoad.Add(normalize("JA/ShirtMaleColor/" + shirt.Name), shirt.TextureMaleColor);
                if (shirt.TextureFemaleColor != null)
                    ToLoad.Add(normalize("JA/ShirtFemaleColor/" + shirt.Name), shirt.TextureFemaleColor);
            }
            foreach ( var pants in Mod.instance.Pants )
                ToLoad.Add(normalize("JA/Pants/" + pants.Name), pants.Texture);
            foreach ( var boots in Mod.instance.Boots )
            {
                ToLoad.Add(normalize("JA/Boots/" + boots.Name), boots.Texture);
                ToLoad.Add(normalize("JA/BootsColor/" + boots.Name), boots.TextureColor);
            }
            // TODO custom fence when they implement them in vanilla
        }

        public void InvalidateUsed()
        {
            Mod.instance.Helper.Content.InvalidateCache(asset => this.Files.ContainsKey(asset.AssetName));
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return this.Files.ContainsKey(asset.AssetName);
        }

        public void Edit<T>(IAssetData asset)
        {
            this.Files[asset.AssetName](asset);
        }

        private void InjectDataObjectInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    Log.Verbose($"Injecting to objects: {obj.Name}: {obj.GetObjectInformation()}");
                    data.Add(obj.Name.Replace(' ', '_'), obj.GetObjectInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {obj.Name}: {e}");
                }
            }
        }
        private void InjectDataObjectContextTags(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    string tags = string.Join(", ", obj.ContextTags);
                    Log.Verbose($"Injecting to object context tags: {obj.Name}: {tags}");
                    if (!data.TryGetValue(obj.Name, out string prevTags) || prevTags == "")
                        data[obj.Name] = tags;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object context tags for {obj.Name}: {e}");
                }
            }
        }
        private void InjectDataCrops(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    Log.Verbose($"Injecting to crops: {crop.GetSeedId()}: {crop.GetCropInformation()}");
                    data.Add(crop.GetSeedId().Replace(' ', '_'), crop.GetCropInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop for {crop.Name}: {e}");
                }
            }
        }
        private void InjectDataFruitTrees(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    Log.Verbose($"Injecting to fruit trees: {fruitTree.GetSaplingId()}: {fruitTree.GetFruitTreeInformation()}");
                    data.Add(fruitTree.GetSaplingId().Replace(' ', '_'), fruitTree.GetFruitTreeInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree for {fruitTree.Name}: {e}");
                }
            }
        }
        private void InjectDataCookingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null)
                        continue;
                    if (obj.Category != ObjectCategory.Cooking)
                        continue;
                    Log.Verbose($"Injecting to cooking recipes: {obj.Name}: {obj.Recipe.GetRecipeString(obj)}");
                    data.Add(obj.Name, obj.Recipe.GetRecipeString(obj));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting cooking recipe for {obj.Name}: {e}");
                }
            }
        }
        private void InjectDataCraftingRecipes(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    if (obj.Recipe == null)
                        continue;
                    if (obj.Category == ObjectCategory.Cooking)
                        continue;
                    Log.Verbose($"Injecting to crafting recipes: {obj.Name}: {obj.Recipe.GetRecipeString(obj)}");
                    data.Add(obj.Name.Replace(' ', '_'), obj.Recipe.GetRecipeString(obj));
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
                    Log.Verbose($"Injecting to crafting recipes: {big.Name}: {big.Recipe.GetRecipeString(big)}");
                    data.Add(big.Name.Replace(' ', '_'), big.Recipe.GetRecipeString(big));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {big.Name}: {e}");
                }
            }
        }
        private void InjectDataBigCraftablesInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    Log.Verbose($"Injecting to big craftables: {big.Name}: {big.GetCraftableInformation()}");
                    data.Add(big.Name.Replace(' ', '_'), big.GetCraftableInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {big.Name}: {e}");
                }
            }
        }
        private void InjectDataHats(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    Log.Verbose($"Injecting to hats: {hat.Name}: {hat.GetHatInformation()}");
                    data.Add(hat.Name.Replace(' ', '_'), hat.GetHatInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting hat information for {hat.Name}: {e}");
                }
            }
        }
        private void InjectDataWeapons(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    Log.Verbose($"Injecting to weapons: {weapon.Name}: {weapon.GetWeaponInformation()}");
                    data.Add(weapon.Name.Replace(' ', '_'), weapon.GetWeaponInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting weapon information for {weapon.Name}: {e}");
                }
            }
        }
        private void InjectDataClothingInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    Log.Verbose($"Injecting to clothing information: {shirt.Name}: {shirt.GetClothingInformation()}");
                    data.Add(shirt.Name.Replace(' ', '_'), shirt.GetClothingInformation());
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
                    Log.Verbose($"Injecting to clothing information: {pants.Name}: {pants.GetClothingInformation()}");
                    data.Add(pants.Name.Replace(' ', '_'), pants.GetClothingInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting clothing information for {pants.Name}: {e}");
                }
            }
        }
        private void InjectDataTailoringRecipes(IAssetData asset)
        {
            var data = asset.GetData<List<TailorItemRecipe>>();
            foreach (var recipe in Mod.instance.Tailoring)
            {
                try
                {
                    Log.Verbose($"Injecting to tailoring recipe: {recipe.ToGameData()}");
                    data.Add(recipe.ToGameData());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting tailoring recipe: {e}");
                }
            }
        }
        private void InjectDataBoots(IAssetData asset)
        {
            var data = asset.AsDictionary<string, string>().Data;
            foreach (var boots in Mod.instance.Boots)
            {
                try
                {
                    Log.Verbose($"Injecting to boots: {boots.Name}: {boots.GetBootsInformation()}");
                    data.Add(boots.Name.Replace(' ', '_'), boots.GetBootsInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting boots information for {boots.Name}: {e}");
                }
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (ToLoad.ContainsKey(asset.AssetName))
                return true;
            // TODO once they add custom fences
            /*
            foreach (var fence in Mod.instance.Fences)
            {
                if (asset.AssetNameEquals("LooseSprites\\Fence" + fence.CorrespondingObject?.Name))
                    return true;
            }*/
            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (ToLoad.ContainsKey(asset.AssetName))
                return (T)(object)ToLoad[asset.AssetName];
            // TODO once they add custom fences
            /*
            foreach (var fence in Mod.instance.Fences)
            {
                if (asset.AssetNameEquals("LooseSprites\\Fence" + fence.CorrespondingObject?.Name()))
                    return (T)(object)fence.Texture;
            }
            */
            return default(T);
        }
    }
}
