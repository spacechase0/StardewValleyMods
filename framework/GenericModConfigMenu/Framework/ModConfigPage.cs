using System;
using System.Collections.Generic;
using GenericModConfigMenu.Framework.ModOption;

namespace GenericModConfigMenu.Framework
{
    /// <summary>A page of options in a mod's config UI.</summary>
    internal class ModConfigPage
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique page ID.</summary>
        public string PageId { get; }

        /// <summary>The page title shown in its UI.</summary>
        public Func<string> PageTitle { get; private set; }

        /// <summary>The options on the page.</summary>
        public List<BaseModOption> Options { get; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="pageId">The unique page ID.</param>
        /// <param name="pageTitle">The page title shown in its UI, or <c>null</c> to show the <paramref name="pageId"/> value.</param>
        public ModConfigPage(string pageId, Func<string> pageTitle)
        {
            pageTitle ??= () => pageId;

            this.PageId = pageId;
            this.PageTitle = pageTitle;
        }

        /// <summary>Set the <see cref="PageTitle"/> value.</summary>
        /// <param name="value">The value to set.</param>
        [Obsolete("This is only intended to support backwards compatibility. Most code should set the value in the constructor instead.")]
        public void SetPageTitle(Func<string> value)
        {
            this.PageTitle = value;
        }
    }
}
