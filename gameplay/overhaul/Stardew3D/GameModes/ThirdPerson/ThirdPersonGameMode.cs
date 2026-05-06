using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewModdingAPI;
using StardewValley;
using static Stardew3D.GameModes.IGameMode;

namespace Stardew3D.GameModes.ThirdPerson;
public class ThirdPersonGameMode : BaseGameMode, IGameplayGameMode
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/ThirdPerson";
    public override string[] Tags => [CategoryFlatscreen, CategoryThirdPerson, FeaturePointAndClick];

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

    public override IReadOnlyList<IGameCursor> Cursors => []; // TODO

    public override void SwitchOn(IGameMode previousMode)
    {
        base.SwitchOn(previousMode);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Mod.Config.FieldOfViewDegrees), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 10000);
    }

    public override void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling)
    {
        if (Mod.Instance.Helper.Input.IsDown(SButton.Up))
            Camera.RotationX += MathHelper.ToRadians(2);
        if (Mod.Instance.Helper.Input.IsDown(SButton.Down))
            Camera.RotationX -= MathHelper.ToRadians(2);
        if (Mod.Instance.Helper.Input.IsDown(SButton.Left))
            Camera.RotationY -= MathHelper.ToRadians(2);
        if (Mod.Instance.Helper.Input.IsDown(SButton.Right))
            Camera.RotationY += MathHelper.ToRadians(2);

        var minVerticalRot = MathHelper.ToRadians(15);
        var maxVerticalRot = MathHelper.ToRadians(75);
        Camera.RotationX = Util.Clamp(-maxVerticalRot, Camera.RotationX, -minVerticalRot);
        Camera.RotationY = Util.Wrap(Camera.RotationY, 0, MathHelper.ToRadians(360));

        defaultInputHandling(ref keyboardState, ref mouseState, ref gamePadState);
    }

    protected override void UpdateCamera()
    {
        Camera.Target = Game1.player.StandingPixel3D + new Vector3(0, Game1.player.swimming.Value ? 0.25f : 1.75f, 0);
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
