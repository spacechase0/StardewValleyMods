using HarmonyLib;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.Patches
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
                if ( data != null ) // This means it was disabled
                    __result = !data.HideFromShippingCollection;
                else
                    __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( StardewValley.Object ), nameof( StardewValley.Object.isSapling ) )]
    public static class ObjectIsSaplingPatch
    {
        public static bool Prefix( StardewValley.Object __instance, ref bool __result )
        {
            if ( __instance is CustomObject cobj && !string.IsNullOrEmpty( cobj.Data.Plants ) )
            {
                var data = Mod.Find( cobj.Data.Plants );
                if ( data is FruitTreePackData )
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }
}
