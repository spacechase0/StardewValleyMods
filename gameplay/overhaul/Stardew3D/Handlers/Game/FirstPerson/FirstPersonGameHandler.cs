using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using Stardew3D.Rendering;
using StardewValley;
using static Stardew3D.Handlers.Game.IGameHandler;

namespace Stardew3D.Handlers.Game.FirstPerson;
public class FirstPersonGameHandler : CommonGameHandler, IFirstPersonGameHandler
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";
    public override string[] Tags => [CategoryFlatscreen, CategoryFirstPerson];

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
                if (Game1.input.GetGamePadState().IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.DPadUp)) dir.Y += 1;
                if (Game1.input.GetGamePadState().IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.DPadDown)) dir.Y -= 1;
                if (Game1.input.GetGamePadState().IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.DPadRight)) dir.X += 1;
                if (Game1.input.GetGamePadState().IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.DPadLeft)) dir.X -= 1;

                Vector2 joyDir = Game1.input.GetGamePadState().ThumbSticks.Left;
                joyDir.Y = -joyDir.Y;
                float joyLen = joyDir.Length();
                if (joyLen < 0.2)
                    joyDir = Vector2.Zero;
                else
                {
                    joyDir.X -= joyDir.X * 0.2f;
                    joyDir.Y -= joyDir.Y * 0.2f;
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
    public override IReadOnlyList<IGameCursor> Cursors => [new FirstPersonCursor(this)];

    public override void SwitchOn(IGameHandler previousHandler)
    {
        base.SwitchOn(previousHandler);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Mod.Config.FieldOfViewDegrees), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 10000);
    }

    public override void SwitchOff(IGameHandler nextHandler)
    {
        base.SwitchOff(nextHandler);
        Game1.game1.IsMouseVisible = Game1.options.hardwareCursor;
    }

    private bool hadMenuOpen = false;
    private bool wasActive = false;
    public override void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling)
    {
        Game1.game1.IsMouseVisible = Game1.activeClickableMenu != null && Game1.options.hardwareCursor;
        if (Game1.activeClickableMenu == null)
        {
            // TODO: gamepad support
            Point center = new(Game1.game1.Window.ClientBounds.Width / 2, Game1.game1.Window.ClientBounds.Height / 2);
            Point diff = Mouse.GetState().Position - center;
            if (GameRunner.instance.IsActive)
            {
                Mouse.SetPosition(center.X, center.Y);

                if (!hadMenuOpen && wasActive)
                {
                    // TODO: sensitivity settings and invert axis
                    Camera.RotationForHorizontal = Util.Wrap(Camera.RotationForHorizontal + diff.X * -0.005f, 0, MathHelper.ToRadians(360));
                    Camera.RotationForVertical = Util.Clamp(MathHelper.ToRadians(-89), Camera.RotationForVertical + diff.Y * -0.005f, MathHelper.ToRadians(89));
                }
            }
        }

        // ...

        defaultInputHandling( ref keyboardState, ref mouseState, ref gamePadState);
    }

    public override void AfterUpdate()
    {
        base.AfterUpdate();

        hadMenuOpen = Game1.activeClickableMenu != null;
        wasActive = GameRunner.instance.IsActive;
    }

    protected override void UpdateCamera()
    {
        Camera.Position = Game1.player.StandingPixel3D + new Vector3( 0, 1.75f, 0 );
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
