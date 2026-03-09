using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GenericModConfigMenu.Api.Elements;
using GenericModConfigMenu.Api.Pages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

#nullable enable
namespace GenericModConfigMenu.Api;

/// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu.</summary>
public interface IGenericModConfigMenuApi
#if GMCM_LEGACY_API
    : ILegacyGenericModConfigMenuApi_DO_NOT_USE
#endif
{
    public IModConfig RegisterConfig(IManifest manifest, Action resetEverythingToDefault, Action saveConfig);

    public bool TryGetLocalizedName(string modId, [NotNullWhen(true)] out string? name, [NotNullWhen(true)] out string? description);
    public bool TryGetCurrentMenu([NotNullWhen(true)] out IModConfig? config, [NotNullWhen(true)] out IPage? page);

    public IElementContainer CreateSection();
    public IElementGrid CreateGrid(int columns);

    public IDialogueBox CreateReusableDialogueBox(Func<IDialogueBox, string> displayName);
    public IEditingDialogueBox<T> CreateReusableEditingDialogueBox<T>(Func<IDialogueBox, string> displayName);

    public IStaticElement CreateHeader(float sizeMult);
    public IStaticElement CreateParagraph(Func<IElement, string> text);
    public IStaticElement CreateFormattedParagraph(Func<IElement, string> text); // Should probably be markdown, right?
    public IStaticElement CreateImage(Func<IElement, Texture2D> texture, Func<IElement, Rectangle?>? textureRegion = null, Func<IElement, int>? scale = null);

    public IConfigOption<bool> CreateCheckbox(Func<IConfigOption<bool>, bool> getValue, Action<IConfigOption<bool>, bool> setValue, Action<IConfigOption<bool>, bool> defaultValue);
    public IFormattedConfigOption<int> CreateSlider(Func<IConfigOption<int>, int> minimum, Func<IConfigOption<int>, int> maximum, Func<IConfigOption<int>, int> getValue, Action<IConfigOption<bool>, int> setValue, Action<IConfigOption<bool>, int> defaultValue);
    public IFormattedConfigOption<float> CreateSlider(Func<IConfigOption<float>, float> minimum, Func<IConfigOption<float>, float> maximum, Func<IConfigOption<float>, float> getValue, Action<IConfigOption<float>, float> setValue, Action<IConfigOption<float>, float> defaultValue);
    public IFormattedConfigOption<int> CreateDropdown(Func<IConfigOption<int>, int[]> options, Func<IConfigOption<int>, int> getValue, Action<IConfigOption<int>, int> setValue, Action<IConfigOption<int>, int> defaultValue);
    public IFormattedConfigOption<float> CreateDropdown(Func<IConfigOption<float>, float[]> options, Func<IConfigOption<float>, float> getValue, Action<IConfigOption<float>, float> setValue, Action<IConfigOption<float>, float> defaultValue);
    public IFormattedConfigOption<string> CreateDropdown(Func<IConfigOption<string>, string[]> options, Func<IConfigOption<string>, string> getValue, Action<IConfigOption<string>, string> setValue, Action<IConfigOption<string>, string> defaultValue);
    /// <summary>
    ///     This will create a series of checkboxes for enums marked with [Flags], and a dropdown for all other enums.
    ///     
    ///     For enums marked with [Flags]:
    ///     <list type="bullet">
    ///         <item>It will only create options for values with only a single bit set (which means "combo values" like `Default = A | B | C` won't appear).</item>
    ///         <item>It will skip values that have already appeared previously (meaning if you have `Default = A`, only the one that appeared first in the declaration will be used).</item>
    ///     </list>
    /// </summary>
    public IFormattedConfigOption<TEnum> CreateEnum<TEnum>(Func<IConfigOption<TEnum>, TEnum> getValue, Action<IConfigOption<TEnum>, TEnum> setValue, Action<IConfigOption<TEnum>, TEnum> defaultValue, Func<TEnum[]>? excludeValues = null)
        where TEnum : System.Enum;
    public IConfigOption<int> CreateTextField(Func<IConfigOption<int>, int> getValue, Action<IConfigOption<int>, int> setValue, Action<IConfigOption<int>, int> defaultValue);
    public IConfigOption<float> CreateTextField(Func<IConfigOption<float>, float> getValue, Action<IConfigOption<float>, float> setValue, Action<IConfigOption<float>, float> defaultValue);
    public IConfigOption<string> CreateTextField(Func<IConfigOption<string>, string> getValue, Action<IConfigOption<string>, string> setValue, Action<IConfigOption<string>, string> defaultValue);

    public IConfigOption<SButton> CreateKeybinding(Func<IConfigOption<SButton>, SButton> getValue, Action<IConfigOption<SButton>, SButton> setValue, Action<IConfigOption<SButton>, SButton> defaultValue);
    public IConfigOption<KeybindList> CreateKeybinding(Func<IConfigOption<KeybindList>, KeybindList> getValue, Action<IConfigOption<KeybindList>, KeybindList> setValue, Action<IConfigOption<KeybindList>, KeybindList> defaultValue);

    public IConfigOption<T> CreateChooser<T>(Func<IConfigOption<List<T>>, T, IElement> createDisplay,
                                                Func<IConfigOption<List<T>>, T, IEditingDialogueBox<T>> chooseElement,
                                                Func<IConfigOption<T>, T> getValue,
                                                Action<IConfigOption<T>, T> setValue,
                                                Action<IConfigOption<T>, T> defaultValue);
    public IConfigOption<List<T>> CreateUnorderedListOption<T>(Func<IConfigOption<List<T>>, T, IElement> createDisplay,
                                                                  Func<IConfigOption<List<T>>, T> createNewElement,
                                                                  Func<IConfigOption<List<T>>, T, IEditingDialogueBox<T>> editElement,
                                                                  Func<IConfigOption<List<T>>, List<T>> getValue,
                                                                  Action<IConfigOption<List<T>>, List<T>> setValue,
                                                                  Action<IConfigOption<List<T>>, List<T>> defaultValue);
    public IConfigOption<List<T>> CreateOrderedListOption<T>(Func<IConfigOption<List<T>>, T, IElement> createDisplay,
                                                                Func<IConfigOption<List<T>>, T> createNewElement,
                                                                Func<IConfigOption<List<T>>, T, IEditingDialogueBox<T>> editElement,
                                                                Func<IConfigOption<List<T>>, List<T>> getValue,
                                                                Action<IConfigOption<List<T>>, List<T>> setValue,
                                                                Action<IConfigOption<List<T>>, List<T>> defaultValue);

    public IElement CreateItemDisplay(Item item, Side? nameSide);
    public IElement CreateNpcSpriteDisplay(string npc, Side? nameSide, bool useCurrent = true);
    public IElement CreateNpcPortraitDisplay(string npc, Side? nameSide, bool useCurrent = true);
    public IEditingDialogueBox<TItem> CreateReusableItemChoosingDialogueBox<TItem>(Func<IDialogueBox, string> displayName, Func<IEditingDialogueBox<TItem>, Item[]> choices)
        where TItem : Item;

    public IElement CreateCustomElement(ICustomConfigOption customImpl);
}
