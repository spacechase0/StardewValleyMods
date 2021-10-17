using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace RushOrders.Framework
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

        /// <summary>Get a translation equivalent to "         *RUSH*".</summary>
        public static string Clint_Rush_NameSuffix()
        {
            return I18n.GetByKey("clint.rush.name-suffix");
        }

        /// <summary>Get a translation equivalent to "The tool will take one day to upgrade.".</summary>
        public static string Clint_Rush_Description()
        {
            return I18n.GetByKey("clint.rush.description");
        }

        /// <summary>Get a translation equivalent to "Thanks. I'll get started right away. It should be ready tomorrow.".</summary>
        public static string Clint_Rush_Dialogue()
        {
            return I18n.GetByKey("clint.rush.dialogue");
        }

        /// <summary>Get a translation equivalent to "       =INSTANT=".</summary>
        public static string Clint_Instant_NameSuffix()
        {
            return I18n.GetByKey("clint.instant.name-suffix");
        }

        /// <summary>Get a translation equivalent to "The tool will be immediately upgraded.".</summary>
        public static string Clint_Instant_Description()
        {
            return I18n.GetByKey("clint.instant.description");
        }

        /// <summary>Get a translation equivalent to "Thanks. I'll get started right away. It should be ready in a few minutes.".</summary>
        public static string Clint_Instant_Dialogue()
        {
            return I18n.GetByKey("clint.instant.dialogue");
        }

        /// <summary>Get a translation equivalent to "Rush your building construction? ({{daysLeft}} days left.)".</summary>
        /// <param name="daysLeft">The value to inject for the <c>{{daysLeft}}</c> token.</param>
        public static string Robin_RushQuestion(object daysLeft)
        {
            return I18n.GetByKey("robin.rush-question", new { daysLeft });
        }

        /// <summary>Get a translation equivalent to "Yes ({{price}}g)".</summary>
        /// <param name="price">The value to inject for the <c>{{price}}</c> token.</param>
        public static string Robin_RushAnswerYes(object price)
        {
            return I18n.GetByKey("robin.rush-answer-yes", new { price });
        }

        /// <summary>Get a translation equivalent to "No".</summary>
        public static string Robin_RushAnswerNo()
        {
            return I18n.GetByKey("robin.rush-answer-no");
        }

        /// <summary>Get a translation equivalent to "You do not have enough money.".</summary>
        public static string Robin_NotEnoughMoney()
        {
            return I18n.GetByKey("robin.not-enough-money");
        }

        /// <summary>Get a translation equivalent to "Price: Tool - One Day".</summary>
        public static string Config_PriceToolOneDay_Name()
        {
            return I18n.GetByKey("config.price-tool-one-day.name");
        }

        /// <summary>Get a translation equivalent to "The price multiplier for a one-day tool upgrade.".</summary>
        public static string Config_PriceToolOneDay_Tooltip()
        {
            return I18n.GetByKey("config.price-tool-one-day.tooltip");
        }

        /// <summary>Get a translation equivalent to "Price: Tool - Instant".</summary>
        public static string Config_PriceToolInstant_Name()
        {
            return I18n.GetByKey("config.price-tool-instant.name");
        }

        /// <summary>Get a translation equivalent to "The price multiplier for an instant upgrade.".</summary>
        public static string Config_PriceToolInstant_Tooltip()
        {
            return I18n.GetByKey("config.price-tool-instant.tooltip");
        }

        /// <summary>Get a translation equivalent to "Price: Building - Accelerate".</summary>
        public static string Config_PriceBuilding_Name()
        {
            return I18n.GetByKey("config.price-building.name");
        }

        /// <summary>Get a translation equivalent to "The price multiplier to accelerate building construction by one day.".</summary>
        public static string Config_PriceBuilding_Tooltip()
        {
            return I18n.GetByKey("config.price-building.tooltip");
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

