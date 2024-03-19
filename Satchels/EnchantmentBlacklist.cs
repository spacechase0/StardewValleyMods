using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Enchantments;

namespace Satchels
{
    [HarmonyPatch(typeof(SwiftToolEnchantment), nameof(SwiftToolEnchantment.CanApplyTo))]
    public static class SwiftEnchantmentBlacklistPatch
    {
        public static void Postfix(Item item, ref bool __result)
        {
            if (item is Satchel)
                __result = false;
        }
    }

    [HarmonyPatch(typeof(EfficientToolEnchantment), nameof(EfficientToolEnchantment.CanApplyTo))]
    public static class EfficientEnchantmentBlacklistPatch
    {
        public static void Postfix(Item item, ref bool __result)
        {
            if (item is Satchel)
                __result = false;
        }
    }
}
