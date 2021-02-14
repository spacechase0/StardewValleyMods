using Harmony;
using JsonAssets.Game;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    [HarmonyPatch(typeof(Farmer), nameof( Farmer.getItemCountInList ) )]
    public static class FarmerGetItemCountPatch
    {
        public static bool Prefix( Farmer __instance, IList<Item> list, int item_index, int min_price, ref int __result )
        {
            if ( Mod.itemLookup.ContainsKey( item_index ) )
            {
                __result = 0;
                for ( int i = 0; i < list.Count; ++i )
                {
                    var item = list[ i ];
                    if ( item != null && item is CustomObject obj && obj.FullId.GetHashCode() == item_index )
                        __result += obj.Stack;
                }

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch( typeof( Farmer ), nameof( Farmer.removeItemsFromInventory ) )]
    public static class FarmerRemoveItemsPatch
    {
        public static bool Prefix( Farmer __instance, int index, int stack, ref bool __result )
        {
            if ( Mod.itemLookup.ContainsKey( index ) )
            {
                if ( __instance.hasItemInInventory( index, stack ) )
                {
                    for ( int i = 0; i < __instance.items.Count; ++i )
                    {
                        var item = __instance.items[ i ];
                        if ( !( item is CustomObject obj ) || obj.FullId.GetHashCode() != index )
                            continue;

                        if ( item.Stack > stack )
                        {
                            item.Stack -= stack;
                            break;
                        }
                        else
                        {
                            stack -= item.Stack;
                            __instance.items[ i ] = null;

                            if ( stack == 0 )
                                break;
                        }
                    }
                    __result = true;
                }
                else
                {
                    __result = false;
                }
            }
            return false;
        }
    }


    [HarmonyPatch( typeof( Farmer ), nameof( Farmer.eatObject ) )]
    public static class FarmerEatObjectgPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            return Common.RedirectForFakeObjectInformationTranspiler2( gen, original, insns );
        }
    }

    [HarmonyPatch( typeof(Farmer), nameof( Farmer.doneEating ) )]
    public static class FarmerDoneEatingPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            return Common.RedirectForFakeObjectInformationTranspiler2( gen, original, insns );
        }
    }
}
