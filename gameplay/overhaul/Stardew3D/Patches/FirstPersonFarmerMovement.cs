using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Stardew3D.Handlers.Game.FirstPerson;
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
        if (Mod.State.ActiveHandler is not IFirstPersonGameHandler handler)
            return;
        if (__instance != Game1.player || !__instance.CanMove)
            return;

        __instance.movementDirections.Clear();

        Vector2 forward = new(handler.MovementFacing.X, handler.MovementFacing.Z);
        if (forward == Vector2.Zero)
            return;
        forward.Normalize();
        Vector2 right = Vector2.Transform(forward, Matrix.CreateRotationZ(MathHelper.ToRadians(90)));

        Vector2 movement = forward * handler.MovementAmount.Y + right * handler.MovementAmount.X;
        movement += handler.MovementAmountForced;

        if (movement.X < 0)
            __instance.movementDirections.Add(Game1.left);
        if (movement.X > 0)
            __instance.movementDirections.Add(Game1.right);
        if (movement.Y < 0)
            __instance.movementDirections.Add(Game1.up);
        if (movement.Y > 0)
            __instance.movementDirections.Add(Game1.down);
    }
}

[HarmonyPatch(typeof(Farmer), "MovePositionImpl")]
internal static class FirstPersonFarmerMovementPatch2
{
    public static void Prefix(Farmer __instance, int direction, ref float movementSpeedX, ref float movementSpeedY)
    {
        if (Mod.State.ActiveHandler is not IFirstPersonGameHandler handler)
            return;
        if (__instance != Game1.player || !__instance.CanMove)
            return;

        Vector2 forward = new(handler.MovementFacing.X, handler.MovementFacing.Z);
        if (forward == Vector2.Zero)
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
    }
}
