using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CapstoneProfessions.Framework
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

        /// <summary>Get a translation equivalent to "Capstone Profession".</summary>
        public static string Menu_Title()
        {
            return I18n.GetByKey("menu.title");
        }

        /// <summary>Get a translation equivalent to "You have maxed out all of your skills... Choose a capstone".</summary>
        public static string Menu_Extra()
        {
            return I18n.GetByKey("menu.extra");
        }

        /// <summary>Get a translation equivalent to "Timelapse".</summary>
        public static string Profession_Time_Name()
        {
            return I18n.GetByKey("profession.time.name");
        }

        /// <summary>Get a translation equivalent to "The day is 20% longer.".</summary>
        public static string Profession_Time_Description()
        {
            return I18n.GetByKey("profession.time.description");
        }

        /// <summary>Get a translation equivalent to "Name Brand".</summary>
        public static string Profession_Profit_Name()
        {
            return I18n.GetByKey("profession.profit.name");
        }

        /// <summary>Get a translation equivalent to "Everything ships for 5% more.".</summary>
        public static string Profession_Profit_Description()
        {
            return I18n.GetByKey("profession.profit.description");
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

