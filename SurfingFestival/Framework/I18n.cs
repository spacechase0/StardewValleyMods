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

        /// <summary>Get a translation equivalent to "Brown Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardBrown_Name()
        {
            return I18n.GetByKey("craftables.surfboard-brown.name");
        }

        /// <summary>Get a translation equivalent to "A brown surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardBrown_Description()
        {
            return I18n.GetByKey("craftables.surfboard-brown.description");
        }

        /// <summary>Get a translation equivalent to "Cool Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardCool_Name()
        {
            return I18n.GetByKey("craftables.surfboard-cool.name");
        }

        /// <summary>Get a translation equivalent to "A surfboard with a cool, bright green pattern on it that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardCool_Description()
        {
            return I18n.GetByKey("craftables.surfboard-cool.description");
        }

        /// <summary>Get a translation equivalent to "Hibiscus Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardHibiscus_Name()
        {
            return I18n.GetByKey("craftables.surfboard-hibiscus.name");
        }

        /// <summary>Get a translation equivalent to "A pink and yellow surfboard with hibiscus on it that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardHibiscus_Description()
        {
            return I18n.GetByKey("craftables.surfboard-hibiscus.description");
        }

        /// <summary>Get a translation equivalent to "Red Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardRed_Name()
        {
            return I18n.GetByKey("craftables.surfboard-red.name");
        }

        /// <summary>Get a translation equivalent to "A red surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardRed_Description()
        {
            return I18n.GetByKey("craftables.surfboard-red.description");
        }

        /// <summary>Get a translation equivalent to "Stripe Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardStripe_Name()
        {
            return I18n.GetByKey("craftables.surfboard-stripe.name");
        }

        /// <summary>Get a translation equivalent to "A blue stiped surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardStripe_Description()
        {
            return I18n.GetByKey("craftables.surfboard-stripe.description");
        }

        /// <summary>Get a translation equivalent to "White Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardWhite_Name()
        {
            return I18n.GetByKey("craftables.surfboard-white.name");
        }

        /// <summary>Get a translation equivalent to "A white surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardWhite_Description()
        {
            return I18n.GetByKey("craftables.surfboard-white.description");
        }

        /// <summary>Get a translation equivalent to "Surfing Trophy".</summary>
        public static string Craftables_SurfingTrophy_Name()
        {
            return I18n.GetByKey("craftables.surfing-trophy.name");
        }

        /// <summary>Get a translation equivalent to "A trophy for winning the surfing festival.".</summary>
        public static string Craftables_SurfingTrophy_Description()
        {
            return I18n.GetByKey("craftables.surfing-trophy.description");
        }

        /// <summary>Get a translation equivalent to "Black Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskBlack_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-black.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskBlack_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-black.description");
        }

        /// <summary>Get a translation equivalent to "Blue Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskBlue_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-blue.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskBlue_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-blue.description");
        }

        /// <summary>Get a translation equivalent to "Green Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskGreen_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-green.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskGreen_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-green.description");
        }

        /// <summary>Get a translation equivalent to "Orange Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskOrange_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-orange.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskOrange_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-orange.description");
        }

        /// <summary>Get a translation equivalent to "Pink Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskPink_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-pink.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskPink_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-pink.description");
        }

        /// <summary>Get a translation equivalent to "Red Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskRed_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-red.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskRed_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-red.description");
        }

        /// <summary>Get a translation equivalent to "Cheap Amulet?".</summary>
        public static string Items_CheapAmulet_Name()
        {
            return I18n.GetByKey("items.cheap-amulet.name");
        }

        /// <summary>Get a translation equivalent to "Something seems off about this amulet. Wonder why it's so cheap.".</summary>
        public static string Items_CheapAmulet_Description()
        {
            return I18n.GetByKey("items.cheap-amulet.description");
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

