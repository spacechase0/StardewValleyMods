using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarryChest.Overrides
{
    public static class ItemCanStackHook
    {
        public static bool Prefix(Item __instance, ISalable other, ref bool __result )
        {
            // We're checking the `.ParentSheetIndex` instead of `is Chest` because when you break a chest 
            // and pick it up it isn't a chest instance, it's just an object with the chest index.
            if ( __instance.ParentSheetIndex== 130 && (other is StardewValley.Object obj && obj.ParentSheetIndex == 130) )
            {
                Chest c1 = __instance as Chest;
                Chest c2 = other as Chest;
                if ( c1 != null && c1.items.Count != 0 || c2 != null && c2.items.Count != 0 )
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}
