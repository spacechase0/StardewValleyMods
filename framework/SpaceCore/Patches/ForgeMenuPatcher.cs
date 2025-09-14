using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Menus;
using StardewValley.Objects;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="ForgeMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ForgeMenuPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        private static CustomForgeRecipe justCrafted = null;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.GenerateHighlightDictionary)),
                postfix: this.GetHarmonyMethod(nameof(After_GenerateHighlightDictionary))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.IsValidCraft)),
                prefix: this.GetHarmonyMethod(nameof(Before_IsValidCraft))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.CraftItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_CraftItem))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.SpendLeftItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_SpendLeftItem))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.SpendRightItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_SpendRightItem))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.GetForgeCost)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetForgeCost))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>("_leftIngredientSpotClicked"),
                transpiler: this.GetHarmonyMethod(nameof(Transpile__leftIngredientSpotClicked))
            );

            harmony.Patch(
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.draw), new[] { typeof(SpriteBatch) }),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="ForgeMenu.GenerateHighlightDictionary"/>.</summary>
        private static void After_GenerateHighlightDictionary(ForgeMenu __instance)
        {
            var this__highlightDictionary_ = SpaceCore.Instance.Helper.Reflection.GetField<Dictionary<Item, bool>>(__instance, "_highlightDictionary");

            var this__highlightDictionary = this__highlightDictionary_.GetValue();
            List<Item> item_list = new List<Item>(__instance.inventory.actualInventory);
            if (Game1.player.leftRing.Value != null)
            {
                item_list.Add(Game1.player.leftRing.Value);
            }
            if (Game1.player.rightRing.Value != null)
            {
                item_list.Add(Game1.player.rightRing.Value);
            }
            foreach (Item item in item_list)
            {
                if (item == null)
                {
                    continue;
                }
                if (__instance.leftIngredientSpot.item == null && __instance.rightIngredientSpot.item == null)
                {
                    foreach (var recipe in CustomForgeRecipe.Recipes)
                    {
                        if (recipe.BaseItem.HasEnoughFor(item) || recipe.IngredientItem.HasEnoughFor(item))
                            this__highlightDictionary[item] = true;
                    }
                }
            }
        }

        /// <summary>The method to call before <see cref="ForgeMenu.IsValidCraft"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_IsValidCraft(ForgeMenu __instance, Item left_item, Item right_item, ref bool __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach (var recipe in CustomForgeRecipe.Recipes)
            {
                if (recipe.BaseItem.HasEnoughFor(left_item) && recipe.IngredientItem.HasEnoughFor(right_item))
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.SpendLeftItem"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_SpendLeftItem(ForgeMenu __instance)
        {
            if (ForgeMenuPatcher.justCrafted != null)
            {
                ForgeMenuPatcher.justCrafted.BaseItem.Consume(ref __instance.leftIngredientSpot.item);
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.SpendLeftItem"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_SpendRightItem(ForgeMenu __instance)
        {
            if (ForgeMenuPatcher.justCrafted != null)
            {
                ForgeMenuPatcher.justCrafted.IngredientItem.Consume(ref __instance.rightIngredientSpot.item);
                ForgeMenuPatcher.justCrafted = null;
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.CraftItem"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_CraftItem(ForgeMenu __instance, Item left_item, Item right_item, bool forReal, ref Item __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach (var recipe in CustomForgeRecipe.Recipes)
            {
                if (recipe.BaseItem.HasEnoughFor(left_item) && recipe.IngredientItem.HasEnoughFor(right_item))
                {
                    if (forReal)
                        ForgeMenuPatcher.justCrafted = recipe;
                    __result = recipe.CreateResult(left_item, right_item);
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.GetForgeCost"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_GetForgeCost(ForgeMenu __instance, Item left_item, Item right_item, ref int __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach (var recipe in CustomForgeRecipe.Recipes)
            {
                if (recipe.BaseItem.HasEnoughFor(left_item) && recipe.IngredientItem.HasEnoughFor(right_item))
                {
                    __result = recipe.CinderShardCost;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method which transpiles <see cref="ForgeMenu.draw(SpriteBatch)"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile__leftIngredientSpotClicked(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator ilgen)
        {
            List<CodeInstruction> insns = new();
            insns.AddRange(instructions);

            for (int i = 0; i < insns.Count - 1; ++i)
            {
                if (insns[i].opcode == OpCodes.Ret)
                    insns[i].opcode = OpCodes.Nop;
            }

            return insns;
        }


        /// <summary>The method which transpiles <see cref="ForgeMenu.draw(SpriteBatch)"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Draw(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator ilgen)
        {
            var insns = instructions.MethodReplacer(
                from: PatchHelper.RequireMethod<ForgeMenu>(nameof(ForgeMenu.GetForgeCost)),
                to: PatchHelper.RequireMethod<ForgeMenuPatcher>(nameof(GetAndDrawCost))
            );

            List<CodeInstruction> ret = new();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldfld && (insn.operand as FieldInfo).Name == "equipmentIcons")
                {
                    int insertAt = ret.Count; // Weird spot to add my instructions (in between a ldloc0 and using it), but it works well with the label nonsense going on

                    Label label1 = ilgen.DefineLabel();
                    Label label2 = ilgen.DefineLabel();
                    insn.labels.Add(label2);

                    ret.InsertRange(insertAt,
                        new CodeInstruction[]
                        {
                            new CodeInstruction(OpCodes.Ldloc_3),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ForgeMenuPatcher), nameof(IsLeftCraftIngredient))),
                            new CodeInstruction(OpCodes.Brfalse, label1),
                            new CodeInstruction(OpCodes.Ldc_I4_1),
                            new CodeInstruction(OpCodes.Stloc_1),
                            new CodeInstruction(OpCodes.Ldloc_3) { labels = { label1 } },
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ForgeMenuPatcher), nameof(IsRightCraftIngredient))),
                            new CodeInstruction(OpCodes.Brfalse, label2),
                            new CodeInstruction(OpCodes.Ldc_I4_1),
                            new CodeInstruction(OpCodes.Stloc_2),
                        });
                }

                ret.Add(insn);
            }

            return ret;
        }

        private static int GetAndDrawCost(ForgeMenu forgeMenu, Item leftItem, Item rightItem)
        {
            int cost = forgeMenu.GetForgeCost(forgeMenu.leftIngredientSpot.item, forgeMenu.rightIngredientSpot.item);

            if (cost is not (10 or 15 or 20))
                Game1.spriteBatch.DrawString(Game1.dialogueFont, "x" + cost, new Vector2(forgeMenu.xPositionOnScreen + 345, forgeMenu.yPositionOnScreen + 320), new Color(226, 124, 65));

            return cost;
        }

        private static bool IsLeftCraftIngredient(Item item)
        {
            if (item == null)
                return false;
            foreach (var recipe in CustomForgeRecipe.Recipes)
            {
                if (recipe.BaseItem.HasEnoughFor(item))
                    return true;
            }

            return false;
        }

        private static bool IsRightCraftIngredient(Item item)
        {
            if (item == null)
                return false;
            foreach (var recipe in CustomForgeRecipe.Recipes)
            {
                if (recipe.IngredientItem.HasEnoughFor(item))
                    return true;
            }

            return false;
        }
    }
}
