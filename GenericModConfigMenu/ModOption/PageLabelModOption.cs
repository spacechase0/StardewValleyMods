using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class PageLabelModOption : ReadOnlyModOption
    {
        /*********
        ** Accessors
        *********/
        public string NewPage { get; }


        /*********
        ** Public methods
        *********/
        public PageLabelModOption(string name, string desc, string newPage, ModConfig mod)
            : base(name, desc, mod)
        {
            this.NewPage = newPage;
        }
    }
}
