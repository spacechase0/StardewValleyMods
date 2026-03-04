using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using Stardew3D.Handlers.Game;
using Stardew3D.Handlers.Game.FirstPersonVR;

namespace Stardew3D.Handlers.Menu;
internal class GenericMenuHandler<TMenu> : RendererFor<MenuModelData, TMenu>, IUpdateHandler
    where TMenu : IClickableMenu
{
    public VRGameHandler GameHandler;

    public Matrix BaseOrientation;
    public Vector3 DisplayPosition;
    public Vector2 DisplaySize;

    public Dictionary<ClickableComponent, BoundingBox> Clickables { get; } = new();

    public Dictionary<IGameCursor, Matrix?> cursorTargetMapping = new();

    public GenericMenuHandler(VRGameHandler handler, TMenu menu)
        : base(menu)
    {
        GameHandler = handler;

        var basePosition = handler.Camera.Position;
        BaseOrientation = handler.Camera.ViewMatrix.NoTranslation().Inverted();

        // TODO: Configurable distance for these menus
        DisplayPosition = basePosition + BaseOrientation.Forward * 5;
        DisplaySize = new Vector2(Game1.game1.uiScreen.Width / (float)Game1.game1.uiScreen.Height, 1) * 3;
    }

    public virtual void Update(IUpdateHandler.UpdateContext ctx)
    {
        ctx.ForceUpdateIfNotAlreadyRun(ctx);

        foreach ( var cursor in GameHandler.Cursors.Reverse() )
            HandleCursor(ctx, cursor);
    }

    protected virtual void HandleCursor(IUpdateHandler.UpdateContext ctx, IGameCursor cursor)
    {
        BoundingBox display = new(new(-DisplaySize.X / 2, -DisplaySize.Y / 2, 0), new(DisplaySize.X / 2, DisplaySize.Y / 2, 0.05f));

        Matrix cursorTransform = Matrix.CreateTranslation(-DisplayPosition) * BaseOrientation.Inverted();
        Vector3 cursorPos = Vector3.Transform(cursor.PointerPosition, cursorTransform);
        Vector3 cursorDir = Vector3.TransformNormal(cursor.PointerFacing, cursorTransform);
        Ray cursorRay = new(cursorPos, cursorDir);
        var spot = cursorRay.Intersects(display);
        if (spot.HasValue)
        {
            var intersectionPoint = cursorRay.Position + cursorRay.Direction * spot.Value;
            cursorTargetMapping[cursor] = Matrix.CreateTranslation(intersectionPoint) * Matrix.CreateLookAt(intersectionPoint, cursor.PointerPosition, cursor.PointerUp);

            Vector2 clickableLocal = new((intersectionPoint.X - display.Min.X) / (display.Max.X - display.Min.X) * Game1.game1.uiScreen.Bounds.Width,
                                         Game1.game1.uiScreen.Bounds.Height - (intersectionPoint.Y - display.Min.Y) / (display.Max.Y - display.Min.Y) * Game1.game1.uiScreen.Bounds.Height);
            Object.performHoverAction((int)clickableLocal.X, (int)clickableLocal.Y);
        }
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new RenderData(ctx, this);
    }

    protected class RenderData : RenderData<GenericMenuHandler<TMenu>>
    {
        private int menuInstance = -1;
        private int[] cursorInstances;

        public RenderData(IRenderHandler.RenderContext ctx, GenericMenuHandler<TMenu> parent)
            : base(ctx, parent)
        {
            if (ctx.TargetScreen == Game1.game1.uiScreen)
                return;

            menuInstance = Batch.AddNonInstanced((env, color, world, view, proj) =>
            {
                RenderHelper.DrawQuad(Game1.game1.uiScreen, Vector3.Zero, Parent.DisplaySize, Game1.game1.uiScreen.Bounds, Parent.BaseOrientation.Backward, upOverride: Parent.BaseOrientation.Up, col: color, additionalTransform: world);
            }, Matrix.Identity, hasTransparency: true);

            cursorInstances = new int[Parent.GameHandler.Cursors.Count];
            for (int i = cursorInstances.Length - 1; i >= 0; --i)
            {
                bool flip = (parent.GameHandler.Cursors[i] as FirstPersonVRCursor)?.FlipMenuSprite ?? false;
                cursorInstances[i] = Batch.AddNonInstanced((env, color, world, view, proj) =>
                {
                    var size = new Vector2(16f / Game1.game1.uiScreen.Width, 16f / Game1.game1.uiScreen.Height) * Game1.pixelZoom * Parent.DisplaySize * 4;
                    var size3d = new Vector3(size.X, size.Y, 0);

                    var offset = Matrix.Identity;
                    offset *= Matrix.CreateTranslation(world.Down * size3d / 2) * Matrix.CreateTranslation(world.Right * size3d / 2);
                    offset *= Matrix.CreateTranslation(world.Forward * 0.1f);

                    RenderHelper.GenericEffect.View = view;
                    RenderHelper.GenericEffect.Projection = proj;
                    RenderHelper.DrawQuad(Game1.mouseCursors, Vector3.Zero, size, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, 16, 16), Vector3.Forward, upOverride: Vector3.Up, col: color, additionalTransform: world, texCoordEffect: flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                }, Matrix.Identity, hasTransparency: true);
            }
        }

        public override void Update(IRenderHandler.RenderContext ctx)
        {
            base.Update(ctx);

            ctx.WorldBatch.UpdateNonInstanced(menuInstance, Matrix.CreateTranslation(Parent.DisplayPosition));

            foreach (var cursor in Parent.GameHandler.Cursors.Reverse())
            {
                if (!Parent.cursorTargetMapping.TryGetValue(cursor, out var cursorTransform) || !cursorTransform.HasValue)
                    continue;

                ctx.WorldBatch.UpdateNonInstanced(menuInstance, cursorTransform.Value);
            }
        }
    }
}
