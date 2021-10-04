using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace CookingSkill.Framework
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

        /// <summary>Get a translation equivalent to "Cooking".</summary>
        public static string Skill_Name()
        {
            return I18n.GetByKey("skill.name");
        }

        /// <summary>Get a translation equivalent to "+{{bonus}}% edibility in home-cooked foods".</summary>
        /// <param name="bonus">The value to inject for the <c>{{bonus}}</c> token.</param>
        public static string Skill_LevelUpPerk(object bonus)
        {
            return I18n.GetByKey("skill.level-up-perk", new { bonus });
        }

        /// <summary>Get a translation equivalent to "Gourmet".</summary>
        public static string Gourmet_Name()
        {
            return I18n.GetByKey("gourmet.name");
        }

        /// <summary>Get a translation equivalent to "+20% sell price".</summary>
        public static string Gourmet_Desc()
        {
            return I18n.GetByKey("gourmet.desc");
        }

        /// <summary>Get a translation equivalent to "Satisfying".</summary>
        public static string Satisfying_Name()
        {
            return I18n.GetByKey("satisfying.name");
        }

        /// <summary>Get a translation equivalent to "+25% buff duration once eaten".</summary>
        public static string Satisfying_Desc()
        {
            return I18n.GetByKey("satisfying.desc");
        }

        /// <summary>Get a translation equivalent to "Efficient".</summary>
        public static string Efficient_Name()
        {
            return I18n.GetByKey("efficient.name");
        }

        /// <summary>Get a translation equivalent to "15% chance to not consume ingredients".</summary>
        public static string Efficient_Desc()
        {
            return I18n.GetByKey("efficient.desc");
        }

        /// <summary>Get a translation equivalent to "Professional Chef".</summary>
        public static string ProfessionalChef_Name()
        {
            return I18n.GetByKey("professional-chef.name");
        }

        /// <summary>Get a translation equivalent to "Home-cooked meals are always at least silver".</summary>
        public static string ProfessionalChef_Desc()
        {
            return I18n.GetByKey("professional-chef.desc");
        }

        /// <summary>Get a translation equivalent to "Intense Flavors".</summary>
        public static string IntenseFlavors_Name()
        {
            return I18n.GetByKey("intense-flavors.name");
        }

        /// <summary>Get a translation equivalent to "Food buffs are one level stronger once eaten\n(+20% for max energy or magnetism)".</summary>
        public static string IntenseFlavors_Desc()
        {
            return I18n.GetByKey("intense-flavors.desc");
        }

        /// <summary>Get a translation equivalent to "Secret Spices".</summary>
        public static string SecretSpices_Name()
        {
            return I18n.GetByKey("secret-spices.name");
        }

        /// <summary>Get a translation equivalent to "Provides a few random buffs when eating unbuffed food".</summary>
        public static string SecretSpices_Desc()
        {
            return I18n.GetByKey("secret-spices.desc");
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

