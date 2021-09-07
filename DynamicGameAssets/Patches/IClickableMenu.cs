using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DynamicGameAssets.Patches
{
    public static class DrawHoverTextPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler(gen, original, insns);
        }
    }
}
