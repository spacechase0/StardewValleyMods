using Harmony;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    [HarmonyPatch( typeof( ShippingMenu ), nameof( ShippingMenu.parseItems ) )]
    public static class ShippingMenuParsePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            return PatchCommon.RedirectForFakeObjectIdTranspiler( gen, original, insns );
        }
    }
}
