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

namespace JsonAssets.Framework
{
    internal class ContentInjector1
    {
        private delegate void Injector(IAssetData asset);
        private readonly Dictionary<string, Injector> Files;
        private readonly Dictionary<string, object> ToLoad;
        public ContentInjector1()
        {
            //normalize with
            this.Files = new Dictionary<string, Injector>
            {
                {"Data\\Objects", this.InjectDataObjectInformation},
                {"Data\\Crops", this.InjectDataCrops},
                {"Data\\FruitTrees", this.InjectDataFruitTrees},
                {"Data\\CookingRecipes", this.InjectDataCookingRecipes},
                {"Data\\CraftingRecipes", this.InjectDataCraftingRecipes},
                {"Data\\BigCraftablesInformation", this.InjectDataBigCraftablesInformation},
                {"Data\\hats", this.InjectDataHats},
                {"Data\\Weapons", this.InjectDataWeapons},
                {"Data\\ClothingInformation", this.InjectDataClothingInformation},
                {"Data\\TailoringRecipes", this.InjectDataTailoringRecipes},
                {"Data\\Boots", this.InjectDataBoots},
                {"spacechase0.SpaceCore/ObjectExtensionData", this.InjectDataObjectExtensionData }
            };

            this.ToLoad = new();
            foreach (var obj in Mod.instance.Objects)
            {
                ToLoad.Add("JA/Object/" + obj.Name, obj.Texture);
                if (obj.TextureColor != null)
                    ToLoad.Add("JA/ObjectColor/" + obj.Name, obj.TextureColor);
            }
            foreach (var crop in Mod.instance.Crops)
            {
                ToLoad.Add("JA/Crop/" + crop.Name, crop.Texture);
                if (crop.GiantTexture != null)
                    ToLoad.Add("JA/CropGiant/" + crop.Name, crop.GiantTexture);
            }
            foreach (var ftree in Mod.instance.FruitTrees)
                ToLoad.Add("JA/FruitTree/" + ftree.Name, ftree.Texture);
            foreach (var big in Mod.instance.BigCraftables)
            {
                ToLoad.Add("JA/BigCraftable0/" + big.Name, big.Texture);
                for (int i = 0; i < big.ExtraTextures.Length; ++i)
                    ToLoad.Add("JA/BigCraftable" + (i + 1) + "/" + big.Name, big.ExtraTextures[i]);
            }
            foreach (var hat in Mod.instance.Hats)
                ToLoad.Add("JA/Hat/" + hat.Name, hat.Texture);
            foreach (var weapon in Mod.instance.Weapons)
                ToLoad.Add("JA/Weapon/" + weapon.Name, weapon.Texture);
            foreach ( var shirt in Mod.instance.Shirts )
            {
                ToLoad.Add("JA/ShirtMale/" + shirt.Name, shirt.TextureMale);
                if (shirt.TextureFemale != null)
                    ToLoad.Add("JA/ShirtFemale/" + shirt.Name, shirt.TextureFemale);
                if (shirt.TextureMaleColor != null)
                    ToLoad.Add("JA/ShirtMaleColor/" + shirt.Name, shirt.TextureMaleColor);
                if (shirt.TextureFemaleColor != null)
                    ToLoad.Add("JA/ShirtFemaleColor/" + shirt.Name, shirt.TextureFemaleColor);
            }
            foreach ( var pants in Mod.instance.Pants )
                ToLoad.Add("JA/Pants/" + pants.Name, pants.Texture);
            foreach ( var boots in Mod.instance.Boots )
            {
                ToLoad.Add("JA/Boots/" + boots.Name, boots.Texture);
                ToLoad.Add("JA/BootsColor/" + boots.Name, boots.TextureColor);
            }
            // TODO custom fence when they implement them in vanilla

            Mod.instance.Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            InvalidateUsed();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (ToLoad.ContainsKey(e.NameWithoutLocale.Name.Replace('\\', '/')))
                e.LoadFrom(() => ToLoad[e.NameWithoutLocale.Name.Replace('\\', '/')], StardewModdingAPI.Events.AssetLoadPriority.Medium);

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
                    data.Add(obj.Name.FixIdJA(), obj.GetObjectInformation());
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
                    data.Add(obj.Name, new()
                    {
                        CategoryTextOverride = obj.CategoryTextOverride,
                        CategoryColorOverride = obj.CategoryColorOverride,
                        CanBeTrashed = obj.CanTrash,
                        CanBeShipped = obj.CanSell,
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting object information for {obj.Name}: {e}");
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
                    List<Season> seasons = new();
                    foreach (string season in crop.Seasons)
                        seasons.Add(Enum.Parse<Season>(season.Substring(0, 1).ToUpper() + season.Substring(1)));

                    List<string> colors = new();
                    foreach (var c in crop.Colors)
                    {
                        colors.Add("#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2"));
                    }

                    Log.Verbose($"Injecting to crops: {crop.GetSeedId()}: {crop.GetCropInformation()}");
                    data.Add(crop.GetSeedId().FixIdJA(), new()
                    {
                        Seasons = seasons,
                        DaysInPhase = new(crop.Phases),
                        RegrowDays = crop.RegrowthPhase,
                        IsRaised = crop.TrellisCrop,
                        IsPaddyCrop = crop.CropType == CropType.Paddy,
                        HarvestItemId = "(O)" + crop.Product.ToString().FixIdJA(),
                        HarvestMinStack = crop.Bonus.MinimumPerHarvest,
                        HarvestMaxStack = crop.Bonus.MaximumPerHarvest,
                        HarvestMaxIncreasePerFarmingLevel = crop.Bonus.MaxIncreasePerFarmLevel,
                        ExtraHarvestChance = crop.Bonus.ExtraChance,
                        HarvestMethod = crop.HarvestWithScythe ? StardewValley.GameData.Crops.HarvestMethod.Scythe : StardewValley.GameData.Crops.HarvestMethod.Grab,
                        TintColors = colors,
                        Texture = "JA/Crop/" + crop.Name,
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crop for {crop.Name}: {e}");
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
                    data.Add(fruitTree.GetSaplingId().FixIdJA(), new()
                    {
                        DisplayName = fruitTree.Name,
                        Seasons = new(new[] { Enum.Parse<Season>(fruitTree.Season.Substring(0, 1).ToUpper() + fruitTree.Season.Substring(1)) } ),
                        Fruit = new( new[]
                        {
                            new StardewValley.GameData.FruitTrees.FruitTreeFruitData()
                            {
                                ItemId = "(O)" + fruitTree.Product.ToString(),
                            }
                        } ),
                        Texture = "JA/FruitTree/" + fruitTree.Name,
                        TextureSpriteRow = 0,

                    });
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
                    data.Add(obj.Name.FixIdJA(), obj.Recipe.GetRecipeString(obj));
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
                    data.Add(big.Name.FixIdJA(), big.Recipe.GetRecipeString(big));
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting crafting recipe for {big.Name}: {e}");
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
                    Log.Verbose($"Injecting to big craftables: {big.Name}: {big.GetCraftableInformation()}");
                    data.Add(big.Name.FixIdJA(), big.GetCraftableInformation());
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
                    data.Add(hat.Name.FixIdJA(), hat.GetHatInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting hat information for {hat.Name}: {e}");
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
                    data.Add(weapon.Name.FixIdJA(), new()
                    {
                        Name = weapon.Name,
                        DisplayName = weapon.LocalizedName(),
                        Description = weapon.LocalizedDescription(),
                        MinDamage = weapon.MinimumDamage,
                        MaxDamage = weapon.MaximumDamage,
                        Knockback = (float)weapon.Knockback,
                        Speed = weapon.Speed,
                        Precision = weapon.Accuracy,
                        Defense = weapon.Defense,
                        Type = (int)weapon.Type,
                        MineBaseLevel = weapon.MineDropVar,
                        MineMinLevel = weapon.MineDropMinimumLevel,
                        AreaOfEffect = weapon.ExtraSwingArea,
                        CritChance = ( float ) weapon.CritChance,
                        CritMultiplier = ( float ) weapon.CritMultiplier,
                        Texture = $"JA/Weapon/{weapon.Name}",
                        SpriteIndex = 0,
                    }); ;
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
                    data.Add(shirt.Name.FixIdJA(), shirt.GetClothingInformation());
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
                    data.Add(pants.Name.FixIdJA(), pants.GetClothingInformation());
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
                    data.Add(boots.Name.FixIdJA(), boots.GetBootsInformation());
                }
                catch (Exception e)
                {
                    Log.Error($"Exception injecting boots information for {boots.Name}: {e}");
                }
            }
        }
    }
}
