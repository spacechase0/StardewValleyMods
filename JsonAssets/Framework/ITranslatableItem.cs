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
    }
}
