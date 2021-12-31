using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;

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
        public override void Apply(Harmony harmony, IMonitor monitor)
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

            harmony.Patch(
                original: this.RequireMethod<GameLocation>( nameof( GameLocation.GetLocationContext ) ),
                prefix: this.GetHarmonyMethod( nameof( Before_GetLocationContext ) )
            );
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

        private static bool Before_GetLocationContext( GameLocation __instance, ref GameLocation.LocationContext __result )
        {
            __result = GetLocationContextImpl( __instance );
            return false;
        }

        private static GameLocation.LocationContext GetLocationContextImpl( GameLocation loc )
        {
            if ( loc.locationContext == ( GameLocation.LocationContext ) ( -1 ) )
            {
                if ( loc.map == null )
                {
                    loc.reloadMap();
                }
                loc.locationContext = GameLocation.LocationContext.Default;
                string location_context = null;
                PropertyValue value = null;
                if ( loc.map == null )
                {
                    return GameLocation.LocationContext.Default;
                }
                location_context = ( ( !loc.map.Properties.TryGetValue( "LocationContext", out value ) ) ? "" : value.ToString() );
                bool foundCustom = false;
                foreach ( var kvp in SpaceCore.CustomLocationContexts )
                {
                    if ( kvp.Value.Name == location_context )
                    {
                        loc.locationContext = kvp.Key;
                        foundCustom = true;
                        break;
                    }
                }
                if ( !foundCustom && location_context != "" && !Enum.TryParse<GameLocation.LocationContext>( location_context, out loc.locationContext ) )
                {
                    loc.locationContext = GameLocation.LocationContext.Default;
                }
            }
            return loc.locationContext;
        }
    }
}
