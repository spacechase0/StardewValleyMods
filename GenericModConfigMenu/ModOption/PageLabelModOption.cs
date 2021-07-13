using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class PageLabelModOption : BaseModOption
    {
        public string NewPage { get; }

        public override void SyncToMod()
        {
        }

        public override void Save()
        {
        }

        public PageLabelModOption(string name, string desc, string newPage, ModConfig mod)
            : base(name, desc, name, mod)
        {
            this.NewPage = newPage;
        }
    }
}
