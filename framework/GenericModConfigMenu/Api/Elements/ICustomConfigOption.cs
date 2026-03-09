using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

#nullable enable
namespace GenericModConfigMenu.Api.Elements;

public interface ICustomConfigOption
{
    public IElement OwningElement { set; }
    public Rectangle ElementBounds { set; }
    public Rectangle ScreenBounds { set; }

    public Point Size { get; }
    public bool HasUnsavedChanges { get; }

    public IEnumerable<ClickableComponent> GamepadSnapRegions { get; }
    public bool UsingGamepadMovement { get; }

    /// <summary>
    /// Called upon cursor release, if the press also happened on this element.
    /// 
    /// Prefer using this over <see cref="HandleCursorPress" /> or <see cref="HandleCursorRelease" /> where possible.
    /// </summary>
    /// <param name="cursorPos">The cursor position, relative to this element's bounding box's top-left corner.</param>
    /// <param name="type">The click type (left click or right click).</param>
    /// <returns></returns>
    public bool HandleCursorClick(Point cursorPos, CursorClickType type);
    public bool HandleCursorPress(Point cursorPos, CursorClickType type);
    public bool HandleCursorRelease(Point cursorPos, CursorClickType type);

    /// <returns><c>true</c> if <see cref="GamepadSnapRegions"/> or <see cref="UsingGamepadMovement"/> has changed, <c>false</c> otherwise.</returns>
    public bool Update(GameTime gameTime);
    public void Draw(SpriteBatch sb, GameTime gameTime);

    public void BeforeMenuOpened();
    public void BeforeMenuClosed();

    public void BeforeSave();
    public void AfterSave();

    /// <summary>
    /// This is used for when an individual page is reset to default values, rather than the whole mod. This should not actually save the value.
    /// (When an individual page is reset to default values, it does not save these values immediately, but instead has the default values as pending changes that need saving.)
    /// </summary>
    public void BeforeReset();
    /// <summary>
    /// This is used for when an individual page is reset to default values, rather than the whole mod. This should not actually save the value.
    /// (When an individual page is reset to default values, it does not save these values immediately, but instead has the default values as pending changes that need saving.)
    /// </summary>
    public void AfterReset();
}
