using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Inventories;

namespace Satchels
{
    internal static class Extensions
    {
        public static Item DepositItem(this Inventory inv, Item item)
        {
            // Taken from Farmer.addItemToInventory
            for (int i = 0; i < inv.Count; i++)
            {
                Item item2 = inv[i];
                if (item.canStackWith(item2))
                {
                    int num = item2.addToStack(item);
                    if (num <= 0)
                    {
                        return null;
                    }

                    item.Stack = num;
                }
            }

            for (int j = 0; j < inv.Count; j++)
            {
                if (inv[j] == null)
                {
                    inv[j] = item;
                    return null;
                }
            }

            return item;
        }
    }
}
