using System;
using System.Linq;
using GenericModConfigMenu.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="configManager">Manages the registered mod config menus.</param>
        internal Api(ModConfigManager configManager)
        {
            this.ConfigManager = configManager;
        }


        /****
        ** Must be called first
        ****/
        /// <inheritdoc />
        public void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(revertToDefault, nameof(revertToDefault));
            this.AssertNotNull(saveToFile, nameof(saveToFile));

            if (this.ConfigManager.Get(mod, assert: false) != null)
                throw new InvalidOperationException($"The '{mod.Name}' mod has already registered a config menu, so it can't do it again.");

            this.ConfigManager.Set(mod, new ModConfig(mod, revertToDefault, saveToFile));
        }


        /****
        ** Basic options
        ****/
        /// <inheritdoc />
        public void RegisterLabel(IManifest mod, string labelName, string labelDesc)
        {
            this.AssertNotNull(mod, nameof(mod));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new SectionTitleModOption(labelName, labelDesc, modConfig));
        }

        /// <inheritdoc />
        public void RegisterParagraph(IManifest mod, string paragraph)
        {
            this.AssertNotNull(mod, nameof(mod));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new ParagraphModOption(paragraph, modConfig));
        }

        /// <inheritdoc />
        public void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4)
        {
            this.AssertNotNull(mod, nameof(mod));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new ImageModOption(texPath, texRect, scale, modConfig));
        }

        /// <inheritdoc />
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet)
        {
            this.RegisterSimpleOption<bool>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        /// <inheritdoc />
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet)
        {
            this.RegisterSimpleOption<int>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        /// <inheritdoc />
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet)
        {
            this.RegisterSimpleOption<float>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        /// <inheritdoc />
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet)
        {
            this.RegisterSimpleOption<string>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        /// <inheritdoc />
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet)
        {
            this.RegisterSimpleOption<SButton>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        /// <inheritdoc />
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet)
        {
            this.RegisterSimpleOption<KeybindList>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        /// <inheritdoc />
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max)
        {
            this.RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 1);
        }

        /// <inheritdoc />
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max)
        {
            this.RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 0.01f);
        }

        /// <inheritdoc />
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval)
        {
            this.RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        }

        /// <inheritdoc />
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval)
        {
            this.RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        }

        /// <inheritdoc />
        public void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(optionGet, nameof(optionGet));
            this.AssertNotNull(optionSet, nameof(optionSet));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new ChoiceModOption<string>(optionName, optionDesc, typeof(string), optionGet, optionSet, choices, optionName, modConfig));
        }


        /****
        ** Multi-page management
        ****/
        /// <inheritdoc />
        public void StartNewPage(IManifest mod, string pageName)
        {
            this.AssertNotNull(mod, nameof(mod));

            this.ConfigManager
                .Get(mod, assert: true)
                .SetActiveRegisteringPage(pageName);
        }

        /// <inheritdoc />
        public void OverridePageDisplayName(IManifest mod, string pageName, string displayName)
        {
            this.AssertNotNull(mod, nameof(mod));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            if (!modConfig.Options.TryGetValue(pageName, out ModConfigPage page))
                throw new ArgumentException("Page not registered");

            page.PageTitle = displayName;
        }

        /// <inheritdoc />
        public void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage)
        {
            this.AssertNotNull(mod, nameof(mod));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.AddOption(new PageLinkModOption(labelName, labelDesc, newPage, modConfig));
        }


        /****
        ** Advanced
        ****/
        /// <inheritdoc />
        public void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave)
        {
            this.RegisterComplexOption<object>(mod, optionName, optionDesc, widgetUpdate, widgetDraw, onSave);
        }

        public void RegisterComplexOption<T>(IManifest mod, string optionName, string optionDesc, Func<Vector2, T, T> widgetUpdate, Func<SpriteBatch, Vector2, T, T> widgetDraw, Action<T> onSave)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(widgetUpdate, nameof(widgetUpdate));
            this.AssertNotNull(widgetDraw, nameof(widgetDraw));
            this.AssertNotNull(onSave, nameof(onSave));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);

            object Update(Vector2 v2, object o) => widgetUpdate.Invoke(v2, (T)o);
            object Draw(SpriteBatch b, Vector2 v2, object o) => widgetDraw.Invoke(b, v2, (T)o);
            void Save(object o) => onSave.Invoke((T)o);

            modConfig.AddOption(new ComplexModOption(optionName, optionDesc, Update, Draw, Save, modConfig));
        }

        /// <inheritdoc />
        public void SetDefaultIngameOptinValue(IManifest mod, bool optedIn)
        {
            this.AssertNotNull(mod, nameof(mod));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.DefaultEditableInGame = optedIn;
        }

        /// <inheritdoc />
        public void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler)
        {
            this.SubscribeToChange<bool>(mod, changeHandler);
        }

        /// <inheritdoc />
        public void SubscribeToChange(IManifest mod, Action<string, int> changeHandler)
        {
            this.SubscribeToChange<int>(mod, changeHandler);
        }

        /// <inheritdoc />
        public void SubscribeToChange(IManifest mod, Action<string, float> changeHandler)
        {
            this.SubscribeToChange<float>(mod, changeHandler);
        }

        /// <inheritdoc />
        public void SubscribeToChange(IManifest mod, Action<string, string> changeHandler)
        {
            this.SubscribeToChange<string>(mod, changeHandler);
        }

        public void SubscribeToChange<T>(IManifest mod, Action<string, T> changeHandler)
        {
            this.AssertNotNull(mod, nameof(changeHandler));

            this.InternalSubscribeToChange(mod, (id, value) =>
            {
                if (value is T v)
                    changeHandler.Invoke(id, v);
            });
        }

        public void InternalSubscribeToChange(IManifest mod, Action<string, object> changeHandler)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(changeHandler, nameof(changeHandler));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);
            modConfig.ChangeHandlers.Add(changeHandler);
        }

        /// <inheritdoc />
        public void OpenModMenu(IManifest mod)
        {
            this.AssertNotNull(mod, nameof(mod));

            Mod.Instance.OpenModMenu(mod);
        }

        /// <inheritdoc />
        public void UnregisterModConfig(IManifest mod)
        {
            this.AssertNotNull(mod, nameof(mod));

            this.ConfigManager.Remove(mod);
        }

        /// <inheritdoc />
        public bool TryGetCurrentMenu(out IManifest mod, out string page)
        {
            if (Game1.activeClickableMenu is not SpecificModConfigMenu menu)
                menu = null;

            mod = menu?.Manifest;
            page = menu?.CurrPage;
            return menu is not null;
        }


        /*********
        ** Private methods
        *********/
        private void RegisterSimpleOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet)
        {
            this.RegisterSimpleOption(mod, optionName, optionName, optionDesc, optionGet, optionSet);
        }

        private void RegisterSimpleOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(optionGet, nameof(optionGet));
            this.AssertNotNull(optionSet, nameof(optionSet));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);

            Type[] valid = new[] { typeof(bool), typeof(int), typeof(float), typeof(string), typeof(SButton), typeof(KeybindList) };
            if (!valid.Contains(typeof(T)))
                throw new ArgumentException("Invalid config option type.");

            modConfig.AddOption(new SimpleModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, id, modConfig));
        }

        private void RegisterClampedOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval)
        {
            this.RegisterClampedOption(mod, optionName, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        }

        private void RegisterClampedOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(optionGet, nameof(optionGet));
            this.AssertNotNull(optionSet, nameof(optionSet));

            ModConfig modConfig = this.ConfigManager.Get(mod, assert: true);

            Type[] valid = new[] { typeof(int), typeof(float) };
            if (!valid.Contains(typeof(T)))
                throw new ArgumentException("Invalid config option type.");

            modConfig.AddOption(new NumericModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, min, max, interval, id, modConfig));
        }

        /// <summary>Assert that a required parameter is not null.</summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">The parameter value is null.</exception>
        private void AssertNotNull(object value, string paramName)
        {
            if (value is null)
                throw new ArgumentNullException(paramName);
        }
    }
}
