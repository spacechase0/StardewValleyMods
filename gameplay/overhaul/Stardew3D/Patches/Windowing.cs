using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Stardew3D.Handlers.Game;

namespace Stardew3D.Patches;

[HarmonyPatch(typeof(Game), nameof(Game.IsActive), MethodType.Getter)]
internal static class WindowAlwaysActiveInVRPatch
{
    public static void Postfix(ref bool __result)
    {
        if (Stardew3D.Mod.State.ActiveHandler is not VRGameHandler vr)
        {
            return;
        }
        __result = true;
    }
}
