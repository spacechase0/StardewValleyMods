using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Tools;

namespace SpaceCore.VanillaAssetExpansion
{
    /* Weapons:
     * CanBeTrashed: true/false
     **/

    [HarmonyPatch(typeof(Item), nameof(Item.canBeTrashed))]
    public static class WeaponTrashablePatch
    {
        public static void Postfix(Item __instance, ref bool __result)
        {
            if (__instance is MeleeWeapon weapon)
            {
                if ((weapon.GetData()?.CustomFields?.TryGetValue("CanBeTrashed", out string str) ?? false) &&
                     bool.TryParse(str, out bool val) && !val)
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Item), nameof(Item.canBeDropped))]
    public static class WeaponDroppablePatch
    {
        public static void Postfix(Item __instance, ref bool __result)
        {
            WeaponTrashablePatch.Postfix(__instance, ref __result);
        }
    }
}
