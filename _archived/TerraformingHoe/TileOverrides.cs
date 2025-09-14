using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Network;

namespace TerraformingHoe
{
    public enum TileOverride
    {
        Water,
        Grass
    }

    public static class GameLocation_TileOverrides
    {
        internal class Holder { public readonly NetVector2Dictionary<int, NetInt> Value = new(); }

        internal static ConditionalWeakTable<GameLocation, Holder> values = new();

        public static void set_tileOverrides(this GameLocation farmer, NetVector2Dictionary<int, NetInt> newVal)
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetVector2Dictionary<int, NetInt> get_tileOverrides(this GameLocation farmer)
        {
            var holder = values.GetOrCreateValue(farmer);
            return holder.Value;
        }
    }

    [HarmonyPatch(typeof(GameLocation), "initNetFields")]
    public static class GameLocationAddTilesNetFieldPatch
    {
        public static void Postfix(GameLocation __instance)
        {
            __instance.NetFields.AddField(__instance.get_tileOverrides());
        }
    }
}
