using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceShared.APIs;
using Stardew3D.GameModes.VR;
using StardewValley;
using StardewValley.GameData.Locations;

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

#if false
        ____currentMouseState = new(-1, -1, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        return;
#endif

        // TODO: Handle differently based on current input context
        // TODO: Better scroll deadzones
        //*
        if (Math.Abs(vr.Menu_CurrentScroll.X) >= 0.65f)
            scrollX += vr.Menu_CurrentScroll.X;
        if (Math.Abs(vr.Menu_CurrentScroll.Y) >= 0.65f)
            scrollY += vr.Menu_CurrentScroll.Y;
        ____currentMouseState = new(vr.EmulatedCursor.X, vr.EmulatedCursor.Y, (int)scrollY, vr.Menu_LeftClick ? ButtonState.Pressed : ButtonState.Released, ____currentMouseState.MiddleButton, vr.Menu_RightClick ? ButtonState.Pressed : ButtonState.Released, ____currentMouseState.XButton1, ____currentMouseState.XButton2, (int)scrollX);
        //*/
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

[HarmonyPatch]
public static class CustomSwitchToolButtonPatch
{
    public static int myWhichWay = 0;

    [HarmonyPatch(typeof(Game1), nameof(Game1.pressSwitchToolButton))]
    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    internal static void PatchedOriginal()
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, MethodBase original)
        {
            //var sc = Mod.Instance.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");

            var matcher = new CodeMatcher(insns);
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Options), nameof(Options.invertScrollDirection))));
            matcher.Advance(-1);

            var labels = matcher.Labels.ToList();
            matcher.Labels.Clear();

            matcher.Insert(
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(CustomSwitchToolButtonPatch), nameof(CustomSwitchToolButtonPatch.myWhichWay)))
                {
                    labels = labels,
                },
                //new CodeInstruction(OpCodes.Stloc, sc.GetLocalIndexForMethod(original, "whichWay").Single())
                new CodeInstruction(OpCodes.Stloc, 0)
            );

            return matcher.Instructions();
        }
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.IsPerformingMousePlacement))]
public static class AlwaysMousePlacementSortaPatch1
{
    public static void Postfix(ref bool __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = true;
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.GetPlacementGrabTile))]
public static class AlwaysMousePlacementSortaPatch2
{
    public static Vector2 placementGrabTile = new Vector2(-1, -1);

    public static void Postfix(ref Vector2 __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = placementGrabTile;
    }
}


[HarmonyPatch(typeof(Utility), nameof(Utility.isWithinTileWithLeeway))]
public static class AllowPlacementIfWeSaySoPatch
{
    public static void Postfix(ref bool __result)
    {
        // Vanilla only uses this function in the placement stuff
        if (Mod.State.ActiveMode != null)
            __result = true;
    }
}
