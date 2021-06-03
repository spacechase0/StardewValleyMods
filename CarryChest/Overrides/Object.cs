using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarryChest.Overrides
{
    public static class ObjectDescriptionHook
    {
        public static void Postfix(StardewValley.Object __instance, ref string __result)
        {
            if ( __instance.ParentSheetIndex == 130 )
            {
                var chest = __instance as Chest;
                __result += "\n" + $"Contains {chest?.items?.Count ?? 0} items.";
            }
        }
    }
}
