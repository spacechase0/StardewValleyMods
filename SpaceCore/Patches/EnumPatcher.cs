using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Enum"/>.</summary>
    [SuppressMessage( "ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony )]
    internal class EnumPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply( Harmony harmony, IMonitor monitor )
        {
            //var meth = typeof( Enum ).GetMethod( "TryParse", 1, new[] { typeof( string ), typeof( object ) } ).MakeGenericMethod( typeof( GameLocation.LocationContext ) );
            // https://stackoverflow.com/a/5218492
            var meth = typeof( Enum ).GetMethods()
                                     .Where( m => m.Name == "TryParse" )
                                     .Select( m => new { Method = m, Params = m.GetParameters(), Args = m.GetGenericArguments() } )
                                     .Where( x => x.Params.Length == 2 && x.Args.Length == 1 && x.Params[ 1 ].ParameterType.FullName == x.Args[ 0 ].FullName  )
                                     .Select( x => x.Method )
                                     .First();
            /*
            // Not supported by MonoMod
            harmony.Patch(
                original: meth,
                transpiler: this.GetHarmonyMethod( nameof( Before_TryParse ) )
            );
            */

            //harmony.Patch( original: this.RequireMethod< Enum >( nameof( Enum.GetValues ) )
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Multiplayer.processIncomingMessage"/>.</summary>
        private static bool Before_TryParse( string value, ref GameLocation.LocationContext? result, ref bool __result )
        {
            foreach ( var kvp in SpaceCore.CustomLocationContexts )
            {
                if ( kvp.Value.Name == value )
                {
                    result = kvp.Key;
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }
}
