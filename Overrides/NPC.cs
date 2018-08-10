using SpaceCore.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
