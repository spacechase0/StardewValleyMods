using JsonAssets.PackData;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    public static class Common
    {
        public static void DoShop( string key, ShopMenu shop )
        {
            if ( !Mod.todaysShopEntries.ContainsKey( key ) )
                return;

            foreach ( var entry in Mod.todaysShopEntries[ key ] )
            {
                entry.AddToShop( shop );
            }
        }
        public static void DoShopStock( string key, Dictionary<ISalable, int[]> data )
        {
            if ( !Mod.todaysShopEntries.ContainsKey( key ) )
                return;

            foreach ( var entry in Mod.todaysShopEntries[ key ] )
            {
                entry.AddToShopStock( data );
            }
        }

        public static string GetFakeObjectInformation( IDictionary<int, string> data, int index )
        {
            if ( Mod.itemLookup.ContainsKey( index ) )
            {
                return ( ( ObjectPackData ) Mod.Find( Mod.itemLookup[ index ] ) ).GetFakeData();
            }
            return data[ index ];
        }
    }
}
