using System;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders a sub-header in a config section.</summary>
    internal class SectionSubHeaderModOption : ReadOnlyModOption
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="text">The title text to show in the form.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public SectionSubHeaderModOption(Func<string> text, ModConfig mod)
            : base(text, null, mod) { }
    }
}
