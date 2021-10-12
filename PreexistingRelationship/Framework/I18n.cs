using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace PreexistingRelationship.Framework
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

        /// <summary>Get a translation equivalent to "Preexisting Relationship".</summary>
        public static string Menu_Title()
        {
            return I18n.GetByKey("menu.title");
        }

        /// <summary>Get a translation equivalent to "You've been talking to someone for a while, \nand finally moved to the valley to be with them... \nWho is it?".</summary>
        public static string Menu_Text()
        {
            return I18n.GetByKey("menu.text");
        }

        /// <summary>Get a translation equivalent to "Accept".</summary>
        public static string Menu_Button_Accept()
        {
            return I18n.GetByKey("menu.button.accept");
        }

        /// <summary>Get a translation equivalent to "Cancel".</summary>
        public static string Menu_Button_Cancel()
        {
            return I18n.GetByKey("menu.button.cancel");
        }

        /// <summary>Get a translation equivalent to "Your spouse has moved in!".</summary>
        public static string Married()
        {
            return I18n.GetByKey("married");
        }

        /// <summary>Get a translation equivalent to "This NPC is already married.".</summary>
        public static string SpouseTaken()
        {
            return I18n.GetByKey("spouse-taken");
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

