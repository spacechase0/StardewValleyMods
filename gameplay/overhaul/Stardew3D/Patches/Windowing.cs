using HarmonyLib;
using Microsoft.Xna.Framework;
using Stardew3D.GameModes.VR;

namespace Stardew3D.Patches;

[HarmonyPatch(typeof(Game), nameof(Game.IsActive), MethodType.Getter)]
internal static class WindowAlwaysActiveInVRPatch
{
    public static void Postfix(ref bool __result)
    {
        if (Stardew3D.Mod.State.ActiveMode is not VRGameMode vr)
        {
            return;
        }
        __result = true;
    }
}
