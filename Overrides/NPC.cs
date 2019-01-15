using SpaceCore.Events;
using StardewValley;

namespace SpaceCore.Overrides
{
    public class AfterGiftGivenHook
    {
        public static void Postfix( NPC __instance, StardewValley.Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
        {
            SpaceEvents.InvokeAfterGiftGiven(__instance, o, giver);
        }
    }
}
