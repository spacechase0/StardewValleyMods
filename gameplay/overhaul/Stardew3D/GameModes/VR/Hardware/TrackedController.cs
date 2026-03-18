using System.Numerics;
using Valve.VR;

namespace Stardew3D.GameModes.VR.Hardware;
public class TrackedController : TrackedDevice
{
    internal ulong[] _buttonMasks;
    internal string[] _buttonNames;
    internal bool[] _buttonsTouched;
    internal bool[] _buttonsPressed;
    internal EVRControllerAxisType[] _axisTypes = new EVRControllerAxisType[Valve.VR.OpenVR.k_unControllerStateAxisCount];
    internal EVRControllerAxisType[] _axisTypesNoEmpty;
    internal Microsoft.Xna.Framework.Vector2[] _axisValues;

    public string Type { get; } // Apparently this is driver-specific? So not much practical use
    public ETrackedControllerRole Role { get; }
    public IReadOnlyList<bool> ButtonsTouched => _buttonsTouched;
    public IReadOnlyList<bool> ButtonsPressed => _buttonsPressed;
    public IReadOnlyList<string> ButtonNames => _buttonNames;
    public IReadOnlyList<EVRControllerAxisType> AxisTypes => _axisTypesNoEmpty;
    public IReadOnlyList<Microsoft.Xna.Framework.Vector2> AxisValues => _axisValues;

    public TrackedController(uint deviceIndex)
        : base(deviceIndex)
    {
        Role = (ETrackedControllerRole)CheckProp<int>(ETrackedDeviceProperty.Prop_ControllerRoleHint_Int32);
        Type = CheckProp<string>(ETrackedDeviceProperty.Prop_ControllerType_String);
        CheckProp<string>(ETrackedDeviceProperty.Prop_AttachedDeviceId_String);

        var supportedButtons = CheckProp<ulong>(ETrackedDeviceProperty.Prop_SupportedButtons_Uint64);
        int buttonCount = BitOperations.PopCount(supportedButtons);
        _buttonMasks = new ulong[buttonCount];
        _buttonsTouched = new bool[buttonCount];
        _buttonsPressed = new bool[buttonCount];
        _buttonNames = new string[buttonCount];
        ulong currMask = 1;
        for (int i = 0; i < buttonCount; ++i)
        {
            while ((supportedButtons & currMask) == 0)
            {
                currMask <<= 1;
                if (currMask == 0) // Overflowed
                    break;
            }
            if (currMask == 0)
                break;

            _buttonMasks[i] = currMask;
            _buttonNames[i] = Valve.VR.OpenVR.System.GetButtonIdNameFromEnum((EVRButtonId)currMask);

            currMask <<= 1;
        }

        List<EVRControllerAxisType> axisTypes = new();
        for ( int i = 0; i < Valve.VR.OpenVR.k_unControllerStateAxisCount; ++i )
        {
            var type = ( EVRControllerAxisType ) CheckProp<int>(ETrackedDeviceProperty.Prop_Axis0Type_Int32 + i);

            _axisTypes[i] = type;
            if (type != EVRControllerAxisType.k_eControllerAxis_None)
            {
                axisTypes.Add(type);
            }
        }
        _axisTypesNoEmpty = axisTypes.ToArray();
        _axisValues = new Microsoft.Xna.Framework.Vector2[axisTypes.Count];
    }
    public override string ToString()
    {
        return $"TrackedController[{Role}, DI={DeviceIndex}]";
    }
}
