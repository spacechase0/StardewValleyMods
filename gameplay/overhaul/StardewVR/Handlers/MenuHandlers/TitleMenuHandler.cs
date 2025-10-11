using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D.Data;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Render;
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewVR.Handlers.Game;
using static Stardew3D.Handlers.IRenderHandler;

namespace StardewVR.Handlers.Menu;
internal class TitleMenuHandler : GenericMenuHandler<TitleMenu>
{
    private List<(Func<ClickableTextureComponent> Button, BoundingBox BoundingBox)> clickables = new();

    public TitleMenuHandler(VRGameHandler handler, TitleMenu menu)
        : base(handler, menu )
    {
        Vector3 spot = new(0, 3, -15f);
        clickables = new()
        {
            new(
                () => menu.buttons.First( c => c.name == "New" ),
                new BoundingBox(new(spot.X + -5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X + -5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f))
            ),
            new(
                () => menu.buttons.First( c => c.name == "Load" ),
                new BoundingBox(new(spot.X + -1.85f - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X + -1.85f - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f))
            ),
            new(
                () => menu.buttons.First( c => c.name == "Co-op" ),
                new BoundingBox(new(spot.X +  1.85f - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X +  1.85f - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f))
            ),
            new (
                () => menu.buttons.First( c => c.name == "Exit" ),
                new BoundingBox(new(spot.X +  5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X +  5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f))
            ),
        };
    }

    private bool ShowingMainMenu
    {
        get
        {
            var subMenu = TitleMenu.subMenu;
            if (subMenu != null && subMenu is not AboutMenu && subMenu is not LanguageSelectionMenu)
                return false;

            return true;
        }
    }

    public override void Update(IUpdateHandler.UpdateContext ctx)
    {
        ctx.ForceUpdateIfNotAlreadyRun(ctx);

        var subMenu = TitleMenu.subMenu;
        if (subMenu != null)
        {
            var subHandlers = Stardew3D.Mod.State.GetUpdateHandlersFor(subMenu);
            foreach (var subHandler in subHandlers)
            {
                subHandler?.Update(ctx);
            }
        }


        if (ShowingMainMenu && subMenu == null)
        {
            bool setCursorPos = false;

            var leftCursor = new Ray(GameHandler.Global_SecondaryPointerPosition, GameHandler.Global_SecondaryPointerOrientation.Forward);
            var rightCursor = new Ray(GameHandler.Global_PrimaryPointerPosition, GameHandler.Global_PrimaryPointerOrientation.Forward);
            foreach (var button in clickables)
            {
                if (subMenu == null)
                {
                    float? intersection = rightCursor.Intersects(button.BoundingBox);
                    if (intersection.HasValue)
                    {
                        Vector3 intersectionPoint = rightCursor.Position + rightCursor.Direction * intersection.Value;
                        RenderHelper.DrawQuad(Game1.mouseCursors, intersectionPoint + rightCursor.Direction * -1f, new(0.5f), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, 16, 16), -rightCursor.Direction, upOverride: GameHandler.RightController.CurrentRotation.Up);
                        Vector2 clickableLocal = new((intersectionPoint.X - button.BoundingBox.Min.X) / (button.BoundingBox.Max.X - button.BoundingBox.Min.X) * button.Button().bounds.Width,
                                                        button.Button().bounds.Height - (intersectionPoint.Y - button.BoundingBox.Min.Y) / (button.BoundingBox.Max.Y - button.BoundingBox.Min.Y) * button.Button().bounds.Height);
                        Game1.setMousePosition(button.Button().bounds.Location + clickableLocal.ToPoint(), true);
                        setCursorPos = true;
                    }
                }
            }

            if (!setCursorPos)
            {
                Game1.setMousePosition(0, 0, true);
            }
        }
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new RenderData(ctx, this);
    }

    private class RenderData : RenderData<TitleMenuHandler>
    {
        public ModelObject skybox;
        public ModelObject title;
        public ModelObject buttonNewIdle;
        public ModelObject buttonLoadIdle;
        public ModelObject buttonCoopIdle;
        public ModelObject buttonExitIdle;
        public ModelObject buttonNewHover;
        public ModelObject buttonLoadHover;
        public ModelObject buttonCoopHover;
        public ModelObject buttonExitHover;

        private int[] titleInstances;
        private int[] skyboxInstances;
        private List<(Func<ClickableTextureComponent> Button, ModelObject IdleModel, int[] IdleInstances, ModelObject HoverModel, int[] HoverInstances)> clickables = new();
        private int mouse;

        public RenderData(RenderContext ctx, TitleMenuHandler parent)
            : base(ctx, parent)
        {
            // TODO: Use model associations instead
            skybox = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/Skybox");
            title = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle");
            buttonNewIdle = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/New/Idle");
            buttonLoadIdle = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/Load/Idle");
            buttonCoopIdle = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/Coop/Idle");
            buttonExitIdle = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/Exit/Idle");
            buttonNewHover = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/New/Hover");
            buttonLoadHover = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/Load/Hover");
            buttonCoopHover = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/Coop/Hover");
            buttonExitHover = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/GameTitle/Buttons/Exit/Hover");

            skyboxInstances = skybox.Draw(Batch, Matrix.Identity);

            Vector3 spot = new(0, 3, -15f);
            titleInstances = title.Draw(ctx.WorldBatch, Matrix.Identity);

            // TODO: Music toggle button, language menu button, about menu button

            clickables = new()
            {
                new(
                    parent.clickables[0].Button,
                    buttonNewIdle,
                    buttonNewIdle.Draw( Batch, Matrix.Identity ),
                    buttonNewHover,
                    buttonNewHover.Draw( Batch, Matrix.Identity )
                ),
                new(
                    parent.clickables[1].Button,
                    buttonLoadIdle,
                    buttonLoadIdle.Draw( Batch, Matrix.Identity ),
                    buttonLoadHover,
                    buttonLoadHover.Draw( Batch, Matrix.Identity )
                ),
                new(
                    parent.clickables[2].Button,
                    buttonCoopIdle,
                    buttonCoopIdle.Draw( Batch, Matrix.Identity ),
                    buttonCoopHover,
                    buttonCoopHover.Draw( Batch, Matrix.Identity )
                ),
                new (
                    parent.clickables[3].Button,
                    buttonExitIdle,
                    buttonExitIdle.Draw( Batch, Matrix.Identity ),
                    buttonExitHover,
                    buttonExitHover.Draw( Batch, Matrix.Identity )
                ),
            };

            mouse = Batch.AddNonInstanced((env, color, world, view, proj) =>
            {
                if (!Parent.ShowingMainMenu || !renderMousePos.HasValue)
                    return;

                var size = new Vector2(16f / Game1.game1.uiScreen.Width, 16f / Game1.game1.uiScreen.Height) * Game1.pixelZoom * Parent.DisplaySize * 4;
                var size3d = new Vector3(size.X, size.Y, 0);
                var forward = Vector3.TransformNormal(renderMouseFacing, world);
                var up = Vector3.TransformNormal(renderMouseUp, world);
                var offset = Matrix.Identity;
                offset *= Matrix.CreateTranslation(-up * size3d / 2) * Matrix.CreateTranslation(Vector3.Cross(up, forward) * size3d / 2);
                offset *= Matrix.CreateTranslation(forward * 0.1f);

                RenderHelper.GenericEffect.View = view;
                RenderHelper.GenericEffect.Projection = proj;
                RenderHelper.DrawQuad(Game1.mouseCursors, Vector3.Zero, size, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, 16, 16), forward, upOverride: up, col: color, additionalTransform: offset * world );
                //RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Zero, size, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, 16, 16), forward, upOverride: up, col: color, additionalTransform: offset * world );
            }, Matrix.Identity, hasTransparency: true);
        }

        public override void Update(RenderContext ctx)
        {
            base.Update(ctx);

            var subMenu = TitleMenu.subMenu;
            if (subMenu != null)
            {
                var subHandlers = Stardew3D.Mod.State.GetRenderHandlersFor(subMenu);
                foreach (var subHandler in subHandlers)
                {
                    subHandler?.Render(ctx);
                }
            }

            //RenderHelper.DebugRenderGrid();

            skybox.Update(Batch, skyboxInstances, Matrix.CreateTranslation(ctx.WorldCamera.Position));

            if (!Parent.ShowingMainMenu)
            {
                title.Update(Batch, titleInstances, Matrix.Identity, Color.Transparent);
                foreach (var entry in clickables)
                {
                    entry.IdleModel.Update(Batch, entry.IdleInstances, Matrix.Identity, Color.Transparent);
                    entry.HoverModel.Update(Batch, entry.HoverInstances, Matrix.Identity, Color.Transparent);
                }
                return;
            }

            Vector3 spot = new(0, 3, -15f);
            var env = ctx.WorldEnvironment;
            var handler = Parent.GameHandler;

            title.Update(Batch, titleInstances, Matrix.CreateTranslation(spot) * Matrix.CreateTranslation(0, 3, 0), color: Color.White);

            bool foundMouse = false;
            var secondaryCursor = new Ray(handler.Global_SecondaryPointerPosition, handler.Global_SecondaryPointerOrientation.Forward);
            var primaryCursor = new Ray(handler.Global_PrimaryPointerPosition, handler.Global_PrimaryPointerOrientation.Forward);
            foreach (var button in clickables)
            {
                BoundingBox box = Parent.clickables.First(b => b.Button() == button.Button()).BoundingBox;
                var model = button.IdleModel;
                var hiddenModel = button.HoverModel;
                int[] inst = button.IdleInstances;
                int[] hiddenInst = button.HoverInstances;

                if (TitleMenu.subMenu == null)
                {
                    float? intersection = primaryCursor.Intersects(box);
                    if (intersection.HasValue)
                    {
                        Util.Swap(ref model, ref hiddenModel);
                        Util.Swap(ref inst, ref hiddenInst);

                        Vector2 mousePos = Game1.getMousePosition().ToVector2() / Parent.DisplaySize - Parent.DisplaySize / 2;
                        renderMousePos = primaryCursor.Position + primaryCursor.Direction * intersection.Value;
                        renderMouseFacing = -primaryCursor.Direction;
                        renderMouseUp = handler.Global_PrimaryPointerOrientation.Up;
                        Batch.UpdateNonInstanced(mouse, Matrix.CreateTranslation(renderMousePos.Value) * ctx.WorldTransform);

                        foundMouse = true;
                    }
                }

                Vector3 center = new((box.Min.X + box.Max.X) / 2, (box.Min.Y + box.Max.Y) / 2, (box.Min.Z + box.Max.Z) / 2);
                model.Update(Batch, inst, Matrix.CreateScale(button.Button().scale / button.Button().baseScale) * Matrix.CreateTranslation(center), color: Color.White);
                hiddenModel.Update(Batch, hiddenInst, Matrix.CreateScale(button.Button().scale / button.Button().baseScale) * Matrix.CreateTranslation(center), color: Color.Transparent);
            }

            if (!foundMouse)
            {
                renderMousePos = null;
            }
        }

        private Vector3? renderMousePos = null;
        private Vector3 renderMouseFacing = Vector3.Forward;
        private Vector3 renderMouseUp = Vector3.Up;
    }
}
