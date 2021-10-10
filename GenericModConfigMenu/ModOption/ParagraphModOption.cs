using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    /// <summary>A mod option which renders a paragraph of text.</summary>
    internal class ParagraphModOption : ReadOnlyModOption
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="paragraph">The paragraph text to show in the form.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public ParagraphModOption(string paragraph, ModConfig mod)
            : base(paragraph, "", mod) { }
    }
}
