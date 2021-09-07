using System.Collections.Generic;
using HarmonyLib;
using StardewValley;
using StardewValley.GameData.Crafting;
using StardewValley.Menus;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch(typeof(TailoringMenu), nameof(TailoringMenu.GetRecipeForItems))]
    public static class TailoringMenuGetRecipePatch
    {
        public static bool Prefix(TailoringMenu __instance, Item left_item, Item right_item, ref TailorItemRecipe __result)
        {
            if (left_item == null || right_item == null)
                return true;

            for (int i = 0; i < Mod.customTailoringRecipes.Count; ++i)
            {
                var recipe = Mod.customTailoringRecipes[i];

                bool okay = true;

                foreach (string tag in recipe.FirstItemTags)
                {
                    if (!left_item.HasContextTag(tag))
                    {
                        okay = false;
                        break;
                    }
                }
                if (!okay)
                    continue;
                foreach (string tag in recipe.SecondItemTags)
                {
                    if (!right_item.HasContextTag(tag))
                    {
                        okay = false;
                        break;
                    }
                }

                if (okay)
                {
                    __result = new TailorItemRecipe()
                    {
                        FirstItemTags = recipe.FirstItemTags,
                        SecondItemTags = recipe.SecondItemTags,
                        SpendRightItem = recipe.ConsumeSecondItem,
                        CraftedItemIDs = new List<string>(new string[] { "DGA/TailoringRecipe/" + i })
                    };
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TailoringMenu), nameof(TailoringMenu.CraftItem))]
    public static class TailoringMenuCraftPatch
    {
        public static bool Prefix(TailoringMenu __instance, Item left_item, Item right_item, ref Item __result)
        {
            if (left_item == null || right_item == null)
                return false;

            var recipe = __instance.GetRecipeForItems(left_item, right_item);
            if (recipe != null && recipe.CraftedItemIDs != null && recipe.CraftedItemIDs.Count == 1 && recipe.CraftedItemIDs[0].StartsWith("DGA/TailoringRecipe/"))
            {
                var customRecipe = Mod.customTailoringRecipes[int.Parse(recipe.CraftedItemIDs[0].Substring("DGA/TailoringRecipe/".Length))];
                __result = customRecipe.CraftedItem.Choose().Create();
                return false;
            }

            return true;
        }
    }
}
