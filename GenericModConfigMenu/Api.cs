using System;
using System.Linq;
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

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func<Vector2, object, object> widgetUpdate,
                                   Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                   Action<object> onSave);

        void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, int> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, float> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, string> changeHandler);

        void OpenModMenu(IManifest mod);
    }

    public class Api : IApi
    {
        public void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile)
        {
            if (Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod already registered");
            Mod.instance.configs.Add(mod, new ModConfig(mod, revertToDefault, saveToFile));
        }

        public void UnregisterModConfig(IManifest mod)
        {
            if (Mod.instance.configs.ContainsKey(mod))
                Mod.instance.configs.Remove(mod);
        }

        public void SetDefaultIngameOptinValue(IManifest mod, bool optedIn)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");
            var modConfig = Mod.instance.configs[mod];
            modConfig.DefaultOptedIngame = optedIn;
        }

        public void StartNewPage(IManifest mod, string pageName)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            var modConfig = Mod.instance.configs[mod];
            if (modConfig.Options.ContainsKey(pageName))
                modConfig.ActiveRegisteringPage = modConfig.Options[pageName];
            else
                modConfig.Options.Add(pageName, modConfig.ActiveRegisteringPage = new ModConfig.ModPage(pageName));
        }
        public void OverridePageDisplayName(IManifest mod, string pageName, string displayName)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");
            if (!Mod.instance.configs[mod].Options.ContainsKey(pageName))
                throw new ArgumentException("Page not registered");

            Mod.instance.configs[mod].Options[pageName].DisplayName = displayName;
        }

        public void RegisterLabel(IManifest mod, string labelName, string labelDesc)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");
            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new LabelModOption(labelName, labelDesc, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }
        public void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");
            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new PageLabelModOption(labelName, labelDesc, newPage, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }

        public void RegisterParagraph(IManifest mod, string paragraph)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");
            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new ParagraphModOption(paragraph, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }

        public void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");
            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new ImageModOption(texPath, texRect, scale, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet) => this.RegisterSimpleOption<bool>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet) => this.RegisterSimpleOption<int>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet) => this.RegisterSimpleOption<float>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet) => this.RegisterSimpleOption<string>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet) => this.RegisterSimpleOption<SButton>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet) => this.RegisterSimpleOption<KeybindList>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet) => this.RegisterSimpleOption(mod, optionName, optionName, optionDesc, optionGet, optionSet);


        public void RegisterSimpleOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Type[] valid = new[] { typeof(bool), typeof(int), typeof(float), typeof(string), typeof(SButton), typeof(KeybindList) };
            if (!valid.Contains(typeof(T)))
            {
                throw new ArgumentException("Invalid config option type.");
            }
            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new SimpleModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, id, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max) => this.RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 1);
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max) => this.RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 0.01f);

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval) => this.RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval) => this.RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);


        public void RegisterClampedOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval) => this.RegisterClampedOption(mod, optionName, optionName, optionDesc, optionGet, optionSet, min, max, interval);

        public void RegisterClampedOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Type[] valid = new[] { typeof(int), typeof(float) };
            if (!valid.Contains(typeof(T)))
            {
                throw new ArgumentException("Invalid config option type.");
            }
            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new ClampedModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, min, max, interval, id, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }

        public void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices) => this.RegisterChoiceOption(mod, optionName, optionName, optionDesc, optionGet, optionSet, choices);

        public void RegisterChoiceOption(IManifest mod, string id, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new ChoiceModOption<string>(optionName, optionDesc, typeof(string), optionGet, optionSet, choices, id, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }

        public void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                           Func<Vector2, object, object> widgetUpdate,
                                           Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                           Action<object> onSave)
            => this.RegisterComplexOption<object>(mod, optionName, optionDesc, widgetUpdate, widgetDraw, onSave);

        public void RegisterComplexOption<T>(IManifest mod, string optionName, string optionDesc,
                                                Func<Vector2, T, T> widgetUpdate,
                                                Func<SpriteBatch, Vector2, T, T> widgetDraw,
                                                Action<T> onSave)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Func<Vector2, object, object> update = (Vector2 v2, object o) => widgetUpdate.Invoke(v2, (T)o);
            Func<SpriteBatch, Vector2, object, object> draw = (SpriteBatch b, Vector2 v2, object o) => widgetDraw.Invoke(b, v2, (T)o);
            Action<object> save = (object o) => onSave.Invoke((T)o);

            Mod.instance.configs[mod].ActiveRegisteringPage.Options.Add(new ComplexModOption(optionName, optionDesc, update, draw, save, mod) { AvailableInGame = Mod.instance.configs[mod].DefaultOptedIngame });
            if (Mod.instance.configs[mod].DefaultOptedIngame)
                Mod.instance.configs[mod].HasAnyInGame = true;
        }

        public void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler) => this.SubscribeToChange<bool>(mod, changeHandler);
        public void SubscribeToChange(IManifest mod, Action<string, int> changeHandler) => this.SubscribeToChange<int>(mod, changeHandler);
        public void SubscribeToChange(IManifest mod, Action<string, float> changeHandler) => this.SubscribeToChange<float>(mod, changeHandler);
        public void SubscribeToChange(IManifest mod, Action<string, string> changeHandler) => this.SubscribeToChange<string>(mod, changeHandler);

        public void SubscribeToChange<T>(IManifest mod, Action<string, T> changeHandler)
        {
            this.InternalSubscribeToChange(mod, (id, value) =>
             {
                 if (value is T v)
                     changeHandler.Invoke(id, v);
             });
        }

        public void InternalSubscribeToChange(IManifest mod, Action<string, object> changeHandler)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Mod.instance.configs[mod].ActiveRegisteringPage.ChangeHandler.Add(changeHandler);
        }

        public void OpenModMenu(IManifest mod)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            if (Game1.activeClickableMenu is TitleMenu)
                TitleMenu.subMenu = new SpecificModConfigMenu(mod, false);
            else
                Game1.activeClickableMenu = new SpecificModConfigMenu(mod, false);
        }

    }
}
