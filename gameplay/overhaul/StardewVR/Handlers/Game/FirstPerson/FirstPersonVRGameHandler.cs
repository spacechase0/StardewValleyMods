using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using static Stardew3D.Handlers.IRenderHandler;
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
    public override IReadOnlyList<IGameCursor> Cursors => cursors;

    public FirstPersonVRGameHandler()
    {
        cursors =
        [
            new FirstPersonVRCursor(() => Global_PrimaryPointerPosition, () => Global_PrimaryPointerOrientation.Forward, () => Global_PrimaryPointerOrientation.Up,
                                    () => Menu_Primary_LeftClick, () => Menu_Primary_RightClick, () => Menu_Primary_CurrentScroll,
                                    () => Game1.player.ActiveItem),
            new FirstPersonVRCursor(() => Global_SecondaryPointerPosition, () => Global_SecondaryPointerOrientation.Forward, () => Global_SecondaryPointerOrientation.Up,
                                    () => Menu_Secondary_LeftClick, () => Menu_Secondary_RightClick, () => Menu_Secondary_CurrentScroll,
                                    () => null)
            {
                FlipMenuSprite = true,
            },
        ];
    }

    private Vector3 notReadyOffset = Vector3.Zero;
    private Vector3 lastHeadsetPosition = Vector3.Zero;
    public override void BeforeUpdate()
    {
        base.BeforeUpdate();

        foreach (var cursor in cursors)
        {
            (cursor as FirstPersonVRCursor)?.Update();
        }

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

    private RenderBatcher extraBatch = new(Game1.graphics.GraphicsDevice);
    public override bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        var handSize = 0.125f / 4;

        if (ActiveEye.HasValue && step == RenderSteps.FullScene)
        {
            extraBatch.ClearData();

            IEnumerable<Color> cols = [Color.Blue, Color.Red];
            IEnumerator<Color> colIt = cols.GetEnumerator();
            foreach (var cursor_ in Cursors)
            {
                var cursor = cursor_;
                colIt.MoveNext();
                var cursorTransform = Matrix.Identity;
                cursorTransform.Translation = cursor.Position;
                cursorTransform.Forward = cursor.Facing;
                cursorTransform.Up = cursor.Up;
                cursorTransform.Right = Vector3.Cross(cursor.Facing, cursor.Up);
                //cursorTransform = Matrix.CreateBillboard(cursor.Position, cursor.Position + cursor.Facing, -cursor.Facing, cursor.Up);
                Matrix pointerOrientation = cursorTransform.NoTranslation();

                extraBatch.AddNonInstanced((env, col, world, view, proj) =>
                {
                    Color colFront = col, colSide = col, colBack = col;
                    colSide.R = (byte)(colSide.R * 0.75f);
                    colSide.G = (byte)(colSide.G * 0.75f);
                    colSide.B = (byte)(colSide.B * 0.75f);
                    colBack.R = (byte)(colBack.R * 0.5f);
                    colBack.G = (byte)(colBack.G * 0.5f);
                    colBack.B = (byte)(colBack.B * 0.5f);

                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Right * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Right, colSide, Vector3.Up, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Left * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Left, colSide, Vector3.Up, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Up * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Up, colSide, Vector3.Forward, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Down * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Down, colSide, Vector3.Backward, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Forward, colFront, Vector3.Up, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Backward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Backward, colBack, Vector3.Up, additionalTransform: world);

                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Up, upOverride: Vector3.Forward, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Down, upOverride: Vector3.Forward, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Left, upOverride: Vector3.Forward, additionalTransform: world);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Right, upOverride: Vector3.Forward, additionalTransform: world);
                }, cursorTransform, colIt.Current);

                if (cursor.Holding != null)
                {
                    // TODO: Move these over to a farmer addon renderer

                    var renderers = Stardew3D.Mod.State.GetRenderHandlersFor(cursor.Holding);
                    foreach (var renderer in renderers)
                    {
                        renderer?.Render(new()
                        {
                            Time = Game1.currentGameTime,

                            MenuSpriteBatch = sb,

                            WorldBatch = extraBatch,
                            WorldEnvironment = WorldRenderer.CurrentEnvironment,
                            WorldCamera = Camera,
                            WorldTransform = Matrix.CreateScale(1f / 8)
                                * Matrix.CreateFromQuaternion // Was getting a gimbal lock otherwise
                                (
                                      Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(45)))
                                    * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(-90)))
                                    * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(0)))
                                )
                                * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, -0.045f))
                                //* Matrix.CreateRotationX(MathHelper.ToRadians(90))
                                //* Matrix.CreateRotationY(MathHelper.ToRadians(-88))
                                //* Matrix.CreateRotationZ(MathHelper.ToRadians(0))
                                * cursorTransform
                                //* Matrix.CreateTranslation(cursorTransform.Translation)
                                * Matrix.CreateTranslation(new Vector3(0.0f,0.0f,0.0f)),
                            CanBillboard = false,

                            Reset = true,
                        });
                    }

                }
            }
            extraBatch.DrawBatched(WorldRenderer.CurrentEnvironment, Matrix.Identity, Camera.ViewMatrix, ProjectionMatrix);
        }

        return base.AfterRender(step, sb, time, targetScreen);
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
