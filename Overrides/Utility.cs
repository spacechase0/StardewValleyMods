using SpaceCore.Events;
using StardewValley.Events;

namespace SpaceCore.Overrides
{
    public class NightlyFarmEventHook
    {
        public static void Postfix( ref FarmEvent __result )
        {
            SpaceEvents.InvokeChooseNightlyFarmEvent( __result );
        }
    }
}
