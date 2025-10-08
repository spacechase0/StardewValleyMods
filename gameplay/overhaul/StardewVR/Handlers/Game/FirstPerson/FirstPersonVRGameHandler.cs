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
using Stardew3D.Handlers.Game;
using Stardew3D.Handlers.Game.FirstPerson;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewVR.Hardware;
using Valve.VR;
using static OpenVR.NET.Devices.VrDevice;
using static Stardew3D.Handlers.Game.IGameHandler;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace StardewVR.Handlers.Game.FirstPerson;

public class FirstPersonVRGameHandler : VRGameHandler
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";
    public override string[] Tags => [CategoryVR, CategoryFirstPerson];

    protected override void UpdateCameraPosition()
    {
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
    }
}
