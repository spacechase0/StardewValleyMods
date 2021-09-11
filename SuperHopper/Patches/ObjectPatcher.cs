using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace SuperHopper
{
    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.minutesElapsed))]
    public static class ObjectPatcher
    {
        public static bool Prefix(StardewValley.Object __instance, int minutes, GameLocation environment)
        {
            if (__instance is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest && chest.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex(chest.heldObject.Value, StardewValley.Object.iridiumBar))
            {
                environment.objects.TryGetValue(chest.TileLocation - new Vector2(0, 1), out StardewValley.Object aboveObj);
                if (aboveObj is Chest aboveChest && chest.items.Count < chest.GetActualCapacity() && aboveChest.items.Count > 0)
                {
                    chest.items.Add(aboveChest.items[0]);
                    aboveChest.items.RemoveAt(0);
                }
                // Not doing for now because I'd need to handle every machine's special rules, like changing ParentSheetIndex
                /*
                else if ( aboveObj != null && aboveObj?.GetType() == typeof( StardewValley.Object ) && aboveObj.bigCraftable.Value && aboveObj.MinutesUntilReady == 0 && chest.items.Count < chest.GetActualCapacity() )
                {
                    chest.addItem( aboveObj.heldObject.Value );
                    aboveObj.heldObject.Value = null;
                }
                */

                environment.objects.TryGetValue(chest.TileLocation + new Vector2(0, 1), out StardewValley.Object belowObj);
                if (belowObj is Chest belowChest && chest.items.Count > 0 && belowChest.items.Count < belowChest.GetActualCapacity())
                {
                    belowChest.items.Add(chest.items[0]);
                    chest.items.RemoveAt(0);
                }
                return false;
            }

            return true;
        }
    }
}
