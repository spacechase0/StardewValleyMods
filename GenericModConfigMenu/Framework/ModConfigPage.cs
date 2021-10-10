using System;
using System.Collections.Generic;
using GenericModConfigMenu.ModOption;

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
        public string PageTitle { get; set; }

        /// <summary>The callbacks to invoke when an option value changes.</summary>
        public List<Action<string, object>> ChangeHandler { get; } = new();

        /// <summary>The options on the page.</summary>
        public List<BaseModOption> Options { get; set; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="pageId">The unique page ID.</param>
        public ModConfigPage(string pageId)
        {
            this.PageId = pageId;
            this.PageTitle = this.PageId;
        }
    }
}
