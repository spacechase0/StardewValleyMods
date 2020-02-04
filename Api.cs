using GenericModConfigMenu.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu
{
    public interface IApi
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

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

        public void RegisterLabel(IManifest mod, string labelName, string labelDesc)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");
            Mod.instance.configs[mod].Options.Add(new LabelModOption(labelName, labelDesc, mod));
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet) => RegisterSimpleOption<bool>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet) => RegisterSimpleOption<int>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet) => RegisterSimpleOption<float>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet) => RegisterSimpleOption<string>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet) => RegisterSimpleOption<SButton>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet) => RegisterSimpleOption(mod, optionName, optionName, optionDesc, optionGet, optionSet);


        public void RegisterSimpleOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Type[] valid = new Type[] { typeof(bool), typeof(int), typeof(float), typeof(string), typeof(SButton) };
            if (!valid.Contains(typeof(T)))
            {
                throw new ArgumentException("Invalid config option type.");
            }
            Mod.instance.configs[mod].Options.Add(new SimpleModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, id, mod));
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max) => RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 1);
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max) => RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, 0.01f);

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval) => RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval) => RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max, interval);


        public void RegisterClampedOption<T>(IManifest mod, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval) => RegisterClampedOption(mod, optionName, optionName, optionDesc, optionGet, optionSet, min, max, interval);

        public void RegisterClampedOption<T>(IManifest mod, string id, string optionName, string optionDesc, Func<T> optionGet, Action<T> optionSet, T min, T max, T interval)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Type[] valid = new Type[] { typeof(int), typeof(float) };
            if (!valid.Contains(typeof(T)))
            {
                throw new ArgumentException("Invalid config option type.");
            }
            Mod.instance.configs[mod].Options.Add(new ClampedModOption<T>(optionName, optionDesc, typeof(T), optionGet, optionSet, min, max, interval, id, mod));
        }

        public void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices) => RegisterChoiceOption(mod, optionName, optionName, optionDesc, optionGet, optionSet, choices);

        public void RegisterChoiceOption(IManifest mod, string id, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Mod.instance.configs[mod].Options.Add(new ChoiceModOption<string>(optionName, optionDesc, typeof(string), optionGet, optionSet, choices, id, mod));
        }

        public void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                           Func<Vector2, object, object> widgetUpdate,
                                           Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                           Action<object> onSave)
            => RegisterComplexOption<object>(mod, optionName, optionDesc, widgetUpdate, widgetDraw, onSave);

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

            Mod.instance.configs[mod].Options.Add(new ComplexModOption(optionName, optionDesc, update, draw, save, mod));
        }

        public void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler) => SubscribeToChange<bool>(mod, changeHandler);
        public void SubscribeToChange(IManifest mod, Action<string, int> changeHandler) => SubscribeToChange<int>(mod, changeHandler);
        public void SubscribeToChange(IManifest mod, Action<string, float> changeHandler) => SubscribeToChange<float>(mod, changeHandler);
        public void SubscribeToChange(IManifest mod, Action<string, string> changeHandler) => SubscribeToChange<string>(mod, changeHandler);

        public void SubscribeToChange<T>(IManifest mod, Action<string, T> changeHandler)
        {
            InternalSubscribeToChange(mod, (id, value) =>
             {
                 if (value is T v)
                     changeHandler.Invoke(id, v);
             });
        }

        public void InternalSubscribeToChange(IManifest mod, Action<string, object> changeHandler)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            Mod.instance.configs[mod].ChangeHandler.Add(changeHandler);
        }

        public void OpenModMenu(IManifest mod)
        {
            if (!Mod.instance.configs.ContainsKey(mod))
                throw new ArgumentException("Mod not registered");

            if (Game1.activeClickableMenu is TitleMenu)
                TitleMenu.subMenu = new SpecificModConfigMenu(mod);
            else
                Game1.activeClickableMenu = new SpecificModConfigMenu(mod);
        }
        
    }
}
