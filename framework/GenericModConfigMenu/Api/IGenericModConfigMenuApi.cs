using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

#nullable enable

namespace GenericModConfigMenu.Api;

public enum Side
{
    Left,
    Top,
    Right,
    Bottom,
}

public enum CursorClickType
{
    LeftClick,
    RightClick,
}

public interface IModConfigElementCustomImplementation
{
    public IModConfigElement OwningElement { set; }
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

public enum ModConfigVisibilityContext
{
    TitleMenu = 1 << 0,
    InGame = 1 << 1,

    Everywhere = TitleMenu | InGame,
}

public enum ModConfigPositionElementAnchor
{
    After,
    Before,
}

public interface IModConfigElement
{
    public IModConfig RootConfig { get; }
    public IModConfigPageContents Owner { get; }
    public IModConfigElementContainer? Parent { get; }

    public string Label { get; }
    public bool IsVisible { get; }

    public IModConfigElement SetLabel(Func<IModConfigElement, string> label);
    public IModConfigElement SetTooltip(Func<IModConfigElement, string> tooltip);

    public IModConfigElement SetContextVisibility(ModConfigVisibilityContext context, bool visible);
    public IModConfigElement SetDefaultVisibility(Func<IModConfigElement, bool> visible);
    public IModConfigElement SetCurrentVisibility(bool visible);

    public IModConfigElement SetMinimumSize(int? width, int? height);
    public IModConfigElement SetHorizontalPositionAnchor(ModConfigPositionElementAnchor anchor = ModConfigPositionElementAnchor.After, IModConfigElement? relativeTo = null);
    public IModConfigElement SetVerticalPositionAnchor(ModConfigPositionElementAnchor anchor = ModConfigPositionElementAnchor.After, IModConfigElement? relativeTo = null);
    public IModConfigElement SetOuterSpacing(int left = 0, int top = 0, int right = 0, int bottom = 0);
}

public interface IModConfigElementContainer : IModConfigElement
{
    public IModConfigElementContainer AddChildren(params IModConfigElement[] children);

    public IModConfigElementContainer SetInnerSpacing(Vector2 spacing);
}

public interface IModConfigElementGrid : IModConfigElementContainer
{
    public int Columns { get; }


    public IModConfigElementContainer SetCellMinimumSize(Vector2 size);
    public IModConfigElementContainer SetChild(int column, int row, IModConfigElement child);
}

public interface IModConfigStaticElement : IModConfigElement
{
    public delegate void ClickHandler(IModConfigStaticElement option, Vector2 pixelOnElement);
    public IModConfigStaticElement AddClickHandler(ClickHandler handler);
}

public interface IModConfigOption<T> : IModConfigElement
{
    public T PendingValue { get; set; }

    public delegate bool Validator(IModConfigOption<T> option, T value, out string error);
    public IModConfigOption<T> SetSaveValidator(Validator validator);
}

public interface IModConfigOptionValueFormatted<T> : IModConfigOption<T>
{
    public IModConfigOptionValueFormatted<T> SetDisplayFormatter(Func<IModConfigOptionValueFormatted<T>, T, string> formatter);
}

public interface IModConfigPageContents
{
    public IModConfig RootConfig { get; }
    public IModConfigPage Parent { get; }

    public IModConfigPage AddElements(params IModConfigElement[] elements);

    public bool TryOpenDialogue(IModConfigDialogueBox dialogue);
}

public interface IModConfigPage : IModConfigPageContents
{
    public IModConfigPage CreateSubPage(Func<string> pageDisplayName, out IModConfigPage newPage);

    public bool TryOpen();
    public void ForceOpen();
}

public interface IModConfigDialogueBox : IModConfigPageContents
{
}

public interface IModConfigEditingDialogueBox<T> : IModConfigDialogueBox
{
    public IModConfigOption<T> Invoker { get; }
}

public interface IModConfig
{
    public IManifest Owner { get; }
    public IModConfigPage RootPage { get; }

    public IModConfig SetCanOpen(ModConfigVisibilityContext context, bool canOpen);
}

/// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu.</summary>
public interface IGenericModConfigMenuApi
#if GMCM_LEGACY_API
    : ILegacyGenericModConfigMenuApi_DO_NOT_USE
#endif
{
    public IModConfig RegisterConfig(IManifest manifest, Action resetEverythingToDefault, Action saveConfig);

    public bool TryGetLocalizedName(string modId, [NotNullWhen(true)] out string? name, [NotNullWhen(true)] out string? description);
    public bool TryGetCurrentMenu([NotNullWhen(true)] out IModConfig? config, [NotNullWhen(true)] out IModConfigPage? page);

    public IModConfigElementContainer CreateSection();
    public IModConfigElementGrid CreateGrid(int columns);

    public IModConfigDialogueBox CreateReusableDialogueBox(Func<IModConfigDialogueBox, string> displayName);
    public IModConfigEditingDialogueBox<T> CreateReusableEditingDialogueBox<T>(Func<IModConfigDialogueBox, string> displayName);

    public IModConfigStaticElement CreateHeader(float sizeMult);
    public IModConfigStaticElement CreateParagraph(Func<IModConfigElement, string> text);
    public IModConfigStaticElement CreateFormattedParagraph(Func<IModConfigElement, string> text); // Should probably be markdown, right?
    public IModConfigStaticElement CreateImage(Func<IModConfigElement, Texture2D> texture, Func<IModConfigElement, Rectangle?>? textureRegion = null, Func<IModConfigElement, int>? scale = null);

    public IModConfigOption<bool> CreateCheckbox(Func<IModConfigOption<bool>, bool> getValue, Action<IModConfigOption<bool>, bool> setValue, Action<IModConfigOption<bool>, bool> defaultValue);
    public IModConfigOptionValueFormatted<int> CreateSlider(Func<IModConfigOption<int>, int> minimum, Func<IModConfigOption<int>, int> maximum, Func<IModConfigOption<int>, int> getValue, Action<IModConfigOption<bool>, int> setValue, Action<IModConfigOption<bool>, int> defaultValue);
    public IModConfigOptionValueFormatted<float> CreateSlider(Func<IModConfigOption<float>, float> minimum, Func<IModConfigOption<float>, float> maximum, Func<IModConfigOption<float>, float> getValue, Action<IModConfigOption<float>, float> setValue, Action<IModConfigOption<float>, float> defaultValue);
    public IModConfigOptionValueFormatted<int> CreateDropdown(Func<IModConfigOption<int>, int[]> options, Func<IModConfigOption<int>, int> getValue, Action<IModConfigOption<int>, int> setValue, Action<IModConfigOption<int>, int> defaultValue);
    public IModConfigOptionValueFormatted<float> CreateDropdown(Func<IModConfigOption<float>, float[]> options, Func<IModConfigOption<float>, float> getValue, Action<IModConfigOption<float>, float> setValue, Action<IModConfigOption<float>, float> defaultValue);
    public IModConfigOptionValueFormatted<string> CreateDropdown(Func<IModConfigOption<string>, string[]> options, Func<IModConfigOption<string>, string> getValue, Action<IModConfigOption<string>, string> setValue, Action<IModConfigOption<string>, string> defaultValue);
    /// <summary>
    ///     This will create a series of checkboxes for enums marked with [Flags], and a dropdown for all other enums.
    ///     
    ///     For enums marked with [Flags]:
    ///     <list type="bullet">
    ///         <item>It will only create options for values with only a single bit set (which means "combo values" like `Default = A | B | C` won't appear).</item>
    ///         <item>It will skip values that have already appeared previously (meaning if you have `Default = A`, only the one that appeared first in the declaration will be used).</item>
    ///     </list>
    /// </summary>
    public IModConfigOptionValueFormatted<TEnum> CreateEnum<TEnum>(Func<IModConfigOption<TEnum>, TEnum> getValue, Action<IModConfigOption<TEnum>, TEnum> setValue, Action<IModConfigOption<TEnum>, TEnum> defaultValue, Func<TEnum[]>? excludeValues = null)
        where TEnum : System.Enum;
    public IModConfigOption<int> CreateTextField(Func<IModConfigOption<int>, int> getValue, Action<IModConfigOption<int>, int> setValue, Action<IModConfigOption<int>, int> defaultValue);
    public IModConfigOption<float> CreateTextField(Func<IModConfigOption<float>, float> getValue, Action<IModConfigOption<float>, float> setValue, Action<IModConfigOption<float>, float> defaultValue);
    public IModConfigOption<string> CreateTextField(Func<IModConfigOption<string>, string> getValue, Action<IModConfigOption<string>, string> setValue, Action<IModConfigOption<string>, string> defaultValue);

    public IModConfigOption<SButton> CreateKeybinding(Func<IModConfigOption<SButton>, SButton> getValue, Action<IModConfigOption<SButton>, SButton> setValue, Action<IModConfigOption<SButton>, SButton> defaultValue);
    public IModConfigOption<KeybindList> CreateKeybinding(Func<IModConfigOption<KeybindList>, KeybindList> getValue, Action<IModConfigOption<KeybindList>, KeybindList> setValue, Action<IModConfigOption<KeybindList>, KeybindList> defaultValue);

    public IModConfigOption<T> CreateChooser<T>(Func<IModConfigOption<List<T>>, T, IModConfigElement> createDisplay,
                                                Func<IModConfigOption<List<T>>, T, IModConfigEditingDialogueBox<T>> chooseElement,
                                                Func<IModConfigOption<T>, T> getValue,
                                                Action<IModConfigOption<T>, T> setValue,
                                                Action<IModConfigOption<T>, T> defaultValue);
    public IModConfigOption<List<T>> CreateUnorderedListOption<T>(Func<IModConfigOption<List<T>>, T, IModConfigElement> createDisplay,
                                                                  Func<IModConfigOption<List<T>>, T> createNewElement,
                                                                  Func<IModConfigOption<List<T>>, T, IModConfigEditingDialogueBox<T>> editElement,
                                                                  Func<IModConfigOption<List<T>>, List<T>> getValue,
                                                                  Action<IModConfigOption<List<T>>, List<T>> setValue,
                                                                  Action<IModConfigOption<List<T>>, List<T>> defaultValue);
    public IModConfigOption<List<T>> CreateOrderedListOption<T>(Func<IModConfigOption<List<T>>, T, IModConfigElement> createDisplay,
                                                                Func<IModConfigOption<List<T>>, T> createNewElement,
                                                                Func<IModConfigOption<List<T>>, T, IModConfigEditingDialogueBox<T>> editElement,
                                                                Func<IModConfigOption<List<T>>, List<T>> getValue,
                                                                Action<IModConfigOption<List<T>>, List<T>> setValue,
                                                                Action<IModConfigOption<List<T>>, List<T>> defaultValue);

    public IModConfigElement CreateItemDisplay(Item item, Side? nameSide);
    public IModConfigElement CreateNpcSpriteDisplay(string npc, Side? nameSide, bool useCurrent = true);
    public IModConfigElement CreateNpcPortraitDisplay(string npc, Side? nameSide, bool useCurrent = true);
    public IModConfigEditingDialogueBox<TItem> CreateReusableItemChoosingDialogueBox<TItem>(Func<IModConfigDialogueBox, string> displayName, Func<IModConfigEditingDialogueBox<TItem>, Item[]> choices)
        where TItem : Item;

    public IModConfigElement CreateCustomElement(IModConfigElementCustomImplementation customImpl);
}
