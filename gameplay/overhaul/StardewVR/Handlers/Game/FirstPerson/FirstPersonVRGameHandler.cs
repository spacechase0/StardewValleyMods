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

public class FirstPersonVRGameHandler : VRGameHandler, IFirstPersonGameHandler
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";
    public override string[] Tags => [CategoryVR, CategoryFirstPerson];

    public Vector3 MovementFacing => Camera.Forward;
    public Vector2 MovementAmount
    {
        get
        {
            Vector2 joyDir = World_MovementJoystick;
            float joyLen = joyDir.Length();
            if (joyLen < 0.2)
                joyDir = Vector2.Zero;
            else
            {
                joyDir.X -= joyDir.X * 0.2f;
                joyDir.Y -= joyDir.Y * 0.2f;
            }
            return joyDir;
        }
    }
    public Vector2 MovementAmountForced => new Vector2(Headset.CurrentPosition.X, Headset.CurrentPosition.Z) - new Vector2(lastHeadsetPosition.X, lastHeadsetPosition.Z);
    private FirstPersonVRCursor[] cursors;
    public IReadOnlyList<IFirstPersonCursor> Cursors => cursors;

    public FirstPersonVRGameHandler()
    {
        cursors =
        [
            new FirstPersonVRCursor(() => Global_PrimaryPointerPosition, () => Global_PrimaryPointerOrientation.Forward, () => Game1.player.ActiveItem,
                                    () => Menu_Primary_LeftClick, () => Menu_Primary_RightClick, () => Menu_Primary_CurrentScroll),
            new FirstPersonVRCursor(() => Global_SecondaryPointerPosition, () => Global_SecondaryPointerOrientation.Forward, () => null,
                                    () => Menu_Primary_LeftClick, () => Menu_Primary_RightClick, () => Menu_Secondary_CurrentScroll),
        ];
    }

    private Vector3 notReadyOffset = Vector3.Zero;
    private Vector3 lastHeadsetPosition = Vector3.Zero;
    public override void BeforeUpdate()
    {
        base.BeforeUpdate();

        if (Context.IsWorldReady)
        {
            notReadyOffset = Vector3.Zero;
        }
        else
        {
            Vector2 forward = new(MovementFacing.X, MovementFacing.Z);
            if (forward == Vector2.Zero)
                return;
            forward.Normalize();
            Vector2 right = Vector2.Transform(forward, Matrix.CreateRotationZ(MathHelper.ToRadians(90)));
            Vector2 movement = forward * MovementAmount.Y + right * MovementAmount.X;
            notReadyOffset += new Vector3(movement.X, 0, movement.Y) * 4 / Game1.tileSize;
        }

        float rotJoyAngle = MathF.Atan2(World_RotationJoystick.Y, World_RotationJoystick.X);
        rotJoyAngle = Util.Wrap(rotJoyAngle + MathHelper.ToRadians(90), 0, MathF.PI * 2);
        float rotMargin = MathHelper.ToRadians(30);
        float rotRight = MathHelper.ToRadians(0 + 90);
        float rotLeft = MathHelper.ToRadians(180 + 90);
        if (Math.Abs(World_RotationJoystick.X) >= 0.2 && ((rotJoyAngle >= rotLeft - rotMargin && rotJoyAngle <= rotLeft + rotMargin) ||
                                                           (rotJoyAngle >= rotRight - rotMargin && rotJoyAngle <= rotRight + rotMargin)))
        {
            float turnAmt = World_RotationJoystick.X + 0.2f * -MathF.Sign(World_RotationJoystick.X);
            Camera.AdditionalRotationY += MathHelper.ToRadians(-turnAmt);
        }
        //Camera.AdditionalRotationY = 0;

        var rotMatrix = Matrix.CreateRotationY(Camera.AdditionalRotationY);

        Global_PrimaryPointerPosition += -Headset.CurrentPosition;
        Global_SecondaryPointerPosition += -Headset.CurrentPosition;
        Global_PrimaryPointerPosition = Vector3.Transform(Global_PrimaryPointerPosition, rotMatrix);
        Global_SecondaryPointerPosition = Vector3.Transform(Global_SecondaryPointerPosition, rotMatrix);
        Global_PrimaryPointerPosition += Camera.Position;
        Global_SecondaryPointerPosition += Camera.Position;
        Global_PrimaryPointerOrientation *= rotMatrix;
        Global_SecondaryPointerOrientation *= rotMatrix;
    }

    public override void AfterUpdate()
    {
        base.AfterUpdate();
        lastHeadsetPosition = Headset.CurrentPosition;
    }

    protected override void UpdateCameraPosition()
    {
        if (Context.IsWorldReady)
        {
            Camera.Position = Game1.player.GetPosition3D();
            Camera.Position += new Vector3(0, Headset.CurrentPosition.Y, 0);
        }
        else
        {
            Camera.Position = Vector3.Zero;
            Camera.Position += Headset.CurrentPosition;
            Camera.Position += notReadyOffset;
        }
    }
}
