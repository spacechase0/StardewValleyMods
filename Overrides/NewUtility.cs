using Harmony;
using SpaceCore.Events;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Events;
using System;
using System.Reflection;

namespace SpaceCore.Overrides
{
    [HarmonyPatch(typeof(Utility), "pickFarmEvent")]
    internal class NightlyFarmEventHook
    {
        public static void Postfix( ref FarmEvent __result )
        {
            SpaceEvents.InvokeChooseNightlyFarmEvent( __result );
        }
    }
}
