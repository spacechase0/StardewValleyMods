using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MoonMisadventures.Game.Items;
using Netcode;
using StardewValley;

namespace MoonMisadventures.VirtualProperties
{
    public static class Farmer_Necklace
    {
        internal class Holder { public readonly NetRef<Item> Value = new(); }

        internal static ConditionalWeakTable< Farmer, Holder > values = new();

        public static void set_necklaceItem( this Farmer farmer, NetRef<Item> newVal )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetRef<Item> get_necklaceItem( this Farmer farmer )
        {
            var holder = values.GetOrCreateValue( farmer );
            return holder.Value;
        }

        public static bool HasNecklace( this Farmer farmer, string type )
        {
            if ( farmer.get_necklaceItem().Value == null )
                return false;
            return ( farmer.get_necklaceItem().Value as Necklace ).ItemId == type;
        }
    }
}
