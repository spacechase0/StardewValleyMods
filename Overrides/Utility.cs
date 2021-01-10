using Harmony;
using SpaceCore.Events;
using StardewValley;
using StardewValley.Events;

namespace SpaceCore.Overrides
{
    public class NightlyFarmEventHook
    {
        public static void Postfix( ref FarmEvent __result )
        {
            __result = SpaceEvents.InvokeChooseNightlyFarmEvent( __result );
        }
    }
}
