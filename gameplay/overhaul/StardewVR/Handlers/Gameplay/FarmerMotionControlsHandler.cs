using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewVR.Handlers.Game;

namespace StardewVR.Handlers.Gameplay;
internal class FarmerMotionControlsHandler : RendererFor<ModelData, Farmer>, IUpdateHandler
{
    public VRGameHandler GameHandler;

    public FarmerMotionControlsHandler(VRGameHandler handler, Farmer obj)
        : base(obj)
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
        // TODO: optimize
        if (cursor.Holding is not Tool tool) return;

        InteractionData interaction = null;
        foreach (var idEntry in tool.GetExtendedQualifiedIds())
            interaction ??= InteractionData.Get(idEntry);
        if (interaction == null)
            return;

        var baseTransform = GetHeldTransformFor(cursor);
        var basePrevTransform = lastGrips.GetOrCreateValue(cursor).Value;

        List<(IEnumerable Values, Func<object, Vector2> Position2D, Func<object, Vector3> Position, Action<object> PerformAction)> check =
        [
            new(Game1.player.currentLocation.terrainFeatures.Values.ToArray(),
                (obj) => (obj as TerrainFeature).getBoundingBox().Center.ToVector2(),
                (obj) => (obj as TerrainFeature).getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map),
                obj =>
                {
                    var tf = obj as TerrainFeature;
                    if ( tf.performToolAction( tool, 0, tf.Tile ) )
                        Game1.player.currentLocation.terrainFeatures.Remove(tf.Tile);
                }),
            new(Game1.player.currentLocation.resourceClumps.ToArray(),
                (obj) => (obj as TerrainFeature).getBoundingBox().Center.ToVector2(),
                (obj) => (obj as TerrainFeature).getBoundingBox().Center.ToVector2().To3D(Game1.player.currentLocation.Map),
                obj =>
                {
                    var rc = obj as ResourceClump;
                    if ( rc.performToolAction( tool, 0, rc.Tile ) )
                        Game1.player.currentLocation.resourceClumps.Remove(rc);
                }),
            new(Game1.player.currentLocation.Objects.Values.ToArray(),
                (obj) => (obj as StardewValley.Object).TileLocation * Game1.tileSize + new Vector2( 0.5f, 0.5f ),
                (obj) => (obj as StardewValley.Object).TileLocation.ToPoint().To3D(Game1.player.currentLocation.Map),
                obj =>
                {
                    var o = obj as StardewValley.Object;
                    if ( o.performToolAction( tool ) )
                        Game1.player.currentLocation.Objects.Remove(o.TileLocation);
                }),
            new(Game1.player.currentLocation.characters.ToArray(),
                (obj) => (obj as NPC).StandingPixel.ToVector2(),
                (obj) => (obj as NPC).StandingPixel3D,
                obj =>
                {
                    var n = obj as NPC;
                    if ( n is Monster monster && tool is MeleeWeapon weapon )
                    {
                        // TODO: This whole thing should be reverse patched from MeleeWeapon.DoDamage...
                        // And also GameLocation.damageMonster, considering we don't actually want to iterate over every monster here...
                        float effectiveCritChance = weapon.critChance.Value;
                        if (weapon.type.Value == 1)
                        {
                            effectiveCritChance += 0.005f;
                            effectiveCritChance *= 1.12f;
                        }
                        Game1.player.currentLocation.damageMonster(monster.GetBoundingBox(), (int)(weapon.minDamage.Value * (1f + Game1.player.buffs.AttackMultiplier)), (int)(weapon.maxDamage.Value * (1f + Game1.player.buffs.AttackMultiplier)), isBomb: false, weapon.knockback.Value * (1f + Game1.player.buffs.KnockbackMultiplier), (int)(weapon.addedPrecision.Value * (1f + Game1.player.buffs.WeaponPrecisionMultiplier)), effectiveCritChance * (1f + Game1.player.buffs.CriticalChanceMultiplier), weapon.critMultiplier.Value * (1f + Game1.player.buffs.CriticalPowerMultiplier), weapon.type.Value != 1 || !weapon.isOnSpecial, Game1.player);
                    }
                    else
                    {
                        n.hitWithTool( tool );
                    }
                })
        ];
        foreach (var container in check)
        {
            foreach (var entry in container.Values)
            {
                if (Vector2.DistanceSquared(Game1.player.Position, container.Position2D(entry)) >= MathF.Pow(Game1.tileSize * 2, 2))
                    continue;

                InteractionData tfInteraction = null;
                foreach (var idEntry in entry.GetExtendedQualifiedIds())
                    tfInteraction ??= InteractionData.Get(idEntry);

                if (tfInteraction == null)
                    continue;

                var tfTransform = Matrix.CreateTranslation(container.Position(entry));

                foreach (var toolArea in interaction.Areas)
                {
                    if (!toolArea.Purpose.StartsWith($"{Stardew3D.Mod.Instance.ModManifest.UniqueID}/ToolAction/"))
                        continue;

                    var transform = toolArea.Transform * baseTransform;
                    var prevTransform = toolArea.Transform * basePrevTransform;
                    var verts = toolArea.GetShape().Transform(transform);
                    var prevVerts = toolArea.GetShape().Transform(prevTransform);

                    foreach (var tfArea in tfInteraction.Areas)
                    {
                        if (tfArea.Purpose != $"{Stardew3D.Mod.Instance.ModManifest.UniqueID}/Action")
                            continue;

                        var treeVerts = tfArea.GetTransformedShape().Transform(tfTransform);

                        if (!GJK_EPA_BCP.CheckIntersection(verts, treeVerts, out var contact, out var depth, out var normal))
                            continue;
                        if (GJK_EPA_BCP.CheckIntersection(prevVerts, treeVerts, out _, out _, out _))
                            continue;

                        Vector2 objVel = Vector2.Zero;
                        if (entry is Character c)
                            objVel = new Vector2(c.xVelocity, c.yVelocity) / Game1.tileSize;

                        Vector3 vel = cursor.LinearVelocity;// + new Vector3(objVel.X, 0, objVel.Y);
                        // TODO: Fix the following stuff for angular velocity
                        //vel += cursor.AngularVelocity * Vector3.Distance( baseTransform.Translation, transform.Translation );
                        //Log.Debug($"vel:{vel.Length()} {vel} - ({cursor.LinearVelocity} {cursor.AngularVelocity} {Vector3.Distance(baseTransform.Translation, transform.Translation)})");

                        if (vel.Length() < 0.625f)
                            continue;

                        if (toolArea.Purpose == $"{Stardew3D.Mod.Instance.ModManifest.UniqueID}/ToolAction/Impact")
                        {
                            // Only allow hits that are going similarly angled to the tool's angle
                            // If you hit it pointing the wrong way, the tool will be oriented the wrong way, so the hit will be ignored
                            if (Vector3.Dot(transform.Left.Normalized(), vel.Normalized()) < 0.5) // about 60 degrees in any direction
                                continue;
                        }

                        tool.lastUser = Game1.player;
                        tool.swingTicker++;
                        container.PerformAction(entry);
                    }
                }
            }
        }

        lastGrips.AddOrUpdate(cursor, new(baseTransform));
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
