using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Stardew3D;
using Stardew3D.Rendering;
using StardewVR.Hardware;
using Valve.VR;

namespace StardewVR.GameHandlers;
public interface IVRGameHandler : IGameHandler
{
    public CVRSystem VR { get; }

    public IReadOnlyList<TrackedDevice> Devices { get; }
    public TrackedHeadset Headset { get; }
    public TrackedController LeftController { get; }
    public TrackedController RightController { get; }

    public ICamera Camera { get; }

    public Point EmulatedCursor { get; set; }

    // TODO: Make all these less hardcoded here and specific to menus
    public Vector3 PrimaryPointerPosition { get; }
    public Matrix PrimaryPointerOrientation { get; }
    public Vector3 SecondaryPointerPosition { get; }
    public Matrix SecondaryPointerOrientation { get; }
    public bool LeftClick { get; }
    public bool RightClick { get; }
    public Vector2 CurrentScroll { get; }
}
