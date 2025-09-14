using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceCore.VanillaAssetExpansion;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crafting;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace JsonAssets.Framework
{
    internal class ContentInjector1
    {
        private delegate void Injector(IAssetData asset);
        private readonly Dictionary<string, Injector> Files;
        private readonly Dictionary<string, Texture2D> ToLoad;
        public ContentInjector1()
        {
            //normalize with
            this.Files = new Dictionary<string, Injector>
            {
                {"Data\\Objects", this.InjectDataObjectInformation},
                {"Data\\Crops", this.InjectDataCrops},
                {"Data\\GiantCrops", this.InjectDataGiantCrops},
                {"Data\\FruitTrees", this.InjectDataFruitTrees},
                {"Data\\CookingRecipes", this.InjectDataCookingRecipes},
                {"Data\\CraftingRecipes", this.InjectDataCraftingRecipes},
                {"Data\\BigCraftables", this.InjectDataBigCraftablesInformation},
                {"Data\\hats", this.InjectDataHats},
                {"Data\\Weapons", this.InjectDataWeapons},
                {"Data\\Pants", this.InjectDataPants},
                {"Data\\Shirts", this.InjectDataShirts},
                {"Data\\TailoringRecipes", this.InjectDataTailoringRecipes},
                {"Data\\Boots", this.InjectDataBoots},
                {"Data\\Fences", this.InjectDataFences },
                {"spacechase0.SpaceCore\\ObjectExtensionData", this.InjectDataObjectExtensionData }
            };

            this.ToLoad = new();
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    ToLoad.Add("JA/Object/" + obj.Name.FixIdJA("O"), obj.GetTexture());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading object texture for {obj.Name.FixIdJA("O")}: {e}");
                }
            }
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    ToLoad.Add("JA/Crop/" + crop.Name.FixIdJA("Crop"), crop.Texture);
                    if (crop.GiantTexture != null)
                        ToLoad.Add("JA/CropGiant/" + crop.Name.FixIdJA("Crop"), crop.GiantTexture);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading crop texture for {crop.Name}: {e}");
                }
            }
            foreach (var ftree in Mod.instance.FruitTrees)
            {
                try
                {
                    ToLoad.Add("JA/FruitTree/" + ftree.Name.FixIdJA("FruitTree"), ftree.Texture);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading fruit tree texture for {ftree.Name.FixIdJA("FruitTree")}: {e}");
                }
            }
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    ToLoad.Add("JA/BigCraftable/" + big.Name.FixIdJA("BC"), big.GetTexture());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading big craftable texture for {big.Name.FixIdJA("BC")}: {e}");
                }
            }
            foreach (var hat in Mod.instance.Hats)
            {
                try
                {
                    ToLoad.Add("JA/Hat/" + hat.Name.FixIdJA("H"), hat.Texture);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading hat texture for {hat.Name.FixIdJA("H")}: {e}");
                }
            }
            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    ToLoad.Add("JA/Weapon/" + weapon.Name.FixIdJA("W"), weapon.Texture);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading weapon texture for {weapon.Name.FixIdJA("W")}: {e}");
                }
            }
            foreach ( var shirt in Mod.instance.Shirts )
            {
                try
                {
                    ToLoad.Add("JA/Shirts/" + shirt.Name.FixIdJA("S"), shirt.GetShirtTexture());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading shirt texture for {shirt.Name.FixIdJA("S")}: {e}");
                }
            }
            foreach ( var pants in Mod.instance.Pants )
            {
                try
                {
                    ToLoad.Add("JA/Pants/" + pants.Name.FixIdJA("P"), pants.Texture);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading pants texture for {pants.Name.FixIdJA("P")}: {e}");
                }
            }
            foreach ( var boots in Mod.instance.Boots )
            {
                try
                {
                    ToLoad.Add("JA/Boots/" + boots.Name.FixIdJA("B"), boots.Texture);
                    ToLoad.Add("JA/BootsColor/" + boots.Name.FixIdJA("B"), boots.TextureColor);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading boots texture for {boots.Name.FixIdJA("B")}: {e}");
                }
            }
            foreach (var fence in Mod.instance.Fences)
            {
                try
                {
                    ToLoad.Add("JA/Fence/" + fence.Name.FixIdJA("O"), fence.Texture);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception loading fence texture for {fence.Name.FixIdJA("O")}");
                }
            }

            Mod.instance.Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            InvalidateUsed();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (ToLoad.ContainsKey(e.NameWithoutLocale.Name.Replace('\\', '/')))
                e.LoadFrom(() => {
                    Texture2D tex = new Texture2D(Game1.graphics.GraphicsDevice, ToLoad[e.NameWithoutLocale.Name.Replace('\\', '/')].Width, ToLoad[e.NameWithoutLocale.Name.Replace('\\', '/')].Height);
                    tex.CopyFromTexture(ToLoad[e.NameWithoutLocale.Name.Replace('\\', '/')]);
                    return tex;
                }
                , StardewModdingAPI.Events.AssetLoadPriority.Medium);

            if (Files.ContainsKey(e.NameWithoutLocale.Name.Replace('/', '\\')))
                e.Edit((asset) => Files[e.NameWithoutLocale.Name.Replace('/', '\\')](asset));
        }

        public void InvalidateUsed()
        {
            Mod.instance.Helper.GameContent.InvalidateCache(asset => this.Files.ContainsKey(asset.NameWithoutLocale.Name.Replace('/', '\\')));
        }

        private void InjectDataObjectInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.Objects.ObjectData>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    Log.Verbose($"Injecting to objects: {obj.Name}: {obj.GetObjectInformation()}");
                    data.Add(obj.Name.FixIdJA("O"), obj.GetObjectInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {obj.Name}: {e}");
                }
            }
        }
        private void InjectDataObjectExtensionData(IAssetData asset)
        {
            var data = asset.AsDictionary<string, ObjectExtensionData>().Data;
            foreach (var obj in Mod.instance.Objects)
            {
                try
                {
                    Log.Verbose($"Injecting to object extension data: {obj.Name}");
                    data.Add(obj.Name.FixIdJA("O"), new()
                    {
                        CategoryTextOverride = obj.CategoryTextOverride,
                        CategoryColorOverride = obj.CategoryColorOverride,
                        CanBeTrashed = obj.CanTrash,
                        CanBeShipped = obj.CanSell,
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {obj.Name.FixIdJA("O")}: {e}");
                }
            }
        }

        private void InjectDataCrops(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.Crops.CropData>().Data;
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    Log.Verbose($"Injecting to crops: {crop.GetSeedId()}: {crop.GetCropInformation()}");
                    data.Add(crop.GetSeedId().FixIdJA("O"), crop.GetCropInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop for {crop.Name.FixIdJA("Crop")}: {e}");
                }
            }
        }
        private void InjectDataGiantCrops(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.GiantCrops.GiantCropData>().Data;
            foreach (var crop in Mod.instance.Crops)
            {
                try
                {
                    if (crop.GiantTexture != null)
                    {
                        Log.Verbose($"Injecting to crops: {crop.GetSeedId()}: {crop.GetGiantCropInformation()}");
                        data.Add(crop.GetSeedId().FixIdJA("O"), crop.GetGiantCropInformation());
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting giant crop for {crop.Name.FixIdJA("Crop")}: {e}");
                }
            }
        }
        private void InjectDataFruitTrees(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.FruitTrees.FruitTreeData>().Data;
            foreach (var fruitTree in Mod.instance.FruitTrees)
            {
                try
                {
                    Log.Verbose($"Injecting to fruit trees: {fruitTree.GetSaplingId()}: {fruitTree.GetFruitTreeInformation()}");
                    data.Add(fruitTree.GetSaplingId().FixIdJA("O"), fruitTree.GetFruitTreeInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting fruit tree for {fruitTree.Name.FixIdJA("FruitTree")}: {e}");
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
                    Log.Verbose($"Injecting to cooking recipes: {obj.Name.FixIdJA("O")}: {obj.Recipe.GetRecipeString(obj)}");
                    data.Add(obj.Name.FixIdJA("O"), obj.Recipe.GetRecipeString(obj));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting cooking recipe for {obj.Name.FixIdJA("O")}: {e}");
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
                    Log.Verbose($"Injecting to crafting recipes: {obj.Name.FixIdJA("O")}: {obj.Recipe.GetRecipeString(obj)}");
                    data.Add(obj.Name.FixIdJA("O"), obj.Recipe.GetRecipeString(obj));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {obj.Name.FixIdJA("O")}: {e}");
                }
            }
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    if (big.Recipe == null)
                        continue;
                    Log.Verbose($"Injecting to crafting recipes: {big.Name.FixIdJA("BC")}: {big.Recipe.GetRecipeString(big)}");
                    data.Add(big.Name.FixIdJA("BC"), big.Recipe.GetRecipeString(big));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {big.Name.FixIdJA("BC")}: {e}");
                }
            }
        }
        private void InjectDataBigCraftablesInformation(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.BigCraftables.BigCraftableData>().Data;
            foreach (var big in Mod.instance.BigCraftables)
            {
                try
                {
                    Log.Verbose($"Injecting to big craftables: {big.Name.FixIdJA("BC")}: {big.GetCraftableInformation()}");
                    data.Add(big.Name.FixIdJA("BC"), big.GetCraftableInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {big.Name.FixIdJA("BC")}: {e}");
                }
            }
        }
        private void InjectDataPants(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.Pants.PantsData>().Data;
            foreach (var pants in Mod.instance.Pants)
            {
                try
                {
                    Log.Verbose($"Injecting to pants: {pants.Name.FixIdJA("P")}: {pants.GetPantsInformation()}");
                    data.Add(pants.Name.FixIdJA("P"), pants.GetPantsInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting pants information for {pants.Name.FixIdJA("P")}: {e}");
                }
            }
        }
        private void InjectDataShirts(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.Shirts.ShirtData>().Data;
            foreach (var shirt in Mod.instance.Shirts)
            {
                try
                {
                    Log.Verbose($"Injecting to shirts: {shirt.Name.FixIdJA("S")}: {shirt.GetShirtInformation()}");
                    data.Add(shirt.HasFemaleVariant ? shirt.Name.FixIdJA("S") + "_M" : shirt.Name.FixIdJA("S"), shirt.GetShirtInformation());
                    if (shirt.HasFemaleVariant)
                    {
                        data.Add(shirt.Name.FixIdJA("S") + "_F", shirt.GetFemaleShirtInformation());
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting shirt information for {shirt.Name.FixIdJA("S")}: {e}");
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
                    Log.Verbose($"Injecting to hats: {hat.Name.FixIdJA("H")}: {hat.GetHatInformation()}");
                    data.Add(hat.Name.FixIdJA("H"), hat.GetHatInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting hat information for {hat.Name.FixIdJA("H")}: {e}");
                }
            }
        }
        private void InjectDataWeapons(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.Weapons.WeaponData>().Data;
            foreach (var weapon in Mod.instance.Weapons)
            {
                try
                {
                    Log.Verbose($"Injecting to weapons: {weapon.Name}: {weapon.GetWeaponInformation()}");
                    data.Add(weapon.Name.FixIdJA("W"),weapon.GetWeaponInformation()); ;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting weapon information for {weapon.Name.FixIdJA("W")}: {e}");
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
                    Log.Verbose($"Injecting to boots: {boots.Name.FixIdJA("B")}: {boots.GetBootsInformation()}");
                    data.Add(boots.Name.FixIdJA("B"), boots.GetBootsInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting boots information for {boots.Name.FixIdJA("B")}: {e}");
                }
            }
        }
        private void InjectDataFences(IAssetData asset)
        {
            var data = asset.AsDictionary<string, StardewValley.GameData.Fences.FenceData>().Data;
            foreach (var fence in Mod.instance.Fences)
            {
                try
                {
                    Log.Verbose($"Injecting to fences: {fence.Name}: {fence.GetFenceInformation()}");
                    data.Add(fence.Name.FixIdJA("O"), fence.GetFenceInformation()); ;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting weapon information for {fence.Name.FixIdJA("O")}: {e}");
                }
            }
        }
    }
}
