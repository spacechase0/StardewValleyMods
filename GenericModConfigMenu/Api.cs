using System;
using System.Linq;
using GenericModConfigMenu.Framework;
using GenericModConfigMenu.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu
{
    public interface IApi
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);
        void UnregisterModConfig(IManifest mod);

        void SetDefaultIngameOptinValue(IManifest mod, bool optedIn);

        void StartNewPage(IManifest mod, string pageName);
        void OverridePageDisplayName(IManifest mod, string pageName, string displayName);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage);
        void RegisterParagraph(IManifest mod, string paragraph);
        void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave);

        void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, int> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, float> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, string> changeHandler);

        void OpenModMenu(IManifest mod);
    }

    public class Api : IApi
    {
        /*********
        ** Public methods
        *********/
        public void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(revertToDefault, nameof(revertToDefault));
            this.AssertNotNull(saveToFile, nameof(saveToFile));

            if (Mod.Instance.Configs.ContainsKey(mod))
                throw new InvalidOperationException($"The '{mod.Name}' mod has already registered a config menu, so it can't do it again.");

            Mod.Instance.Configs.Add(mod, new ModConfig(mod, revertToDefault, saveToFile));
        }

        public void UnregisterModConfig(IManifest mod)
        {
            this.AssertNotNull(mod, nameof(mod));

            Mod.Instance.Configs.Remove(mod);
        }

        public void SetDefaultIngameOptinValue(IManifest mod, bool optedIn)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            modConfig.DefaultOptedIngame = optedIn;
        }

        public void StartNewPage(IManifest mod, string pageName)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            if (modConfig.Options.TryGetValue(pageName, out ModConfig.ModPage page))
                modConfig.ActiveRegisteringPage = page;
            else
                modConfig.Options.Add(pageName, modConfig.ActiveRegisteringPage = new ModConfig.ModPage(pageName));
        }

        public void OverridePageDisplayName(IManifest mod, string pageName, string displayName)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            if (!modConfig.Options.TryGetValue(pageName, out ModConfig.ModPage page))
                throw new ArgumentException("Page not registered");

            page.DisplayName = displayName;
        }

        public void RegisterLabel(IManifest mod, string labelName, string labelDesc)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            modConfig.ActiveRegisteringPage.Options.Add(new LabelModOption(labelName, labelDesc, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

        public void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            modConfig.ActiveRegisteringPage.Options.Add(new PageLabelModOption(labelName, labelDesc, newPage, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

        public void RegisterParagraph(IManifest mod, string paragraph)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            modConfig.ActiveRegisteringPage.Options.Add(new ParagraphModOption(paragraph, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

        public void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            modConfig.ActiveRegisteringPage.Options.Add(new ImageModOption(texPath, texRect, scale, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet)
        {
            this.RegisterSimpleOption<bool>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet)
        {
            this.RegisterSimpleOption<int>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet)
        {
            this.RegisterSimpleOption<float>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet)
        {
            this.RegisterSimpleOption<string>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet)
        {
            this.RegisterSimpleOption<SButton>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet)
        {
            this.RegisterSimpleOption<KeybindList>(mod, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet)
        {
            this.RegisterSimpleOption(mod, optionName, optionName, optionDesc, optionGet, optionSet);
        }

        public void RegisterSimpleOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(optionGet, nameof(optionGet));
            this.AssertNotNull(optionSet, nameof(optionSet));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            Type[] valid = new[] { typeof(bool), typeof(int), typeof(float), typeof(string), typeof(SButton), typeof(KeybindList) };
            if (!valid.Contains(typeof(T)))
                throw new ArgumentException("Invalid config option type.");

            modConfig.ActiveRegisteringPage.Options.Add(new SimpleModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, id, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max)
        {
            this.RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 1);
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max)
        {
            this.RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 0.01f);
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval)
        {
            this.RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval)
        {
            this.RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        }

        public void RegisterClampedOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval)
        {
            this.RegisterClampedOption(mod, optionName, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        }

        public void RegisterClampedOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(optionGet, nameof(optionGet));
            this.AssertNotNull(optionSet, nameof(optionSet));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            Type[] valid = new[] { typeof(int), typeof(float) };
            if (!valid.Contains(typeof(T)))
                throw new ArgumentException("Invalid config option type.");

            modConfig.ActiveRegisteringPage.Options.Add(new ClampedModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, min, max, interval, id, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

        public void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            this.RegisterChoiceOption(mod, optionName, optionName, optionDesc, optionGet, optionSet, choices);
        }

        public void RegisterChoiceOption(IManifest mod, string id, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            this.AssertNotNull(mod, nameof(mod));
            this.AssertNotNull(optionGet, nameof(optionGet));
            this.AssertNotNull(optionSet, nameof(optionSet));
            this.AssertModRegistered(mod, out ModConfig modConfig);

            modConfig.ActiveRegisteringPage.Options.Add(new ChoiceModOption<string>(optionName, optionDesc, typeof(string), optionGet, optionSet, choices, id, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

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
            this.AssertModRegistered(mod, out ModConfig modConfig);

            object Update(Vector2 v2, object o) => widgetUpdate.Invoke(v2, (T)o);
            object Draw(SpriteBatch b, Vector2 v2, object o) => widgetDraw.Invoke(b, v2, (T)o);
            void Save(object o) => onSave.Invoke((T)o);

            modConfig.ActiveRegisteringPage.Options.Add(new ComplexModOption(optionName, optionDesc, Update, Draw, Save, mod) { AvailableInGame = modConfig.DefaultOptedIngame });
            if (modConfig.DefaultOptedIngame)
                modConfig.HasAnyInGame = true;
        }

        public void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler)
        {
            this.SubscribeToChange<bool>(mod, changeHandler);
        }

        public void SubscribeToChange(IManifest mod, Action<string, int> changeHandler)
        {
            this.SubscribeToChange<int>(mod, changeHandler);
        }

        public void SubscribeToChange(IManifest mod, Action<string, float> changeHandler)
        {
            this.SubscribeToChange<float>(mod, changeHandler);
        }

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
            this.AssertModRegistered(mod, out ModConfig modConfig);

            modConfig.ActiveRegisteringPage.ChangeHandler.Add(changeHandler);
        }

        public void OpenModMenu(IManifest mod)
        {
            this.AssertNotNull(mod, nameof(mod));

            if (Game1.activeClickableMenu is TitleMenu)
                TitleMenu.subMenu = new SpecificModConfigMenu(mod, false);
            else
                Game1.activeClickableMenu = new SpecificModConfigMenu(mod, false);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Assert that the given mod has registered a config menu.</summary>
        /// <param name="mod">The mod manifest to check.</param>
        /// <param name="modConfig">The registered mod configuration.</param>
        /// <exception cref="InvalidOperationException">The mod hasn't registered a config menu.</exception>
        private void AssertModRegistered(IManifest mod, out ModConfig modConfig)
        {
            if (!Mod.Instance.Configs.TryGetValue(mod, out modConfig))
                throw new InvalidOperationException($"The '{mod.Name}' mod hasn't registered a config menu, so it can't edit it.");
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
