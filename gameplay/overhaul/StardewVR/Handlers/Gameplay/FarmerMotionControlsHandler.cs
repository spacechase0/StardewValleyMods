using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoScene.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D;
using Stardew3D.Data;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Game;
using Stardew3D.Handlers.Game.FirstPerson;
using Stardew3D.Handlers.Render;
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewVR.Handlers.Game;

namespace StardewVR.Handlers.Gameplay;
internal class FarmerMotionControlsHandler : RendererFor<ModelData, Farmer>, IUpdateHandler
{
    public VRGameHandler GameHandler;

    public FarmerMotionControlsHandler(VRGameHandler handler, Farmer obj)
        : base($"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Character)Farmer", obj)
    {
        GameHandler = handler;
    }

    public Matrix GetHeldTransformFor(IGameCursor cursor, bool forDisplay = false)
    {
        return Matrix.Identity
            * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f))
            * Matrix.CreateScale(1f / 3)
            //* Matrix.CreateTranslation(new Vector3(0.075f, 0.15f, -0.0f))
            * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f))
            * Matrix.CreateFromQuaternion // Was getting a gimbal lock otherwise
            (
                  Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(-45)))
                * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(-90)))
                * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(0)))
            )
            * Matrix.CreateTranslation(new Vector3(0f, -0.125f, -0.0f))
            * cursor.Grip;
    }

    public virtual void Update(IUpdateHandler.UpdateContext ctx)
    {
        if (Game1.player.currentLocation != Game1.currentLocation)
            return;

        foreach ( var cursor in GameHandler.Cursors )
            HandleCursor(ctx, cursor);
    }

    private ConditionalWeakTable<IGameCursor, Holder<Matrix>> lastGrips = new();
    protected virtual void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor)
    {
        // TODO: more generic, support more things too
        // TODO: and optimize
        if (cursor.Holding is not Axe axe) return;
        axe.lastUser = Game1.player;

        var interaction = InteractionData.Get(axe.QualifiedItemId);
        interaction ??= InteractionData.Get($"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/ToolTypes){axe.GetToolData()?.ClassName}");
        interaction ??= InteractionData.Get(axe.QualifiedItemId.Substring(0, axe.QualifiedItemId.IndexOf(')') + 1));
        if (interaction == null)
            return;

        var transform = GetHeldTransformFor(cursor);
        var prevTransform = lastGrips.GetOrCreateValue(cursor).Value;
        var verts = interaction.Areas[0].GetTransformedShape();
        var prevVerts = verts.ToArray();
        for (int i = 0; i < verts.Length; ++i)
        {
            verts[i] = Vector3.Transform(verts[i], transform);
            prevVerts[i] = Vector3.Transform(prevVerts[i], prevTransform);
        }

        foreach (var entry in Game1.player.currentLocation.terrainFeatures.Values)
        {
            if (entry is not Tree tree)
                continue;

            var treeInteraction = InteractionData.Get($"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Tree){tree.treeType.Value}");
            treeInteraction ??= InteractionData.Get($"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Tree)");

            var treeTransform = Matrix.CreateTranslation(tree.Tile.ToPoint().To3D(Game1.player.currentLocation.Map));
            var treeVerts = treeInteraction.Areas[0].GetTransformedShape();
            for (int i = 0; i < treeVerts.Length; ++i)
            {
                treeVerts[i] = Vector3.Transform(treeVerts[i], treeTransform);
            }

            if (Math.Abs(64 - tree.Tile.X) < 3 || Math.Abs(33 - tree.Tile.Y) < 3)
            {
                transform = transform;
            }

            if (!GJK_EPA_BCP.CheckIntersection(verts, treeVerts, out var contact, out var depth, out var normal))
                continue;
            if (GJK_EPA_BCP.CheckIntersection(prevVerts, treeVerts, out _, out _, out _))
                continue;

            if (cursor.LinearVelocity.Length() < 1) // TODO: take angular into account too?
                continue;
            Log.Debug("CHOP");

            if (tree.performToolAction(axe, 0, tree.Tile))
                Game1.player.currentLocation.terrainFeatures.Remove(tree.Tile);
        }

        lastGrips.AddOrUpdate(cursor, new(transform));
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new RenderData(ctx, this);
    }

    private class RenderData : RenderData<FarmerMotionControlsHandler>
    {
        private int[] rayInstances;
        private int[] gripInstances;
        //private int[] heldInstances;

        public RenderData(IRenderHandler.RenderContext ctx, FarmerMotionControlsHandler parent)
            : base(ctx, parent)
        {
            if (ctx.TargetScreen == Game1.game1.uiScreen)
                return;

            rayInstances = new int[Parent.GameHandler.Cursors.Count];
            gripInstances = new int[Parent.GameHandler.Cursors.Count];
            //heldInstances = new int[Parent.GameHandler.Cursors.Count];
            for (int i_ = 0; i_ < rayInstances.Length; ++i_)
            {
                int i = i_;
                var cursor = Parent.GameHandler.Cursors[i];
                Color col = i == 0 ? Color.Blue : Color.Red;
                var handSize = 0.125f / 4;

                // TODO: These should be able to be converted to instanced??
                rayInstances[i] = Batch.AddNonInstanced((env, color, world, view, proj) =>
                {
                    RenderHelper.GenericEffect.View = view;
                    RenderHelper.GenericEffect.Projection = proj;
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Up, upOverride: Vector3.Forward, additionalTransform: Parent.GameHandler.Cursors[i].Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Down, upOverride: Vector3.Forward, additionalTransform: Parent.GameHandler.Cursors[i].Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Left, upOverride: Vector3.Forward, additionalTransform: Parent.GameHandler.Cursors[i].Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Right, upOverride: Vector3.Forward, additionalTransform: Parent.GameHandler.Cursors[i].Pointer);
                }, Matrix.Identity, staysVisibleAfterFrame: true);
                gripInstances[i] = Batch.AddNonInstanced((env, color, world, view, proj) =>
                {
                    Color colFront = col, colSide = col, colBack = col;
                    colSide.R = (byte)(colSide.R * 0.75f);
                    colSide.G = (byte)(colSide.G * 0.75f);
                    colSide.B = (byte)(colSide.B * 0.75f);
                    colBack.R = (byte)(colBack.R * 0.5f);
                    colBack.G = (byte)(colBack.G * 0.5f);
                    colBack.B = (byte)(colBack.B * 0.5f);

                    RenderHelper.GenericEffect.View = view;
                    RenderHelper.GenericEffect.Projection = proj;
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Right * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Right, colSide, Vector3.Up, additionalTransform: Parent.GameHandler.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Left * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Left, colSide, Vector3.Up, additionalTransform: Parent.GameHandler.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Up * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Up, colSide, Vector3.Forward, additionalTransform: Parent.GameHandler.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Down * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Down, colSide, Vector3.Backward, additionalTransform: Parent.GameHandler.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Forward, colFront, Vector3.Up, additionalTransform: Parent.GameHandler.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Backward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Backward, colBack, Vector3.Up, additionalTransform: Parent.GameHandler.Cursors[i].Grip);

                    if (Stardew3D.Mod.State.RenderDebugInteractions)
                    {
                        // Doesn't work
#if false
                        if (cursor.LinearVelocity.Length() > 0.1)
                        {
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward*0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Up, col: Color.LightGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.LinearVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.LinearVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Down, col: Color.LightGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.LinearVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.LinearVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Left, col: Color.LightGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.LinearVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.LinearVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Right, col: Color.LightGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.LinearVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.LinearVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                        }
                        if (false&&cursor.AngularVelocity.Length() > 0.001)
                        {
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Up, col: Color.DarkGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.AngularVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.AngularVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Down, col: Color.DarkGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.AngularVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.AngularVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Left, col: Color.DarkGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.AngularVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.AngularVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                            RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 0.5f, new(0.02f, 1), new(0, 0, 1, 1), Vector3.Right, col: Color.DarkGreen, upOverride: Vector3.Forward, additionalTransform: Matrix.CreateScale(cursor.AngularVelocity.Length()) * Matrix.CreateLookAt(Vector3.Zero, cursor.AngularVelocity, Vector3.Up) * Parent.GameHandler.Cursors[i].Grip);
                        }
#endif
                    }
                }, Matrix.Identity, staysVisibleAfterFrame: true);
            }
        }

        public override void Update(IRenderHandler.RenderContext ctx)
        {
            base.Update(ctx);

            for ( int i = 0; i < Parent.GameHandler.Cursors.Count; ++i )
            {
                var cursor = Parent.GameHandler.Cursors[i];

                var renderers = Stardew3D.Mod.State.GetRenderHandlersFor(cursor.Holding);
                foreach (var renderer in renderers)
                {
                    renderer?.Render(new()
                    {
                        Time = ctx.Time,
                        TargetScreen = ctx.TargetScreen,

                        MenuSpriteBatch = ctx.MenuSpriteBatch,

                        WorldBatch = ctx.WorldBatch,
                        WorldEnvironment = ctx.WorldEnvironment,
                        WorldCamera = ctx.WorldCamera,
                        WorldTransform = Parent.GetHeldTransformFor(cursor, forDisplay: true),
                        CanBillboard = false,

                        Reset = ctx.Reset,
                        ForceRenderIfNotAlreadyRun = ctx.ForceRenderIfNotAlreadyRun,
                    });
                }
            }
        }
    }
}
