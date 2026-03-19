using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using Valve.VR;
using static Stardew3D.GameModes.IGameMode;

namespace Stardew3D.GameModes.VR;
public abstract partial class VRGameMode : BaseGameMode
{
    public override Camera Camera { get; } = new();
    public override Matrix ProjectionMatrix { get; protected set; }

    protected OpenVR.NET.VR _vr;
    public CVRSystem VR { get; private set; }

    internal RenderTarget2D leftScreen, rightScreen;
    internal RenderTarget2D uiScreen => Game1.game1.uiScreen;

    protected override bool NeedsRenderTargetHandling => false;

    public Point EmulatedCursor { get; set; }

    private TimeSpan oldInactiveSleepTime, oldMaxTime, oldTargetTime;
    private bool oldFixedTimestemp, oldVsync;
    private bool oldGamepadControls;
    private Options.GamepadModes oldGamepadMode;

    private IClickableMenu lastMenu = null;
    private ConditionalWeakTable<IClickableMenu, RenderBatcher> menuBatchers = new();

    public override void SwitchOn(IGameMode previousMode)
    {
        base.SwitchOn(previousMode);

        if (previousMode is VRGameMode vrHandler)
        {
            _vr = vrHandler._vr;
            VR = vrHandler.VR;

            leftScreen = vrHandler.leftScreen;
            rightScreen = vrHandler.rightScreen;

            oldInactiveSleepTime = vrHandler.oldInactiveSleepTime;
            oldFixedTimestemp = vrHandler.oldFixedTimestemp;
            oldMaxTime = vrHandler.oldMaxTime;
            oldTargetTime = vrHandler.oldTargetTime;
            oldVsync = vrHandler.oldVsync;
            oldGamepadControls = vrHandler.oldGamepadControls;
            oldGamepadMode = vrHandler.oldGamepadMode;
        }
        else
        {
            _vr = new();
            _vr.Events.OnLog += (s, e, o) => Log.Monitor.Log($"{s}\n{o}", e > OpenVR.NET.EventType.InitializationSuccess && e < OpenVR.NET.EventType.NoFous ? LogLevel.Error : LogLevel.Debug);
            if (!_vr.TryStart())
            {
                Log.Error("Failed to start VR");
                _vr = null;
                Stardew3D.Mod.State.ActiveMode = null;
                return;
            }
            VR = _vr.CVR;

            var aerr = Valve.VR.OpenVR.Applications.AddApplicationManifest(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "game.vrmanifest"), true);
            if (aerr != EVRApplicationError.None) Log.Error($"Failed to add application manifest to OpenVR: {aerr}");

            uint screenWidth = 0, screenHeight = 0;
            VR.GetRecommendedRenderTargetSize(ref screenWidth, ref screenHeight);
            leftScreen = new(Game1.graphics.GraphicsDevice, (int)screenWidth, (int)screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, Mod.Config.MultisampleCount, RenderTargetUsage.PreserveContents);
            leftScreen.Name = "VR Headset (Left Eye)";
            rightScreen = new(Game1.graphics.GraphicsDevice, (int)screenWidth, (int)screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, Mod.Config.MultisampleCount, RenderTargetUsage.PreserveContents);
            rightScreen.Name = "VR Headset (Right Eye)";
            //uiScreen = new(Game1.graphics.GraphicsDevice, Game1.viewport.Width, Game1.viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, Mod.Config.MultisampleCount, RenderTargetUsage.PreserveContents);

            // We absolutely do not want the game to slow down when the window isn't active.
            // That would cause comfort problems in VR
            oldInactiveSleepTime = GameRunner.instance.InactiveSleepTime;
            GameRunner.instance.InactiveSleepTime = TimeSpan.Zero;

            // Similarly, we need to go above 60 FPS. As much as the game and OpenVR will let us, essentially.
            // We implement our own "fixed time step" specifically for the Update stuff, so that it isn't called as fast the framerate. (TODO)
            oldFixedTimestemp = GameRunner.instance.IsFixedTimeStep;
            GameRunner.instance.IsFixedTimeStep = false;

            oldMaxTime = GameRunner.instance.MaxElapsedTime;
            //GameRunner.instance.MaxElapsedTime = TimeSpan.Zero;

            oldTargetTime = GameRunner.instance.TargetElapsedTime;
            // Set once we get the refresh rate for the headset

            // Don't want to be limited by desktop FPS
            oldVsync = Game1.graphics.SynchronizeWithVerticalRetrace;
            Game1.graphics.SynchronizeWithVerticalRetrace = false;

            // Sometimes we force gamepad input for convenience
            oldGamepadControls = Game1.options.gamepadControls;
            oldGamepadMode = Game1.options.gamepadMode;
        }

        SwitchOnInput(previousMode);
    }

    public override void SwitchOff(IGameMode nextMode)
    {
        if (_vr != null && nextMode is not VRGameMode)
        {
            _vr.GracefullyExit();
            _vr = null;
            VR = null;

            Game1.viewport.Width = uiScreen.Width;
            Game1.viewport.Height = uiScreen.Height;

            leftScreen.Dispose();
            rightScreen.Dispose();
            if (uiScreen != Game1.game1.uiScreen)
                uiScreen.Dispose();
            leftScreen = rightScreen /*= uiScreen*/ = null;

            GameRunner.instance.InactiveSleepTime = oldInactiveSleepTime;
            GameRunner.instance.IsFixedTimeStep = oldFixedTimestemp;
            GameRunner.instance.MaxElapsedTime = oldMaxTime;
            GameRunner.instance.TargetElapsedTime = oldTargetTime;
            Game1.graphics.SynchronizeWithVerticalRetrace = oldVsync;
            Game1.options.gamepadControls = oldGamepadControls;
            Game1.options.gamepadMode = oldGamepadMode;
        }

        menuBatchers.Clear();

        base.SwitchOff(nextMode);
    }
    public override void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling)
    {
        defaultInputHandling(ref keyboardState, ref mouseState, ref gamePadState);
        //mouseState = new(EmulatedCursor.X, EmulatedCursor.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
    }

    public override void BeforeUpdate()
    {
        base.BeforeUpdate();
        if (VR == null) return;

        _vr.Update();
        UpdateInput();

        // Gonna need a SMAPI update past 4.5.1 for Helper.Input.Press to work for controllers when one isn't connected
        Game1.options.gamepadMode = Options.GamepadModes.ForceOn;
        Game1.options.gamepadControls = true;
        if (World_HotbarLeft)
        {
            Mod.Instance.Helper.Input.Press(SButton.LeftTrigger);
        }
        if (World_HotbarRight)
        {
            Mod.Instance.Helper.Input.Press(SButton.RightTrigger);
        }
    }
    public override void AfterUpdate()
    {
        base.AfterUpdate();
        if (VR == null) return;
    }

    private bool processingDraw = false;
    protected EVREye? ActiveEye = null;
    private xTile.Dimensions.Rectangle oldViewport;
    private xTile.Dimensions.Rectangle oldUiViewport;
    protected override WorldRenderer.RenderMode WorldRenderMode
    {
        get
        {
            if (ActiveEye == EVREye.Eye_Left)
                return WorldRenderer.RenderMode.RecreateRenderData;

            if (ActiveEye == EVREye.Eye_Right)
                return WorldRenderer.RenderMode.ClearDataAfterRendering;

            return base.WorldRenderMode;
        }
    }
    public override bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender)
    {
        if (_vr == null) return true;

        if (ActiveEye == null)
        {
            if (!processingDraw)
            {
                if (step == RenderSteps.FullScene)
                {
                    try
                    {
                        processingDraw = true;
                        _vr.UpdateDraw();

                        var drawMeth = Mod.Instance.Helper.Reflection.GetMethod(Game1.game1, "_draw");

                        drawMeth.Invoke(time, uiScreen);
                        Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen); // for flush

                        ActiveEye = EVREye.Eye_Left;
                        drawMeth.Invoke(time, leftScreen);
                        Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen); // for flush
                        var tex = Mod.GetTextureFrom(leftScreen);
                        var texBounds = new VRTextureBounds_t() { uMin = 0, uMax = 1, vMin = 1, vMax = 0 };
                        Valve.VR.OpenVR.Compositor.Submit(ActiveEye.Value, ref tex, ref texBounds, EVRSubmitFlags.Submit_Default);

                        ActiveEye = EVREye.Eye_Right;
                        drawMeth.Invoke(time, rightScreen);
                        Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen); // for flush
                        tex = Mod.GetTextureFrom(rightScreen);
                        Valve.VR.OpenVR.Compositor.Submit(ActiveEye.Value, ref tex, ref texBounds, EVRSubmitFlags.Submit_Default);

                        Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen);
                        Game1.graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Cyan, 1, 0);
                        sb.Begin();
                        sb.Draw(leftScreen, new Rectangle(0, 0, uiScreen.Width, uiScreen.Height), Color.White);
                        sb.DrawString(Game1.smallFont, $"FPS (target {Headset?.RefreshRate}): {(int)(1f / Game1.currentGameTime.ElapsedGameTime.TotalSeconds)}", new(10, 10), Color.Black);
                        //sb.Draw(uiScreen, new Rectangle(0, 0, uiScreen.Width, uiScreen.Height), Color.White);
                        sb.End();

                        return true;
                    }
                    finally
                    {
                        ActiveEye = null;
                        processingDraw = false;
                    }
                }
            }
            else
            {
                Game1.graphics.GraphicsDevice.SetRenderTarget(uiScreen);
                if (step == RenderSteps.FullScene)
                {
                    Game1.graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Transparent, 1, 0);
                }
                else if (step < RenderSteps.MenuBackground)
                {
                    return false;
                }
                /*
                else if (step >= RenderSteps.MenuBackground && step < RenderSteps.GlobalFade)
                {
                    return true;
                }
                //*/

                return base.HandleRender(step, sb, time, uiScreen, defaultRender);
            }
        }
        else
        {
            Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
            Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;
            Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen);

            if (step == RenderSteps.FullScene)
            {
                oldViewport = Game1.viewport;
                oldUiViewport = Game1.uiViewport;
                Game1.viewport.Width = targetScreen.Width;
                Game1.viewport.Height = targetScreen.Height;
                Game1.uiViewport.Width = uiScreen.Width;
                Game1.uiViewport.Height = uiScreen.Height;

                UpdateCamera();

                Game1.graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.CornflowerBlue, 1, 0);
                return true;
            }
            else if (step == RenderSteps.World)
            {
                base.HandleRender(step, sb, time, targetScreen, defaultRender);
                if (Game1.activeClickableMenu != null)
                {
                    var menuBatch = menuBatchers.GetValue(Game1.activeClickableMenu, _ => new(Game1.graphics.GraphicsDevice));
                    var currentMenuHandlers = Stardew3D.Mod.State.GetRenderHandlersFor(Game1.activeClickableMenu);
                    foreach (var handler in currentMenuHandlers)
                    {
                        handler?.Render(new()
                        {
                            Time = time,
                            TargetScreen = targetScreen,

                            MenuSpriteBatch = sb,

                            WorldBatch = menuBatch,
                            WorldEnvironment = WorldRenderer.CurrentEnvironment,
                            WorldCamera = Camera,
                            WorldTransform = Matrix.Identity
                        });
                    }
                    menuBatch.PrepareSprites(Matrix.Identity, Camera);
                    menuBatch.DrawBatched(WorldRenderer.CurrentEnvironment, Matrix.Identity, Camera.ViewMatrix, ProjectionMatrix);
                    menuBatch.HideInstancesAfterFrame();
                }
                return false;
            }
            else if (step == RenderSteps.Minigame || step == RenderSteps.OverlayTemporarySprites)
            {
                // TODO: how to handle these
            }
            else if (step >= RenderSteps.MenuBackground)
            {
                return false;
            }
        }

        return false;
    }
    public override bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;

        if (ActiveEye.HasValue && step == RenderSteps.FullScene)
        {
            // todo - config for ui distance and scale?

            //RenderHelper.DebugRenderGrid();
            //RenderHelper.DebugRender(Camera);

            if (!Context.IsWorldReady)
            {
                float rat = uiScreen.Width / (float)uiScreen.Height;

                if (Game1.activeClickableMenu != null)
                {
                    var menuBatch = menuBatchers.GetValue(Game1.activeClickableMenu, _ => new(Game1.graphics.GraphicsDevice));
                    var currentMenuHandlers = Stardew3D.Mod.State.GetRenderHandlersFor(Game1.activeClickableMenu);
                    foreach (var handler in currentMenuHandlers)
                    {
                        handler?.Render(new()
                        {
                            Time = time,
                            TargetScreen = targetScreen,

                            MenuSpriteBatch = sb,

                            WorldBatch = menuBatch,
                            WorldEnvironment = WorldRenderer.CurrentEnvironment,
                            WorldCamera = Camera,
                            WorldTransform = Matrix.Identity
                        });
                    }
                    menuBatch.PrepareSprites(Matrix.Identity, Camera);
                    menuBatch.DrawBatched(WorldRenderer.CurrentEnvironment, Matrix.Identity, Camera.ViewMatrix, ProjectionMatrix);
                    menuBatch.HideInstancesAfterFrame();
                }
            }
            else
            {
                if (Game1.displayHUD && Game1.activeClickableMenu == null)
                {
                    float dist = 2.5f;
                    float scale = 2;
                    float rat = uiScreen.Width / (float)uiScreen.Height;
                    Game1.graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1, 0); // TODO: Could do this as an optional config...
                    RenderHelper.DrawBillboard(Camera, uiScreen, Camera.Position + Camera.Forward * dist, new Vector2(rat, 1) * scale, uiScreen.Bounds);
                }
            }

            if (ActiveEye.HasValue)
            {
                Game1.viewport = oldViewport;
                Game1.uiViewport = oldUiViewport;
            }
        }

        return true;
    }

    protected abstract void UpdateCameraPosition();

    protected override void UpdateCamera()
    {
        if (!ActiveEye.HasValue) return;
        if (Headset == null) return;

        Camera.Position = Vector3.Zero;
        Camera.Position += Headset.CurrentPosition;
        UpdateCameraPosition();

        Camera.HeadsetRotation = Headset.CurrentRotation;
        Camera.AdditionalTransform = Matrix.Identity;// VR.GetEyeToHeadTransform(ActiveEye.Value).ToMonogame().Invert();
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;

        var baseProj = VR.GetProjectionMatrix(ActiveEye.Value, 0.1f, 10000).ToMonogame();
        var eyeToHead = VR.GetEyeToHeadTransform(ActiveEye.Value).ToMonogame().Inverted();
        var headsetTransform = Camera.ViewMatrix.Inverted();
        //headsetTransform = (Headset.CurrentRotation * Matrix.CreateTranslation( Headset.CurrentPosition )).Invert();
        headsetTransform = Matrix.Identity;
        ProjectionMatrix = headsetTransform * eyeToHead * baseProj;

        RenderHelper.GenericEffect.Projection = ProjectionMatrix;
    }
}
