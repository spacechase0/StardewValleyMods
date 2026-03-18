using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using Stardew3D.GameModes.VR;
using StardewValley;

namespace Stardew3D.Patches;

// We have to patch these because SMAPI cries when we override Game1.input with our type
// And SInputState isn't public so we can't inherit from it

[HarmonyPatch(typeof(InputState), nameof(InputState.UpdateStates))]
internal static class InputStateOverridesInVRPatch
{
    private static float scrollX, scrollY;
    public static void Postfix(InputState __instance, ref KeyboardState ____currentKeyboardState, ref MouseState ____currentMouseState, GamePadState ____currentGamepadState)
    {
        if (Stardew3D.Mod.State.ActiveMode is not VRGameMode vr)
        {
            return;
        }

#if true
        ____currentMouseState = new(-1, -1, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        return;
#endif

        // TODO: Handle differently based on current input context
        // TODO: Better scroll deadzones
        if (Math.Abs(vr.Menu_CurrentScroll.X) >= 0.65f)
            scrollX += vr.Menu_CurrentScroll.X;
        if (Math.Abs(vr.Menu_CurrentScroll.Y) >= 0.65f)
            scrollY += vr.Menu_CurrentScroll.Y;
        ____currentMouseState = new(vr.EmulatedCursor.X, vr.EmulatedCursor.Y, (int)scrollY, vr.Menu_LeftClick ? ButtonState.Pressed : ButtonState.Released, ____currentMouseState.MiddleButton, vr.Menu_RightClick ? ButtonState.Pressed : ButtonState.Released, ____currentMouseState.XButton1, ____currentMouseState.XButton2, (int)scrollX);
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.didPlayerJustLeftClick))]
internal static class LeftClickDetectionPatch
{
    public static void Postfix(ref bool __result)
    {
        if (Mod.State.ActiveMode == null)
            return;

        __result = false;
        foreach (var cursor in Mod.State.ActiveMode.Cursors)
        {
            if (cursor.UseItemHeld)
                __result = true;
        }
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.didPlayerJustRightClick))]
internal static class RightClickDetectionPatch
{
    public static void Postfix(ref bool __result)
    {
        if (Mod.State.ActiveMode == null)
            return;

        __result = false;
        foreach (var cursor in Mod.State.ActiveMode.Cursors)
        {
            if (cursor.InteractJustPressed)
                __result = true;
        }
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
internal static class LeftClickPreventionPatch
{
    public static bool Prefix()
    {
        if (Mod.State.ActiveMode == null)
            return true;

        return false;
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.pressActionButton))]
internal static class RightClickPreventionPatch
{
    public static bool Prefix()
    {
        if (Mod.State.ActiveMode == null)
            return true;

        return false;
    }
}

[HarmonyPatch(typeof(InputState), nameof(InputState.SetMousePosition))]
internal static class SetMousePositionOverrideInVRPatch
{
    public static bool Prefix(int x, int y)
    {
        if (Stardew3D.Mod.State.ActiveMode is not VRGameMode vr)
        {
            return true;
        }

        vr.EmulatedCursor = new(x, y);
        return false;
    }
}
