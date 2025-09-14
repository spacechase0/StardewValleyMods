using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;

namespace Stardew3D.Patches;

[HarmonyPatch(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer))]
public static class Game1ForceRenderOnBufferPatch
{
    public static void Postfix(ref bool __result)
    {
        __result = true;
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.SetWindowSize))]
public static class Game1AddDepthAndStencilToScreenPatch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
    {
        List<CodeInstruction> ret = new();

        foreach (var insn in insns)
        {
            if (insn.opcode == OpCodes.Ldstr && insn.operand is string str && str == "Screen")
            {
                ret[ret.Count - 7].opcode = OpCodes.Ldc_I4_3;
            }

            ret.Add(insn);
        }

        return ret;
    }
}
