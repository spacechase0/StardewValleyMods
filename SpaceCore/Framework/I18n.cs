using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace SpaceCore.Framework
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

        /// <summary>Get a translation equivalent to "Custom Skill Page".</summary>
        public static string Config_CustomSkillPage_Name()
        {
            return I18n.GetByKey("config.custom-skill-page.name");
        }

        /// <summary>Get a translation equivalent to "Whether to show the custom skill page.\nThis will move the wallet so that there is room for more skills.".</summary>
        public static string Config_CustomSkillPage_Tooltip()
        {
            return I18n.GetByKey("config.custom-skill-page.tooltip");
        }

        /// <summary>Get a translation equivalent to "Support All Professions Mod".</summary>
        public static string Config_SupportAllProfessionsMod_Name()
        {
            return I18n.GetByKey("config.support-all-professions-mod.name");
        }

        /// <summary>Get a translation equivalent to "When All Professions is installed, whether to automatically apply custom professions registered through SpaceCore when their level is reached.".</summary>
        public static string Config_SupportAllProfessionsMod_Tooltip()
        {
            return I18n.GetByKey("config.support-all-professions-mod.tooltip");
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

