using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
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
        /// <param name="name">The link text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the link, or <c>null</c> to disable the tooltip.</param>
        /// <param name="pageId">The unique ID of the page to open when the link is clicked.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public PageLinkModOption(string name, string tooltip, string pageId, ModConfig mod)
            : base(name, tooltip, mod)
        {
            this.PageId = pageId;
        }
    }
}
