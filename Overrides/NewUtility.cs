using Harmony;
using SpaceCore.Events;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Events;
using System;
using System.Reflection;

namespace SpaceCore.Overrides
{
    public class NewUtility
    {
        internal static void hijack( HarmonyInstance harmony )
        {
            harmony.Patch(typeof(Utility).GetMethod("pickFarmEvent", BindingFlags.Static | BindingFlags.Public),
                          new HarmonyMethod(typeof(NewUtility).GetMethod("pickFarmEvent_post")),
                          null);
        }

        // TODO: Make this do IL hooking instead of pre + no execute original
        public static void pickFarmEvent_post( ref FarmEvent __result )
        {
            SpaceEvents.InvokeChooseNightlyFarmEvent( __result );
        }
    }
}
