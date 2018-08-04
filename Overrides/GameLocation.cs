using Microsoft.Xna.Framework;
using SpaceCore.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace SpaceCore.Overrides
{
    public class ActionHook
    {
        public static bool Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation)
        {
            return !SpaceEvents.InvokeActionActivated(who, action, tileLocation);
        }
    }

    public class TouchActionHook
    {
        public static bool Prefix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            return !SpaceEvents.InvokeTouchActionActivated(Game1.player, fullActionString, new Location(0,0));
        }
    }
}
