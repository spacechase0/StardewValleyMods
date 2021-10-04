using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace LuckSkill.Framework
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

        /// <summary>Get a translation equivalent to "Luck".</summary>
        public static string Skill_Name()
        {
            return I18n.GetByKey("skill.name");
        }

        /// <summary>Get a translation equivalent to "Luck Increased".</summary>
        public static string Skill_LevelUp()
        {
            return I18n.GetByKey("skill.level-up");
        }

        /// <summary>Get a translation equivalent to "Fortunate".</summary>
        public static string Fortunate_Name()
        {
            return I18n.GetByKey("fortunate.name");
        }

        /// <summary>Get a translation equivalent to "Fortunate".</summary>
        public static string Fortunate_Desc()
        {
            return I18n.GetByKey("fortunate.desc");
        }

        /// <summary>Get a translation equivalent to "Popular Helper".</summary>
        public static string PopularHelper_Name()
        {
            return I18n.GetByKey("popular-helper.name");
        }

        /// <summary>Get a translation equivalent to "Daily quests occur three times as often.".</summary>
        public static string PopularHelper_Desc()
        {
            return I18n.GetByKey("popular-helper.desc");
        }

        /// <summary>Get a translation equivalent to "Lucky".</summary>
        public static string Lucky_Name()
        {
            return I18n.GetByKey("lucky.name");
        }

        /// <summary>Get a translation equivalent to "20% chance for max daily luck.".</summary>
        public static string Lucky_Desc()
        {
            return I18n.GetByKey("lucky.desc");
        }

        /// <summary>Get a translation equivalent to "Un-unlucky".</summary>
        public static string UnUnlucky_Name()
        {
            return I18n.GetByKey("un-unlucky.name");
        }

        /// <summary>Get a translation equivalent to "Never have bad luck.".</summary>
        public static string UnUnlucky_Desc()
        {
            return I18n.GetByKey("un-unlucky.desc");
        }

        /// <summary>Get a translation equivalent to "Shooting Star".</summary>
        public static string ShootingStar_Name()
        {
            return I18n.GetByKey("shooting-star.name");
        }

        /// <summary>Get a translation equivalent to "Nightly events occur twice as often.".</summary>
        public static string ShootingStar_Desc()
        {
            return I18n.GetByKey("shooting-star.desc");
        }

        /// <summary>Get a translation equivalent to "Spirit Child".</summary>
        public static string SpiritChild_Name()
        {
            return I18n.GetByKey("spirit-child.name");
        }

        /// <summary>Get a translation equivalent to "Giving gifts makes junimos happy. They might help your farm.\n(15% chance for some form of farm advancement.)".</summary>
        public static string SpiritChild_Desc()
        {
            return I18n.GetByKey("spirit-child.desc");
        }

        /// <summary>Get a translation equivalent to "The junimos gave you a prismatic shard!".</summary>
        public static string JunimoRewards_PrismaticShard()
        {
            return I18n.GetByKey("junimo-rewards.prismatic-shard");
        }

        /// <summary>Get a translation equivalent to "The junimos advanced your crops!".</summary>
        public static string JunimoRewards_GrowCrops()
        {
            return I18n.GetByKey("junimo-rewards.grow-crops");
        }

        /// <summary>Get a translation equivalent to "The junimos made some of your animals more fond of you!".</summary>
        public static string JunimoRewards_AnimalFriendship()
        {
            return I18n.GetByKey("junimo-rewards.animal-friendship");
        }

        /// <summary>Get a translation equivalent to "The junimos grew your grass and repaired your fences!".</summary>
        public static string JunimoRewards_GrowGrass()
        {
            return I18n.GetByKey("junimo-rewards.grow-grass");
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

