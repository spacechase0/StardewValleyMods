using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace ResponsiveKnockback
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static ConditionalWeakTable<AbstractNetSerializable, Holder<bool>> sharedAuthority = new();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(AbstractNetSerializable), nameof(AbstractNetSerializable.MarkDirty))]
    public static class ANSMarkDirtyPatch
    {
        public static bool Prefix(AbstractNetSerializable __instance)
        {
            if (!Mod.sharedAuthority.TryGetValue(__instance, out var varHolder))
                return true;

            if (!varHolder.Value && Game1.IsMasterGame)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(Monster), "initNetFields")]
    public static class MonsterNetFieldsPatch
    {
        public static void Postfix(Monster __instance)
        {
            Mod.sharedAuthority.Add(__instance.position.Field, new Holder<bool>() { Value = true });
        }
    }

    [HarmonyPatch(typeof(Monster), nameof(Monster.setTrajectory))]
    public static class MonsterSetTrajectoryPatch
    {
        public static void Postfix(Monster __instance, Vector2 trajectory)
        {
            if (!Game1.IsMasterGame)
            {
                if (Math.Abs(trajectory.X) > Math.Abs(__instance.xVelocity))
                {
                    __instance.xVelocity = trajectory.X;
                }
                if (Math.Abs(trajectory.Y) > Math.Abs(__instance.yVelocity))
                {
                    __instance.yVelocity = trajectory.Y;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Monster), nameof(Monster.update))]
    public static class MonsterUpdatePatch
    {
        public static void Postfix(Monster __instance, GameTime time)
        {
            if (!Game1.IsMasterGame)
            {
                __instance.MovePosition(time, Game1.viewport, __instance.currentLocation);
            }
        }
    }
}
