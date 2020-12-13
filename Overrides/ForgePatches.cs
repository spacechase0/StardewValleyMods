using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Overrides
{
    [HarmonyPatch(typeof(ForgeMenu), nameof(ForgeMenu.IsValidCraft))]
    public static class ForgeCanCraftPatch
    {
        public static bool Prefix( ForgeMenu __instance, Item left_item, Item right_item, ref bool __result )
        {
            if ( left_item == null || right_item == null )
                return true;

            foreach ( var recipe in Mod.instance.forge )
            {
                if ( left_item.Name == recipe.BaseItemName &&
                     right_item.GetContextTags().Contains( recipe.IngredientContextTag ) &&
                     Mod.instance.epu.CheckConditions( recipe.AbleToForgeConditions ) )
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( ForgeMenu ), nameof( ForgeMenu.CraftItem ) )]
    public static class ForgeCraftPatch
    {
        public static bool Prefix( ForgeMenu __instance, Item left_item, Item right_item, bool forReal, ref Item __result )
        {
            if ( left_item == null || right_item == null )
                return true;

            foreach ( var recipe in Mod.instance.forge )
            {
                if ( left_item.Name == recipe.BaseItemName &&
                     right_item.GetContextTags().Contains( recipe.IngredientContextTag ) &&
                     Mod.instance.epu.CheckConditions( recipe.AbleToForgeConditions ) )
                {
                    __result = Utility.fuzzyItemSearch( recipe.ResultItemName );
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( ForgeMenu ), nameof( ForgeMenu.GetForgeCost ) )]
    public static class ForgeCostPatch
    {
        public static bool Prefix( ForgeMenu __instance, Item left_item, Item right_item, ref int __result )
        {
            if ( left_item == null || right_item == null )
                return true;

            foreach ( var recipe in Mod.instance.forge )
            {
                if ( left_item.Name == recipe.BaseItemName &&
                     right_item.GetContextTags().Contains( recipe.IngredientContextTag ) &&
                     Mod.instance.epu.CheckConditions( recipe.AbleToForgeConditions ) )
                {
                    __result = recipe.CinderShardCost;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( ForgeMenu ), nameof( ForgeMenu.draw ), typeof( SpriteBatch ) )]
    public static class ForgeDrawCostPatch
    {
        public static void DrawCost()
        {
            var forgeMenu = Game1.activeClickableMenu as ForgeMenu;
            int cost = forgeMenu.GetForgeCost( forgeMenu.leftIngredientSpot.item, forgeMenu.rightIngredientSpot.item );
            
            if ( cost != 10 && cost != 15 && cost != 20 )
            {
                Game1.spriteBatch.DrawString( Game1.dialogueFont, "x" + cost, new Vector2( forgeMenu.xPositionOnScreen + 345, forgeMenu.yPositionOnScreen + 320 ), new Color( 226, 124, 65 ) );
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            var newInsns = new List<CodeInstruction>();
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Callvirt && insn.operand is MethodInfo meth )
                {
                    if ( meth.Name == "GetForgeCost" )
                    {
                        newInsns.Add( insn );
                        newInsns.Add( new CodeInstruction( OpCodes.Call, AccessTools.Method( typeof( ForgeDrawCostPatch ), nameof( ForgeDrawCostPatch.DrawCost ) ) ) );
                        continue;
                    }
                }
                newInsns.Add( insn );
            }

            return newInsns;
        }
    }
}
