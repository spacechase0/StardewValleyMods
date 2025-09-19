using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SixLabors.ImageSharp.Processing;
using SpaceShared;
using Stardew3D;
using Stardew3D.FirstPerson;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Mods;
using StardewVR.Hardware;
using Valve.VR;
using static Stardew3D.IGameHandler;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace StardewVR.GameHandlers.FirstPerson;

public class FirstPersonVRGameHandler : FirstPersonGameHandler, IVRGameHandler
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";
    public override string[] Tags => [CategoryVR, CategoryFirstPerson];

    public override Camera Camera { get; } = new();

    public Point EmulatedCursor { get; set; }

    private global::OpenVR.NET.VR _vr;
    public CVRSystem VR { get; private set; }

    internal RenderTarget2D leftScreen, rightScreen;
    internal RenderTarget2D uiScreen => Game1.game1.uiScreen;

    private delegate TrackedDevice TrackedDeviceFactoryFunction( uint deviceIndex );
    private TrackedDeviceFactoryFunction[] TrackedDeviceFactory =
    {
        null, // Invalid
        deviceIndex => new TrackedHeadset( deviceIndex ), // HMD
        deviceIndex => new TrackedController( deviceIndex ), // Controller
        deviceIndex => new TrackedDevice( deviceIndex ), // GenericTracker
        deviceIndex => new TrackedDevice( deviceIndex ), // TrackingReference
        deviceIndex => new TrackedDevice( deviceIndex ), // DisplayRedirect
    };
    private List<TrackedDevice> _devices = new();
    public IReadOnlyList<TrackedDevice> Devices => _devices;

    private int? headsetIndex, leftControllerIndex, rightControllerIndex;
    public TrackedHeadset Headset => headsetIndex.HasValue ? _devices[headsetIndex.Value] as TrackedHeadset : null;
    public TrackedController LeftController => leftControllerIndex.HasValue ? _devices[leftControllerIndex.Value] as TrackedController : null;
    public TrackedController RightController => rightControllerIndex.HasValue ? _devices[rightControllerIndex.Value] as TrackedController : null;

    // TODO: Make all these less hardcoded here and specific to menus
    private ulong globalActionSetHandle, menuActionSetHandle;
    private ulong pointerPrimaryActionHandle, pointerSecondaryActionHandle;
    private ulong leftClickActionHandle, rightClickActionHandle, scrollActionHandle;
    public Vector3 PrimaryPointerPosition { get; private set; }
    public Matrix PrimaryPointerOrientation { get; private set; }
    public Vector3 SecondaryPointerPosition { get; private set; }
    public Matrix SecondaryPointerOrientation { get; private set; }
    public bool LeftClick { get; private set; }
    public bool RightClick { get; private set; }
    public Vector2 CurrentScroll { get; private set; }

    private TimeSpan oldInactiveSleepTime, oldMaxTime, oldTargetTime;
    private bool oldFixedTimestemp, oldVsync;
    public override void SwitchOn()
    {
        base.SwitchOn();

        _vr = new();
        _vr.Events.OnLog += (s, e, o) => Log.Monitor.Log($"{s}\n{o}", e > OpenVR.NET.EventType.InitializationSuccess && e < OpenVR.NET.EventType.NoFous ? LogLevel.Error : LogLevel.Debug);
        if (!_vr.TryStart())
        {
            Log.Error("Failed to start VR");
            return;
        }
        VR = _vr.CVR;

        var aerr = Valve.VR.OpenVR.Applications.AddApplicationManifest(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "game.vrmanifest"), true);
        if (aerr != EVRApplicationError.None) Log.Error($"Failed to add application manifest to OpenVR: {aerr}");

        uint screenWidth = 0, screenHeight = 0;
        VR.GetRecommendedRenderTargetSize(ref screenWidth, ref screenHeight);
        leftScreen = new(Game1.graphics.GraphicsDevice, (int)screenWidth, (int)screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
        leftScreen.Name = "VR Headset (Left Eye)";
        rightScreen = new(Game1.graphics.GraphicsDevice, (int)screenWidth, (int)screenHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
        rightScreen.Name = "VR Headset (Right Eye)";
        //uiScreen = new(Game1.graphics.GraphicsDevice, Game1.viewport.Width, Game1.viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

        globalActionSetHandle = menuActionSetHandle = Valve.VR.OpenVR.k_ulInvalidActionSetHandle;
        pointerPrimaryActionHandle = pointerSecondaryActionHandle = Valve.VR.OpenVR.k_ulInvalidActionHandle;
        leftClickActionHandle = rightClickActionHandle = scrollActionHandle = Valve.VR.OpenVR.k_ulInvalidActionHandle;
        var err = Valve.VR.OpenVR.Input.SetActionManifestPath(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "openvr_input_bindings", "actions.json"));
        if (err != EVRInputError.None) Log.Error($"Failed to set action manifest for OpenVR input: {err}");
        err = Valve.VR.OpenVR.Input.GetActionSetHandle("/actions/global", ref globalActionSetHandle);
        if (err != EVRInputError.None) Log.Error($"Failed to get global action set handle for OpenVR input: {err}");
        err = Valve.VR.OpenVR.Input.GetActionSetHandle("/actions/menu", ref menuActionSetHandle);
        if (err != EVRInputError.None) Log.Error($"Failed to get menu action set handle for OpenVR input: {err}");
        err = Valve.VR.OpenVR.Input.GetActionHandle("/actions/global/in/pointer_primary", ref pointerPrimaryActionHandle);
        if (err != EVRInputError.None) Log.Error($"Failed to get pointer primary action handle for OpenVR input: {err}");
        err = Valve.VR.OpenVR.Input.GetActionHandle("/actions/global/in/pointer_secondary", ref pointerSecondaryActionHandle);
        if (err != EVRInputError.None) Log.Error($"Failed to get pointer primary action handle for OpenVR input: {err}");
        err = Valve.VR.OpenVR.Input.GetActionHandle("/actions/menu/in/left_click", ref leftClickActionHandle);
        if (err != EVRInputError.None) Log.Error($"Failed to get left click action handle for OpenVR input: {err}");
        err = Valve.VR.OpenVR.Input.GetActionHandle("/actions/menu/in/right_click", ref rightClickActionHandle);
        if (err != EVRInputError.None) Log.Error($"Failed to get right click action handle for OpenVR input: {err}");
        err = Valve.VR.OpenVR.Input.GetActionHandle("/actions/menu/in/scroll", ref scrollActionHandle);
        if (err != EVRInputError.None) Log.Error($"Failed to get scroll action handle for OpenVR input: {err}");

        // We absolutely do not want the game to slow down when the window isn't active.
        // That would cause comfort problems in vR
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
    }

    public override void SwitchOff()
    {
        if (_vr != null)
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
        }

        base.SwitchOff();
    }

    private void UpdateInput()
    {
        if (VR == null) return;

        _vr.UpdateInput();

        // From: https://github.com/ValveSoftware/openvr/wiki/IVRSystem::GetDeviceToAbsoluteTrackingPose
        // Without this there is a bit of nausea (at least for me), I believe due to your movements in-world being delayed from your real one
        float fSecondsSinceLastVsync = 0;
        ulong pulFrameCounter = 0;
        VR.GetTimeSinceLastVsync(ref fSecondsSinceLastVsync, ref pulFrameCounter);
        ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;
        float fDisplayFrequency = VR.GetFloatTrackedDeviceProperty(Headset?.DeviceIndex ?? Valve.VR.OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref err);
        float fFrameDuration = 1f / fDisplayFrequency;
        float fVsyncToPhotons = VR.GetFloatTrackedDeviceProperty(Headset?.DeviceIndex ?? Valve.VR.OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float, ref err);
        float fPredictedSecondsFromNow = fFrameDuration - fSecondsSinceLastVsync + fVsyncToPhotons;

        var seatedPoses = new TrackedDevicePose_t[Valve.VR.OpenVR.k_unMaxTrackedDeviceCount];
        var standingPoses = new TrackedDevicePose_t[seatedPoses.Length];
        VR.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseSeated, fPredictedSecondsFromNow, seatedPoses);
        VR.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, fPredictedSecondsFromNow, standingPoses);

        for (uint deviceInd = 0; deviceInd < seatedPoses.Length; ++deviceInd)
        {
            var seated = seatedPoses[deviceInd];
            var standing = standingPoses[deviceInd];
            var seatedTransform = seated.mDeviceToAbsoluteTracking.ToMonogame();
            var standingTransform = standing.mDeviceToAbsoluteTracking.ToMonogame();

            TrackedDevice existing = _devices.FirstOrDefault(dev => dev.Connected && dev.DeviceIndex == deviceInd);
            if (!Valve.VR.OpenVR.System.IsTrackedDeviceConnected(deviceInd))
            {
                if (existing != null)
                    existing.Connected = false;
                continue;
            }

            if (existing == null)
            {
                var deviceClass = Valve.VR.OpenVR.System.GetTrackedDeviceClass(deviceInd);
                var factoryFunc = TrackedDeviceFactory[(int)deviceClass];
                if (factoryFunc != null)
                {
                    _devices.Add(existing = factoryFunc(deviceInd));
                    if (deviceClass == ETrackedDeviceClass.HMD)
                        headsetIndex = _devices.Count - 1;
                    else if (deviceClass == ETrackedDeviceClass.Controller)
                    {
                        var controller = existing as TrackedController;
                        switch (controller.Role)
                        {
                            case ETrackedControllerRole.LeftHand:
                                leftControllerIndex = _devices.Count - 1;
                                break;
                            case ETrackedControllerRole.RightHand:
                                rightControllerIndex = _devices.Count - 1;
                                break;
                        }
                    }
                }
            }

            if (existing != null)
            {
                if (!seated.bPoseIsValid)
                    continue;

                existing.SeatedPosition = seatedTransform.Translation;
                existing.SeatedRotation = seatedTransform.NoTranslation();
                existing.StandingPosition = standingTransform.Translation;
                existing.StandingRotation = standingTransform.NoTranslation();

                if (existing is TrackedController controller)
                {
                    existing.SeatedRotation*= Matrix.CreateRotationX(MathHelper.ToRadians(-35)) * existing.SeatedRotation; // TODO: Better method of determining pointer - maybe using openvr actions?
                    existing.StandingRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-35))* existing.StandingRotation; // TODO: Better method of determining pointer - maybe using openvr actions?

                    VRControllerState_t state = default;
                    bool valid = Valve.VR.OpenVR.System.GetControllerState(deviceInd, ref state, (uint)Marshal.SizeOf<VRControllerState_t>());
                    if (valid)
                    {
                        for (int i = 0; i < controller._buttonMasks.Length; i++)
                        {
                            ulong buttonMask = controller._buttonMasks[i];
                            controller._buttonsPressed[i] = (state.ulButtonPressed & buttonMask) != 0;
                            controller._buttonsTouched[i] = (state.ulButtonTouched & buttonMask) != 0;
                        }

                        VRControllerAxis_t[] axisValues = [state.rAxis0, state.rAxis1, state.rAxis2, state.rAxis3, state.rAxis4];
                        int currAxisIndex = 0;
                        for (int i = 0; i < controller._axisTypes.Length; ++i)
                        {
                            var type = controller._axisTypes[i];
                            if (type == EVRControllerAxisType.k_eControllerAxis_None)
                                continue;
                            controller._axisValues[currAxisIndex++] = new(axisValues[i].x, axisValues[i].y);
                        }
                    }
                }
            }
        }

        unsafe
        {
            var ierr = Valve.VR.OpenVR.Input.UpdateActionState([new() { ulActionSet = globalActionSetHandle, }, new() { ulActionSet = menuActionSetHandle, }], (uint)sizeof(VRActiveActionSet_t));
            if (ierr != EVRInputError.None) Log.Error($"Failed to update action state for OpenVR input: {ierr}");
            InputDigitalActionData_t digitalInput = new();
            ierr = Valve.VR.OpenVR.Input.GetDigitalActionData(leftClickActionHandle, ref digitalInput, (uint)sizeof(InputDigitalActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            if (ierr != EVRInputError.None) Log.Error($"Failed to get left click action data for OpenVR input: {ierr}");
            LeftClick = digitalInput.bState;
            ierr = Valve.VR.OpenVR.Input.GetDigitalActionData(rightClickActionHandle, ref digitalInput, (uint)sizeof(InputDigitalActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            if (ierr != EVRInputError.None) Log.Error($"Failed to get right click action data for OpenVR input: {ierr}");
            RightClick = digitalInput.bState;
            InputAnalogActionData_t analogInput = new();
            ierr = Valve.VR.OpenVR.Input.GetAnalogActionData(scrollActionHandle, ref analogInput, (uint)sizeof(InputAnalogActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            if (ierr != EVRInputError.None) Log.Error($"Failed to get scroll action data for OpenVR input: {ierr}");
            CurrentScroll = new(analogInput.x, analogInput.y);
            InputPoseActionData_t poseInput = new();
            ierr = Valve.VR.OpenVR.Input.GetPoseActionDataRelativeToNow(pointerPrimaryActionHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, 0, ref poseInput, (uint)sizeof(InputPoseActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            if (ierr != EVRInputError.None) Log.Error($"Failed to get primary pointer action data for OpenVR input: {ierr}");
            PrimaryPointerPosition = poseInput.pose.mDeviceToAbsoluteTracking.ToMonogame().Translation;
            PrimaryPointerOrientation = poseInput.pose.mDeviceToAbsoluteTracking.ToMonogame().NoTranslation();
            ierr = Valve.VR.OpenVR.Input.GetPoseActionDataRelativeToNow(pointerSecondaryActionHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, 0, ref poseInput, (uint)sizeof(InputPoseActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            if (ierr != EVRInputError.None) Log.Error($"Failed to get second pointer action data for OpenVR input: {ierr}");
            SecondaryPointerPosition = poseInput.pose.mDeviceToAbsoluteTracking.ToMonogame().Translation;
            SecondaryPointerOrientation = poseInput.pose.mDeviceToAbsoluteTracking.ToMonogame().NoTranslation();

            PrimaryPointerPosition += Camera.Position - Camera.HeadsetRelativePosition;
            SecondaryPointerPosition += Camera.Position - Camera.HeadsetRelativePosition;
        }
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
    }
    public override void AfterUpdate()
    {
        base.BeforeUpdate();
        if (VR == null) return;
    }

    private bool processingDraw = false;
    protected EVREye? ActiveEye = null;
    private xTile.Dimensions.Rectangle oldViewport;
    private xTile.Dimensions.Rectangle oldUiViewport;
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

                DoCamera();

                Game1.graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.CornflowerBlue, 1, 0);
                return true;
            }
            else if (step == RenderSteps.World)
            {
                base.HandleRender(step, sb, time, targetScreen, defaultRender);
                if (Game1.activeClickableMenu != null)
                {
                    var currentMenuHandlers = Stardew3D.Mod.State.GetMenuHandlersFor(Game1.activeClickableMenu);
                    foreach (var handler in currentMenuHandlers)
                    {
                        handler.RenderMenu(step, sb, time, targetScreen, (step, sb, time, targetScreen) => { });
                    }
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
                    bool hasRenderedDefaultYet = false;
                    void RenderMenuDefault(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
                    {
                        if (hasRenderedDefaultYet)
                            return;

                        RenderHelper.DrawQuad(uiScreen, Vector3.Forward * 3 + Vector3.Up * 2, new Vector2(rat, 1) * 3, uiScreen.Bounds, Vector3.Backward);
                        hasRenderedDefaultYet = true;
                    }

                    var currentMenuHandlers = Stardew3D.Mod.State.GetMenuHandlersFor(Game1.activeClickableMenu);
                    foreach (var handler in currentMenuHandlers)
                    {
                        handler.RenderMenu(RenderSteps.World, sb, time, targetScreen, RenderMenuDefault);
                    }

                    if (currentMenuHandlers.Length == 0)
                    {
                        RenderMenuDefault(step, sb, time, targetScreen);
                    }
                }
            }
            else
            {
                if (Game1.displayHUD && Game1.activeClickableMenu == null)
                {
                    float dist = 4f;
                    float scale = 3;
                    float rat = uiScreen.Width / (float)uiScreen.Height;
                    Game1.graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1, 0); // TODO: Could do this as an optional config...
                    RenderHelper.DrawBillboard(Camera, uiScreen, Camera.Position + Camera.HeadsetRotation.Forward * dist, new Vector2(rat, 1) * scale, uiScreen.Bounds);
                }
            }

            void DrawHand( Vector3 pointerPosition, Matrix pointerOrientation, Color col )
            {
                var handSize = 0.125f/4;
                Color colFront = col, colSide = col, colBack = col;
                colSide.R = (byte)(colSide.R * 0.75f);
                colSide.G = (byte)(colSide.G * 0.75f);
                colSide.B = (byte)(colSide.B * 0.75f);
                colBack.R = (byte)(colBack.R * 0.5f);
                colBack.G = (byte)(colBack.G * 0.5f);
                colBack.B = (byte)(colBack.B * 0.5f);

                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + Vector3.Transform(Vector3.Right * handSize / 2, pointerOrientation), Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Transform(Vector3.Right, pointerOrientation), colSide, Vector3.Transform(Vector3.Up, pointerOrientation) );
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + Vector3.Transform(Vector3.Left * handSize / 2, pointerOrientation), Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Transform(Vector3.Left, pointerOrientation), colSide, Vector3.Transform(Vector3.Up, pointerOrientation));
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + Vector3.Transform(Vector3.Up * handSize / 2, pointerOrientation), Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Transform(Vector3.Up, pointerOrientation), colSide, Vector3.Transform(Vector3.Forward, pointerOrientation));
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + Vector3.Transform(Vector3.Down * handSize / 2, pointerOrientation), Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Transform(Vector3.Down, pointerOrientation), colSide, Vector3.Transform(Vector3.Backward, pointerOrientation));
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + Vector3.Transform(Vector3.Forward * handSize / 2, pointerOrientation), Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Transform(Vector3.Forward, pointerOrientation), colFront, Vector3.Transform(Vector3.Up, pointerOrientation));
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + Vector3.Transform(Vector3.Backward * handSize / 2, pointerOrientation), Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Transform(Vector3.Backward, pointerOrientation), colBack, Vector3.Transform(Vector3.Up, pointerOrientation));

                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + pointerOrientation.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), pointerOrientation.Up, upOverride: pointerOrientation.Forward);
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + pointerOrientation.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), pointerOrientation.Down, upOverride: pointerOrientation.Forward);
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + pointerOrientation.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), pointerOrientation.Left, upOverride: pointerOrientation.Forward);
                RenderHelper.DrawQuad(Game1.staminaRect, pointerPosition + pointerOrientation.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), pointerOrientation.Right, upOverride: pointerOrientation.Forward);
            }
            DrawHand(SecondaryPointerPosition, SecondaryPointerOrientation, Color.Red);
            DrawHand(PrimaryPointerPosition, PrimaryPointerOrientation, Color.Blue);

            if (ActiveEye.HasValue)
            {
                Game1.viewport = oldViewport;
                Game1.uiViewport = oldUiViewport;
            }
        }

        return true;
    }

    protected override void DoCamera()
    {
        if (!ActiveEye.HasValue) return;
        if (Headset == null) return;

        if (Context.IsWorldReady)
        {
            Camera.Position = Game1.player.GetPosition3D();
            Camera.Position += new Vector3(0, Headset.CurrentPosition.Y, 0);
            Camera.HeadsetRelativePosition = Headset.CurrentPosition;
        }
        else
        {
            Camera.Position = Vector3.Zero;
            Camera.Position += Headset.CurrentPosition;
            Camera.HeadsetRelativePosition = Headset.CurrentPosition;
        }
        Camera.HeadsetRotation = Headset.CurrentRotation;
        Camera.AdditionalTransform = Matrix.Identity;// VR.GetEyeToHeadTransform(ActiveEye.Value).ToMonogame().Invert();
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;

        var baseProj = VR.GetProjectionMatrix(ActiveEye.Value, 0.1f, 10000).ToMonogame();
        var eyeToHead = VR.GetEyeToHeadTransform(ActiveEye.Value).ToMonogame().Invert();
        var headsetTransform = Camera.ViewMatrix.Invert();
        //headsetTransform = (Headset.CurrentRotation * Matrix.CreateTranslation( Headset.CurrentPosition )).Invert();
        headsetTransform = Matrix.Identity;
        ProjectionMatrix = headsetTransform * eyeToHead * baseProj;

        RenderHelper.GenericEffect.Projection = ProjectionMatrix;
    }
}
