using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Menus;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch(typeof(ShippingMenu), nameof(ShippingMenu.parseItems))]
    public static class ShippingMenuParsePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            return PatchCommon.RedirectForFakeObjectIdTranspiler(gen, original, insns);
        }
    }
}
