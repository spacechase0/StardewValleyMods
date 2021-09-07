using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceCore.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="ForgeMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ForgeMenuPatcher : BasePatcher
    {
        private static CustomForgeRecipe justCrafted = null;

        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            /*harmony.Patch(
                original: this.RequireMethod<ForgeMenu>( nameof( ForgeMenu.GenerateHighlightDictionary ) ),
                prefix: this.GetHarmonyMethod( nameof( Before_GenerateHighlightDictionary ) )
            );*/

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
                original: this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.draw), new[] { typeof(SpriteBatch) }),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="ForgeMenu.GenerateHighlightDictionary"/>.</summary>
        /*private static bool Before_GenerateHighlightDictionary( ForgeMenu __instance )
        {
            var this__highlightDictionary_ = SpaceCore.Instance.Helper.Reflection.GetField< Dictionary< Item, bool > >( __instance, "_highlightDictionary" );

            this__highlightDictionary_.SetValue( new Dictionary<Item, bool>() );
            var this__highlightDictionary = this__highlightDictionary_.GetValue();
            List<Item> item_list = new List<Item>(__instance.inventory.actualInventory);
            if ( Game1.player.leftRing.Value != null )
            {
                item_list.Add( Game1.player.leftRing.Value );
            }
            if ( Game1.player.rightRing.Value != null )
            {
                item_list.Add( Game1.player.rightRing.Value );
            }
            foreach ( Item item in item_list )
            {
                if ( item == null )
                {
                    continue;
                }
                if ( Utility.IsNormalObjectAtParentSheetIndex( item, 848 ) )
                {
                    this__highlightDictionary[ item ] = true;
                }
                else if ( __instance.leftIngredientSpot.item == null && __instance.rightIngredientSpot.item == null )
                {
                    bool valid = false;
                    if ( item is Ring )
                    {
                        valid = true;
                    }
                    if ( item is Tool && BaseEnchantment.GetAvailableEnchantmentsForItem( item as Tool ).Count > 0 )
                    {
                        valid = true;
                    }
                    if ( BaseEnchantment.GetEnchantmentFromItem( null, item ) != null )
                    {
                        valid = true;
                    }
                    foreach ( var recipe in CustomForgeRecipe.Recipes )
                    {
                        if ( recipe.BaseItem.HasEnoughFor( item ) || recipe.IngredientItem.HasEnoughFor( item ) )
                            valid = true;
                    }
                    this__highlightDictionary[ item ] = valid;
                }
                else if ( __instance.leftIngredientSpot.item != null && __instance.rightIngredientSpot.item != null )
                {
                    this__highlightDictionary[ item ] = false;
                }
                else if ( __instance.leftIngredientSpot.item != null )
                {
                    this__highlightDictionary[ item ] = __instance.IsValidCraft( __instance.leftIngredientSpot.item, item );
                }
                else
                {
                    this__highlightDictionary[ item ] = __instance.IsValidCraft( item, __instance.rightIngredientSpot.item );
                }
            }

            return false;
        }*/

        /// <summary>The method to call before <see cref="ForgeMenu.IsValidCraft"/>.</summary>
        private static bool Before_IsValidCraft(ForgeMenu __instance, Item left_item, Item right_item, ref bool __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.BaseItem.HasEnoughFor( left_item ) && recipe.IngredientItem.HasEnoughFor( right_item ) )
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.SpendLeftItem"/>.</summary>
        private static bool Before_SpendLeftItem(ForgeMenu __instance)
        {
            if ( ForgeMenuPatcher.justCrafted != null )
            {
                ForgeMenuPatcher.justCrafted.BaseItem.Consume( ref __instance.leftIngredientSpot.item );
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.SpendLeftItem"/>.</summary>
        private static bool Before_SpendRightItem( ForgeMenu __instance )
        {
            if ( ForgeMenuPatcher.justCrafted != null )
            {
                ForgeMenuPatcher.justCrafted.IngredientItem.Consume( ref __instance.rightIngredientSpot.item );
                ForgeMenuPatcher.justCrafted = null;
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="ForgeMenu.CraftItem"/>.</summary>
        private static bool Before_CraftItem(ForgeMenu __instance, Item left_item, Item right_item, bool forReal, ref Item __result)
        {
            if (left_item == null || right_item == null)
                return true;

            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.BaseItem.HasEnoughFor( left_item ) && recipe.IngredientItem.HasEnoughFor( right_item ) )
                {
                    if ( forReal )
                        ForgeMenuPatcher.justCrafted = recipe;
                    __result = recipe.CreateResult( left_item, right_item );
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

            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.BaseItem.HasEnoughFor( left_item ) && recipe.IngredientItem.HasEnoughFor( right_item ) )
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
