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
using static Stardew3D.IGameHandler;

namespace Stardew3D.FirstPerson;
public class FirstPersonGameHandler : ModGameHandler
{
    public override string Id => $"{Mod.Instance.ModManifest.UniqueID}/FirstPerson";
    public override string[] Tags => [Category3D, CategoryFirstPerson];

    public override Matrix ProjectionMatrix { get; protected set; }
    public override Camera Camera { get; } = new();

    public override void SwitchOn()
    {
        base.SwitchOn();
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Mod.Config.FieldOfViewDegrees), Game1.graphics.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 256);
        RenderHelper.GenericEffect.Projection = ProjectionMatrix;
    }

    public override void SwitchOff()
    {
        base.SwitchOff();
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
                    Camera.RotationForVertical = Util.Clamp(MathHelper.ToRadians(-90), Camera.RotationForVertical + diff.Y * -0.005f, MathHelper.ToRadians(90));
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

    protected override void DoCamera()
    {
        Camera.Position = Game1.player.GetPosition3D() + new Vector3( 0, 1.75f, 0 );
        RenderHelper.GenericEffect.View = Camera.ViewMatrix;
    }
}
