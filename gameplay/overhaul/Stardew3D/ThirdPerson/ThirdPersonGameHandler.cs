using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewValley;
using static Stardew3D.IGameHandler;
using static StardewValley.Minigames.MineCart.MapJunimo;

namespace Stardew3D.ThirdPerson;
public class ThirdPersonGameHandler : ModGameHandler
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/ThirdPerson";
    public override string[] Tags => [Category3D, CategoryThirdPerson];

    public override Matrix ProjectionMatrix { get; protected set; }
    public override Camera Camera { get; } = new();

    public override void SwitchOn()
    {
        base.SwitchOn();
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Mod.Config.FieldOfViewDegrees), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 256);
        RenderHelper.GenericEffect.Projection = ProjectionMatrix;
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

    float f = 0;
    protected override void DoCamera()
    {
        Camera.Target = Game1.player.GetPosition3D();
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
