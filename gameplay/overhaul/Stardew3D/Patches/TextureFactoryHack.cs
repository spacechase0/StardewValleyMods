using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics.Pipeline;

namespace Stardew3D.Patches;

[HarmonyPatch(typeof(TextureFactory<byte[]>), nameof(TextureFactory<byte[]>.UseTexture))]
internal static class TeextureFactoryHackPatch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
    {
        var code = new CodeMatcher(insns, ilgen)
            .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(GraphicsResource), nameof(GraphicsResource.Name))));
        int pos = code.Pos + 1;
        code.MatchStartBackwards(new CodeMatch(insn => insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == "ConvertTexture"))
            .Advance(2);
        code.RemoveInstructions(pos - (code.Pos + 1) + 1);

        var ret = code.Instructions();
        return ret;
    }
}
