using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using Valve.VR;
using static Stardew3D.GameModes.IGameMode;
using Stardew3D.GameModes;
using Stardew3D.GameModes.VR.Hardware;
using Stardew3D.Utilities;

namespace Stardew3D.GameModes.VR;
public abstract partial class VRGameMode
{
    private delegate TrackedDevice TrackedDeviceFactoryFunction(uint deviceIndex);
    private static TrackedDeviceFactoryFunction[] TrackedDeviceFactory =
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

    public abstract class ActionData
    {
        public readonly string ActionSet;
        public readonly string Action;
        public ulong ActionHandle { get; internal set; }

        public ActionData(string actionSet, string action)
        {
            ActionSet = actionSet;
            Action = action;
        }

        public abstract void Update();
    }

    public class PoseAction : ActionData
    {

        public Vector3 LinearVelocity { get; internal set; }
        public Vector3 AngularVelocity { get; internal set; }
        public Matrix Transform { get; internal set; }

        public Vector3 Position => Transform.Translation;
        public Matrix Orientation => Transform.NoTranslation();

        public PoseAction(string actionSet, string action)
        :   base(actionSet, action)
        {
        }

        public override void Update()
        {
            InputPoseActionData_t data = new();
            EVRInputError ierr;
            unsafe
            {
                ierr = Valve.VR.OpenVR.Input.GetPoseActionDataRelativeToNow(ActionHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, 0, ref data, (uint)sizeof(InputPoseActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            }
            if (ierr != EVRInputError.None)
            {
                Log.Error($"Failed to get {Action} action data for OpenVR input: {ierr}");
                return;
            }

            if (data.bActive)
            {
                LinearVelocity = data.pose.vVelocity.ToMonogame();
                AngularVelocity = data.pose.vAngularVelocity.ToMonogame();
                Transform = data.pose.mDeviceToAbsoluteTracking.ToMonogame();
            }
        }

        public void ApplyWorldTransform(Vector3 originalOrigin, Matrix mat)
        {
            Vector3 newOrigin = mat.Translation;
            Matrix rot = mat.NoTranslation();

            Vector3 newPos = Vector3.Transform(Position - originalOrigin, mat.NoTranslation()) + mat.Translation;
            Matrix newOrientation = Orientation;
            newOrientation *= rot;
            Vector3 newLinVel = Vector3.Transform(LinearVelocity, rot);
            Vector3 newAngVel = Vector3.Transform(AngularVelocity, rot);

            Transform = newOrientation * Matrix.CreateTranslation(newPos);
            LinearVelocity = newLinVel;
            AngularVelocity = newAngVel;
        }
    }

    public class DigitalAction : ActionData
    {
        public bool Value { get; internal set; }

        public static implicit operator bool(DigitalAction action) => action.Value;

        public DigitalAction(string actionSet, string action)
        : base(actionSet, action)
        {
        }

        public override void Update()
        {
            InputDigitalActionData_t data = new();
            EVRInputError ierr;
            unsafe
            {
                ierr = Valve.VR.OpenVR.Input.GetDigitalActionData(ActionHandle, ref data, (uint)sizeof(InputDigitalActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            }
            if (ierr != EVRInputError.None)
            {
                Log.Error($"Failed to get {Action} action data for OpenVR input: {ierr}");
                return;
            }

            if (data.bActive)
            {
                Value = data.bState;
            }
        }
    }

    public class Vector2Action : ActionData
    {
        public float X => Value.X;
        public float Y => Value.Y;
        public Vector2 Value { get; internal set; }

        public static implicit operator Vector2(Vector2Action action) => action.Value;

        public Vector2Action(string actionSet, string action)
        : base(actionSet, action)
        {
        }

        public override void Update()
        {
            InputAnalogActionData_t data = new();
            EVRInputError ierr;
            unsafe
            {
                ierr = Valve.VR.OpenVR.Input.GetAnalogActionData(ActionHandle, ref data, (uint)sizeof(InputAnalogActionData_t), Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
            }
            if (ierr != EVRInputError.None)
            {
                Log.Error($"Failed to get {Action} action data for OpenVR input: {ierr}");
                return;
            }

            if (data.bActive)
            {
                Value = new( data.x, data.y );
            }
        }
    }

    // After generating this data, generate the `actions.json` from it as well, for the sake of modded keybinds
    private Dictionary<string, ulong> actionSets = new();
    private List<ActionData> actions = new();

    public PoseAction Pointer_Primary { get; } = new("global", "pointer_primary");
    public PoseAction Pointer_Secondary { get; } = new("global", "pointer_secondary");
    public PoseAction Grip_Primary { get; } = new("global", "grip_primary");
    public PoseAction Grip_Secondary { get; } = new("global", "grip_secondary");
    public DigitalAction Menu_LeftClick { get; } = new("menu", "left_click");
    public DigitalAction Menu_RightClick { get; } = new("menu", "right_click");
    public Vector2Action Menu_CurrentScroll { get; } = new("menu", "scroll");
    public Vector2Action World_MovementJoystick { get; } = new("world", "movement");
    public Vector2Action World_RotationJoystick { get; } = new("world", "rotation");
    public DigitalAction World_HotbarLeft { get; } = new("world", "hotbar_left");
    public DigitalAction World_HotbarRight { get; } = new("world", "hotbar_right");
    public DigitalAction World_UseItem { get; } = new("world", "use_item");
    public DigitalAction World_Interact { get; } = new("world", "interact");

    private void SwitchOnInput(IGameMode previousMode)
    {
        if (previousMode is VRGameMode vrHandler)
        {
            _devices = vrHandler._devices;
            headsetIndex = vrHandler.headsetIndex;
            leftControllerIndex = vrHandler.leftControllerIndex;
            rightControllerIndex = vrHandler.rightControllerIndex;

            actionSets = vrHandler.actionSets;
            actions = vrHandler.actions;

        }
        else
        {
            actions = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(prop => prop.PropertyType.IsAssignableTo(typeof(ActionData)))
                .Select(prop => prop.GetValue( this ) as ActionData)
                .ToList();

            actionSets.Clear();
            foreach (var action in actions)
            {
                actionSets.TryAdd(action.ActionSet, Valve.VR.OpenVR.k_ulInvalidActionSetHandle);
                action.ActionHandle = Valve.VR.OpenVR.k_ulInvalidActionHandle;
            }

            // TODO: Generate actions.json from the above

            var err = Valve.VR.OpenVR.Input.SetActionManifestPath(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "openvr_input_bindings", "actions.json"));
            if (err != EVRInputError.None) Log.Error($"Failed to set action manifest for OpenVR input: {err}");

            foreach (var set in actionSets.Keys)
            {
                ulong handle = Valve.VR.OpenVR.k_ulInvalidActionSetHandle;
                err = Valve.VR.OpenVR.Input.GetActionSetHandle($"/actions/{set}", ref handle);
                if (err != EVRInputError.None)
                    Log.Error($"Failed to get {set} action set handle for OpenVR input: {err}");

                actionSets[set] = handle;
            }


            foreach (var action in actions)
            {
                ulong handle = Valve.VR.OpenVR.k_ulInvalidActionHandle;
                err = Valve.VR.OpenVR.Input.GetActionSetHandle($"/actions/{action.ActionSet}/in/{action.Action}", ref handle);
                if (err != EVRInputError.None)
                    Log.Error($"Failed to get {action.Action} action handle for OpenVR input: {err}");

                action.ActionHandle = handle;
            }
        }
    }


    private void UpdateInput()
    {
        if (VR == null) return;

        _vr.UpdateInput();

        // From: https://github.com/ValveSoftware/openvr/wiki/IVRSystem::GetDeviceToAbsoluteTrackingPose
        // Without this there is a bit of nausea (at least for me), I believe due to your position in-world being delayed from your real one
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
                    existing.SeatedRotation *= Matrix.CreateRotationX(MathHelper.ToRadians(-35)) * existing.SeatedRotation; // TODO: Better method of determining pointer - maybe using openvr actions?
                    existing.StandingRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-35)) * existing.StandingRotation; // TODO: Better method of determining pointer - maybe using openvr actions?

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

        EVRInputError ierr;
        VRActiveActionSet_t[] actionSetHandles = actionSets.Values.Select(handle => new VRActiveActionSet_t()
        {
            ulActionSet = handle,
        }).ToArray();

        unsafe
        {
            ierr = Valve.VR.OpenVR.Input.UpdateActionState(actionSetHandles, (uint)sizeof(VRActiveActionSet_t));
        }
        if (ierr != EVRInputError.None)
            Log.Error($"Failed to update action states for OpenVR input: {ierr}");

        foreach (var action in actions)
            action.Update();
    }
}
