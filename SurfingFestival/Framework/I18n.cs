using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace SurfingFestival.Framework
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

        /// <summary>Get a translation equivalent to "Laps: {{laps}}/2".</summary>
        /// <param name="laps">The value to inject for the <c>{{laps}}</c> token.</param>
        public static string Ui_Laps(object laps)
        {
            return I18n.GetByKey("ui.laps", new { laps });
        }

        /// <summary>Get a translation equivalent to "Ranking".</summary>
        public static string Ui_Ranking()
        {
            return I18n.GetByKey("ui.ranking");
        }

        /// <summary>Get a translation equivalent to "Add some wood to the bonfire".</summary>
        public static string Ui_Wood()
        {
            return I18n.GetByKey("ui.wood");
        }

        /// <summary>Get a translation equivalent to "You placed 50 wood on the bonfire.".</summary>
        public static string Dialog_Wood()
        {
            return I18n.GetByKey("dialog.wood");
        }

        /// <summary>Get a translation equivalent to "...That's odd. Why did the fire turn purple?".</summary>
        public static string Dialog_Shorts()
        {
            return I18n.GetByKey("dialog.shorts");
        }

        /// <summary>Get a translation equivalent to "You received 1500g.".</summary>
        public static string Dialog_PrizeMoney()
        {
            return I18n.GetByKey("dialog.prize-money");
        }

        /// <summary>Get a translation equivalent to "Stardrop Boost".</summary>
        public static string Item_Boost()
        {
            return I18n.GetByKey("item.boost");
        }

        /// <summary>Get a translation equivalent to "Pufferfish Projectile".</summary>
        public static string Item_HomingProjectile()
        {
            return I18n.GetByKey("item.homing-projectile");
        }

        /// <summary>Get a translation equivalent to "Seeking Cloud".</summary>
        public static string Item_FirstPlaceProjectile()
        {
            return I18n.GetByKey("item.first-place-projectile");
        }

        /// <summary>Get a translation equivalent to "Junimo Power".</summary>
        public static string Item_Invincibility()
        {
            return I18n.GetByKey("item.invincibility");
        }

        /// <summary>Get a translation equivalent to "A sign reads: Want to get ahead, now and in the future? Make an offer.".</summary>
        public static string Secret_Text()
        {
            return I18n.GetByKey("secret.text");
        }

        /// <summary>Get a translation equivalent to "Throw 100,000g into the box?".</summary>
        public static string Secret_Yes()
        {
            return I18n.GetByKey("secret.yes");
        }

        /// <summary>Get a translation equivalent to "Leave".</summary>
        public static string Secret_No()
        {
            return I18n.GetByKey("secret.no");
        }

        /// <summary>Get a translation equivalent to "You can't afford that.".</summary>
        public static string Secret_Broke()
        {
            return I18n.GetByKey("secret.broke");
        }

        /// <summary>Get a translation equivalent to "You feel different somehow.".</summary>
        public static string Secret_Purchased()
        {
            return I18n.GetByKey("secret.purchased");
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

