using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Gameplay;
internal class FarmerPointAndClickControlsHandler : FarmerWorldControlsBaseHandler
{
    public readonly IGameMode GameMode;

    public FarmerPointAndClickControlsHandler(IGameMode mode, Farmer obj)
        : base(mode, obj, 4)
    {
        GameMode = mode;
    }

    public class SelectionData
    {
        public object Selected { get; set; }
        public Vector3[] SelectedDisplay { get; set; }
        public float Distance { get; set; } = 4;
    }

    public ConditionalWeakTable<IGameCursor, SelectionData> lastHovered = new();

    protected override void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor)
    {
        var sel = lastHovered.GetOrCreateValue(cursor);
        sel.Selected = null;
        sel.SelectedDisplay = default;
        sel.Distance = 4;

        base.HandleCursor(ctx, cursor);

        if (sel.Selected != null)
        {
            if (cursor.UseItemJustReleased)
            {
                Use(cursor, sel.Selected);
            }
            else if (cursor.InteractJustPressed)
            {
                Interact(cursor, sel.Selected);
            }
        }
    }

    private void Use(IGameCursor cursor, object sel)
    {
        // TODO
    }

    private void Interact(IGameCursor cursor, object sel)
    {
        Item oldTemp = Object.TemporaryItem;
        if (cursor.Holding is Item && Object.ActiveItem != cursor.Holding)
        {
            Object.TemporaryItem = cursor.Holding as Item;
        }

        try
        {
            switch (sel)
            {
                case TerrainFeature tf:
                    tf.performUseAction(tf.Tile);
                    break;
                case Building b:
                    //b.doAction(???, Object);
                    break;
                case Furniture f:
                    // TODO: Reverse patch from GameLocation.checkAction
                    {
                        if (cursor.Holding is StardewValley.Object heldObj && f.performObjectDropInAction(heldObj, probe: false, Object))
                            break;
                        f.checkForAction(Object);
                    }
                    break;
                case StardewValley.Object o:
                    // TODO: Reverse patch from GameLocation.checkAction
                    if (o.Type is "Crafting" or "interactive")
                    {
                        if (cursor.Holding is not StardewValley.Object && o.checkForAction(Object))
                            break;

                        if (cursor.Holding is Item heldItem)
                        {
                            var oldHeld = o.heldObject.Value;
                            o.heldObject.Value = null;
                            bool probe = o.performObjectDropInAction(heldItem, probe: true, Object);
                            o.heldObject.Value = oldHeld;
                            bool perform = o.performObjectDropInAction(heldItem, probe: false, Object, returnFalseIfItemConsumed: true);

                            if (!Object.ignoreItemConsumptionThisFrame && perform)
                            {
                                Object.reduceActiveItemByOne();
                                break;
                            }
                        }

                        o.checkForAction(Object);
                    }
                    else if (o.IsSpawnedObject)
                    {
                        int oldQual = o.quality.Value;
                        Random rand = Utility.CreateDaySaveRandom(o.TileLocation.X, o.TileLocation.Y * 777);
                        if (o.isForage())
                            o.Quality = o.Location.GetHarvestSpawnedObjectQuality(Object, o.isForage(), o.TileLocation, rand);

                        if (o.questItem.Value && o.questId != null && o.questId.Value != "0" && !Object.hasQuest(o.questId.Value))
                            break;

                        if (Object.couldInventoryAcceptThisItem(o))
                        {
                            Object.currentLocation.localSound("pickUpItem");
                            DelayedAction.playSoundAfterDelay("coin", 300);

                            if (!Object.currentLocation.isFarmBuildingInterior())
                            {
                                if (o.isForage())
                                    Object.currentLocation.OnHarvestedForage(Object, o);

                                if (o.ItemId == "789" && Object.currentLocation.Name == "LewisBasement")
                                {
                                    Bat bat = new Bat(Vector2.Zero, -789);
                                    bat.focusedOnFarmers = true;
                                    Game1.changeMusicTrack("none");
                                    Object.currentLocation.playSound("cursed_mannequin");
                                    Object.currentLocation.characters.Add(bat);
                                }
                            }
                            else
                            {
                                Object.gainExperience(0, 5);
                            }

                            Object.addItemToInventoryBool(o.getOne());
                            Game1.stats.ItemsForaged++;
                            if (Object.professions.Contains(13) && rand.NextDouble() < 0.2 && !o.questItem.Value && Object.couldInventoryAcceptThisItem(o) && !Object.currentLocation.isFarmBuildingInterior())
                            {
                                Object.addItemToInventoryBool(o.getOne());
                                Object.gainExperience(2, 7);
                            }

                            Object.currentLocation.objects.Remove(o.TileLocation);
                            break;
                        }
                        o.Quality = oldQual;
                    }
                    break;
                case NPC n:
                    n.checkAction(Object, n.currentLocation);
                    break;
                case FarmAnimal f:
                    if (!f.wasPet.Value)
                        f.pet(Object);
                    break;
            }
        }
        finally
        {
            Object.TemporaryItem = oldTemp;
        }
    }

    protected override bool CheckInteractionPurpose(string purpose)
    {
        return purpose == $"{Mod.Instance.ModManifest.UniqueID}/Action";
    }

    protected override void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor, object obj, Matrix transform, InteractionData interaction, InteractionArea area)
    {
        // Hacky solution, hope it works / doesn't break horrifically
        Vector3[] myVerts =
        [
            cursor.PointerPosition,
            cursor.PointerPosition + cursor.PointerFacing * CullRange,
            cursor.PointerPosition + cursor.PointerUp * (1f / 16)
        ];
        Vector3[] areaVerts = area.GetTransformedShape().Transform(transform);
        if (!GJK_EPA_BCP.CheckIntersection(myVerts, areaVerts, out Vector3 contactPoint, out _, out _))
            return;

        float dist = Vector3.Distance(cursor.PointerPosition, contactPoint);

        var sel = lastHovered.GetOrCreateValue(cursor);
        if (dist > sel.Distance)
            return;

        sel.Selected = obj;
        sel.SelectedDisplay = area.GetTransformedTriangleVertices().Transform(transform);
        sel.Distance = dist;
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new RenderData(ctx, this);
    }

    private class RenderData : RenderData<FarmerPointAndClickControlsHandler>
    {
        private int[] rayInstances;
        private int[] gripInstances;

        public RenderData(IRenderHandler.RenderContext ctx, FarmerPointAndClickControlsHandler parent)
            : base(ctx, parent)
        {
            if (ctx.TargetScreen == Game1.game1.uiScreen)
                return;

            rayInstances = new int[Parent.GameMode.Cursors.Count];
            gripInstances = new int[Parent.GameMode.Cursors.Count];
            for (int i_ = 0; i_ < rayInstances.Length; ++i_)
            {
                int i = i_;
                var cursor = Parent.GameMode.Cursors[i];
                var sel = Parent.lastHovered.GetOrCreateValue(cursor);
                Color col = i == 0 ? Color.Blue : Color.Red;
                var handSize = 0.125f / 4;

                // TODO: Should these be able to be converted to instanced??
                rayInstances[i] = Batch.AddDirect((env, color, world, view, proj) =>
                {
                    float len = Parent.lastHovered.GetValue(cursor, (_) => new SelectionData()).Distance;

                    RenderHelper.GenericEffect.View = view;
                    RenderHelper.GenericEffect.Projection = proj;
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * len / 2, new(0.01f, len), new(0, 0, 1, 1), Vector3.Up, upOverride: Vector3.Forward, additionalTransform: Parent.GameMode.Cursors[i].Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * len / 2, new(0.01f, len), new(0, 0, 1, 1), Vector3.Down, upOverride: Vector3.Forward, additionalTransform: Parent.GameMode.Cursors[i].Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * len / 2, new(0.01f, len), new(0, 0, 1, 1), Vector3.Left, upOverride: Vector3.Forward, additionalTransform: Parent.GameMode.Cursors[i].Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * len / 2, new(0.01f, len), new(0, 0, 1, 1), Vector3.Right, upOverride: Vector3.Forward, additionalTransform: Parent.GameMode.Cursors[i].Pointer);

                    if (sel.Selected != null)
                    {
                        SimpleVertex[] v = sel.SelectedDisplay.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.White * 0.2f)).ToArray();

                        RenderHelper.GenericEffect.Texture = Game1.staminaRect;
                        RenderHelper.GenericEffect.World = Matrix.Identity;
                        {
                            RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing_Transparent_1"];
                            foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
                            }
                        }
                        {
                            RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing_Transparent_2"];
                            foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
                            }
                        }
                    }
                }, Matrix.Identity, staysVisibleAfterFrame: true, hasTransparency: true);
                gripInstances[i] = Batch.AddDirect((RenderBatcher.RenderDirect)((env, color, world, view, proj) =>
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
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Right * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Right, colSide, Vector3.Up, additionalTransform: Parent.GameMode.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Left * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Left, colSide, Vector3.Up, additionalTransform: Parent.GameMode.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Up * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Up, colSide, Vector3.Forward, additionalTransform: Parent.GameMode.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Down * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Down, colSide, Vector3.Backward, additionalTransform: Parent.GameMode.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Forward, colFront, Vector3.Up, additionalTransform: Parent.GameMode.Cursors[i].Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Backward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Backward, colBack, Vector3.Up, additionalTransform: Parent.GameMode.Cursors[i].Grip);

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
                }), Matrix.Identity, staysVisibleAfterFrame: true);
            }
        }
    }
}
