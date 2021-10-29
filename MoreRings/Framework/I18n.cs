using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace MoreRings.Framework
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

        /// <summary>Get a translation equivalent to "Quality+ Ring".</summary>
        public static string QualityRing_Name()
        {
            return I18n.GetByKey("quality-ring.name");
        }

        /// <summary>Get a translation equivalent to "A chance for higher quality crop harvests.".</summary>
        public static string QualityRing_Description()
        {
            return I18n.GetByKey("quality-ring.description");
        }

        /// <summary>Get a translation equivalent to "Refreshing Ring".</summary>
        public static string RefreshingRing_Name()
        {
            return I18n.GetByKey("refreshing-ring.name");
        }

        /// <summary>Get a translation equivalent to "I wonder what it does.".</summary>
        public static string RefreshingRing_Description()
        {
            return I18n.GetByKey("refreshing-ring.description");
        }

        /// <summary>Get a translation equivalent to "Ring of Regeneration".</summary>
        public static string RingOfRegeneration_Name()
        {
            return I18n.GetByKey("ring-of-regeneration.name");
        }

        /// <summary>Get a translation equivalent to "Slowly heal over time.".</summary>
        public static string RingOfRegeneration_Description()
        {
            return I18n.GetByKey("ring-of-regeneration.description");
        }

        /// <summary>Get a translation equivalent to "Ring of Diamond Booze".</summary>
        public static string RingOfDiamondBooze_Name()
        {
            return I18n.GetByKey("ring-of-diamond-booze.name");
        }

        /// <summary>Get a translation equivalent to "Negates the tipsy effect of lighter alcohol.".</summary>
        public static string RingOfDiamondBooze_Description()
        {
            return I18n.GetByKey("ring-of-diamond-booze.description");
        }

        /// <summary>Get a translation equivalent to "Ring of Wide Nets".</summary>
        public static string RingOfWideNets_Name()
        {
            return I18n.GetByKey("ring-of-wide-nets.name");
        }

        /// <summary>Get a translation equivalent to "Increases fishing bar size by 50%.".</summary>
        public static string RingOfWideNets_Description()
        {
            return I18n.GetByKey("ring-of-wide-nets.description");
        }

        /// <summary>Get a translation equivalent to "Ring of Far Reaching".</summary>
        public static string RingOfFarReaching_Name()
        {
            return I18n.GetByKey("ring-of-far-reaching.name");
        }

        /// <summary>Get a translation equivalent to "Use tools at a distance.".</summary>
        public static string RingOfFarReaching_Description()
        {
            return I18n.GetByKey("ring-of-far-reaching.description");
        }

        /// <summary>Get a translation equivalent to "Ring of True Sight".</summary>
        public static string RingOfTrueSight_Name()
        {
            return I18n.GetByKey("ring-of-true-sight.name");
        }

        /// <summary>Get a translation equivalent to "See beyond what you can see.".</summary>
        public static string RingOfTrueSight_Description()
        {
            return I18n.GetByKey("ring-of-true-sight.description");
        }

        /// <summary>Get a translation equivalent to "Quality+ Ring: Chance".</summary>
        public static string Config_QualityRingChance_Name()
        {
            return I18n.GetByKey("config.quality-ring-chance.name");
        }

        /// <summary>Get a translation equivalent to "The percentage chance of a higher crop quality with the Quality+ Ring equipped.".</summary>
        public static string Config_QualityRingChance_Description()
        {
            return I18n.GetByKey("config.quality-ring-chance.description");
        }

        /// <summary>Get a translation equivalent to "Ring of Wide Nets: Multiplier".</summary>
        public static string Config_RingOfWideNetsMultiplier_Name()
        {
            return I18n.GetByKey("config.ring-of-wide-nets-multiplier.name");
        }

        /// <summary>Get a translation equivalent to "The multiplier applied to the fishing bar size with the Ring of Wide Nets equipped.".</summary>
        public static string Config_RingOfWideNetsMultiplier_Description()
        {
            return I18n.GetByKey("config.ring-of-wide-nets-multiplier.description");
        }

        /// <summary>Get a translation equivalent to "Ring of Regeneration: Regen".</summary>
        public static string Config_RingOfRegenerationRate_Name()
        {
            return I18n.GetByKey("config.ring-of-regeneration-rate.name");
        }

        /// <summary>Get a translation equivalent to "The health regen rate per second with the Ring of Regeneration equipped.".</summary>
        public static string Config_RingOfRegenerationRate_Description()
        {
            return I18n.GetByKey("config.ring-of-regeneration-rate.description");
        }

        /// <summary>Get a translation equivalent to "Refreshing Ring: Regen".</summary>
        public static string Config_RefreshingRingRate_Name()
        {
            return I18n.GetByKey("config.refreshing-ring-rate.name");
        }

        /// <summary>Get a translation equivalent to "The stamina regen rate per second with the Refreshing Ring equipped.".</summary>
        public static string Config_RefreshingRingRate_Description()
        {
            return I18n.GetByKey("config.refreshing-ring-rate.description");
        }

        /// <summary>Get a translation equivalent to "Ring of Far Reaching: Distance".</summary>
        public static string Config_RingOfFarReachingDistance_Name()
        {
            return I18n.GetByKey("config.ring-of-far-reaching-distance.name");
        }

        /// <summary>Get a translation equivalent to "The distance in tiles at which you can use tools with the Ring of Far Reaching equipped.".</summary>
        public static string Config_RingOfFarReachingDistance_Description()
        {
            return I18n.GetByKey("config.ring-of-far-reaching-distance.description");
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

