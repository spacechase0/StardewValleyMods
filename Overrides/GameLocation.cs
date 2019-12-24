using Microsoft.Xna.Framework;
using SpaceCore.Events;
using StardewValley;
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

    public static class ExplodeHook
    {
        public static void Postfix(GameLocation __instance, Vector2 tileLocation, int radius, Farmer who)
        {
            SpaceEvents.InvokeBombExploded(who, tileLocation, radius);
        }
    }
}
