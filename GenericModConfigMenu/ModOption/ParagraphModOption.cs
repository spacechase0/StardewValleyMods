using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class ParagraphModOption : BaseModOption
    {
        /*********
        ** Public methods
        *********/
        public ParagraphModOption(string paragraph, ModConfig mod)
            : base(paragraph, "", paragraph, mod) { }

        /// <inheritdoc />
        public override void SyncToMod() { }

        /// <inheritdoc />
        public override void Save() { }
    }
}
