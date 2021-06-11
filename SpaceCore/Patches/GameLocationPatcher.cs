using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Harmony;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="GameLocation"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class GameLocationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.performAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PerformAction))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.performTouchAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PerformTouchAction))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.explode)),
                postfix: this.GetHarmonyMethod(nameof(After_Explode))
            );

            //harmony.Patch(
            //    original: this.RequireMethod<GameLocation>(nameof(GameLocation.updateEvenIfFarmerIsntHere)),
            //    postfix: this.GetHarmonyMethod(nameof(After_UpdateEvenIfFarmerIsntHere))
            //);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="GameLocation.performAction"/>.</summary>
        private static bool Before_PerformAction(GameLocation __instance, string action, Farmer who, Location tileLocation)
        {
            return !SpaceEvents.InvokeActionActivated(who, action, tileLocation);
        }

        /// <summary>The method to call before <see cref="GameLocation.performTouchAction"/>.</summary>
        private static bool Before_PerformTouchAction(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            return !SpaceEvents.InvokeTouchActionActivated(Game1.player, fullActionString, new Location(0, 0));
        }

        /// <summary>The method to call after <see cref="GameLocation.explode"/>.</summary>
        private static void After_Explode(GameLocation __instance, Vector2 tileLocation, int radius, Farmer who)
        {
            SpaceEvents.InvokeBombExploded(who, tileLocation, radius);
        }

        ///// <summary>The method to call after <see cref="GameLocation.updateEvenIfFarmerIsntHere"/>.</summary>
        //private static void After_UpdateEvenIfFarmerIsntHere(GameLocation __instance, GameTime time)
        //{
        //    // TODO: Optimize, maybe config file too?
        //    __instance.terrainFeatures.Values.DoIf((tf) => tf is IUpdateEvenWithoutFarmer, (tf) => (tf as IUpdateEvenWithoutFarmer).UpdateEvenWithoutFarmer(__instance, time));
        //    __instance.Objects.Values.DoIf((o) => o is IUpdateEvenWithoutFarmer, (o) => (o as IUpdateEvenWithoutFarmer).UpdateEvenWithoutFarmer(__instance, time));
        //}
    }
}
