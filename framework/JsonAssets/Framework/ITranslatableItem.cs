using System.Collections.Generic;

namespace JsonAssets.Framework
{
    /// <summary>An item whose display text can be translated.</summary>
    internal interface ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The default item name.</summary>
        string Name { get; set; }

        /// <summary>The default item description.</summary>
        string Description { get; set; }

        /// <summary>The item name translations by language code.</summary>
        Dictionary<string, string> NameLocalization { get; }

        /// <summary>The item description translations by language code.</summary>
        Dictionary<string, string> DescriptionLocalization { get; }

        /// <summary>An optional translation key in the content pack's <c>i18n</c> files, used to populate the <see cref="NameLocalization"/> (<c>{{key}}.name</c>) and <see cref="DescriptionLocalization"/> (<c>{{key}}.description</c>) fields. Translations from <c>i18n/default.json</c> (if any) will overwrite <see cref="Name"/> and <see cref="Description"/>.</summary>
        public string TranslationKey { get; set; }
    }
}
