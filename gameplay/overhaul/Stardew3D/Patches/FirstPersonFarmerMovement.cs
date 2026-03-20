using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Stardew3D.GameModes.FirstPerson;
using StardewValley;

namespace Stardew3D.Patches;

[HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
internal static class FirstPersonFarmerMovementPatch1
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns)
    {
        return new CodeMatcher(insns)
            .MatchStartForward(new CodeMatch(CodeInstruction.LoadField(typeof(Farmer), nameof(Farmer.movementDirections))))
            .Advance(-1)
            .Insert(new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.Call(typeof(FirstPersonFarmerMovementPatch1), nameof(PrepareMovementDirections)))
            .Instructions();
    }

    private static void PrepareMovementDirections(Farmer __instance)
    {
        if (Mod.State.ActiveMode is not IFirstPersonGameMode handler)
            return;
        if (__instance != Game1.player || !__instance.CanMove)
            return;

        __instance.movementDirections.Clear();

        Vector2 forward = new(handler.MovementFacing.X, handler.MovementFacing.Z);
        if (forward == Vector2.Zero || handler.MovementAmount == Vector2.Zero)
            return;
        forward.Normalize();
        Vector2 right = Vector2.Transform(forward, Matrix.CreateRotationZ(MathHelper.ToRadians(90)));

        Vector2 movement = forward * handler.MovementAmount.Y + right * handler.MovementAmount.X;
        movement *= __instance.getMovementSpeed();
        movement += handler.MovementAmountForced * Game1.tileSize;

        if (movement.X < 0.001)
            __instance.movementDirections.Add(Game1.left);
        if (movement.X > 0.001)
            __instance.movementDirections.Add(Game1.right);
        if (movement.Y < 0.001)
            __instance.movementDirections.Add(Game1.up);
        if (movement.Y > 0.001)
            __instance.movementDirections.Add(Game1.down);
    }
}

[HarmonyPatch(typeof(Farmer), "MovePositionImpl")]
internal static class FirstPersonFarmerMovementPatch2
{
    public static void Prefix(Farmer __instance, int direction, ref float movementSpeedX, ref float movementSpeedY)
    {
        if (Mod.State.ActiveMode is not IFirstPersonGameMode handler)
            return;
        if (__instance != Game1.player || !__instance.CanMove)
            return;

        Vector2 forward = new(handler.MovementFacing.X, handler.MovementFacing.Z);
        if (forward == Vector2.Zero || handler.MovementAmount == Vector2.Zero)
            return;
        forward.Normalize();
        Vector2 right = Vector2.Transform(forward, Matrix.CreateRotationZ(MathHelper.ToRadians(90)));

        Vector2 movement = forward * handler.MovementAmount.Y + right * handler.MovementAmount.X;
        movement *= __instance.getMovementSpeed();
        movement += handler.MovementAmountForced;

        switch (direction)
        {
            case Game1.left: movementSpeedX = movement.X < 0 ? movement.X : 0; break;
            case Game1.right: movementSpeedX = movement.X > 0 ? movement.X : 0; break;
            case Game1.up: movementSpeedY = movement.Y < 0 ? movement.Y : 0; break;
            case Game1.down: movementSpeedY = movement.Y > 0 ? movement.Y : 0; break;
        }
        // TODO: Currently, I don't think these are lining up correctly with the headset movement.
        //       I'm not sure why, but I'm getting movement in directions I shouldn't.
        if (movement.Y < -0.1)
        {
            //Log.Debug("BEFORE:" + __instance.Position + $" {movement}");
        }
    }
    public static void Postfix(Farmer __instance)
    {
        if (Mod.State.ActiveMode is not IFirstPersonGameMode handler)
            return;
        if (__instance != Game1.player || !__instance.CanMove)
            return;

        Vector2 forward = new(handler.MovementFacing.X, handler.MovementFacing.Z);
        if (forward == Vector2.Zero || handler.MovementAmount == Vector2.Zero)
            return;

        //Log.Debug("AFTER: " + __instance.Position+"\n");
    }
}

[HarmonyPatch(typeof(Farmer), nameof(Farmer.nextPosition))]
internal static class FirstPersonFarmerMovementPatch3
{
    public static void Postfix(Farmer __instance, int direction, ref Rectangle __result)
    {
        if (Mod.State.ActiveMode is not IFirstPersonGameMode handler)
            return;
        if (__instance != Game1.player || !__instance.CanMove)
            return;

        Vector2 forward = new(handler.MovementFacing.X, handler.MovementFacing.Z);
        if (forward == Vector2.Zero || handler.MovementAmount == Vector2.Zero)
            return;
        forward.Normalize();
        Vector2 right = Vector2.Transform(forward, Matrix.CreateRotationZ(MathHelper.ToRadians(90)));

        Vector2 movement = forward * handler.MovementAmount.Y + right * handler.MovementAmount.X;
        movement *= __instance.getMovementSpeed();
        movement += handler.MovementAmountForced;

        __result = __instance.GetBoundingBox();
        switch (direction)
        {
            case Game1.left: __result.X = (int)(__result.X + (movement.X < 0 ? movement.X : 0)); break;
            case Game1.right: __result.X = (int)(__result.X + (movement.X > 0 ? movement.X : 0)); break;
            case Game1.up: __result.Y = (int)(__result.Y + (movement.Y < 0 ? movement.Y : 0)); break;
            case Game1.down: __result.Y = (int)(__result.Y + (movement.Y > 0 ? movement.Y : 0)); break;
        }
    }
}
