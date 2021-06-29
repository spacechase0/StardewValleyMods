using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CookingSkill.Framework;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CookingSkill.Patches
{
    /// <summary>Applies Harmony patches to <see cref="CraftingRecipe"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CraftingRecipePatcher : BasePatcher
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to actually consume items for the current recipe.</summary>
        public static bool ShouldConsumeItems { get; set; }

        /// <summary>The items consumed by the last recipe, if any.</summary>
        public static IList<ConsumedItem> LastUsedItems { get; } = new List<ConsumedItem>();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<CraftingRecipe>(nameof(CraftingRecipe.consumeIngredients)),
                prefix: this.GetHarmonyMethod(nameof(Before_ConsumeIngredients))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="CraftingRecipe.consumeIngredients"/>.</summary>
        /// <returns>Returns whether to skip the original method.</returns>
        /// <remarks>This is copied verbatim from the original method with some changes (marked with comments).</remarks>
        public static bool Before_ConsumeIngredients(ref CraftingRecipe __instance, List<Chest> additional_materials)
        {
            CraftingRecipePatcher.LastUsedItems.Clear();
            var recipe = __instance;

            Dictionary<int, int> recipeList = recipe.recipeList;
            for (int index1 = recipeList.Count - 1; index1 >= 0; --index1)
            {
                int recipe1 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                bool flag = false;
                for (int index2 = Game1.player.Items.Count - 1; index2 >= 0; --index2)
                {
                    if (Game1.player.Items[index2] is SObject obj && !obj.bigCraftable.Value && (obj.ParentSheetIndex == recipeList.Keys.ElementAt<int>(index1) || obj.Category == recipeList.Keys.ElementAt<int>(index1) || CraftingRecipe.isThereSpecialIngredientRule(obj, recipeList.Keys.ElementAt<int>(index1))))
                    {
                        int recipe2 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                        recipe1 -= obj.Stack;

                        // custom code begins
                        CraftingRecipePatcher.LastUsedItems.Add(new ConsumedItem(obj));
                        if (CraftingRecipePatcher.ShouldConsumeItems)
                        {
                            // custom code ends
                            obj.Stack -= recipe2;
                            if (obj.Stack <= 0)
                                Game1.player.Items[index2] = null;
                        }
                        if (recipe1 <= 0)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (additional_materials != null && !flag)
                {
                    foreach (Chest additionalMaterial in additional_materials)
                    {
                        if (additionalMaterial == null)
                            continue;

                        for (int index3 = additionalMaterial.items.Count - 1; index3 >= 0; --index3)
                        {
                            if (additionalMaterial.items[index3] != null && additionalMaterial.items[index3] is SObject && (additionalMaterial.items[index3].ParentSheetIndex == recipeList.Keys.ElementAt<int>(index1) || additionalMaterial.items[index3].Category == recipeList.Keys.ElementAt<int>(index1) || CraftingRecipe.isThereSpecialIngredientRule((SObject)additionalMaterial.items[index3], recipeList.Keys.ElementAt<int>(index1))))
                            {
                                int num = Math.Min(recipe1, additionalMaterial.items[index3].Stack);
                                recipe1 -= num;
                                // custom code begins
                                CraftingRecipePatcher.LastUsedItems.Add(new ConsumedItem(additionalMaterial.items[index3] as SObject));
                                if (CraftingRecipePatcher.ShouldConsumeItems)
                                {
                                    // custom code ends
                                    additionalMaterial.items[index3].Stack -= num;
                                    if (additionalMaterial.items[index3].Stack <= 0)
                                        additionalMaterial.items[index3] = null;
                                }
                                if (recipe1 <= 0)
                                    break;
                            }
                        }
                        if (recipe1 <= 0)
                            break;
                    }
                }
            }

            return true;
        }
    }
}
