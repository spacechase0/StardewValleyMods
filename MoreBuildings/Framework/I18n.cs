using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace MoreBuildings.Framework
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

        /// <summary>Get a translation equivalent to "Spooky Shed".</summary>
        public static string SpookyShed_Name()
        {
            return I18n.GetByKey("spooky-shed.name");
        }

        /// <summary>Get a translation equivalent to "An empty building. But spooky, too.".</summary>
        public static string SpookyShed_Description()
        {
            return I18n.GetByKey("spooky-shed.description");
        }

        /// <summary>Get a translation equivalent to "Fishing Shack".</summary>
        public static string FishingShack_Name()
        {
            return I18n.GetByKey("fishing-shack.name");
        }

        /// <summary>Get a translation equivalent to "A shack for fishing.".</summary>
        public static string FishingShack_Description()
        {
            return I18n.GetByKey("fishing-shack.description");
        }

        /// <summary>Get a translation equivalent to "Mini Spa".</summary>
        public static string MiniSpa_Name()
        {
            return I18n.GetByKey("mini-spa.name");
        }

        /// <summary>Get a translation equivalent to "A place to relax and recharge.".</summary>
        public static string MiniSpa_Description()
        {
            return I18n.GetByKey("mini-spa.description");
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

