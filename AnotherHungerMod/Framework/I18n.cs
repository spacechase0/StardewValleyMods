using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace AnotherHungerMod.Framework
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

        /// <summary>Get a translation equivalent to "Fullness".</summary>
        public static string Buff_Full()
        {
            return I18n.GetByKey("buff.full");
        }

        /// <summary>Get a translation equivalent to "Hungry".</summary>
        public static string Buff_Hungry()
        {
            return I18n.GetByKey("buff.hungry");
        }

        /// <summary>Get a translation equivalent to "Fullness UI (X)".</summary>
        public static string Config_FullnessUiX_Name()
        {
            return I18n.GetByKey("config.fullness-ui-X.name");
        }

        /// <summary>Get a translation equivalent to "The X position of the fullness UI.".</summary>
        public static string Config_FullnessUiX_Tooltip()
        {
            return I18n.GetByKey("config.fullness-ui-X.tooltip");
        }

        /// <summary>Get a translation equivalent to "Fullness UI (Y)".</summary>
        public static string Config_FullnessUiY_Name()
        {
            return I18n.GetByKey("config.fullness-ui-Y.name");
        }

        /// <summary>Get a translation equivalent to "The Y position of the fullness UI.".</summary>
        public static string Config_FullnessUiY_Tooltip()
        {
            return I18n.GetByKey("config.fullness-ui-Y.tooltip");
        }

        /// <summary>Get a translation equivalent to "Max Fullness".</summary>
        public static string Config_FullnessMax_Name()
        {
            return I18n.GetByKey("config.fullness-max.name");
        }

        /// <summary>Get a translation equivalent to "The maximum amount of fullness you can have.".</summary>
        public static string Config_FullnessMax_Tooltip()
        {
            return I18n.GetByKey("config.fullness-max.tooltip");
        }

        /// <summary>Get a translation equivalent to "Fullness Drain".</summary>
        public static string Config_FullnessDrain_Name()
        {
            return I18n.GetByKey("config.fullness-drain.name");
        }

        /// <summary>Get a translation equivalent to "The amount of fullness to drain per in-game minute.".</summary>
        public static string Config_FullnessDrain_Tooltip()
        {
            return I18n.GetByKey("config.fullness-drain.tooltip");
        }

        /// <summary>Get a translation equivalent to "Edibility Multiplier".</summary>
        public static string Config_EdibilityMultiplier_Name()
        {
            return I18n.GetByKey("config.edibility-multiplier.name");
        }

        /// <summary>Get a translation equivalent to "A multiplier for the amount of fullness you get, based on the food's edibility.".</summary>
        public static string Config_EdibilityMultiplier_Tooltip()
        {
            return I18n.GetByKey("config.edibility-multiplier.tooltip");
        }

        /// <summary>Get a translation equivalent to "Positive Buff Threshold".</summary>
        public static string Config_PositiveBuffThreshold_Name()
        {
            return I18n.GetByKey("config.positive-buff-threshold.name");
        }

        /// <summary>Get a translation equivalent to "The minimum fullness needed for positive buffs to apply.".</summary>
        public static string Config_PositiveBuffThreshold_Tooltip()
        {
            return I18n.GetByKey("config.positive-buff-threshold.tooltip");
        }

        /// <summary>Get a translation equivalent to "Negative Buff Threshold".</summary>
        public static string Config_NegativeBuffThreshold_Name()
        {
            return I18n.GetByKey("config.negative-buff-threshold.name");
        }

        /// <summary>Get a translation equivalent to "The maximum fullness before negative buffs apply.".</summary>
        public static string Config_NegativeBuffThreshold_Tooltip()
        {
            return I18n.GetByKey("config.negative-buff-threshold.tooltip");
        }

        /// <summary>Get a translation equivalent to "Starvation Damage".</summary>
        public static string Config_StarvationDamage_Name()
        {
            return I18n.GetByKey("config.starvation-damage.name");
        }

        /// <summary>Get a translation equivalent to "The amount of starvation damage taken every in-game minute when you have no fullness.".</summary>
        public static string Config_StarvationDamage_Tooltip()
        {
            return I18n.GetByKey("config.starvation-damage.tooltip");
        }

        /// <summary>Get a translation equivalent to "Unfed Spouse Penalty".</summary>
        public static string Config_UnfedSpousePenalty_Name()
        {
            return I18n.GetByKey("config.unfed-spouse-penalty.name");
        }

        /// <summary>Get a translation equivalent to "The relationship points penalty for not feeding your spouse.".</summary>
        public static string Config_UnfedSpousePenalty_Tooltip()
        {
            return I18n.GetByKey("config.unfed-spouse-penalty.tooltip");
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

