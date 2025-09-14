using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewValley;
using StardewVR.GameHandlers;

namespace StardewVR.Patches;

// We have to patch these because SMAPI cries when we override Game1.input with our type
// And SInputState isn't public so we can't inherit from it

[HarmonyPatch(typeof(InputState), nameof(InputState.UpdateStates))]
internal static class InputStateOverridesInVRPatch
{
    private static float scrollX, scrollY;
    public static void Postfix(InputState __instance, ref KeyboardState ____currentKeyboardState, ref MouseState ____currentMouseState, GamePadState ____currentGamepadState)
    {
        if (Stardew3D.Mod.State.ActiveHandler is not IVRGameHandler vr)
        {
            return;
        }

        // TODO: Handle differently based on current input context
        // TODO: Better scroll deadzones
        if (Math.Abs(vr.CurrentScroll.X) >= 0.65f)
            scrollX += vr.CurrentScroll.X;
        if (Math.Abs(vr.CurrentScroll.Y) >= 0.65f)
            scrollY += vr.CurrentScroll.Y;
        ____currentMouseState = new(vr.EmulatedCursor.X, vr.EmulatedCursor.Y, (int)scrollY, vr.LeftClick ? ButtonState.Pressed : ButtonState.Released, ____currentMouseState.MiddleButton, vr.RightClick ? ButtonState.Pressed : ButtonState.Released, ____currentMouseState.XButton1, ____currentMouseState.XButton2, (int)scrollX);
    }
}

[HarmonyPatch(typeof(InputState), nameof(InputState.SetMousePosition))]
internal static class SetMousePositionOverrideInVRPatch
{
    public static bool Prefix(int x, int y)
    {
        if (Stardew3D.Mod.State.ActiveHandler is not IVRGameHandler vr)
        {
            return true;
        }

        vr.EmulatedCursor = new(x, y);
        return false;
    }
}
