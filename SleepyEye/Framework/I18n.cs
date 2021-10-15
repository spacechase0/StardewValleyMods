using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace SleepyEye.Framework
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

        /// <summary>Get a translation equivalent to "Tent".</summary>
        public static string Tent_Name()
        {
            return I18n.GetByKey("tent.name");
        }

        /// <summary>Get a translation equivalent to "Sleep here. Sleep there. Sleep everywhere!".</summary>
        public static string Tent_Description()
        {
            return I18n.GetByKey("tent.description");
        }

        /// <summary>Get a translation equivalent to "You camped somewhere strange, so you've returned home.".</summary>
        public static string Messages_SleptAtLostLocation()
        {
            return I18n.GetByKey("messages.slept-at-lost-location");
        }

        /// <summary>Get a translation equivalent to "You camped on the festival grounds, so you've returned home.".</summary>
        public static string Messages_SleptAtFestival()
        {
            return I18n.GetByKey("messages.slept-at-festival");
        }

        /// <summary>Get a translation equivalent to "Seconds Until Save".</summary>
        public static string Config_SecondsUntilSave_Name()
        {
            return I18n.GetByKey("config.seconds-until-save.name");
        }

        /// <summary>Get a translation equivalent to "The number of seconds until the tent tool should trigger a save.".</summary>
        public static string Config_SecondsUntilSave_Tooltip()
        {
            return I18n.GetByKey("config.seconds-until-save.tooltip");
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

