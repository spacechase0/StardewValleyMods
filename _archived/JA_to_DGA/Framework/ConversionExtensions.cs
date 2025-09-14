using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using SpaceShared;
using StardewValley;

namespace JA_to_DGA.Framework
{
    internal static class ConversionExtensions
    {
        public static void AddI18n(this Dictionary<string, Dictionary<string, string>> i18n, string lang, string key, string value)
        {
            if (lang == "en")
                lang = "default";

            if (!i18n.ContainsKey(lang))
                i18n.Add(lang, new Dictionary<string, string>());

            if (i18n.ContainsKey(key))
                i18n[lang][key] = value;
            else
                i18n[lang].Add(key, value);
        }

        private static Dictionary<string, string> ConvertEpu(this IList<string> reqs)
        {
            var ret = new Dictionary<string, string>();
            foreach (string req in reqs)
            {
                string[] toks = req.Split(' ');
                // We're only doing certain ones to reduce my workload. May add more later
                switch (toks[0])
                {
                    case "HasMod":
                        ret.Add("HasMod", toks[1]);
                        break;
                    case "!HasMod":
                        ret.Add("HasMod |contains=" + toks[1], "false");
                        break;
                    case "d":
                        switch (toks[1].ToLower())
                        {
                            case "mon": ret.Add("DayOfWeek", "Monday"); break;
                            case "tue": ret.Add("DayOfWeek", "Tuesday"); break;
                            case "wed": ret.Add("DayOfWeek", "Wednesday"); break;
                            case "thu": ret.Add("DayOfWeek", "Thursday"); break;
                            case "dri": ret.Add("DayOfWeek", "Friday"); break;
                            case "sat": ret.Add("DayOfWeek", "Saturday"); break;
                            case "sun": ret.Add("DayOfWeek", "Sunday"); break;
                            default:
                                Log.Warn("Unknown day of week precondition value: " + toks[1]);
                                break;
                        }
                        break;
                    case "!d":
                        switch (toks[1].ToLower())
                        {
                            case "mon": ret.Add("DayOfWeek |contains=Monday", "false"); break;
                            case "tue": ret.Add("DayOfWeek |contains=Tuesday", "false"); break;
                            case "wed": ret.Add("DayOfWeek |contains=Wednesday", "false"); break;
                            case "thu": ret.Add("DayOfWeek |contains=Thursday", "false"); break;
                            case "fri": ret.Add("DayOfWeek |contains=Friday", "false"); break;
                            case "sat": ret.Add("DayOfWeek |contains=Saturday", "false"); break;
                            case "sun": ret.Add("DayOfWeek |contains=Sunday", "false"); break;
                            default:
                                Log.Warn("Unknown day of week precondition value: " + toks[1]);
                                break;
                        }
                        break;
                    case "w":
                        if (toks[1] == "sunny") ret.Add("Weather", "Sun");
                        else if (toks[1] == "rainy") ret.Add("Weather", "Rain, Storm, Snow");
                        else Log.Warn("Unknown weather precondition value: " + toks[1]);
                        break;
                    case "!w":
                        if (toks[1] == "sunny") ret.Add("Weather |contains=Sun", "false");
                        else if (toks[1] == "rainy") ret.Add("Weather |contains=Rain, Storm, Sun", "false");
                        else Log.Warn("Unknown weather precondition value: " + toks[1]);
                        break;
                    case "y":
                        if (toks[1] == "1") ret.Add("Year", "1");
                        else ret.Add("Query: {{Year}} >= " + toks[1], "true");
                        break;
                    case "!y":
                        if (toks[1] == "1") ret.Add("Query", "{{Year}} >= 2");
                        else ret.Add("Query: {{Year}} >= " + toks[1], "false");
                        break;
                    case "z":
                        ret.Add("Season |contains=" + req.Substring(2).Replace(" ", ", "), "false");
                        break;
                    case "!z":
                        ret.Add("Season |contains=" + req.Substring(3).Replace(" ", ", "), "true");
                        break;
                    case "e":
                    case "!k":
                        ret.Add("HasSeenEvent", req.Substring(2));
                        break;
                    case "!e":
                    case "k":
                        ret.Add("HasSeenEvent |contains=" + req.Substring(2), "false");
                        break;
                    case "t":
                    case "!t":
                        Log.Warn("`t` preconditions will no longer work, since shop entries are calculated at the beginning of the day! Skipping...");
                        break;
                    case "u":
                        ret.Add("Day", req.Substring(2));
                        break;
                    case "!u":
                        ret.Add("Day |contains=" + req.Substring(2), "false");
                        break;
                    case "n":
                        ret.Add("HasFlag", req.Substring(2));
                        break;
                    case "!n":
                        ret.Add("HasFlag |contains=" + req.Substring(2), "false");
                        break;
                    case "f":
                        for (int i = 1; i < toks.Length; i += 2)
                            ret.Add("Relationship:" + toks[i], (int.Parse(toks[i + 1]) / 250).ToString());
                        break;
                    // no idea how to do !f - maybe with query?
                    default:
                        Log.Warn("Unhandled event precondition: " + req);
                        break;
                }
            }
            return ret;
        }

        private static void DoShopEntry(List<DynamicGameAssets.PackData.ShopEntryPackData> shops, string packId, string objId, JsonAssets.PurchaseData data)
        {
            var entry = new DynamicGameAssets.PackData.ShopEntryPackData();
            switch (data.PurchaseFrom)
            {
                case "Dwarf": entry.ShopId = "Dwarf"; break;
                case "Clint": entry.ShopId = "Blacksmith"; break;
                case "Marnie": entry.ShopId = "AnimalSupplies"; break;
                case "Robin": entry.ShopId = "Carpenter"; break;
                case "Marlon": entry.ShopId = "AdventurerGuild"; break;
                case "Gus": entry.ShopId = "Saloon"; break;
                case "Sandy": entry.ShopId = "Sandy"; break;
                case "Pierre": entry.ShopId = "SeedShop"; break;
                case "Willy": entry.ShopId = "FishShop"; break;
                case "Harvey": entry.ShopId = "Hospital"; break;
                case "Maru": entry.ShopId = "Hospital"; break;
                case "Krobus": entry.ShopId = "Krobus"; break;
                case "HatMouse": entry.ShopId = "HatMouse"; break;
                default:
                    Log.Warn("Unhandled shop PurchaseFrom " + data.PurchaseFrom + "!");
                    entry.ShopId = data.PurchaseFrom;
                    break;
            }
            entry.Cost = data.PurchasePrice;
            entry.Item = new DynamicGameAssets.ItemAbstraction() { Value = $"{packId}/{objId}" };
            entry.EnableConditions = data.PurchaseRequirements.ConvertEpu();
            shops.Add(entry);
        }

        public static DynamicGameAssets.PackData.ObjectPackData ConvertObject(this JsonAssets.Data.ObjectData data, string packId, Dictionary<string, Dictionary<string, string>> i18n, List<DynamicGameAssets.PackData.ObjectPackData> objs, List<DynamicGameAssets.PackData.CraftingRecipePackData> crafting, List<DynamicGameAssets.PackData.ShopEntryPackData> shops, List<DynamicGameAssets.PackData.GiftTastePackData> giftTastes)
        {
            var item = new DynamicGameAssets.PackData.ObjectPackData();
            item.ExtensionData.Add("JsonAssetsName", JToken.FromObject(data.Name));
            item.ID = data.Name;
            item.Texture = Path.Combine("assets", "objects", data.Name + ".png");
            i18n.AddI18n("en", $"object.{data.Name}.name", data.Name);
            i18n.AddI18n("en", $"object.{data.Name}.description", data.Description);
            item.Category = (DynamicGameAssets.PackData.ObjectPackData.VanillaCategory)Enum.Parse(typeof(DynamicGameAssets.PackData.ObjectPackData.VanillaCategory), data.Category.ToString());
            i18n.AddI18n("en", $"object.{data.Name}.category", data.CategoryTextOverride);
            item.CategoryColorOverride = data.CategoryColorOverride;
            if (data.IsColored)
                item.TextureColor = Path.Combine("assets", "objects", data.Name + "_color.png");
            item.SellPrice = data.CanSell ? data.Price : null;
            item.CanTrash = data.CanTrash;
            item.IsGiftable = data.CanBeGifted;
            item.HideFromShippingCollection = data.HideFromShippingCollection;
            // recipe done elsewhere
            item.Edibility = data.Edibility;
            item.EdibleIsDrink = data.EdibleIsDrink;
            item.ContextTags = data.ContextTags;
            if (data.EdibleBuffs != null)
            {
                item.EdibleBuffs = new DynamicGameAssets.PackData.ObjectPackData.FoodBuffsData()
                {
                    Farming = data.EdibleBuffs.Farming,
                    Fishing = data.EdibleBuffs.Fishing,
                    Mining = data.EdibleBuffs.Mining,
                    Luck = data.EdibleBuffs.Luck,
                    Foraging = data.EdibleBuffs.Foraging,
                    MaxStamina = data.EdibleBuffs.MaxStamina,
                    MagnetRadius = data.EdibleBuffs.MaxStamina,
                    Speed = data.EdibleBuffs.Speed,
                    Defense = data.EdibleBuffs.Defense,
                    Attack = data.EdibleBuffs.Attack,
                    Duration = data.EdibleBuffs.Duration,
                };
            }
            if (data.CanPurchase)
            {
                ConversionExtensions.DoShopEntry(shops, packId, data.Name, new JsonAssets.PurchaseData()
                {
                    PurchasePrice = data.PurchasePrice,
                    PurchaseFrom = data.PurchaseFrom,
                    PurchaseRequirements = data.PurchaseRequirements,
                });
                foreach (var entry in data.AdditionalPurchaseData)
                {
                    ConversionExtensions.DoShopEntry(shops, packId, data.Name, entry);
                }
            }
            if (data.GiftTastes != null)
            {
                foreach (string taste in data.GiftTastes.Love)
                {
                    if (taste == "Universal")
                        item.UniversalGiftTaste = 80;
                    else
                        giftTastes.Add(new DynamicGameAssets.PackData.GiftTastePackData()
                        {
                            ObjectId = $"{packId}/{item.ID}",
                            Npc = taste,
                            Amount = 80
                        });
                }
                foreach (string taste in data.GiftTastes.Like)
                {
                    if (taste == "Universal")
                        item.UniversalGiftTaste = 45;
                    else
                        giftTastes.Add(new DynamicGameAssets.PackData.GiftTastePackData()
                        {
                            ObjectId = $"{packId}/{item.ID}",
                            Npc = taste,
                            Amount = 45
                        });
                }
                foreach (string taste in data.GiftTastes.Neutral)
                {
                    if (taste == "Universal")
                        item.UniversalGiftTaste = 20;
                    else
                        giftTastes.Add(new DynamicGameAssets.PackData.GiftTastePackData()
                        {
                            ObjectId = $"{packId}/{item.ID}",
                            Npc = taste,
                            Amount = 20
                        });
                }
                foreach (string taste in data.GiftTastes.Dislike)
                {
                    if (taste == "Universal")
                        item.UniversalGiftTaste = -20;
                    else
                        giftTastes.Add(new DynamicGameAssets.PackData.GiftTastePackData()
                        {
                            ObjectId = $"{packId}/{item.ID}",
                            Npc = taste,
                            Amount = -20
                        });
                }
                foreach (string taste in data.GiftTastes.Hate)
                {
                    if (taste == "Universal")
                        item.UniversalGiftTaste = -40;
                    else
                        giftTastes.Add(new DynamicGameAssets.PackData.GiftTastePackData()
                        {
                            ObjectId = $"{packId}/{item.ID}",
                            Npc = taste,
                            Amount = -40
                        });
                }
            }
            foreach (var loc in data.NameLocalization)
                i18n.AddI18n(loc.Key, $"object.{data.Name}.name", loc.Value);
            foreach (var loc in data.DescriptionLocalization)
                i18n.AddI18n(loc.Key, $"object.{data.Name}.description", loc.Value);
            objs.Add(item);

            return item;
        }

        public static DynamicGameAssets.PackData.CraftingRecipePackData ConvertCrafting(this JsonAssets.Data.ObjectData data, string packId, Dictionary<string, Dictionary<string, string>> i18n, List<DynamicGameAssets.PackData.ObjectPackData> objs, List<DynamicGameAssets.PackData.CraftingRecipePackData> crafting, List<DynamicGameAssets.PackData.ShopEntryPackData> shops)
        {
            if (data.Recipe != null)
            {
                var recipe = new DynamicGameAssets.PackData.CraftingRecipePackData();
                recipe.ID = "Converted_" + data.Name + " Recipe";
                i18n.AddI18n("en", $"crafting.Converted_{data.Name} Recipe.name", data.Name);
                i18n.AddI18n("en", $"crafting.Converted_{data.Name} Recipe.description", data.Description);
                recipe.IsCooking = data.Category == JsonAssets.Data.ObjectCategory.Cooking;
                recipe.SkillUnlockName = data.Recipe.SkillUnlockName;
                recipe.SkillUnlockLevel = data.Recipe.SkillUnlockLevel;
                recipe.KnownByDefault = data.Recipe.IsDefault;
                recipe.Result = new List<DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>>()
                {
                    new DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>( 1, new DynamicGameAssets.ItemAbstraction()
                    {
                        Value = $"{packId}/{data.Name}",
                        Type = DynamicGameAssets.ItemAbstraction.ItemType.DGAItem,
                    } )
                };
                recipe.Ingredients = new List<DynamicGameAssets.PackData.CraftingRecipePackData.IngredientAbstraction>();
                foreach (var ingred in data.Recipe.Ingredients)
                {
                    var productObj = objs.FirstOrDefault(o => o.ID == ingred.Object.ToString());
                    var newIngred = new DynamicGameAssets.PackData.CraftingRecipePackData.IngredientAbstraction()
                    {
                        Value = productObj != null ? $"{packId}/{productObj.ID}" : ingred.Object.ToString(),
                        Type = productObj != null ? DynamicGameAssets.ItemAbstraction.ItemType.DGAItem : DynamicGameAssets.ItemAbstraction.ItemType.VanillaObject,
                        Quantity = ingred.Count,
                    };
                    recipe.Ingredients.Add(newIngred);
                }
                crafting.Add(recipe);

                if (data.Recipe.CanPurchase)
                {
                    ConversionExtensions.DoShopEntry(shops, packId, recipe.ID, new JsonAssets.PurchaseData()
                    {
                        PurchasePrice = data.Recipe.PurchasePrice,
                        PurchaseFrom = data.Recipe.PurchaseFrom,
                        PurchaseRequirements = data.Recipe.PurchaseRequirements,
                    });
                    foreach (var entry in data.AdditionalPurchaseData)
                    {
                        ConversionExtensions.DoShopEntry(shops, packId, recipe.ID, entry);
                    }
                }

                return recipe;
            }

            return null;
        }

        public static DynamicGameAssets.PackData.CropPackData ConvertCrop(this JsonAssets.Data.CropData data, string packId, Dictionary<string, Dictionary<string, string>> i18n, List<DynamicGameAssets.PackData.CropPackData> crops, List<DynamicGameAssets.PackData.ObjectPackData> objs, List<DynamicGameAssets.PackData.ShopEntryPackData> shops)
        {
            var item = new DynamicGameAssets.PackData.CropPackData();
            item.ExtensionData.Add("JsonAssetsName", JToken.FromObject(data.Name));
            item.ID = data.Name + " Crop";
            switch (data.CropType)
            {
                case JsonAssets.Data.CropType.Normal: item.Type = DynamicGameAssets.PackData.CropPackData.CropType.Normal; break;
                case JsonAssets.Data.CropType.IndoorsOnly: item.Type = DynamicGameAssets.PackData.CropPackData.CropType.Indoors; break;
                case JsonAssets.Data.CropType.Paddy: item.Type = DynamicGameAssets.PackData.CropPackData.CropType.Paddy; break;
            }
            item.Colors = (data.Colors?.Count ?? 0) > 0 ? new List<Color>(data.Colors) : null;

            for (int i = 0; i <= data.Phases.Count; ++i)
            {
                var phase = new DynamicGameAssets.PackData.CropPackData.PhaseData();
                string texPathBase = Path.Combine("assets", "crops", item.ID + ".png");
                phase.TextureChoices = i == 0 ? (new string[] { $"{texPathBase}:0", $"{texPathBase}:1" }) : (new string[] { $"{texPathBase}:{i + 1}" });
                phase.Trellis = data.TrellisCrop;
                if (i < data.Phases.Count)
                {
                    phase.Length = data.Phases[i];
                }
                if (i == data.Phases.Count)
                {
                    if ((item.Colors?.Count ?? 0) > 0)
                        phase.TextureColorChoices = new string[] { $"{texPathBase}:{i + 2}" };

                    phase.Scythable = data.HarvestWithScythe;
                    if (data.RegrowthPhase != -1)
                    {
                        Log.Warn("Regrowth phase adjusted for " + data.Name + "; see documentation for details.");
                        phase.HarvestedNewPhase = data.Phases.Count - 2;
                    }

                    var productObj = objs.FirstOrDefault(o => o.ID == data.Product.ToString());
                    int productObjPrice = 0;
                    if (productObj != null)
                        productObjPrice = productObj.SellPrice.HasValue ? productObj.SellPrice.Value : 0;
                    else
                    {
                        if (data.Product is long id)
                            productObjPrice = int.Parse(Game1.objectInformation[(int)id].Split('/')[StardewValley.Object.objectInfoPriceIndex]);
                        else
                        {
                            foreach (var entry in Game1.objectInformation)
                            {
                                string[] split = entry.Value.Split('/');
                                if (split[StardewValley.Object.objectInfoNameIndex] == (string)data.Product)
                                {
                                    productObjPrice = int.Parse(split[StardewValley.Object.objectInfoPriceIndex]);
                                    break;
                                }
                            }
                        }
                    }

                    phase.HarvestedExperience = (int)Math.Round(16 * Math.Log(0.018 * productObjPrice + 1, Math.E));
                    var drop = new DynamicGameAssets.PackData.CropPackData.HarvestedDropData()
                    {
                        Item = new List<DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>>(new DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>[]
                        {
                            new DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>( 1, new DynamicGameAssets.ItemAbstraction()
                            {
                                Value = productObj != null ? $"{packId}/{productObj.ID}" : data.Product.ToString(),
                                Type = productObj != null ? DynamicGameAssets.ItemAbstraction.ItemType.DGAItem : ( item.Colors !=  null ? DynamicGameAssets.ItemAbstraction.ItemType.VanillaObjectColored : DynamicGameAssets.ItemAbstraction.ItemType.VanillaObject )
                            } )
                        }),
                        MininumHarvestedQuantity = data.Bonus?.MinimumPerHarvest ?? 1,
                        MaximumHarvestedQuantity = data.Bonus?.MaximumPerHarvest ?? 1,
                        ExtraQuantityChance = (double)(data.Bonus?.ExtraChance ?? 0),
                    };
                    phase.HarvestedDrops.Add(drop);
                }
                item.Phases.Add(phase);
            }

            var dynFields = new List<DynamicGameAssets.PackData.DynamicFieldData>();
            {
                var dynField = new DynamicGameAssets.PackData.DynamicFieldData();
                dynField.Conditions.Add("Season", string.Join(", ", data.Seasons));
                dynField.Fields.Add("CanGrowNow", JToken.FromObject(true));
                dynFields.Add(dynField);
            }
            if ((data.Bonus?.MaxIncreasePerFarmLevel ?? 0) > 0)
            {
                for (int i = 1; i <= 10; ++i)
                {
                    var dynField = new DynamicGameAssets.PackData.DynamicFieldData();
                    dynField.Conditions.Add("SkillLevel:Farming", i.ToString());
                    dynField.Fields.Add($"Phases[{item.Phases.Count - 1}].HarvestedDrops[0].MaximumHarvestedQuantity",
                                         JToken.FromObject(item.Phases[item.Phases.Count - 1].HarvestedDrops[0].MaximumHarvestedQuantity + data.Bonus.MaxIncreasePerFarmLevel * i));
                    dynFields.Add(dynField);
                }
            }
            item.DynamicFields = dynFields.ToArray();

            crops.Add(item);

            var seedsItem = new DynamicGameAssets.PackData.ObjectPackData();
            i18n.AddI18n("en", $"object.{data.SeedName}.name", data.SeedName);
            i18n.AddI18n("en", $"object.{data.SeedName}.description", data.SeedDescription);
            seedsItem.ID = data.SeedName;
            seedsItem.Texture = Path.Combine("assets", "objects", item.ID + "_seeds.png");
            seedsItem.Category = DynamicGameAssets.PackData.ObjectPackData.VanillaCategory.Seeds;
            seedsItem.SellPrice = data.SeedSellPrice == -1 ? null : data.SeedSellPrice;
            seedsItem.Plants = $"{packId}/{item.ID}";
            foreach (var loc in data.SeedNameLocalization)
                i18n.AddI18n(loc.Key, $"object.{data.SeedName}.name", loc.Value);
            foreach (var loc in data.SeedDescriptionLocalization)
                i18n.AddI18n(loc.Key, $"object.{data.SeedName}.description", loc.Value);
            objs.Add(seedsItem);

            data.SeedPurchaseRequirements.Add("z " + string.Join(" ", data.Seasons));
            ConversionExtensions.DoShopEntry(shops, packId, data.SeedName, new JsonAssets.PurchaseData()
            {
                PurchasePrice = data.SeedPurchasePrice,
                PurchaseFrom = data.SeedPurchaseFrom,
                PurchaseRequirements = data.SeedPurchaseRequirements,
            });
            foreach (var entry in data.SeedAdditionalPurchaseData)
            {
                ConversionExtensions.DoShopEntry(shops, packId, data.SeedName, entry);
            }

            return item;
        }

        public static DynamicGameAssets.PackData.FruitTreePackData ConvertFruitTree(this JsonAssets.Data.FruitTreeData data, string packId, Dictionary<string, Dictionary<string, string>> i18n, List<DynamicGameAssets.PackData.FruitTreePackData> fruitTrees, List<DynamicGameAssets.PackData.ObjectPackData> objs, List<DynamicGameAssets.PackData.ShopEntryPackData> shops)
        {
            var item = new DynamicGameAssets.PackData.FruitTreePackData();
            item.ExtensionData.Add("JsonAssetsName", JToken.FromObject(data.Name));
            item.ID = data.Name + " Tree";
            item.Texture = Path.Combine("assets", "fruit-trees", item.ID + ".png");
            var productObj = objs.FirstOrDefault(o => o.ID == data.Product.ToString());
            item.Product = new List<DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>>(new DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>[]
            {
                new DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>( 1, new DynamicGameAssets.ItemAbstraction()
                {
                    Value = productObj != null ? $"{packId}/{productObj.ID}" : data.Product.ToString(),
                    Type = productObj != null ? DynamicGameAssets.ItemAbstraction.ItemType.DGAItem : DynamicGameAssets.ItemAbstraction.ItemType.VanillaObject
                } )
            });
            var dynFields = new List<DynamicGameAssets.PackData.DynamicFieldData>();
            {
                var dynField = new DynamicGameAssets.PackData.DynamicFieldData();
                dynField.Conditions.Add("Season", data.Season);
                dynField.Fields.Add("CanGrowNow", JToken.FromObject(true));
                dynFields.Add(dynField);
            }
            item.DynamicFields = dynFields.ToArray();

            fruitTrees.Add(item);

            var saplingItem = new DynamicGameAssets.PackData.ObjectPackData();
            i18n.AddI18n("en", $"object.{data.SaplingName}.name", data.SaplingName);
            i18n.AddI18n("en", $"object.{data.SaplingName}.description", data.SaplingDescription);
            saplingItem.ID = data.SaplingName;
            saplingItem.Texture = Path.Combine("assets", "objects", item.ID + "_sapling.png");
            saplingItem.Category = DynamicGameAssets.PackData.ObjectPackData.VanillaCategory.Seeds;
            saplingItem.Plants = $"{packId}/{item.ID}";
            foreach (var loc in data.SaplingNameLocalization)
                i18n.AddI18n(loc.Key, $"object.{data.SaplingName}.name", loc.Value);
            foreach (var loc in data.SaplingDescriptionLocalization)
                i18n.AddI18n(loc.Key, $"object.{data.SaplingName}.description", loc.Value);
            objs.Add(saplingItem);

            data.SaplingPurchaseRequirements.Add("z " + data.Season);
            ConversionExtensions.DoShopEntry(shops, packId, data.SaplingName, new JsonAssets.PurchaseData()
            {
                PurchasePrice = data.SaplingPurchasePrice,
                PurchaseFrom = data.SaplingPurchaseFrom,
                PurchaseRequirements = data.SaplingPurchaseRequirements,
            });
            foreach (var entry in data.SaplingAdditionalPurchaseData)
            {
                ConversionExtensions.DoShopEntry(shops, packId, data.SaplingName, entry);
            }

            return item;
        }

        public static DynamicGameAssets.PackData.BigCraftablePackData ConvertBigCraftable(this JsonAssets.Data.BigCraftableData data, string packId, Dictionary<string, Dictionary<string, string>> i18n, List<DynamicGameAssets.PackData.BigCraftablePackData> bigCraftables, List<DynamicGameAssets.PackData.ObjectPackData> objs, List<DynamicGameAssets.PackData.CraftingRecipePackData> crafting, List<DynamicGameAssets.PackData.ShopEntryPackData> shops)
        {
            var item = new DynamicGameAssets.PackData.BigCraftablePackData();
            item.ExtensionData.Add("JsonAssetsName", JToken.FromObject(data.Name));
            item.ID = data.Name;
            item.Texture = Path.Combine("assets", "big-craftables", data.Name + "0.png");
            i18n.AddI18n("en", $"big-craftable.{data.Name}.name", data.Name);
            i18n.AddI18n("en", $"big-craftable.{data.Name}.description", data.Description);
            item.SellPrice = data.Price;
            item.ProvidesLight = data.ProvidesLight;
            if (data.Recipe != null)
            {
                var recipe = new DynamicGameAssets.PackData.CraftingRecipePackData();
                recipe.ID = "Converted_" + data.Name + " Recipe";
                i18n.AddI18n("en", $"crafting.Converted_{data.Name} Recipe.name", data.Name);
                i18n.AddI18n("en", $"crafting.Converted_{data.Name} Recipe.description", data.Description);
                recipe.SkillUnlockName = data.Recipe.SkillUnlockName;
                recipe.SkillUnlockLevel = data.Recipe.SkillUnlockLevel;
                recipe.KnownByDefault = data.Recipe.IsDefault;
                recipe.Result = new List<DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>>()
                {
                    new DynamicGameAssets.Weighted<DynamicGameAssets.ItemAbstraction>( 1, new DynamicGameAssets.ItemAbstraction()
                    {
                        Value = $"{packId}/{item.ID}",
                        Type = DynamicGameAssets.ItemAbstraction.ItemType.DGAItem,
                    } )
                };
                recipe.Ingredients = new List<DynamicGameAssets.PackData.CraftingRecipePackData.IngredientAbstraction>();
                foreach (var ingred in data.Recipe.Ingredients)
                {
                    var productObj = objs.FirstOrDefault(o => o.ID == ingred.Object.ToString());
                    var newIngred = new DynamicGameAssets.PackData.CraftingRecipePackData.IngredientAbstraction()
                    {
                        Value = productObj != null ? $"{packId}/{productObj.ID}" : ingred.Object.ToString(),
                        Type = productObj != null ? DynamicGameAssets.ItemAbstraction.ItemType.DGAItem : DynamicGameAssets.ItemAbstraction.ItemType.VanillaObject,
                        Quantity = ingred.Count,
                    };
                    recipe.Ingredients.Add(newIngred);
                }
                crafting.Add(recipe);

                if (data.Recipe.CanPurchase)
                {
                    ConversionExtensions.DoShopEntry(shops, packId, recipe.ID, new JsonAssets.PurchaseData()
                    {
                        PurchasePrice = data.Recipe.PurchasePrice,
                        PurchaseFrom = data.Recipe.PurchaseFrom,
                        PurchaseRequirements = data.Recipe.PurchaseRequirements,
                    });
                    foreach (var entry in data.AdditionalPurchaseData)
                    {
                        ConversionExtensions.DoShopEntry(shops, packId, recipe.ID, entry);
                    }
                }
            }
            foreach (var loc in data.NameLocalization)
                i18n.AddI18n(loc.Key, $"big-craftable.{data.Name}.name", loc.Value);
            foreach (var loc in data.DescriptionLocalization)
                i18n.AddI18n(loc.Key, $"big-craftable.{data.Name}.description", loc.Value);
            bigCraftables.Add(item);

            return item;
        }

        public static DynamicGameAssets.PackData.HatPackData ConvertHat(this JsonAssets.Data.HatData data, string packId, Dictionary<string, Dictionary<string, string>> i18n, List<DynamicGameAssets.PackData.HatPackData> hats, List<DynamicGameAssets.PackData.ShopEntryPackData> shops)
        {
            var item = new DynamicGameAssets.PackData.HatPackData();
            item.ExtensionData.Add("JsonAssetsName", JToken.FromObject(data.Name));
            item.ID = data.Name;
            item.Texture = Path.Combine("assets", "hats", data.Name + ".png");
            i18n.AddI18n("en", $"hat.{data.Name}.name", data.Name);
            i18n.AddI18n("en", $"hat.{data.Name}.description", data.Description);
            item.HairStyle = data.ShowHair ? DynamicGameAssets.PackData.HatPackData.HairStyleType.Full : DynamicGameAssets.PackData.HatPackData.HairStyleType.Hide;
            item.IgnoreHairstyleOffset = data.IgnoreHairstyleOffset;
            // todo: data.Metadata
            if (data.CanPurchase)
            {
                ConversionExtensions.DoShopEntry(shops, packId, item.ID, new JsonAssets.PurchaseData()
                {
                    PurchasePrice = data.PurchasePrice,
                    PurchaseFrom = "HatMouse",
                });
            }
            foreach (var loc in data.NameLocalization)
                i18n.AddI18n(loc.Key, $"hat.{data.Name}.name", loc.Value);
            foreach (var loc in data.DescriptionLocalization)
                i18n.AddI18n(loc.Key, $"hat.{data.Name}.description", loc.Value);

            hats.Add(item);

            return item;
        }

        public static DynamicGameAssets.PackData.MeleeWeaponPackData ConvertMeleeWeapon(this JsonAssets.Data.WeaponData data, string packId, Dictionary<string, Dictionary<string, string>> i18n, List<DynamicGameAssets.PackData.MeleeWeaponPackData> meleeWeapons, List<DynamicGameAssets.PackData.ShopEntryPackData> shops)
        {
            var item = new DynamicGameAssets.PackData.MeleeWeaponPackData();
            item.ExtensionData.Add("JsonAssetsName", JToken.FromObject(data.Name));
            item.ID = data.Name;
            item.Texture = Path.Combine("assets", "melee-weapons", data.Name + ".png");
            i18n.AddI18n("en", $"melee-weapon.{data.Name}.name", data.Name);
            i18n.AddI18n("en", $"melee-weapon.{data.Name}.description", data.Description);
            item.MinimumDamage = data.MinimumDamage;
            item.MaximumDamage = data.MaximumDamage;
            item.Knockback = data.Knockback;
            item.Speed = data.Speed;
            item.Accuracy = data.Accuracy;
            item.Defense = data.Defense;
            // todo: Mine drop var / minimum level ?
            item.ExtraSwingArea = data.ExtraSwingArea;
            item.CritChance = data.CritChance;
            item.CritMultiplier = data.CritMultiplier;
            item.CanTrash = data.CanTrash;

            if (data.CanPurchase)
            {
                ConversionExtensions.DoShopEntry(shops, packId, item.ID, new JsonAssets.PurchaseData()
                {
                    PurchasePrice = data.PurchasePrice,
                    PurchaseFrom = data.PurchaseFrom,
                    PurchaseRequirements = data.PurchaseRequirements,
                });
                foreach (var entry in data.AdditionalPurchaseData)
                    ConversionExtensions.DoShopEntry(shops, packId, item.ID, entry);
            }

            foreach (var loc in data.NameLocalization)
                i18n.AddI18n(loc.Key, $"melee-weapon.{data.Name}.name", loc.Value);
            foreach (var loc in data.DescriptionLocalization)
                i18n.AddI18n(loc.Key, $"melee-weapon.{data.Name}.description", loc.Value);

            meleeWeapons.Add(item);
            return item;
        }
    }
}
