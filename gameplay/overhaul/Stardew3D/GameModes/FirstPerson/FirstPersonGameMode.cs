using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using Stardew3D.GameModes;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using static Stardew3D.GameModes.IGameMode;

namespace Stardew3D.GameModes.FirstPerson;
public class FirstPersonGameMode : BaseGameMode, IFirstPersonGameMode
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";
    public override string[] Tags => [CategoryFlatscreen, CategoryFirstPerson, FeaturePointAndClick];

    public override Matrix ProjectionMatrix { get; protected set; }
    public override Camera Camera { get; } = new();

    public Vector3 MovementFacing => Camera.Forward;
    public Vector2 MovementAmount
    {
        get
        {
            Vector2 dir = Vector2.Zero;

            if (Game1.options.gamepadControls)
            {
                Vector2 dpadDir = Vector2.Zero;
                if (Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp)) dir.Y += 1;
                if (Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown)) dir.Y -= 1;
                if (Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadRight)) dir.X += 1;
                if (Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadLeft)) dir.X -= 1;

                Vector2 joyDir = Game1.input.GetGamePadState().ThumbSticks.Left;
                float joyLen = joyDir.Length();
                if (joyLen < 0.2)
                    joyDir = Vector2.Zero;
                else
                {
                    if (joyLen > 0.8f)
                        joyDir = joyDir.Normalized() * 0.8f;
                    joyDir -= joyDir * 0.2f;
                    joyDir *= 1f / 0.6f;
                }

                dir = dpadDir + joyDir;
            }
            else
            {
                if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveUpButton)) dir.Y += 1;
                if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveDownButton)) dir.Y -= 1;
                if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveRightButton)) dir.X += 1;
                if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveLeftButton)) dir.X -= 1;
            }

            if (dir.Length() > 1)
                dir.Normalize();

            return dir;
        }
    }
    public Vector2 MovementAmountForced => Vector2.Zero;

    private FirstPersonCursor[] cursors;
    public override IReadOnlyList<IGameCursor> Cursors => cursors;

    public FirstPersonGameMode()
    {
        cursors = [new FirstPersonCursor(this)];
    }

    public override void SwitchOn(IGameMode previousMode)
    {
        base.SwitchOn(previousMode);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Mod.Config.FieldOfViewDegrees), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 10000);
    }

    public override void SwitchOff(IGameMode nextMode)
    {
        base.SwitchOff(nextMode);
        Game1.game1.IsMouseVisible = Game1.options.hardwareCursor;
    }

    private bool hadMenuOpen = false;
    private bool wasActive = false;
    public override void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling)
    {
        Game1.game1.IsMouseVisible = Game1.activeClickableMenu != null && Game1.options.hardwareCursor;

        defaultInputHandling( ref keyboardState, ref mouseState, ref gamePadState);

        if (Game1.activeClickableMenu == null)
        {
            Point center = new(Game1.game1.localMultiplayerWindow.Width / 2, Game1.game1.localMultiplayerWindow.Height / 2);
            Point diff;
            if (Game1.options.gamepadControls)
            {
                diff = (gamePadState.ThumbSticks.Right * 20).ToPoint();
                diff.Y = -diff.Y;

                Game1.setMousePositionRaw(center.X, center.Y);
            }
            else
            {
                diff = mouseState.Position - center;

                if (GameRunner.instance.IsActive)
                    Mouse.SetPosition(center.X, center.Y);
            }

            if (!hadMenuOpen && wasActive)
            {
                // TODO: sensitivity settings and invert axis
                Camera.RotationForHorizontal = Util.Wrap(Camera.RotationForHorizontal + diff.X * -0.005f, 0, MathHelper.ToRadians(360));
                Camera.RotationForVertical = Util.Clamp(MathHelper.ToRadians(-89), Camera.RotationForVertical + diff.Y * -0.005f, MathHelper.ToRadians(89));
            }
        }
    }

    public override void AfterUpdate()
    {
        base.AfterUpdate();

        hadMenuOpen = Game1.activeClickableMenu != null;
        wasActive = GameRunner.instance.IsActive;
    }

    protected override void UpdateCamera()
    {
        Camera.Position = Game1.player.StandingPixel3D + new Vector3( 0, Game1.player.swimming.Value ? 0.25f : 1.75f, 0 );
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
