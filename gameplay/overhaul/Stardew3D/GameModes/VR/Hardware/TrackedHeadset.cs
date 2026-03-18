using StardewValley;
using Valve.VR;

namespace Stardew3D.GameModes.VR.Hardware;
public class TrackedHeadset : TrackedDevice
{
    public float? RefreshRate { get; }

    public TrackedHeadset(uint deviceIndex)
        : base(deviceIndex)
    {
        RefreshRate = CheckProp<float>(ETrackedDeviceProperty.Prop_DisplayFrequency_Float);
        if (RefreshRate.HasValue && RefreshRate.Value > 0)
        {
            GameRunner.instance.TargetElapsedTime = TimeSpan.FromSeconds(1f / RefreshRate.Value);
            GameRunner.instance.IsFixedTimeStep = true;
        }
    }

    public override string ToString()
    {
        return $"TrackedHeadset[DI={DeviceIndex}]";
    }
}
