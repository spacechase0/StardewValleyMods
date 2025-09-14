using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using Valve.VR;

namespace StardewVR.Hardware;
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
