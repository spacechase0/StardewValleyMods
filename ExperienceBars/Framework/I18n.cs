using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace ExperienceBars.Framework
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

        /// <summary>Get a translation equivalent to "X Position".</summary>
        public static string Config_PositionX_Name()
        {
            return I18n.GetByKey("config.position-x.name");
        }

        /// <summary>Get a translation equivalent to "The pixel X position at which to draw the experience bars, relative to the top-left corner of the screen.".</summary>
        public static string Config_PositionX_Tooltip()
        {
            return I18n.GetByKey("config.position-x.tooltip");
        }

        /// <summary>Get a translation equivalent to "Y Position".</summary>
        public static string Config_PositionY_Name()
        {
            return I18n.GetByKey("config.position-y.name");
        }

        /// <summary>Get a translation equivalent to "The pixel Y position at which to draw the experience bars, relative to the top-left corner of the screen.".</summary>
        public static string Config_PositionY_Tooltip()
        {
            return I18n.GetByKey("config.position-y.tooltip");
        }

        /// <summary>Get a translation equivalent to "Toggle Button".</summary>
        public static string Config_ToggleKey_Name()
        {
            return I18n.GetByKey("config.toggle-key.name");
        }

        /// <summary>Get a translation equivalent to "The button which shows or hides the experience bars display. Press Shift and this button to move the display.".</summary>
        public static string Config_ToggleKey_Tooltip()
        {
            return I18n.GetByKey("config.toggle-key.tooltip");
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

