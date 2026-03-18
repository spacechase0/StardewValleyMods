using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Stardew3D.GameModes;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Mods;
using xTile.Dimensions;

namespace Stardew3D;

internal class MyModHooks : DelegatingModHooks
{
    public MyModHooks(ModHooks theParent)
        : base(theParent)
    {
    }

    public override void OnGame1_PerformTenMinuteClockUpdate(Action action)
    {
        if (Mod.State.ActiveMode?.Tags.Contains(IGameMode.CategoryEditor) ?? false)
            return;

        action();
    }

    public override bool OnGameLocation_CheckAction(GameLocation location, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, Func<bool> action)
    {
        var handler = Mod.State.ActiveMode;
        if (handler == null)
        {
            return Parent.OnGameLocation_CheckAction(location, tileLocation, viewport, who, action);
        }

        return false;
    }

    public override void OnGame1_UpdateControlInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, Action action)
    {
        var handler = Mod.State.ActiveMode;
        if (handler == null)
        {
            Parent.OnGame1_UpdateControlInput(ref keyboardState, ref mouseState, ref gamePadState, action);
            return;
        }

        handler?.HandleGameplayInput(ref keyboardState, ref mouseState, ref gamePadState, (ref kb, ref m, ref gp) => Parent.OnGame1_UpdateControlInput(ref kb, ref m, ref gp, action));
    }

    public override bool OnRendering(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D target_screen)
    {
        var handler = Mod.State.ActiveMode;
        if (handler == null)
            return Parent.OnRendering(step, sb, time, target_screen);

        if (handler.HandleRender(step, sb, time, target_screen, Parent.OnRendering))
            return Parent.OnRendering(step, sb, time, target_screen);

        return false;
    }

    public override void OnRendered(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D target_screen)
    {
        var handler = Mod.State.ActiveMode;
        if (handler == null)
        {
            Parent.OnRendered(step, sb, time, target_screen);
            return;
        }

        if (handler.AfterRender(step, sb, time, target_screen))
            Parent.OnRendered(step, sb, time, target_screen);
    }
}
