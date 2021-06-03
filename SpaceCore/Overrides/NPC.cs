using SpaceCore.Events;
using StardewValley;

namespace SpaceCore.Overrides
{
    public class BeforeReceiveObjectHook
    {
        public static bool Prefix( NPC __instance, Farmer who)
        {
            return !SpaceEvents.InvokeBeforeReceiveObject( __instance, who.ActiveObject, who );
        }
    }
    public class AfterGiftGivenHook
    {
        public static void Postfix( NPC __instance, StardewValley.Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
        {
            SpaceEvents.InvokeAfterGiftGiven(__instance, o, giver);
        }
    }
}
