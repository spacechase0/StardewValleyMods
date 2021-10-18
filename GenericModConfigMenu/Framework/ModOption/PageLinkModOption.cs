using System;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a link to another page.</summary>
    internal class PageLinkModOption : ReadOnlyModOption
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique ID of the page to open when the link is clicked.</summary>
        public string PageId { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="pageId">The unique ID of the page to open when the link is clicked.</param>
        /// <param name="text">The link text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the link, or <c>null</c> to disable the tooltip.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public PageLinkModOption(string pageId, Func<string> text, Func<string> tooltip, ModConfig mod)
            : base(text, tooltip, mod)
        {
            this.PageId = pageId;
        }
    }
}
