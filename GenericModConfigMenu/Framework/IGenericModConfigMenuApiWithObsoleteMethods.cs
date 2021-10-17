using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace GenericModConfigMenu.Framework
{
    /// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu, including obsolete methods.</summary>
    public interface IGenericModConfigMenuApiWithObsoleteMethods
    {
        /*********
        ** DO NOT COPY THESE INTO YOUR MOD CODE.
        **
        ** These are only included for backwards compatibility and will be removed in a future version.
        ** Your mod will break if you haven't removed them when that happens.
        *********/
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.Register) + " instead.")] void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.Unregister) + " instead.")] void UnregisterModConfig(IManifest mod);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.Register) + " or " + nameof(IGenericModConfigMenuApi.SetTitleScreenOnlyForNextOptions) + " instead.")] void SetDefaultIngameOptinValue(IManifest mod, bool optedIn);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddPage) + " instead.")] void StartNewPage(IManifest mod, string pageName);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddPage) + " instead.")] void OverridePageDisplayName(IManifest mod, string pageName, string displayName);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddSectionTitle) + " instead.")] void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddParagraph) + " instead.")] void RegisterParagraph(IManifest mod, string paragraph);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddPageLink) + " instead.")] void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddImage) + " instead.")] void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddBoolOption) + " instead.")] void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddNumberOption) + " instead.")] void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddNumberOption) + " instead.")] void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddTextOption) + " instead.")] void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddKeybind) + " instead.")] void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddKeybindList) + " instead.")] void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<KeybindList> optionGet, Action<KeybindList> optionSet);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddNumberOption) + " instead.")] void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddNumberOption) + " instead.")] void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddNumberOption) + " instead.")] void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddNumberOption) + " instead.")] void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddTextOption) + " instead.")] void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddComplexOption) + " instead.")] void RegisterComplexOption(IManifest mod, string optionName, string optionDesc, Func<Vector2, object, object> widgetUpdate, Func<SpriteBatch, Vector2, object, object> widgetDraw, Action<object> onSave);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.OnFieldChanged) + " instead.")] void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.OnFieldChanged) + " instead.")] void SubscribeToChange(IManifest mod, Action<string, int> changeHandler);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.OnFieldChanged) + " instead.")] void SubscribeToChange(IManifest mod, Action<string, float> changeHandler);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.OnFieldChanged) + " instead.")] void SubscribeToChange(IManifest mod, Action<string, string> changeHandler);
        [Obsolete("Use " + nameof(IGenericModConfigMenuApi.AddTextOption) + " instead.")]  void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, string fieldId = null);
    }
}
