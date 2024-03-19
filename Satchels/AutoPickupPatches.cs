using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace Satchels
{
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), new Type[] { typeof(Item) } )]
    public static class FarmerCouldAcceptItemForSatchelPatch1
    {
        public static void Postfix(Farmer __instance, Item item, ref bool __result)
        {
            if (item == null || __result)
                return;

            for (int fi = 0; fi < __instance.maxItems.Value; ++fi)
            {
                Item invItem = __instance.Items[fi];
                if (invItem is Satchel satchel)
                {
                    var pickups = satchel.Upgrades.Where(u => u?.QualifiedItemId == "(O)spacechase0.Satchels_SatchelUpgrade_AutoPickup").ToArray();
                    bool accepts = false;
                    foreach (var pickup in pickups)
                    {
                        if (pickup.modData.TryGetValue(Mod.AutoPickupDataKey, out string pickupData))
                        {
                            string[] ids = pickupData.Split('/');
                            if (ids.Contains(item.QualifiedItemId))
                            {
                                accepts = true;
                                break;
                            }
                        }
                    }
                    if (accepts)
                    {
                        bool hasRoom = false;
                        for (int si = 0; si < satchel.Inventory.Count; ++si)
                        {
                            Item slotItem = satchel.Inventory[si];
                            if (slotItem == null || slotItem.canStackWith(item) && slotItem.Stack + item.Stack <= slotItem.maximumStackSize())
                            {
                                hasRoom = true;
                                break;
                            }
                        }

                        if (hasRoom)
                        {
                            __result = true;
                            return;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), new Type[] { typeof(string), typeof( int ), typeof( int ) })]
    public static class FarmerCouldAcceptItemForSatchelPatch2
    {
        public static void Postfix(Farmer __instance, string id, int stack, int quality, ref bool __result)
        {
            if (__result)
                return;

            var item = ItemRegistry.Create(id, stack, quality);

            for (int fi = 0; fi < __instance.maxItems.Value; ++fi)
            {
                Item invItem = __instance.Items[fi];
                if (invItem is Satchel satchel)
                {
                    var pickups = satchel.Upgrades.Where(u => u?.QualifiedItemId == "(O)spacechase0.Satchels_SatchelUpgrade_AutoPickup").ToArray();
                    bool accepts = false;
                    foreach (var pickup in pickups)
                    {
                        if (pickup.modData.TryGetValue(Mod.AutoPickupDataKey, out string pickupData))
                        {
                            string[] ids = pickupData.Split('/');
                            if (ids.Contains(item.QualifiedItemId))
                            {
                                accepts = true;
                                break;
                            }
                        }
                    }
                    if (accepts)
                    {
                        bool hasRoom = false;
                        for (int si = 0; si < satchel.Inventory.Count; ++si)
                        {
                            Item slotItem = satchel.Inventory[si];
                            if (slotItem == null || slotItem.canStackWith(item) && slotItem.Stack + item.Stack <= slotItem.maximumStackSize())
                            {
                                hasRoom = true;
                                break;
                            }
                        }

                        if (hasRoom)
                        {
                            __result = true;
                            return;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Debris), nameof(Debris.collect))]
    public static class DebrisCollectToSatchelPatch
    {
        public static bool Prefix(Debris __instance, Farmer farmer, ref bool __result)
        {
            bool dont = false;
            if (__instance.item == null)
            {
                dont = __instance.debrisType.Value == 0 && __instance.chunkType.Value == 8;
            }
            if (dont) return true;

            var item = __instance.item;
            if (item == null)
                item = ItemRegistry.Create(__instance.itemId.Value, 1, __instance.itemQuality);

            for (int fi = 0; fi < farmer.maxItems.Value; ++fi)
            {
                Item invItem = farmer.Items[fi];
                if (invItem is Satchel satchel)
                {
                    var pickups = satchel.Upgrades.Where(u => u?.QualifiedItemId == "(O)spacechase0.Satchels_SatchelUpgrade_AutoPickup").ToArray();
                    bool accepts = false;
                    foreach (var pickup in pickups)
                    {
                        if (pickup.modData.TryGetValue(Mod.AutoPickupDataKey, out string pickupData))
                        {
                            string[] ids = pickupData.Split('/');
                            if (ids.Contains(item.QualifiedItemId))
                            {
                                accepts = true;
                                break;
                            }
                        }
                    }
                    if (accepts)
                    {
                        bool hasRoom = false;
                        int slotInd = -1;
                        for (int si = 0; si < satchel.Inventory.Count; ++si)
                        {
                            Item slotItem = satchel.Inventory[si];
                            if (slotItem == null || slotItem.canStackWith(item) && slotItem.Stack + item.Stack <= slotItem.maximumStackSize())
                            {
                                slotInd = si;
                                hasRoom = true;
                                break;
                            }
                        }

                        if (hasRoom)
                        {
                            if (satchel.Inventory[slotInd] == null)
                            {
                                satchel.Inventory[slotInd] = item;
                                __result = true;
                                return false;
                            }
                            else
                            {
                                int leftover = satchel.Inventory[slotInd].addToStack(item);
                                if (leftover > 0)
                                    item.Stack = leftover;
                                else
                                {
                                    __result = true;
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
