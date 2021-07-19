using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="ForgeMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ForgeMenuPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.IsValidCraft)),
                prefix: this.GetHarmonyMethod(nameof(Before_IsValidCraft))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.CraftItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_CraftItem))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.GetForgeCost)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetForgeCost))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.draw), new[] { typeof(SpriteBatch) }),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="ForgeMenu.IsValidCraft"/>.</summary>
        private static bool Before_IsValidCraft(ForgeMenu __instance, Item left_item, Item right_item, ref bool __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach (var recipe in Mod.instance.Forge)
            {
                bool isMatch =
                    left_item.Name == recipe.BaseItemName
                    && right_item.GetContextTags().Contains(recipe.IngredientContextTag)
                    && (!recipe.AbleToForgeConditions.Any() || Mod.instance.Epu.CheckConditions(recipe.AbleToForgeConditions));

                if (isMatch)
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.CraftItem"/>.</summary>
        private static bool Before_CraftItem(ForgeMenu __instance, Item left_item, Item right_item, bool forReal, ref Item __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach (var recipe in Mod.instance.Forge)
            {
                bool isMatch =
                    left_item.Name == recipe.BaseItemName
                    && right_item.GetContextTags().Contains(recipe.IngredientContextTag)
                    && (!recipe.AbleToForgeConditions.Any() || Mod.instance.Epu.CheckConditions(recipe.AbleToForgeConditions));

                if (isMatch)
                {
                    __result = Utility.fuzzyItemSearch(recipe.ResultItemName);
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.GetForgeCost"/>.</summary>
        private static bool Before_GetForgeCost(ForgeMenu __instance, Item left_item, Item right_item, ref int __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach (var recipe in Mod.instance.Forge)
            {
                bool isMatch =
                    left_item.Name == recipe.BaseItemName
                    && right_item.GetContextTags().Contains(recipe.IngredientContextTag)
                    && (!recipe.AbleToForgeConditions.Any() || Mod.instance.Epu.CheckConditions(recipe.AbleToForgeConditions));

                if (isMatch)
                {
                    __result = recipe.CinderShardCost;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method which transpiles <see cref="ForgeMenu.draw(SpriteBatch)"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Draw(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                from: PatchHelper.RequireMethod<ForgeMenu>(nameof(ForgeMenu.GetForgeCost)),
                to: PatchHelper.RequireMethod<ForgeMenuPatcher>(nameof(ForgeMenuPatcher.GetAndDrawCost))
            );
        }

        private static int GetAndDrawCost(ForgeMenu forgeMenu, Item leftItem, Item rightItem)
        {
            int cost = forgeMenu.GetForgeCost(forgeMenu.leftIngredientSpot.item, forgeMenu.rightIngredientSpot.item);

            if (cost is not (10 or 15 or 20))
                Game1.spriteBatch.DrawString(Game1.dialogueFont, "x" + cost, new Vector2(forgeMenu.xPositionOnScreen + 345, forgeMenu.yPositionOnScreen + 320), new Color(226, 124, 65));

            return cost;
        }
    }
}
