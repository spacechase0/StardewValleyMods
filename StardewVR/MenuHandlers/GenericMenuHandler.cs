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
using StardewVR.GameHandlers.FirstPerson;

namespace StardewVR.MenuHandlers;
internal class GenericMenuHandler : IMenuHandler
{
    public IVRGameHandler handler;
    public IClickableMenu menu;

    private Vector3 BasePosition;
    private Matrix BaseOrientation;

    private PBREnvironment env;

    public GenericMenuHandler(IVRGameHandler handler, IClickableMenu menu)
    {
        this.handler = handler;
        this.menu = menu;

        env = PBREnvironment.CreateDefault();

        BasePosition = handler.Camera.Position;
        BaseOrientation = (handler.Camera as StardewVR.GameHandlers.FirstPerson.Camera).HeadsetRotation;
    }

    public void UpdateMenu(GameTime time, Action<GameTime> forceMenuUpdateIfNotAlreadyRun)
    {
        forceMenuUpdateIfNotAlreadyRun(time);
    }

    public void RenderMenu(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Action<RenderSteps, SpriteBatch, GameTime, RenderTarget2D> forceMenuRenderIfNotAlreadyRun)
    {
        bool setCursorPos = false;

        if (step != RenderSteps.World)
        {
            forceMenuRenderIfNotAlreadyRun(step, sb, time, targetScreen);
            return;
        }

        // TODO: Configurable distance for these menus
        var displayPos = BasePosition + BaseOrientation.Forward * 5;
        var displaySize = new Vector2(Game1.game1.uiScreen.Width / (float)Game1.game1.uiScreen.Height, 1) * 3;
        //RenderHelper.DrawQuad(Game1.staminaRect, displayPos - BaseOrientation.Forward * 0.1f, displaySize, Game1.staminaRect.Bounds, BaseOrientation.Backward, upOverride: BaseOrientation.Up);
        RenderHelper.DrawQuad(Game1.game1.uiScreen, displayPos, displaySize, Game1.game1.uiScreen.Bounds, BaseOrientation.Backward, upOverride: BaseOrientation.Up);

        BoundingBox display = new(new(-displaySize.X / 2, -displaySize.Y / 2, 0), new(displaySize.X / 2, displaySize.Y / 2, 0.05f));

        Matrix cursorTransform = Matrix.CreateTranslation( -displayPos ) * BaseOrientation.Invert();
        Vector3 cursorPos = Vector3.Transform(handler.PrimaryPointerPosition, cursorTransform);
        Vector3 cursorDir = Vector3.TransformNormal(handler.PrimaryPointerOrientation.Forward, cursorTransform);
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
}
