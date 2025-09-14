using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MoonMisadventures.Game.Items;
using Netcode;
using StardewValley;
using StardewValley.Monsters;

namespace MoonMisadventures.VirtualProperties
{
    public static class Monster_Shocked
    {
        internal class Holder
        {
            public readonly NetInt shockTimer = new( -1 );
            public Farmer shocker = null;
        }

        internal static ConditionalWeakTable< Monster, Holder > values = new();

        public static void set_shocked( this Monster monster, NetInt newVal )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetInt get_shocked( this Monster monster )
        {
            var holder = values.GetOrCreateValue( monster );
            return holder.shockTimer;
        }
        public static void set_shocker( this Monster monster, Farmer shocker )
        {
            var holder = values.GetOrCreateValue( monster );
            holder.shocker = shocker;
        }

        public static Farmer get_shocker( this Monster monster )
        {
            var holder = values.GetOrCreateValue( monster );
            return holder.shocker;
        }
    }
}
