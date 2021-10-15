using System;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a section title.</summary>
    internal class SectionTitleModOption : ReadOnlyModOption
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="text">The title text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public SectionTitleModOption(Func<string> text, Func<string> tooltip, ModConfig mod)
            : base(text, tooltip, mod) { }
    }
}
