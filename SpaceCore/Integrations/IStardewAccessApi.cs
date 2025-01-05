using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.Integrations;

// TODO: Remove unwanted
public interface IStardewAccessApi
{
    #region Screen reader related

    /// <summary>
    /// The query text for the previously spoken (or currently being spoken) by the screen reader in current menu.
    /// <br />This is used to prevent speaking same texts repeatedly.
    /// <br />Resets when the menu is closed or changed.
    /// </summary>
    public string PrevMenuQueryText { get; set; }

    /// <summary>
    /// The prefix which will get prepended to the next text the screen reader will speak in current menu.
    /// <br />Included in query check, alongside PrevMenuQueryText.
    /// <br />Resets when the menu is closed or changed.
    /// </summary>
    public string MenuPrefixText { get; set; }

    /// <summary>
    /// The suffix which will get appended to the next text the screen reader will speak in current menu.
    /// <br />Included in query check, alongside PrevMenuQueryText.
    /// <br />Resets when the menu is closed or changed.
    /// </summary>
    public string MenuSuffixText { get; set; }

    /// <summary>
    /// The prefix which will get prepended to the next text the screen reader will speak in current menu.
    /// <br />Won't be included in query check.
    /// <br />Resets everytime after use.
    /// </summary>
    public string MenuPrefixNoQueryText { get; set; }

    /// <summary>
    /// The suffix which will get appended to the next text the screen reader will speak in current menu.
    /// <br />Won't be included in query check.
    /// <br />Resets everytime after use.
    /// </summary>
    public string MenuSuffixNoQueryText { get; set; }

    /// <summary>
    /// (Default to `C`) Mainly used to speak some visual information in a menu like some label(s) which isn't
    /// reachable by gamepad naviagtion.
    /// </summary>
    public KeybindList PrimaryInfoKey { get; }

    /// <summary>Speaks the text via the loaded screen reader (if any).</summary>
    /// <param name="text">The text to be narrated.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <returns>true if the text was spoken otherwise false.</returns>
    public bool Say(string text, bool interrupt);

    public bool TranslateAndSay(string translationKey, bool interrupt, object? translationTokens = null, string translationCategory = "default", bool disableTranslationWarnings = false);

    /// <summary>Speaks the text via the loaded screen reader (if any).
    /// <br/>Skips the text narration if the previously narrated text was the same as the one provided.</summary>
    /// <param name="text">The text to be narrated.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <param name="customQuery">If set, uses this instead of <paramref name="text"/> as query to check whether to speak the text or not.</param>
    /// <returns>true if the text was spoken otherwise false.</returns>
    public bool SayWithChecker(string text, bool interrupt, string? customQuery = null);

    public bool TranslateAndSayWithChecker(string translationKey, bool interrupt, object? translationTokens = null, string translationCategory = "default", string? customQuery = null, bool disableTranslationWarnings = false);

    /// <summary>Speaks the text via the loaded screen reader (if any).
    /// <br/>Skips the text narration if the previously narrated text was the same as the one provided.
    /// <br/><br/>Use this when narrating hovered component in menus to avoid interference.</summary>
    /// <param name="text">The text to be narrated.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <param name="customQuery">If set, uses this instead of <paramref name="text"/> as query to check whether to speak the text or not.</param>
    /// <returns>true if the text was spoken otherwise false.</returns>
    public bool SayWithMenuChecker(string text, bool interrupt, string? customQuery = null);

    public bool TranslateAndSayWithMenuChecker(string translationKey, bool interrupt, object? translationTokens = null, string translationCategory = "menu", string? customQuery = null, bool disableTranslationWarnings = false);

    /// <summary>Speaks the text via the loaded screen reader (if any).
    /// <br/>Skips the text narration if the previously narrated text was the same as the one provided.
    /// <br/><br/>Use this when narrating chat messages to avoid interference.</summary>
    /// <param name="text">The text to be narrated.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <returns>true if the text was spoken otherwise false.</returns>
    public bool SayWithChatChecker(string text, bool interrupt);

    /// <summary>Speaks the text via the loaded screen reader (if any).
    /// <br/>Skips the text narration if the previously narrated text was the same as the one provided.
    /// <br/><br/>Use this when narrating texts based on tile position to avoid interference.</summary>
    /// <param name="text">The text to be narrated.</param>
    /// <param name="x">The X location of tile.</param>
    /// <param name="y">The Y location of tile.</param>
    /// <param name="interrupt">Whether to skip the currently speaking text or not.</param>
    /// <returns>true if the text was spoken otherwise false.</returns>
    public bool SayWithTileQuery(string text, int x, int y, bool interrupt);

    public string Translate(string translationKey, object? tokens = null, string translationCategory = "Default", bool disableWarning = false);

    #endregion

    #region Tiles related

    /// <summary>
    /// Search the area using Breadth First Search algorithm(BFS).
    /// </summary>
    /// <param name="center">The starting point.</param>
    /// <param name="limit">The limiting factor or simply radius of the search area.</param>
    /// <returns>A dictionary with all the detected tiles along with the name of the object on it and it's category.</returns>
    public Dictionary<Vector2, (string name, string category)> SearchNearbyTiles(Vector2 center, int limit);

    /// <summary>
    /// Search the entire location using Breadth First Search algorithm(BFS).
    /// </summary>
    /// <returns>A dictionary with all the detected tiles along with the name of the object on it and it's category.</returns>
    public Dictionary<Vector2, (string name, string category)> SearchLocation();

    /// <summary>
    /// Check the tile for any object
    /// </summary>
    /// <param name="tile">The tile where we want to check the name and category of object if any</param>
    /// <returns>Name of the object as the first item (name) and category as the second item (category). Returns null if no object found.</returns>
    public (string? name, string? category) GetNameWithCategoryNameAtTile(Vector2 tile);

    /// <summary>
    /// Check the tile for any object
    /// </summary>
    /// <param name="tile">The tile where we want to check the name and category of object if any</param>
    /// <returns>Name of the object. Returns null if no object found.</returns>
    public string? GetNameAtTile(Vector2 tile);

    #endregion

    #region Inventory and Item related

    /// <summary>
    /// (Legacy! Should not be used from v1.6)
    /// Speaks the hovered inventory slot from the provided <see cref="InventoryMenu"/>.
    /// In case there is nothing in a slot, then it will speak "Empty Slot".
    /// Also plays a sound if the slot is grayed out, like tools in <see cref="GeodeMenu">geode menu</see>.
    /// </summary>
    /// <param name="inventoryMenu">The object of <see cref="InventoryMenu"/> whose inventory is to be spoken.</param>
    /// <param name="giveExtraDetails">(Optional) Whether to speak extra details about the item in slot or not. Default to null in which case it uses <see cref="ModConfig.DisableInventoryVerbosity"/> to get whether to speak extra details or not.</param>
    /// <param name="hoverPrice">(Optional) The price of the hovered item, generally used in <see cref="ShopMenu"/>.</param>
    /// <param name="extraItemToShowIndex">(Optional) The index (probably parentSheetIndex) of the extra item which is generally a requirement for the hovered item in certain menus.</param>
    /// <param name="extraItemToShowAmount">(Optional) The amount or quantity of the extra item which is generally a requirement for the hovered item in certain menus.</param>
    /// <param name="highlightedItemPrefix">(Optional) The prefix to add to the spoken hovered item's details if it is highlighted i.e., not grayed out.</param>
    /// <param name="highlightedItemSuffix">(Optional) The suffix to add to the spoken hovered item's details if it is highlighted i.e., not grayed out.</param>
    /// <param name="hoverX">(Optional) The X position on screen to check. Default to null, in which case it uses the mouse's X position.</param>
    /// <param name="hoverY">(Optional) The Y position on screen to check. Default to null, in which case it uses the mouse's Y position.</param>
    /// <returns>true if any inventory slot was hovered or found at the <paramref name="hoverX"/> and <paramref name="hoverY"/>.</returns>
    public bool SpeakHoveredInventorySlot(InventoryMenu? inventoryMenu,
        bool? giveExtraDetails = null,
        int hoverPrice = -1,
        int extraItemToShowIndex = -1,
        int extraItemToShowAmount = -1,
        string highlightedItemPrefix = "",
        string highlightedItemSuffix = "",
        int? hoverX = null,
        int? hoverY = null);

    /// <summary>
    /// Speaks the hovered inventory slot from the provided <see cref="InventoryMenu"/>.
    /// In case there is nothing in a slot, then it will speak "Empty Slot".
    /// Also plays a sound if the slot is grayed out, like tools in <see cref="GeodeMenu">geode menu</see>.
    /// </summary>
    /// <param name="inventoryMenu">The object of <see cref="InventoryMenu"/> whose inventory is to be spoken.</param>
    /// <param name="giveExtraDetails">(Optional) Whether to speak extra details about the item in slot or not. Default to null in which case it uses <see cref="ModConfig.DisableInventoryVerbosity"/> to get whether to speak extra details or not.</param>
    /// <param name="hoverPrice">(Optional) The price of the hovered item, generally used in <see cref="ShopMenu"/>.</param>
    /// <param name="extraItemToShowIndex">(Optional) The index (probably parentSheetIndex) of the extra item which is generally a requirement for the hovered item in certain menus.</param>
    /// <param name="extraItemToShowAmount">(Optional) The amount or quantity of the extra item which is generally a requirement for the hovered item in certain menus.</param>
    /// <param name="highlightedItemPrefix">(Optional) The prefix to add to the spoken hovered item's details if it is highlighted i.e., not grayed out.</param>
    /// <param name="highlightedItemSuffix">(Optional) The suffix to add to the spoken hovered item's details if it is highlighted i.e., not grayed out.</param>
    /// <param name="hoverX">(Optional) The X position on screen to check. Default to null, in which case it uses the mouse's X position.</param>
    /// <param name="hoverY">(Optional) The Y position on screen to check. Default to null, in which case it uses the mouse's Y position.</param>
    /// <returns>true if any inventory slot was hovered or found at the <paramref name="hoverX"/> and <paramref name="hoverY"/>.</returns>
    public bool SpeakHoveredInventorySlot(InventoryMenu? inventoryMenu,
        bool? giveExtraDetails = null,
        int hoverPrice = -1,
        string? extraItemToShowIndex = null,
        int extraItemToShowAmount = -1,
        string highlightedItemPrefix = "",
        string highlightedItemSuffix = "",
        int? hoverX = null,
        int? hoverY = null);

    /// <summary>
    /// Get the details (name, description, quality, etc.) of an <see cref="Item"/>.
    /// </summary>
    /// <param name="item">The <see cref="Item"/>'s object that we want to get details of.</param>
    /// <param name="giveExtraDetails">(Optional) Whether to also return extra details or not. These include: description, health, stamina and other buffs.</param>
    /// <param name="price">(Optional) Generally the selling price of the item.</param>
    /// <param name="extraItemToShowIndex">(Optional) The index of the extra item which is generally the required item for the given item.</param>
    /// <param name="extraItemToShowAmount">(Optional) The amount or quantity of the extra item.</param>
    /// <returns>The details of the given <paramref name="item"/>.</returns>
    public string GetDetailsOfItem(Item item,
        bool giveExtraDetails = false,
        int price = -1,
        string? extraItemToShowIndex = null,
        int extraItemToShowAmount = -1);

    /// <summary>
    /// (Legacy! Should not be used from v1.6)
    /// Get the details (name, description, quality, etc.) of an <see cref="Item"/>.
    /// </summary>
    /// <param name="item">The <see cref="Item"/>'s object that we want to get details of.</param>
    /// <param name="giveExtraDetails">(Optional) Whether to also return extra details or not. These include: description, health, stamina and other buffs.</param>
    /// <param name="price">(Optional) Generally the selling price of the item.</param>
    /// <param name="extraItemToShowIndex">(Optional) The index of the extra item which is generally the required item for the given item.</param>
    /// <param name="extraItemToShowAmount">(Optional) The amount or quantity of the extra item.</param>
    /// <returns>The details of the given <paramref name="item"/>.</returns>
    public string GetDetailsOfItem(Item item,
        bool giveExtraDetails = false,
        int price = -1,
        int extraItemToShowIndex = -1,
        int extraItemToShowAmount = -1);

    #endregion

    /// <summary>
    /// Speaks the contents of hovered clickable component from the list.
    /// Prioritizes speaking from <see cref="ClickableComponent.ScreenReaderText"/> and <see cref="ClickableComponent.ScreenReaderDescription"/>
    /// and if these are empty, speaks the <see cref="ClickableComponent.name"/> and <see cref="ClickableComponent.label"/> as fallback.
    /// </summary>
    /// <param name="ccList">The list of components to speak from.</param>
    /// <returns>returns true if a hovered component was detected, otherwise false. It also returns true if a component was hovered but it's text was not spoken if either the fields were empty or <see cref="ClickableComponent.ScreenReaderIgnore"/> was true for the hovered component.</returns>
    public bool SpeakHoveredClickableComponentsFromList<T>(List<T> ccList) where T : ClickableComponent;

    /// <summary>
    /// Speaks the contents of the given clickable component.
    /// Prioritizes speaking from <see cref="ClickableComponent.ScreenReaderText"/> and <see cref="ClickableComponent.ScreenReaderDescription"/>
    /// and if these are empty, speaks the <see cref="ClickableComponent.name"/> and <see cref="ClickableComponent.label"/> as fallback.
    /// Ignores speaking if <see cref="ClickableComponent.ScreenReaderIgnore"/> was set to true.
    /// </summary>
    /// <param name="component">The component to speak.</param>
    /// <param name="commonUIButtonType">If set, the mod speaks the localized text for the given common ui button instead of the contents of the component</param>
    public void SpeakClickableComponent(ClickableComponent component, string? commonUIButtonType = null);

    /// <summary>
    /// Speaks the hovered element from list which are being drawn/rendered in slots.
    /// Prioritizes speaking from <see cref="OptionsElement.ScreenReaderText"/> and <see cref="OptionsElement.ScreenReaderDescription"/>
    /// and if these are empty, speaks the <see cref="OptionsElement.label"/> as fallback.
    /// </summary>
    /// <param name="optionSlots">The slots where the elements will be drawn/rendered.</param>
    /// <param name="options">The list of elements.</param>
    /// <param name="currentItemIndex">The index of the element currently being rendered in the first slot.</param>
    /// <returns>returns true if a hovered element was detected, otherwise false. It also returns true if a component was hovered but it's text was not spoken if either the fields were empty or <see cref="OptionsElement.ScreenReaderIgnore"/> was true for the hovered component.</returns>
    public bool SpeakHoveredOptionsElementSlot(List<ClickableComponent> optionSlots, List<OptionsElement> options,
        int currentItemIndex);

    /// <summary>
    /// Speaks the hovered element from <paramref name="options"/>.
    /// Prioritizes speaking from <see cref="OptionsElement.ScreenReaderText"/> and <see cref="OptionsElement.ScreenReaderDescription"/>
    /// and if these are empty, speaks the <see cref="OptionsElement.label"/> as fallback.
    /// </summary>
    /// <remarks>
    /// Only use this when element are being drawn independently from slots or when the element's position is correctly reflected in <see cref="OptionsElement.bounds"/>.
    /// </remarks>
    /// <param name="options">The list of elements.</param>
    /// <returns>returns true if a hovered element was detected, otherwise false. It also returns true if a component was hovered but it's text was not spoken if either the fields were empty or <see cref="OptionsElement.ScreenReaderIgnore"/> was true for the hovered component.</returns>
    public bool SpeakHoveredOptionsElementFromList<T>(List<T> options) where T : OptionsElement;

    /// <summary>
    /// Speaks the component of the given options element.
    /// Prioritizes speaking from <see cref="OptionsElement.ScreenReaderText"/> and <see cref="OptionsElement.ScreenReaderDescription"/>
    /// and if these are empty, speaks the <see cref="OptionsElement.label"/> as fallback.
    /// Ignores speaking if <see cref="OptionsElement.ScreenReaderIgnore"/> was set to true.
    /// </summary>
    /// <param name="element">The element to speak.</param>
    public void SpeakOptionsElement(OptionsElement element);

    /// <summary>
    /// Necessary to be called once if you have manually made a custom menu of your mod accessible.
    /// This will skip stardew access' patch that speaks the hover info in that menu.
    /// </summary>
    /// <param name="fullNameOfClass">The full name of the menu's class.
    /// <example>typeof(MyCustomMenu).FullName</example>
    /// </param>
    public void RegisterCustomMenuAsAccessible(string? fullNameOfClass);

    /// <summary>
    /// Registers a language helper to be used for a specific locale.
    /// </summary>
    /// <param name="locale">The locale for which the helper should be used (e.g., "en", "fr", "es-es").</param>
    /// <param name="helper">An instance of the language helper class implementing <see cref="ILanguageHelper"/>.</param>
    /// <remarks>
    /// The provided helper class should ideally derive from <see cref="LanguageHelperBase"/> for optimal compatibility, though this is not strictly required as long as it implements <see cref="ILanguageHelper"/>.
    /// </remarks>
    public void RegisterLanguageHelper(string locale, Type helperType);
#pragma warning restore CA1822 // Mark members as static
}
