using Harmony;
using JsonAssets.Game;
using JsonAssets.PackData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    [HarmonyPatch( typeof( StardewValley.Object ), nameof( StardewValley.Object.countsForShippedCollection ) )]
    public static class ObjectCountsForShippedCollectionPatch
    {
        public static bool Prefix(StardewValley.Object __instance, ref bool __result)
        {
            if ( __instance is CustomObject cobj )
            {
                __result = !cobj.Data.HideFromShippingCollection;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( StardewValley.Object ), nameof( StardewValley.Object.isIndexOkForBasicShippedCategory ) )]
    public static class ObjectIndexOkForShippedCollectionPatch
    {
        public static bool Prefix( int index, ref bool __result )
        {
            if ( Mod.itemLookup.ContainsKey( index ) )
            {
                var data = Mod.Find( Mod.itemLookup[ index ] ) as ObjectPackData;
                __result = !data.HideFromShippingCollection;
                return false;
            }

            return true;
        }
    }
}
