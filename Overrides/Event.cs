using Microsoft.Xna.Framework;
using SpaceCore.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace SpaceCore.Overrides
{
    public static class EventTryCommandPatch
    {
        internal static Dictionary<string, MethodInfo> customCommands = new Dictionary<string, MethodInfo>();

        public static bool Prefix( Event __instance, GameLocation location, GameTime time, string[] split )
        {
            var _eventCommandArgs = SpaceCore.instance.Helper.Reflection.GetField<object[]>( typeof( Event ), "_eventCommandArgs" ).GetValue();
            var _commandLookup = SpaceCore.instance.Helper.Reflection.GetField<Dictionary<string, MethodInfo>>( typeof( Event ), "_commandLookup" ).GetValue();

            _eventCommandArgs[ 0 ] = ( object ) location;
            _eventCommandArgs[ 1 ] = ( object ) time;
            _eventCommandArgs[ 2 ] = ( object ) split;
            if ( split.Length == 0 )
                return false;
            if ( customCommands.ContainsKey( split[ 0 ] ) )
                customCommands[ split[ 0 ] ].Invoke( null, new object[] { __instance, _eventCommandArgs[ 0 ], _eventCommandArgs[ 1 ], _eventCommandArgs[ 2 ] } );
            else if ( _commandLookup.ContainsKey( split[ 0 ] ) )
                _commandLookup[ split[ 0 ] ].Invoke( ( object ) __instance, _eventCommandArgs );
            else
                SpaceShared.Log.warn( "ERROR: Invalid command: " + split[ 0 ] );

            return false;
        }
    }

    public static class EventActionPatch
    {
        public static bool Prefix( Event __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who  )
        {
            var actionStr = Game1.currentLocation.doesTileHaveProperty( tileLocation.X, tileLocation.Y, "Action", "Buildings" );
            if ( actionStr != null )
                return !SpaceEvents.InvokeActionActivated( who, actionStr, tileLocation );
            return true;
        }
    }
}
