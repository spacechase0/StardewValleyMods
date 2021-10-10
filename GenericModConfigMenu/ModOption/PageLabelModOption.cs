using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class PageLabelModOption : BaseModOption
    {
        /*********
        ** Accessors
        *********/
        public string NewPage { get; }


        /*********
        ** Public methods
        *********/
        public PageLabelModOption(string name, string desc, string newPage, ModConfig mod)
            : base(name, desc, name, mod)
        {
            this.NewPage = newPage;
        }

        /// <inheritdoc />
        public override void SyncToMod() { }

        /// <inheritdoc />
        public override void Save() { }
    }
}
