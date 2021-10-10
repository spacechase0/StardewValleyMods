using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace GenericModConfigMenu.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "Mod Options".</summary>
        public static string Button_ModOptions()
        {
            return I18n.GetByKey("button.mod-options");
        }

        /// <summary>Get a translation equivalent to "Configure Mods".</summary>
        public static string List_Heading()
        {
            return I18n.GetByKey("list.heading");
        }

        /// <summary>Get a translation equivalent to "Cancel".</summary>
        public static string Config_Buttons_Cancel()
        {
            return I18n.GetByKey("config.buttons.cancel");
        }

        /// <summary>Get a translation equivalent to "Default".</summary>
        public static string Config_Buttons_ResetToDefault()
        {
            return I18n.GetByKey("config.buttons.reset-to-default");
        }

        /// <summary>Get a translation equivalent to "Save".</summary>
        public static string Config_Buttons_Save()
        {
            return I18n.GetByKey("config.buttons.save");
        }

        /// <summary>Get a translation equivalent to "Save&amp;Close".</summary>
        public static string Config_Buttons_SaveAndClose()
        {
            return I18n.GetByKey("config.buttons.save-and-close");
        }

        /// <summary>Get a translation equivalent to "Rebinding key: {{optionName}}".</summary>
        /// <param name="optionName">The value to inject for the <c>{{optionName}}</c> token.</param>
        public static string Config_RebindKey_Title(string optionName)
        {
            return I18n.GetByKey("config.rebind-key.title", new { optionName });
        }

        /// <summary>Get a translation equivalent to "Press a key to rebind".</summary>
        public static string Config_RebindKey_SimpleInstructions()
        {
            return I18n.GetByKey("config.rebind-key.simple-instructions");
        }

        /// <summary>Get a translation equivalent to "Press a key combination to rebind".</summary>
        public static string Config_RebindKey_ComboInstructions()
        {
            return I18n.GetByKey("config.rebind-key.combo-instructions");
        }

        /// <summary>Get a translation equivalent to "(None)".</summary>
        public static string Config_RebindKey_NoKey()
        {
            return I18n.GetByKey("config.rebind-key.no-key");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

