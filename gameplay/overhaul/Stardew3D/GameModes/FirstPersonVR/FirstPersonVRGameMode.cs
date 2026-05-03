using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.GameModes.FirstPerson;
using Stardew3D.GameModes.VR;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Mods;
using static Stardew3D.GameModes.IGameMode;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Stardew3D.GameModes.FirstPersonVR;

public class FirstPersonVRGameMode : VRGameMode, IFirstPersonGameMode
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/FirstPersonVR";
    public override string[] Tags => [CategoryVR, CategoryFirstPerson, FeatureMotionControls, FeaturePointAndClick];

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
    public Vector2 MovementAmountForced
    {
        get
        {
            // TODO: Fix. This is just incorrect and bad feeling at the moment, so disabled for now.
            return Vector2.Zero;
            Vector2 diff = (new Vector2(lastHeadsetPosition.X, lastHeadsetPosition.Z) - new Vector2(Headset.CurrentPosition.X, Headset.CurrentPosition.Z)) * Game1.tileSize;
            return Vector2.Transform(diff, Matrix.CreateRotationZ(Camera.AdditionalRotationY));
        }
    }
    private FirstPersonVRCursor[] cursors;
    public override IReadOnlyList<IGameCursor> Cursors => cursors;

    public FirstPersonVRGameMode()
    {
        cursors =
        [
            new FirstPersonVRCursor(() => Pointer_Primary.Transform, () => Grip_Primary.Transform,
                                    () => Grip_Primary.LinearVelocity, () => Grip_Primary.AngularVelocity,
                                    () => Menu_LeftClick, () => Menu_RightClick, () => Menu_CurrentScroll,
                                    () => Game1.player.ActiveItem, () => World_UseItem, () => World_Interact),
            new FirstPersonVRCursor(() => Pointer_Secondary.Transform, () => Grip_Secondary.Transform,
                                    () => Grip_Secondary.LinearVelocity, () => Grip_Secondary.AngularVelocity,
                                    () => false, () => false, () => Vector2.Zero,
                                    () => null, () => false, () => false)
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
        if (Math.Abs(World_RotationJoystick.X) >= 0.2 && (rotJoyAngle >= rotLeft - rotMargin && rotJoyAngle <= rotLeft + rotMargin ||
                                                           rotJoyAngle >= rotRight - rotMargin && rotJoyAngle <= rotRight + rotMargin))
        {
            float turnAmt = World_RotationJoystick.X + 0.2f * -MathF.Sign(World_RotationJoystick.X);
            Camera.AdditionalRotationY += MathHelper.ToRadians(-turnAmt);
        }
        //Camera.AdditionalRotationY = 0;

        var rotMatrix = Matrix.CreateRotationY(Camera.AdditionalRotationY) * Matrix.CreateTranslation(Camera.Position);

        Pointer_Primary.ApplyWorldTransform(Headset.CurrentPosition, rotMatrix);
        Grip_Primary.ApplyWorldTransform(Headset.CurrentPosition, rotMatrix);
        Pointer_Secondary.ApplyWorldTransform(Headset.CurrentPosition, rotMatrix);
        Grip_Secondary.ApplyWorldTransform(Headset.CurrentPosition, rotMatrix);
    }

    public override void AfterUpdate()
    {
        //Log.Debug($"position {Game1.player.Position}");
        //Log.Debug($"fractional? {Game1.player.Position.X % (1f / 64)} {Game1.player.Position.Y % (1f / 64)}");
        //Log.Debug($"headset {(lastHeadsetPosition = Headset.CurrentPosition).X % (1f / 64)} {(lastHeadsetPosition = Headset.CurrentPosition).Z % (1f / 64)}");
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

#if false
            IEnumerable<Color> cols = [Color.Blue, Color.Red];
            IEnumerator<Color> colIt = cols.GetEnumerator();
            foreach (var cursor_ in Cursors)
            {
                var cursor = cursor_;
                colIt.MoveNext();

                extraBatch.AddNonInstanced((env, col, world, view, proj) =>
                {
                    Color colFront = col, colSide = col, colBack = col;
                    colSide.R = (byte)(colSide.R * 0.75f);
                    colSide.G = (byte)(colSide.G * 0.75f);
                    colSide.B = (byte)(colSide.B * 0.75f);
                    colBack.R = (byte)(colBack.R * 0.5f);
                    colBack.G = (byte)(colBack.G * 0.5f);
                    colBack.B = (byte)(colBack.B * 0.5f);

                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Right * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Right, colSide, Vector3.Up, additionalTransform: cursor.Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Left * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Left, colSide, Vector3.Up, additionalTransform: cursor.Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Up * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Up, colSide, Vector3.Forward, additionalTransform: cursor.Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Down * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Down, colSide, Vector3.Backward, additionalTransform: cursor.Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Forward, colFront, Vector3.Up, additionalTransform: cursor.Grip);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Backward * handSize / 2, Vector2.One * handSize, Game1.staminaRect.Bounds, Vector3.Backward, colBack, Vector3.Up, additionalTransform: cursor.Grip);

                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Up, upOverride: Vector3.Forward, additionalTransform: cursor.Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Down, upOverride: Vector3.Forward, additionalTransform: cursor.Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Left, upOverride: Vector3.Forward, additionalTransform: cursor.Pointer);
                    RenderHelper.DrawQuad(Game1.staminaRect, Vector3.Forward * 12.5f, new(0.01f, 25), new(0, 0, 1, 1), Vector3.Right, upOverride: Vector3.Forward, additionalTransform: cursor.Pointer);
                }, Matrix.Identity, colIt.Current);

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
                            WorldTransform = Matrix.Identity
                                * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f))
                                * Matrix.CreateScale(1f / 3)
                                //* Matrix.CreateTranslation(new Vector3(0.075f, 0.15f, -0.0f))
                                * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f))
                                * Matrix.CreateFromQuaternion // Was getting a gimbal lock otherwise
                                (
                                      Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(-45)))
                                    * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(-90)))
                                    * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(0)))
                                )
                                * Matrix.CreateTranslation(new Vector3(0f, -0.125f, -0.0f))
                                * cursor.Grip,
                            /*
                            WorldTransform = Matrix.CreateScale(1f / 3)
                                * Matrix.CreateTranslation(new Vector3(0.075f, 0.15f, -0.0f))
                                * Matrix.CreateFromQuaternion // Was getting a gimbal lock otherwise
                                (
                                      Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(0)))
                                    * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(MathHelper.ToRadians(-90)))
                                    * Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationZ(MathHelper.ToRadians(0)))
                                )
                                * Matrix.CreateTranslation(new Vector3(0.0f, -0.20f, -0.145f))
                                //* Matrix.CreateRotationX(MathHelper.ToRadians(90))
                                //* Matrix.CreateRotationY(MathHelper.ToRadians(-88))
                                //* Matrix.CreateRotationZ(MathHelper.ToRadians(0))
                                * cursorTransform
                                //* Matrix.CreateTranslation(cursorTransform.Translation)
                                * Matrix.CreateTranslation(new Vector3(0.0f,0.0f,0.0f)),
                            */

                            CanBillboard = false,

                            Reset = true,
                        });
                    }

                }
            }
            extraBatch.DrawBatched(WorldRenderer.CurrentEnvironment, Matrix.Identity, Camera.ViewMatrix, ProjectionMatrix);
#endif
        }

        return base.AfterRender(step, sb, time, targetScreen);
    }

    protected override void UpdateCameraPosition()
    {
        if (Context.IsWorldReady)
        {
            Camera.Position = Game1.player.StandingPixel3D;
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
