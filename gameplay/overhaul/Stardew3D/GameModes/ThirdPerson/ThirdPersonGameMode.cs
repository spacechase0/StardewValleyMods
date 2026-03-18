using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewModdingAPI;
using StardewValley;
using static Stardew3D.GameModes.IGameMode;

namespace Stardew3D.GameModes.ThirdPerson;
public class ThirdPersonGameMode : BaseGameMode
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/ThirdPerson";
    public override string[] Tags => [CategoryFlatscreen, CategoryThirdPerson, FeaturePointAndClick];

    public override Matrix ProjectionMatrix { get; protected set; }
    public override Camera Camera { get; } = new();

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
        Camera.Target = Game1.player.StandingPixel3D;
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
