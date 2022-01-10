using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Framework;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crafting;
using StardewValley.Menus;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="TailoringMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class TailoringMenuPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<TailoringMenu>(nameof(TailoringMenu.GetRecipeForItems)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetRecipeForItems))
            );
            harmony.Patch(
                original: this.RequireMethod<TailoringMenu>(nameof(TailoringMenu.CraftItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_CraftItem))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="TailoringMenu.GetRecipeForItems"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_GetRecipeForItems(TailoringMenu __instance, Item left_item, Item right_item, ref TailorItemRecipe __result)
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
                        CraftedItemIDs = new List<string>(new[] { "DGA/TailoringRecipe/" + i })
                    };
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="TailoringMenu.CraftItem"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_CraftItem(TailoringMenu __instance, Item left_item, Item right_item, ref Item __result)
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
