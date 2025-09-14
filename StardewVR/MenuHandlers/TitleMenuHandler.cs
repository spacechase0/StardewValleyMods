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
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewVR.GameHandlers;

namespace StardewVR.MenuHandlers;
internal class TitleMenuHandler : IMenuHandler
{
    public IVRGameHandler handler;
    public TitleMenu menu;

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

    private PBREnvironment env;

    public TitleMenuHandler(IVRGameHandler handler, TitleMenu menu)
    {
        this.handler = handler;
        this.menu = menu;

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

        env = PBREnvironment.CreateDefault();
    }

    public void UpdateMenu(GameTime time, Action<GameTime> forceMenuUpdateIfNotAlreadyRun)
    {
        forceMenuUpdateIfNotAlreadyRun(time);

        var subMenu = TitleMenu.subMenu;
        if (subMenu != null)
        {
            var subHandlers = Stardew3D.Mod.State.GetMenuHandlersFor(subMenu);
            foreach (var subHandler in subHandlers)
            {
                subHandler.UpdateMenu(time, forceMenuUpdateIfNotAlreadyRun);
            }
        }
    }

    public void RenderMenu(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Action<RenderSteps, SpriteBatch, GameTime, RenderTarget2D> forceMenuRenderIfNotAlreadyRun)
    {
        var drawCtx = Stardew3D.Mod.State.ModelManager.DrawContext;
        drawCtx.SetCamera(handler.Camera.ViewMatrix.Invert());
        drawCtx.SetProjectionMatrix(handler.ProjectionMatrix);

        bool showMainMenu = true;
        var subMenu = TitleMenu.subMenu;
        if (subMenu != null && subMenu is not AboutMenu && subMenu is not LanguageSelectionMenu)
            showMainMenu = false;

        if (subMenu != null)
        {
            var subHandlers = Stardew3D.Mod.State.GetMenuHandlersFor(subMenu);
            foreach (var subHandler in subHandlers)
            {
                subHandler.RenderMenu(step, sb, time, targetScreen, forceMenuRenderIfNotAlreadyRun);
            }
        }

        if (step != RenderSteps.World)
        {
            return;
        }

        skybox.Draw(env, Matrix.Identity);

        //RenderHelper.DebugRenderGrid();

        bool setCursorPos = false;
        if (showMainMenu)
        {
            Vector3 spot = new(0, 3, -15f);
            title.Draw(env, Matrix.CreateTranslation(spot) * Matrix.CreateTranslation(0, 3, 0));

            // TODO: Music toggle button, language menu button, about menu button

            Dictionary<BoundingBox, (ClickableTextureComponent Clickable, ModelObject IdleModel, ModelObject HoverModel)> data = new()
            {
                { new BoundingBox(new(spot.X + -5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X + -5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f)), new(menu.buttons.First( c => c.name == "New" ), buttonNewIdle, buttonNewHover ) },
                { new BoundingBox(new(spot.X + -1.85f - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X + -1.85f - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f)), new(menu.buttons.First( c => c.name == "Load" ), buttonLoadIdle, buttonLoadHover ) },
                { new BoundingBox(new(spot.X +  1.85f - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X +  1.85f - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f)), new(menu.buttons.First( c => c.name == "Co-op" ), buttonCoopIdle, buttonCoopHover ) },
                { new BoundingBox(new(spot.X +  5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0), new Vector3(spot.X +  5.5f  - 1.5f, spot.Y + -5f, spot.Z + 0) + new Vector3(3.15f, 2.54f, 0.24f)), new(menu.buttons.First( c => c.name == "Exit" ), buttonExitIdle, buttonExitHover ) },
            };

            var leftCursor = new Ray(handler.SecondaryPointerPosition, handler.SecondaryPointerOrientation.Forward);
            var rightCursor = new Ray(handler.PrimaryPointerPosition, handler.PrimaryPointerOrientation.Forward);
            foreach (var button in data)
            {
                var model = button.Value.IdleModel;

                if (subMenu == null)
                {
                    float? intersection = rightCursor.Intersects(button.Key);
                    if (intersection.HasValue)
                    {
                        Vector3 intersectionPoint = rightCursor.Position + rightCursor.Direction * intersection.Value;
                        RenderHelper.DrawQuad(Game1.mouseCursors, intersectionPoint + rightCursor.Direction * -1f, new(0.5f), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, 16, 16), -rightCursor.Direction, upOverride: handler.RightController.CurrentRotation.Up);
                        Vector2 clickableLocal = new((intersectionPoint.X - button.Key.Min.X) / (button.Key.Max.X - button.Key.Min.X) * button.Value.Clickable.bounds.Width,
                                                     button.Value.Clickable.bounds.Height - (intersectionPoint.Y - button.Key.Min.Y) / (button.Key.Max.Y - button.Key.Min.Y) * button.Value.Clickable.bounds.Height);
                        Game1.setMousePosition(button.Value.Clickable.bounds.Location + clickableLocal.ToPoint(), true);
                        setCursorPos = true;
                        model = button.Value.HoverModel;
                    }
                }

                Vector3 center = new((button.Key.Min.X + button.Key.Max.X) / 2, (button.Key.Min.Y + button.Key.Max.Y) / 2, (button.Key.Min.Z + button.Key.Max.Z) / 2);
                float f = button.Value.Clickable.scale / button.Value.Clickable.baseScale;
                model.Draw(env, Matrix.CreateScale(button.Value.Clickable.scale / button.Value.Clickable.baseScale) * Matrix.CreateTranslation(center));
            }
        }

        if (showMainMenu && subMenu == null && !setCursorPos)
        {
            Game1.setMousePosition(0, 0, true);
        }
    }
}
