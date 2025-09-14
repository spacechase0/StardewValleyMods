using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Network;

namespace MisappliedPhysicalities.VirtualProperties
{
    public static class GameLocationExtensions
    {
        public static NetVector2Dictionary< StardewValley.Object, NetRef< StardewValley.Object > > GetContainerForLayer( this GameLocation loc, Layer layer )
        {
            switch ( layer )
            {
                case Layer.Underground: return loc.get_BelowGroundObjects();
                case Layer.GroundLevel: return loc.netObjects;
                case Layer.Elevated: return loc.get_ElevatedObjects();
            }

            return null;
        }
    }

    public static class GameLocation_ConstantProcessing
    {
        internal class Holder
        {
            public NetList<Vector2,NetVector2> Below = new();
            public NetList<Vector2,NetVector2> Ground = new();
            public NetList<Vector2,NetVector2> Elevated = new();
        }

        internal static ConditionalWeakTable< GameLocation, Holder > values = new();

        public static NetList<Vector2, NetVector2> GetConstantProcessingForLayer( this GameLocation loc, Layer layer )
        {
            var holder = values.GetOrCreateValue( loc );
            switch ( layer )
            {
                case Layer.Underground: return holder.Below;
                case Layer.GroundLevel: return holder.Ground;
                case Layer.Elevated: return holder.Elevated;
            }

            return null;
        }
    }

    public static class GameLocation_BelowGroundObjects
    {
        internal class Holder { public NetVector2Dictionary< StardewValley.Object, NetRef<StardewValley.Object>> Value = new(); }

        internal static ConditionalWeakTable< GameLocation, Holder > values = new();

        public static void set_BelowGroundObjects( this GameLocation loc, NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> newval )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> get_BelowGroundObjects( this GameLocation loc )
        {
            var holder = values.GetOrCreateValue( loc );
            return holder.Value;
        }
    }
    public static class GameLocation_ElevatedObjects
    {
        internal class Holder { public NetVector2Dictionary< StardewValley.Object, NetRef<StardewValley.Object>> Value = new(); }

        internal static ConditionalWeakTable< GameLocation, Holder > values = new();

        public static void set_ElevatedObjects( this GameLocation loc, NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> newval )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> get_ElevatedObjects( this GameLocation loc )
        {
            var holder = values.GetOrCreateValue( loc );
            return holder.Value;
        }
    }
}
