using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crafting;
using System;
using System.Collections.Generic;

namespace JsonAssets
{
    public class ContentInjector : IAssetEditor
    {
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data\\ObjectInformation"))
                return true;
            if (asset.AssetNameEquals("Data\\ObjectContextTags"))
                return true;
            if (asset.AssetNameEquals("Data\\Crops"))
                return true;
            if (asset.AssetNameEquals("Data\\fruitTrees"))
                return true;
            if (asset.AssetNameEquals("Data\\CookingRecipes"))
                return true;
            if (asset.AssetNameEquals("Data\\CraftingRecipes"))
                return true;
            if (asset.AssetNameEquals("Data\\BigCraftablesInformation"))
                return true;
            if (asset.AssetNameEquals("Data\\hats"))
                return true;
            if (asset.AssetNameEquals("Data\\weapons"))
                return true;
            if (asset.AssetNameEquals("Data\\NPCGiftTastes"))
                return true;
            if (asset.AssetNameEquals("Data\\ClothingInformation"))
                return true;
            if (asset.AssetNameEquals("Data\\TailoringRecipes"))
                return true;
            if (asset.AssetNameEquals("Maps\\springobjects"))
                return true;
            if (asset.AssetNameEquals("TileSheets\\crops"))
                return true;
            if (asset.AssetNameEquals("TileSheets\\fruitTrees"))
                return true;
            if (asset.AssetNameEquals("TileSheets\\Craftables") || asset.AssetNameEquals("TileSheets\\Craftables_indoor") || asset.AssetNameEquals("TileSheets\\Craftables_outdoor"))
                return true; // _indoor/_outdoor for Seasonal Immersion compat
            if (asset.AssetNameEquals("Characters\\Farmer\\hats"))
                return true;
            if (asset.AssetNameEquals("TileSheets\\weapons"))
                return true;
            if (asset.AssetNameEquals("Characters\\Farmer\\shirts"))
                return true;
            if (asset.AssetNameEquals("Characters\\Farmer\\pants"))
                return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data\\ObjectInformation"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var obj in Mod.instance.objects)
                {
                    try
                    {
                        Log.verbose($"Injecting to objects: {obj.GetObjectId()}: {obj.GetObjectInformation()}");
                        data.Add(obj.GetObjectId(), obj.GetObjectInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting object information for {obj.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\ObjectContextTags"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                foreach (var obj in Mod.instance.objects)
                {
                    try
                    {
                        string tags = string.Join(", ", obj.ContextTags);
                        Log.verbose($"Injecting to object context tags: {obj.Name}: {tags}");
                        if (data.ContainsKey(obj.Name) && data[obj.Name] == "")
                            data[obj.Name] = tags;
                        else
                            data.Add(obj.Name, tags);
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting object context tags for {obj.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\Crops"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var crop in Mod.instance.crops)
                {
                    try
                    {
                        Log.verbose($"Injecting to crops: {crop.GetSeedId()}: {crop.GetCropInformation()}");
                        data.Add(crop.GetSeedId(), crop.GetCropInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting crop for {crop.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\fruitTrees"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var fruitTree in Mod.instance.fruitTrees)
                {
                    try
                    {
                        Log.verbose($"Injecting to fruit trees: {fruitTree.GetSaplingId()}: {fruitTree.GetFruitTreeInformation()}");
                        data.Add(fruitTree.GetSaplingId(), fruitTree.GetFruitTreeInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting fruit tree for {fruitTree.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\CookingRecipes"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                foreach (var obj in Mod.instance.objects)
                {
                    try
                    {
                        if (obj.Recipe == null)
                            continue;
                        if (obj.Category != ObjectData.Category_.Cooking)
                            continue;
                        Log.verbose($"Injecting to cooking recipes: {obj.Name}: {obj.Recipe.GetRecipeString(obj)}");
                        data.Add(obj.Name, obj.Recipe.GetRecipeString(obj));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting cooking recipe for {obj.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\CraftingRecipes"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                foreach (var obj in Mod.instance.objects)
                {
                    try
                    {
                        if (obj.Recipe == null)
                            continue;
                        if (obj.Category == ObjectData.Category_.Cooking)
                            continue;
                        Log.verbose($"Injecting to crafting recipes: {obj.Name}: {obj.Recipe.GetRecipeString(obj)}");
                        data.Add(obj.Name, obj.Recipe.GetRecipeString(obj));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting crafting recipe for {obj.Name}: {e}");
                    }
                }
                foreach (var big in Mod.instance.bigCraftables)
                {
                    try
                    {
                        if (big.Recipe == null)
                            continue;
                        Log.verbose($"Injecting to crafting recipes: {big.Name}: {big.Recipe.GetRecipeString(big)}");
                        data.Add(big.Name, big.Recipe.GetRecipeString(big));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting crafting recipe for {big.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\BigCraftablesInformation"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var big in Mod.instance.bigCraftables)
                {
                    try
                    {
                        Log.verbose($"Injecting to big craftables: {big.GetCraftableId()}: {big.GetCraftableInformation()}");
                        data.Add(big.GetCraftableId(), big.GetCraftableInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting object information for {big.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\hats"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var hat in Mod.instance.hats)
                {
                    try
                    {
                        Log.verbose($"Injecting to hats: {hat.GetHatId()}: {hat.GetHatInformation()}");
                        data.Add(hat.GetHatId(), hat.GetHatInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting hat information for {hat.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\weapons"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var weapon in Mod.instance.weapons)
                {
                    try
                    {
                        Log.verbose($"Injecting to weapons: {weapon.GetWeaponId()}: {weapon.GetWeaponInformation()}");
                        data.Add(weapon.GetWeaponId(), weapon.GetWeaponInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting weapon information for {weapon.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\NPCGiftTastes"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                // TODO: This could be optimized from mn to... m + n?
                // Basically, iterate through objects and create Dictionary<NPC name, GiftData[]>
                // Iterate through objects, each section and add to dict[npc][approp. section]
                // Point is, I'm doing this the lazy way right now
                var newData = new Dictionary<string, string>(data);
                foreach (var npc in data)
                {
                    if (npc.Key.StartsWith("Universal_"))
                        continue;

                    string[] sections = npc.Value.Split('/');
                    if ( sections.Length != 11 )
                    {
                        Log.warn($"Bad gift taste data for {npc.Key}!");
                        continue;
                    }

                    string loveStr = sections[0];
                    List<string> loveIds = new List<string>(sections[1].Split(' '));
                    string likeStr = sections[2];
                    List<string> likeIds = new List<string>(sections[3].Split(' '));
                    string dislikeStr = sections[4];
                    List<string> dislikeIds = new List<string>(sections[5].Split(' '));
                    string hateStr = sections[6];
                    List<string> hateIds = new List<string>(sections[7].Split(' '));
                    string neutralStr = sections[8];
                    List<string> neutralIds = new List<string>(sections[9].Split(' '));

                    foreach ( var obj in Mod.instance.objects )
                    {
                        if (obj.GiftTastes == null)
                            continue;
                        if (obj.GiftTastes.Love != null && obj.GiftTastes.Love.Contains(npc.Key))
                            loveIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Like != null && obj.GiftTastes.Like.Contains(npc.Key))
                            likeIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Neutral != null && obj.GiftTastes.Neutral.Contains(npc.Key))
                            neutralIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Dislike != null && obj.GiftTastes.Dislike.Contains(npc.Key))
                            dislikeIds.Add(obj.GetObjectId().ToString());
                        if (obj.GiftTastes.Hate != null && obj.GiftTastes.Hate.Contains(npc.Key))
                            hateIds.Add(obj.GetObjectId().ToString());
                    }

                    string loveIdStr = string.Join(" ", loveIds);
                    string likeIdStr = string.Join(" ", likeIds);
                    string dislikeIdStr = string.Join(" ", dislikeIds);
                    string hateIdStr = string.Join(" ", hateIds);
                    string neutralIdStr = string.Join(" ", neutralIds);
                    newData[npc.Key] = $"{loveStr}/{loveIdStr}/{likeStr}/{likeIdStr}/{dislikeStr}/{dislikeIdStr}/{hateStr}/{hateIdStr}/{neutralStr}/{neutralIdStr}/ ";

                    Log.verbose($"Adding gift tastes for {npc.Key}: {newData[npc.Key]}");
                }
                asset.ReplaceWith(newData);
            }
            else if (asset.AssetNameEquals("Data\\ClothingInformation"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var shirt in Mod.instance.shirts)
                {
                    try
                    {
                        Log.verbose($"Injecting to clothing information: {shirt.GetClothingId()}: {shirt.GetClothingInformation()}");
                        data.Add(shirt.GetClothingId(), shirt.GetClothingInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting clothing information for {shirt.Name}: {e}");
                    }
                }
                foreach (var pants in Mod.instance.pantss)
                {
                    try
                    {
                        Log.verbose($"Injecting to clothing information: {pants.GetClothingId()}: {pants.GetClothingInformation()}");
                        data.Add(pants.GetClothingId(), pants.GetClothingInformation());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting clothing information for {pants.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Data\\TailoringRecipes"))
            {
                var data = asset.GetData<List<TailorItemRecipe>>();
                foreach (var recipe in Mod.instance.tailoring)
                {
                    try
                    {
                        Log.verbose($"Injecting to tailoring recipe: {recipe.ToGameData()}");
                        data.Add(recipe.ToGameData());
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting tailoring recipe: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Maps\\springobjects"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);

                foreach (var obj in Mod.instance.objects)
                {
                    try
                    {
                        Log.verbose($"Injecting {obj.Name} sprites @ {objectRect(obj.GetObjectId())}");
                        asset.AsImage().PatchImage(obj.texture, null, objectRect(obj.GetObjectId()));
                        if (obj.IsColored)
                        {
                            Log.verbose($"Injecting {obj.Name} color sprites @ {objectRect(obj.GetObjectId() + 1)}");
                            asset.AsImage().PatchImage(obj.textureColor, null, objectRect(obj.GetObjectId() + 1));
                        }
                    }
                    catch ( Exception e )
                    {
                        Log.error($"Exception injecting sprite for {obj.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("TileSheets\\crops"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);

                foreach (var crop in Mod.instance.crops)
                {
                    try
                    {
                        Log.verbose($"Injecting {crop.Name} crop images @ {cropRect(crop.GetCropSpriteIndex())}");
                        asset.AsImage().PatchExtendedTileSheet(crop.texture, null, cropRect(crop.GetCropSpriteIndex()));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting crop sprite for {crop.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("TileSheets\\fruitTrees"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);

                foreach (var fruitTree in Mod.instance.fruitTrees)
                {
                    try
                    {
                        Log.verbose($"Injecting {fruitTree.Name} fruit tree images @ {fruitTreeRect(fruitTree.GetFruitTreeIndex())}");
                        asset.AsImage().PatchExtendedTileSheet(fruitTree.texture, null, fruitTreeRect(fruitTree.GetFruitTreeIndex()));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting fruit tree sprite for {fruitTree.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("TileSheets\\Craftables") || asset.AssetNameEquals("TileSheets\\Craftables_indoor") || asset.AssetNameEquals("TileSheets\\Craftables_outdoor"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);
                Log.trace($"Big craftables are now ({oldTex.Width}, {Math.Max(oldTex.Height, 4096)})");

                foreach (var big in Mod.instance.bigCraftables)
                {
                    try
                    {
                        Log.verbose($"Injecting {big.Name} sprites @ {bigCraftableRect(big.GetCraftableId())}");
                        asset.AsImage().PatchImage(big.texture, null, bigCraftableRect(big.GetCraftableId()));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting sprite for {big.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Characters\\Farmer\\hats"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);
                Log.trace($"Hats are now ({oldTex.Width}, {Math.Max(oldTex.Height, 4096)})");

                foreach (var hat in Mod.instance.hats)
                {
                    try
                    {
                        Log.verbose($"Injecting {hat.Name} sprites @ {hatRect(hat.GetHatId())}");
                        asset.AsImage().PatchImage(hat.texture, null, hatRect(hat.GetHatId()));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting sprite for {hat.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("TileSheets\\weapons"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);
                Log.trace($"Weapons are now ({oldTex.Width}, {Math.Max(oldTex.Height, 4096)})");

                foreach (var weapon in Mod.instance.weapons)
                {
                    try
                    {
                        Log.verbose($"Injecting {weapon.Name} sprites @ {weaponRect(weapon.GetWeaponId())}");
                        asset.AsImage().PatchImage(weapon.texture, null, weaponRect(weapon.GetWeaponId()));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting sprite for {weapon.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Characters\\Farmer\\shirts"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);
                Log.trace($"Shirts are now ({oldTex.Width}, {Math.Max(oldTex.Height, 4096)})");

                foreach (var shirt in Mod.instance.shirts)
                {
                    try
                    {
                        string rects = shirtRectPlain(shirt.GetMaleIndex()).ToString();
                        if (shirt.Dyeable)
                            rects += ", " + shirtRectDye(shirt.GetMaleIndex()).ToString();
                        if (shirt.HasFemaleVariant)
                        {
                            rects += ", " + shirtRectPlain(shirt.GetFemaleIndex()).ToString();
                            if (shirt.Dyeable)
                                rects += ", " + shirtRectDye(shirt.GetFemaleIndex()).ToString();
                        }

                        Log.verbose($"Injecting {shirt.Name} sprites @ {rects}");
                        asset.AsImage().PatchExtendedTileSheet(shirt.textureMale, null, shirtRectPlain(shirt.GetMaleIndex()));
                        if (shirt.Dyeable)
                            asset.AsImage().PatchExtendedTileSheet(shirt.textureMaleColor, null, shirtRectDye(shirt.GetMaleIndex()));
                        if (shirt.HasFemaleVariant)
                        {
                            asset.AsImage().PatchExtendedTileSheet(shirt.textureFemale, null, shirtRectPlain(shirt.GetFemaleIndex()));
                            if (shirt.Dyeable)
                                asset.AsImage().PatchExtendedTileSheet(shirt.textureFemaleColor, null, shirtRectDye(shirt.GetFemaleIndex()));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting sprite for {shirt.Name}: {e}");
                    }
                }
            }
            else if (asset.AssetNameEquals("Characters\\Farmer\\pants"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);
                Log.trace($"Pants are now ({oldTex.Width}, {Math.Max(oldTex.Height, 4096)})");

                foreach (var pants in Mod.instance.pantss)
                {
                    try
                    {
                        Log.verbose($"Injecting {pants.Name} sprites @ {pantsRect(pants.GetTextureIndex())}");
                        asset.AsImage().PatchExtendedTileSheet(pants.texture, null, pantsRect(pants.GetTextureIndex()));
                    }
                    catch (Exception e)
                    {
                        Log.error($"Exception injecting sprite for {pants.Name}: {e}");
                    }
                }
            }
        }
        internal static Rectangle objectRect(int index)
        {
            return new Rectangle(index % 24 * 16, index / 24 * 16, 16, 16);
        }
        internal static Rectangle cropRect(int index)
        {
            return new Rectangle(index % 2 * 128, index / 2 * 32, 128, 32);
        }
        internal static Rectangle fruitTreeRect(int index)
        {
            return new Rectangle(0, index * 80, 432, 80);
        }
        internal static Rectangle bigCraftableRect(int index)
        {
            return new Rectangle(index % 8 * 16, index / 8 * 32, 16, 32);
        }
        internal static Rectangle hatRect(int index)
        {
            return new Rectangle(index % 12 * 20, index / 12 * 80, 20, 80);
        }
        internal static Rectangle weaponRect(int index)
        {
            return new Rectangle(index % 8 * 16, index / 8 * 16, 16, 16);
        }
        internal static Rectangle shirtRectPlain(int index)
        {
            return new Rectangle(index % 16 * 8, index / 16 * 32, 8, 32);
        }
        internal static Rectangle shirtRectDye(int index)
        {
            var rect = shirtRectPlain(index);
            rect.X += 16 * 8;
            return rect;
        }
        internal static Rectangle pantsRect(int index)
        {
            return new Rectangle(index % 10 * 192, index / 10 * 688, 192, 688);
        }
    }
}
