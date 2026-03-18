using System.Text;
using Microsoft.Xna.Framework;
using SpaceShared;
using Valve.VR;

namespace Stardew3D.GameModes.VR.Hardware;
public class TrackedDevice
{
    public uint DeviceIndex { get; }
    public bool Connected { get; internal set; } = true;

    // TODO: Change to use current universtracking universe
    public Vector3 CurrentPosition => StandingPosition;
    public Matrix CurrentRotation => StandingRotation;

    public Vector3 SeatedPosition { get; internal set; }
    public Matrix SeatedRotation { get; internal set; }
    public Vector3 StandingPosition { get; internal set; }
    public Matrix StandingRotation { get; internal set; }

    private delegate object GetPropFunc(uint deviceIndex, ETrackedDeviceProperty prop, ref ETrackedPropertyError error);

    public T? CheckProp<T>(ETrackedDeviceProperty prop)
    {
        ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;
        T? ret = default;
        switch (typeof(T))
        {
            case Type t when t == typeof(bool):
                ret = (T?)(object)Valve.VR.OpenVR.System.GetBoolTrackedDeviceProperty(DeviceIndex, prop, ref err);
                break;
            case Type t when t == typeof(float):
                ret = (T?)(object)Valve.VR.OpenVR.System.GetFloatTrackedDeviceProperty(DeviceIndex, prop, ref err);
                break;
            case Type t when t == typeof(int):
                ret = (T?)(object)Valve.VR.OpenVR.System.GetInt32TrackedDeviceProperty(DeviceIndex, prop, ref err);
                break;
            case Type t when t == typeof(ulong):
                ret = (T?)(object)Valve.VR.OpenVR.System.GetUint64TrackedDeviceProperty(DeviceIndex, prop, ref err);
                break;
            case Type t when t == typeof(HmdMatrix34_t):
                ret = (T?)(object)Valve.VR.OpenVR.System.GetMatrix34TrackedDeviceProperty(DeviceIndex, prop, ref err);
                break;
            case Type t when t == typeof(string):
                StringBuilder buffer = new((int)Valve.VR.OpenVR.k_unMaxPropertyStringSize + 1);
                var len = Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(DeviceIndex, prop, buffer, Valve.VR.OpenVR.k_unMaxPropertyStringSize, ref err);
                if ( err == ETrackedPropertyError.TrackedProp_Success )
                    ret = (T)(object)buffer.ToString();
                break;
            default:
                throw new NotImplementedException();
        }

        switch (err)
        {
            case ETrackedPropertyError.TrackedProp_Success:
                Log.Debug($"Device {this} - property {prop} value: {ret}");
                return ret;
            case ETrackedPropertyError.TrackedProp_ValueNotProvidedByDevice:
            case ETrackedPropertyError.TrackedProp_UnknownProperty:
                return default;
            default:
                Log.Warn($"Device {this} failed to get property {prop}: {err}");
                return default;
        }
    }

    public TrackedDevice(uint deviceIndex)
    {
        DeviceIndex = deviceIndex;

        // some sort of device id... serial number, maybe?
        CheckProp< bool >( ETrackedDeviceProperty.Prop_WillDriftInYaw_Bool );
        CheckProp< bool >( ETrackedDeviceProperty.Prop_DeviceIsWireless_Bool );
        CheckProp< bool >( ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool );
        CheckProp<string>(ETrackedDeviceProperty.Prop_RegisteredDeviceType_String );
        CheckProp<float>(ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float );
    }

    public override string ToString()
    {
        return $"TrackedDevice[DI={DeviceIndex}]";
    }
}
