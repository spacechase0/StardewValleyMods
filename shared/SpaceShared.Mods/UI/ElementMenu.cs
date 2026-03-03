using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

#if IS_SPACECORE
namespace SpaceCore.UI;

public
#else
namespace SpaceShared.UI;

internal
#endif
abstract class ElementMenu : IClickableMenu
{
    protected RootElement Ui { get; set; }

    private readonly int ScrollSpeed;

    public ElementMenu(int scrollSpeed)
    {
        ScrollSpeed = scrollSpeed;

        MakeUi();
    }

    private void MakeUi()
    {
        Ui = new RootElement(() => currentlySnappedComponent, moveCursorInDirection);
        AddUiContents();
        populateClickableComponentList();
    }
    protected abstract void AddUiContents();

    public override void performHoverAction(int x, int y)
    {
        Ui.MouseHover(new Point(x, y));
        base.performHoverAction(x, y);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (!Ui.LeftClick(new Point(x, y), pressed: true))
            base.receiveLeftClick(x, y, playSound);
    }

    public override void releaseLeftClick(int x, int y)
    {
        if (!Ui.LeftClick(new Point(x, y), pressed: false))
            base.releaseLeftClick(x, y);
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        if (!Ui.RightClick(new Point(x, y), pressed: true))
            base.receiveRightClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key)
    {
        if (Game1.keyboardDispatcher! != null)
            return;

        if (!Ui.KeyPress(key, pressed: true))
            base.receiveKeyPress(key);
    }

    public override void receiveScrollWheelAction(int direction)
    {
        if (!Ui.VerticalScroll(direction / -ScrollSpeed))
          base.receiveScrollWheelAction(direction);
    }

    private int scrollCounter = 0;
    public override void update(GameTime time)
    {
        // Why does IClickableMenu not have a method for this AAAAAAAAAAA
        if (Game1.oldMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
            Game1.input.GetMouseState().RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
        {
            Ui.RightClick(Game1.getMousePosition(true), pressed: false);
        }

        Ui.Update();
        base.update(time);

        if (Game1.input.GetGamePadState().ThumbSticks.Right.Y != 0)
        {
            if (++scrollCounter == 5)
            {
                scrollCounter = 0;
                Ui.VerticalScroll(Math.Sign(Game1.input.GetGamePadState().ThumbSticks.Right.Y) * 120 / -ScrollSpeed);
            }
        }

        if (Ui.GamepadMovementRegionsDirty)
            populateClickableComponentList();
    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);
        Ui.Draw(b);
        upperRightCloseButton?.draw(b);

        Ui.HoveredElement?.DrawTooltip(b);
        drawMouse(b);
    }

    public override void populateClickableComponentList()
    {
        base.populateClickableComponentList();

        foreach (var entry in Ui.GetGamepadMovementRegions().ToArray())
            allClickableComponents.Add(entry);
        Ui.GamepadMovementRegionsDirty = false;

        if (allClickableComponents.Contains(currentlySnappedComponent))
            snapToDefaultClickableComponent();
    }

    public override void snapCursorToCurrentSnappedComponent()
    {
        return (Ui.CurrentSnappedElement?.CurrentlyUsingGamepadMovement(out bool snappy) ?? false) ? !snappy : false;
    }
}
