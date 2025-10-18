using System;
using System.Linq;
using System.Runtime.CompilerServices;

using GenericModConfigMenu.Framework.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace GenericModConfigMenu.Framework
{
    /// <inheritdoc cref="IGenericModConfigMenuApi" />
    public class Api : IGenericModConfigMenuApi
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages the registered mod config menus.</summary>
        private readonly ModConfigManager ConfigManager;

        /// <summary>Open the config UI for a specific mod.</summary>
        private readonly Action<IManifest> OpenModMenuImpl;

        /// <summary>Open the config UI for a specific mod, as a child menu.</summary>
        private readonly Action<IManifest> OpenModMenuImplChild;

        private readonly IManifest mod;

        private readonly Action<string> DeprecationWarner;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="configManager">Manages the registered mod config menus.</param>
        /// <param name="openModMenu">Open the config UI for a specific mod.</param>
        internal Api(IManifest mod, ModConfigManager configManager, Action<IManifest> openModMenu, Action<IManifest> openModMenuChild, Action<string> DeprecationWarner)
        {
            this.mod = mod;
            this.ConfigManager = configManager;
            this.OpenModMenuImpl = openModMenu;
            this.OpenModMenuImplChild = openModMenuChild;
            this.DeprecationWarner = DeprecationWarner;
        }


        /****
        ** Must be called first
        ****/
        /// <inheritdoc />
        public void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = true)
        {
            mod ??= this.mod;
            this.AssertNotNull(reset);
            this.AssertNotNull(save);

            if (this.ConfigManager.Get(mod, assert: false) != null)
                throw new InvalidOperationException($"The '{mod.Name}' mod has already registered a config menu, so it can't do it again.");

            if (this.mod.UniqueID != mod.UniqueID)
                Log.Trace($"{this.mod.UniqueID} is registering on behalf of {mod.UniqueID}");

            this.ConfigManager.Set(mod, new ModConfig(mod, reset, save, titleScreenOnly));
        }

        /****
        ** Basic options
        ****/
        /// <inheritdoc />
        public void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null)
        {
            mod ??= this.mod;
            this.AssertNotNull(text);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new SectionTitleModOption(text, tooltip, modConfig));
        }

        /// <inheritdoc />
        public void AddSubHeader(IManifest mod, Func<string> text)
        {
            mod ??= this.mod;
            this.AssertNotNull(text);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new SectionSubHeaderModOption(text, modConfig));
        }

        /// <inheritdoc />
        public void AddParagraph(IManifest mod, Func<string> text)
        {
            mod ??= this.mod;
            this.AssertNotNull(text);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new ParagraphModOption(text, modConfig));
        }

        /// <inheritdoc />
        public void AddImage(IManifest mod, Func<Texture2D> texture, Rectangle? texturePixelArea = null, int scale = Game1.pixelZoom)
        {
            mod ??= this.mod;
            this.AssertNotNull(texture);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new ImageModOption(texture, texturePixelArea, scale, modConfig));
        }

        /// <inheritdoc />
        public void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null)
        {
            this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
        }

        /// <inheritdoc />
        public void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name = null, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null)
        {
            this.AddNumericOption(mod: mod, name: name, tooltip: tooltip, getValue: getValue, setValue: setValue, min: min, max: max, interval: interval, formatValue: formatValue, fieldId: fieldId);
        }

        /// <inheritdoc />
        public void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name = null, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, Func<float, string> formatValue = null, string fieldId = null)
        {
            this.AddNumericOption(mod: mod, name: name, tooltip: tooltip, getValue: getValue, setValue: setValue, min: min, max: max, interval: interval, formatValue: formatValue, fieldId: fieldId);
        }

        /// <inheritdoc />
        public void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name = null, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null)
        {
            if (allowedValues?.Any() == true)
                this.AddChoiceOption(mod, name, tooltip, getValue, setValue, allowedValues, formatAllowedValue, fieldId);
            else
                this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
        }

        /// <inheritdoc />
        public void AddKeybind(IManifest mod, Func<SButton> getValue, Action<SButton> setValue, Func<string> name = null, Func<string> tooltip = null, string fieldId = null)
        {
            this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
        }

        /// <inheritdoc />
        public void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name = null, Func<string> tooltip = null, string fieldId = null)
        {
            this.AddSimpleOption(mod, name, tooltip, getValue, setValue, fieldId);
        }


        /****
        ** Multi-page management
        ****/
        /// <inheritdoc />
        public void AddPage(IManifest mod, string pageId, Func<string> pageTitle = null)
        {
            mod ??= this.mod;
            this.AssertNotNull(pageId);

            this.ConfigManager
                .Get(mod, assert: true)
                .SetActiveRegisteringPage(pageId, pageTitle);
        }

        /// <inheritdoc />
        public void AddPageLink(IManifest mod, string pageId, Func<string> text, Func<string> tooltip = null)
        {
            mod ??= this.mod;
            this.AssertNotNull(pageId);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new PageLinkModOption(pageId, text, tooltip, modConfig));
        }

        /****
        ** Advanced
        ****/
        /// <inheritdoc />
        public void AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeMenuOpened = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Action beforeMenuClosed = null, Func<int> height = null, string fieldId = null)
        {
            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);

            modConfig.AddOption(new ComplexModOption(fieldId: fieldId, name: name, tooltip: tooltip, mod: modConfig, height: height, draw: draw, beforeMenuOpened: beforeMenuOpened, beforeSave: beforeSave, afterSave: afterSave, beforeReset: beforeReset, afterReset: afterReset, beforeMenuClosed: beforeMenuClosed));
        }

        /// <inheritdoc />
        public void SetTitleScreenOnlyForNextOptions(IManifest mod, bool titleScreenOnly)
        {
            mod ??= this.mod;

            ModConfig config = this.ConfigManager.Get(mod, assert: true);
            config.DefaultTitleScreenOnly = titleScreenOnly;
        }

        /// <inheritdoc />
        public void OnFieldChanged(IManifest mod, Action<string, object> onChange)
        {
            mod ??= this.mod;
            this.AssertNotNull(onChange);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.ChangeHandlers.Add(onChange);
        }

        /// <inheritdoc />
        public void OpenModMenu(IManifest mod)
        {
            mod ??= this.mod;

            this.OpenModMenuImpl(mod);
        }

        /// <inheritdoc />
        public void OpenModMenuAsChildMenu(IManifest mod)
        {
            mod ??= this.mod;

            this.OpenModMenuImplChild(mod);
        }

        /// <inheritdoc />
        public void Unregister(IManifest mod)
        {
            mod ??= this.mod;

            this.ConfigManager.Remove(mod);
        }

        /// <inheritdoc />
        public bool TryGetCurrentMenu(out IManifest mod, out string page)
        {
            if (Mod.ActiveConfigMenu is not SpecificModConfigMenu menu)
                menu = null;

            mod = menu?.Manifest;
            page = menu?.CurrPage;
            return menu is not null;
        }

        /****
        ** Obsolete since 1.8.0
        ****/
        /// <inheritdoc />
        [Obsolete]
        public void AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Func<int> height = null, string fieldId = null)
        {
            this.LogDeprecation(mod, nameof(AddComplexOption) + "(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Func<int> height = null, string fieldId = null)");
            this.AddComplexOption(mod: mod, name: name, tooltip: tooltip, draw: draw, beforeMenuOpened: null, beforeSave: beforeSave, afterSave: afterSave, beforeReset: beforeReset, afterReset: afterReset, beforeMenuClosed: null, height: height, fieldId: fieldId);
        }

        /****
        ** Obsolete since 1.7.0
        ****/
        /// <inheritdoc />
        [Obsolete]
        public void AddComplexOption(IManifest mod, Func<string> name, Func<string> tooltip, Action<SpriteBatch, Vector2> draw, Action saveChanges, Func<int> height = null, string fieldId = null)
        {
            this.LogDeprecation(mod, nameof(AddChoiceOption) + "(IManifest mod, Func<string> name, Func<string> tooltip, Action<SpriteBatch, Vector2> draw, Action saveChanges, Func<int> height = null, string fieldId = null)");
            this.AddComplexOption(mod: mod, name: name, tooltip: tooltip, draw: draw, beforeMenuOpened: null, beforeSave: saveChanges, height: height, fieldId: fieldId);
        }

        /// <inheritdoc />
        [Obsolete]
        public void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name = null, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, string fieldId = null)
        {
            this.LogDeprecation(mod, nameof(AddNumberOption) + "(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name = null, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, string fieldId = null)");
            this.AddNumericOption(mod: mod, name: name, tooltip: tooltip, getValue: getValue, setValue: setValue, min: min, max: max, interval: interval, fieldId: fieldId, formatValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name = null, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, string fieldId = null)
        {
            this.LogDeprecation(mod, nameof(AddNumberOption) + "(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name = null, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, string fieldId = null)");
            this.AddNumericOption(mod: mod, name: name, tooltip: tooltip, getValue: getValue, setValue: setValue, min: min, max: max, interval: interval, fieldId: fieldId, formatValue: null);
        }

        /****
        ** Obsolete since 1.6.0
        ****/
        /// <inheritdoc />
        [Obsolete]
        public void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name = null, Func<string> tooltip = null, string[] allowedValues = null, string fieldId = null)
        {
            this.LogDeprecation(mod, nameof(AddTextOption) + "(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name = null, Func<string> tooltip = null, string[] allowedValues = null, string fieldId = null)");
            this.AddTextOption(mod: mod, getValue: getValue, setValue: setValue, name: name, tooltip: tooltip, allowedValues: allowedValues, formatAllowedValue: null, fieldId: fieldId);
        }

        /****
        ** Obsolete since 1.5.0
        ****/
        /// <inheritdoc />
        [Obsolete]
        public void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile)
        {
            this.LogDeprecation(mod, nameof(RegisterModConfig));
            this.Register(mod: mod, reset: revertToDefault, save: saveToFile, titleScreenOnly: true);
        }

        /// <inheritdoc />
        [Obsolete]
        public void UnregisterModConfig(IManifest mod)
        {
            this.LogDeprecation(mod, nameof(UnregisterModConfig));
            this.Unregister(mod: mod);
        }

        /// <inheritdoc />
        [Obsolete]
        public void SetDefaultIngameOptinValue(IManifest mod, bool optedIn)
        {
            this.LogDeprecation(mod, nameof(SetDefaultIngameOptinValue));
            this.SetTitleScreenOnlyForNextOptions(mod: mod, titleScreenOnly: !optedIn);
        }

        /// <inheritdoc />
        [Obsolete]
        public void StartNewPage(IManifest mod, string pageName)
        {
            this.LogDeprecation(mod, nameof(StartNewPage));
            this.AddPage(mod: mod, pageId: pageName, pageTitle: () => pageName);
        }

        /// <inheritdoc />
        [Obsolete]
        public void OverridePageDisplayName(IManifest mod, string pageName, string displayName)
        {
            this.LogDeprecation(mod, nameof(OverridePageDisplayName));
            mod ??= this.mod;

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            ModConfigPage page = modConfig.Pages.GetOrDefault(pageName) ?? throw new ArgumentException("Page not registered");

            page.SetPageTitle(() => displayName);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterLabel(IManifest mod, string labelName, string labelDesc)
        {
            this.LogDeprecation(mod, nameof(RegisterLabel));
            this.AddSectionTitle(mod: mod, text: () => labelName, tooltip: () => labelDesc);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage)
        {
            this.LogDeprecation(mod, nameof(RegisterPageLabel));
            this.AddPageLink(mod: mod, pageId: newPage, text: () => labelName, tooltip: () => labelDesc);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterParagraph(IManifest mod, string paragraph)
        {
            this.LogDeprecation(mod, nameof(RegisterParagraph));
            this.AddParagraph(mod: mod, text: () => paragraph);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = Game1.pixelZoom)
        {
            this.LogDeprecation(mod, nameof(RegisterImage));
            this.AddImage(mod: mod, texture: () => Game1.content.Load<Texture2D>(texPath), texturePixelArea: texRect, scale: scale);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet)
        {
            this.LogDeprecation(mod, nameof(RegisterSimpleOption));
            this.AddBoolOption(mod: mod, fieldId: optionName, getValue: optionGet, setValue: optionSet, name: () => optionName, tooltip: () => optionDesc);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet)
        {
            this.LogDeprecation(mod, nameof(RegisterSimpleOption));
            this.AddNumericOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, min: null, max: null, interval: null, formatValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet)
        {
            this.LogDeprecation(mod, nameof(RegisterSimpleOption));
            this.AddNumericOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, min: null, max: null, interval: null, formatValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet)
        {
            this.LogDeprecation(mod, nameof(RegisterSimpleOption));
            this.AddTextOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, formatAllowedValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet)
        {
            this.LogDeprecation(mod, nameof(RegisterSimpleOption));
            this.AddKeybind(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet)
        {
            this.LogDeprecation(mod, nameof(RegisterSimpleOption));
            this.AddKeybindList(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max)
        {
            this.LogDeprecation(mod, nameof(RegisterClampedOption));
            this.AddNumericOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: null, formatValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max)
        {
            this.LogDeprecation(mod, nameof(RegisterClampedOption));
            this.AddNumericOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: null, formatValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval)
        {
            this.LogDeprecation(mod, nameof(RegisterClampedOption));
            this.AddNumericOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: interval, formatValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval)
        {
            this.LogDeprecation(mod, nameof(RegisterClampedOption));
            this.AddNumericOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, min: min, max: max, interval: interval, formatValue: null);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            this.LogDeprecation(mod, nameof(RegisterChoiceOption));
            this.AddTextOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, getValue: optionGet, setValue: optionSet, allowedValues: choices);
        }

        /// <inheritdoc />
        [Obsolete]
        public void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave)
        {
            this.LogDeprecation(mod, nameof(RegisterComplexOption));
            mod ??= this.mod;
            this.AssertNotNull(widgetUpdate);
            this.AssertNotNull(widgetDraw);
            this.AssertNotNull(onSave);

            object state = null;

            void Draw(SpriteBatch spriteBatch, Vector2 position)
            {
                state = widgetUpdate(position, state);
                state = widgetDraw(spriteBatch, position, state);
            }

            void Save()
            {
                onSave(state);
            }

            this.AddComplexOption(mod: mod, fieldId: optionName, name: () => optionName, tooltip: () => optionDesc, draw: Draw, saveChanges: Save);
        }

        /// <inheritdoc />
        [Obsolete]
        public void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler)
        {
            this.LogDeprecation(mod, nameof(SubscribeToChange) + "(IManifest mod, Action<string, bool> changeHandler)");
            this.SubscribeToChange<bool>(mod, changeHandler);
        }

        /// <inheritdoc />
        [Obsolete]
        public void SubscribeToChange(IManifest mod, Action<string, int> changeHandler)
        {
            this.LogDeprecation(mod, nameof(SubscribeToChange) + "(IManifest mod, Action<string, int> changeHandler)");
            this.SubscribeToChange<int>(mod, changeHandler);
        }

        /// <inheritdoc />
        [Obsolete]
        public void SubscribeToChange(IManifest mod, Action<string, float> changeHandler)
        {
            this.LogDeprecation(mod, nameof(SubscribeToChange) + "(IManifest mod, Action<string, float> changeHandler)");
            this.SubscribeToChange<float>(mod, changeHandler);
        }

        /// <inheritdoc />
        [Obsolete]
        public void SubscribeToChange(IManifest mod, Action<string, string> changeHandler)
        {
            this.LogDeprecation(mod, nameof(SubscribeToChange) + "(IManifest mod, Action<string, string> changeHandler)");
            this.SubscribeToChange<string>(mod, changeHandler);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Add a simple option without clamped values.</summary>
        /// <typeparam name="T">The option value type.</typeparam>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="fieldId">The unique field ID used when raising field-changed events, or <c>null</c> to generate a random one.</param>
        private void AddSimpleOption<T>(IManifest mod, Func<string> name, Func<string> tooltip, Func<T> getValue, Action<T> setValue, string fieldId)
        {
            mod ??= this.mod;
            this.AssertNotNull(name);
            this.AssertNotNull(getValue);
            this.AssertNotNull(setValue);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);

            Type[] valid = new[] { typeof(bool), typeof(int), typeof(float), typeof(string), typeof(SButton), typeof(KeybindList) };
            if (!valid.Contains(typeof(T)))
                throw new ArgumentException("Invalid config option type.");

            modConfig.AddOption(new SimpleModOption<T>(fieldId, name, tooltip, modConfig, getValue, setValue));
        }

        /// <summary>Add a numeric option with optional clamping.</summary>
        /// <typeparam name="T">The option value type.</typeparam>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        /// <param name="formatValue">Get the display text to show for a value, or <c>null</c> to show the number as-is.</param>
        /// <param name="fieldId">The unique field ID used when raising field-changed events, or <c>null</c> to generate a random one.</param>
        private void AddNumericOption<T>(IManifest mod, Func<string> name, Func<string> tooltip, Func<T> getValue, Action<T> setValue, T? min, T? max, T? interval, Func<T, string> formatValue, string fieldId)
            where T : struct
        {
            mod ??= this.mod;
            this.AssertNotNull(name);
            this.AssertNotNull(getValue);
            this.AssertNotNull(setValue);

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);

            Type[] valid = { typeof(int), typeof(float) };
            if (!valid.Contains(typeof(T)))
                throw new ArgumentException("Invalid config option type.");

            modConfig.AddOption(new NumericModOption<T>(fieldId: fieldId, name: name, tooltip: tooltip, mod: modConfig, getValue: getValue, setValue: setValue, min: min, max: max, interval: interval, formatValue: formatValue));
        }

        /// <summary>Add a dropdown option.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="allowedValues">The values that can be selected, or <c>null</c> to allow any.</param>
        /// <param name="formatAllowedValues">Allows formatting allowed values with a displayed value, or <c>null</c> to use values as labels.</param>
        /// <param name="fieldId">The unique field ID used when raising field-changed events, or <c>null</c> to generate a random one.</param>
        private void AddChoiceOption(IManifest mod, Func<string> name, Func<string> tooltip, Func<string> getValue, Action<string> setValue, string[] allowedValues, Func<string, string> formatAllowedValues, string fieldId)
        {
            mod ??= this.mod;
            this.AssertNotNull(name);
            this.AssertNotNull(getValue);
            this.AssertNotNull(setValue);

            name ??= () => fieldId;

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);

            modConfig.AddOption(new ChoiceModOption<string>(fieldId, name, tooltip, modConfig, getValue, setValue, allowedValues, formatAllowedValues));
        }

        /// <summary>Register a field changed handler for a value type.</summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="changeHandler">The handler to invoke when a field of the given type changes.</param>
        [Obsolete("This only exists to support obsolete methods.")]
        private void SubscribeToChange<TValue>(IManifest mod, Action<string, TValue> changeHandler)
        {
            this.AssertNotNull(changeHandler);

            this.OnFieldChanged(mod, (fieldId, rawValue) =>
            {
                if (rawValue is TValue value)
                    changeHandler(fieldId, value);
            });
        }

        /// <summary>Assert that a required parameter is not null.</summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">The parameter value is null.</exception>
        private void AssertNotNull(object value, [CallerArgumentExpression("value")] string paramName = "")
        {
            if (value is null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>Assert that a required parameter is not null or whitespace.</summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">The parameter value is null.</exception>
        private void AssertNotNullOrWhitespace(string value, [CallerArgumentExpression("value")] string paramName = "")
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(paramName);
        }

        private void LogDeprecation(IManifest client, string method)
        {
            this.DeprecationWarner($"{this.mod.UniqueID} (registering for {client.UniqueID}) is using deprecated code ({method}) that will break in a future version of GMCM.");
        }
    }
}
