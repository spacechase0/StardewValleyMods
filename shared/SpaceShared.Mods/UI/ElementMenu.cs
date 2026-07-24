#if !DEPENDENCY_HAS_SPACESHARED
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
abstract class ElementMenu : IClickableMenu, IDisposable
{
    protected RootElement Ui { get; set; }

    public readonly int ScrollSpeed;

    public ElementMenu(int scrollSpeed)
    {
        ScrollSpeed = scrollSpeed;
    }

    public void Dispose()
    {
        cleanupBeforeExit();
    }

    protected void MakeUi()
    {
        Ui = new RootElement(() => currentlySnappedComponent, c =>
        {
            currentlySnappedComponent = c;
            snapCursorToCurrentSnappedComponent();
        });
        AddUiContents();
        populateClickableComponentList();
    }
    protected abstract void AddUiContents();

    public override void performHoverAction(int x, int y)
    {
        Ui.MouseHover(new Point(x, y));
        base.performHoverAction(x, y);
    }

    protected virtual void UnhandledScroll(int direction)
    {
        base.receiveScrollWheelAction(direction);
    }

    protected virtual void UnhandledLeftClick(int x, int y, bool pressed, bool playSound = true)
    {
        if (pressed)
            base.receiveLeftClick(x, y, playSound);
        else
            base.releaseLeftClick(x, y);
    }

    protected virtual void UnhandledRightClick(int x, int y, bool pressed, bool playSound = true)
    {
        if (pressed)
            base.receiveRightClick(x, y, playSound);
    }

    protected virtual void UnhandledKeyPress(int x, int y, bool pressed, bool playSound = true)
    {
        if (pressed)
            base.receiveRightClick(x, y, playSound);
    }

    protected virtual void UnhandledKeyPress(Keys key)
    {
        base.receiveKeyPress(key);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (!Ui.LeftClick(new Point(x, y), pressed: true))
            UnhandledLeftClick(x, y, true, playSound);
    }

    public override void releaseLeftClick(int x, int y)
    {
        if (!Ui.LeftClick(new Point(x, y), pressed: false))
            UnhandledLeftClick(x, y, false);
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        if (!Ui.RightClick(new Point(x, y), pressed: true))
            UnhandledRightClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key)
    {
        if (Game1.keyboardDispatcher.Subscriber != null)
            return;

        if (!Ui.KeyPress(key))
            UnhandledKeyPress(key);
    }

    public override void receiveScrollWheelAction(int direction)
    {
        if (!Ui.VerticalScroll(direction / -ScrollSpeed))
          UnhandledScroll(direction);
    }

    protected override void cleanupBeforeExit()
    {
        if (Game1.keyboardDispatcher.Subscriber is Element elem && Ui.RecursivelyContains(elem))
            Game1.keyboardDispatcher.Subscriber = null;
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
        else scrollCounter = 0;

        if (Ui.GamepadMovementRegionsDirty)
            populateClickableComponentList();
    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);
        Ui.Draw(b);
        upperRightCloseButton?.draw(b);

        drawMouse(b);
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        MakeUi();
    }

    public override void populateClickableComponentList()
    {
        base.populateClickableComponentList();

        foreach (var entry in Ui.GetGamepadMovementRegions().ToArray())
            allClickableComponents.Add(entry);
        Ui.GamepadMovementRegionsDirty = false;

        if (!allClickableComponents.Contains(currentlySnappedComponent))
            snapToDefaultClickableComponent();
        else
            snapCursorToCurrentSnappedComponent();
    }

    public override bool overrideSnappyMenuCursorMovementBan()
    {
        return (Ui.CurrentSnappedElement?.CurrentlyUsingGamepadMovement(out bool snappy) ?? false) ? !snappy : false;
    }
}
#endif
