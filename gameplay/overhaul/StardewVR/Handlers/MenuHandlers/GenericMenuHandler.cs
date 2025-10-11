using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D;
using Stardew3D.Data;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Render;
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewVR.Handlers.Game;

namespace StardewVR.Handlers.Menu;
internal class GenericMenuHandler<TMenu> : RendererFor<ModelData, TMenu>, IUpdateHandler
    where TMenu : IClickableMenu
{
    public VRGameHandler GameHandler;

    public Matrix BaseOrientation;
    public Vector3 DisplayPosition;
    public Vector2 DisplaySize;

    public GenericMenuHandler(VRGameHandler handler, TMenu menu)
        : base($"({Stardew3D.Mod.Instance.ModManifest.UniqueID}/Menu){menu.GetType().Namespace}.{menu.GetType().Name}", menu)
    {
        GameHandler = handler;

        var basePosition = handler.Camera.Position;
        BaseOrientation = handler.Camera.ViewMatrix.NoTranslation();

        // TODO: Configurable distance for these menus
        DisplayPosition = basePosition + BaseOrientation.Forward * 5;
        DisplaySize = new Vector2(Game1.game1.uiScreen.Width / (float)Game1.game1.uiScreen.Height, 1) * 3;
    }

    public virtual void Update(IUpdateHandler.UpdateContext ctx)
    {
        ctx.ForceUpdateIfNotAlreadyRun(ctx);

        bool setCursorPos = false;

        BoundingBox display = new(new(-DisplaySize.X / 2, -DisplaySize.Y / 2, 0), new(DisplaySize.X / 2, DisplaySize.Y / 2, 0.05f));

        Matrix cursorTransform = Matrix.CreateTranslation(-DisplayPosition) * BaseOrientation.Invert();
        Vector3 cursorPos = Vector3.Transform(GameHandler.Global_PrimaryPointerPosition, cursorTransform);
        Vector3 cursorDir = Vector3.TransformNormal(GameHandler.Global_PrimaryPointerOrientation.Forward, cursorTransform);
        Ray cursor = new(cursorPos, cursorDir);
        var spot = cursor.Intersects(display);
        if (spot.HasValue)
        {
            var intersectionPoint = cursor.Position + cursor.Direction * spot.Value;
            Vector2 clickableLocal = new((intersectionPoint.X - display.Min.X) / (display.Max.X - display.Min.X) * Game1.game1.uiScreen.Bounds.Width,
                                          Game1.game1.uiScreen.Bounds.Height - (intersectionPoint.Y - display.Min.Y) / (display.Max.Y - display.Min.Y) * Game1.game1.uiScreen.Bounds.Height);
            Game1.setMousePosition(Game1.game1.uiScreen.Bounds.Location + clickableLocal.ToPoint(), true);
            setCursorPos = true;
        }

        if (!setCursorPos)
        {
            Game1.setMousePosition(0, 0, true);
        }
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new RenderData(ctx, this);
    }

    private class RenderData : RenderData<GenericMenuHandler<TMenu>>
    {
        private int menuInstance = -1;

        public RenderData(IRenderHandler.RenderContext ctx, GenericMenuHandler<TMenu> parent)
            : base(ctx, parent)
        {
            if (ctx.TargetScreen == Game1.game1.uiScreen)
                return;

            menuInstance = Batch.AddNonInstanced((env, color, world, view, proj) =>
            {
                RenderHelper.DrawQuad(Game1.game1.uiScreen, Vector3.Zero, Parent.DisplaySize, Game1.game1.uiScreen.Bounds, Parent.BaseOrientation.Backward, upOverride: Parent.BaseOrientation.Up, col: color, additionalTransform: world);
            }, Matrix.Identity, hasTransparency: true);
        }

        public override void Update(IRenderHandler.RenderContext ctx)
        {
            base.Update(ctx);
            ctx.WorldBatch.UpdateNonInstanced(menuInstance, Matrix.CreateTranslation(Parent.DisplayPosition));
        }
    }
}
