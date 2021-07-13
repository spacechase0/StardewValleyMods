using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class ParagraphModOption : BaseModOption
    {

        public override void SyncToMod()
        {
        }

        public override void Save()
        {
        }

        public ParagraphModOption(string paragraph, ModConfig mod)
            : base(paragraph, "", paragraph, mod)
        {
        }
    }
}
