using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class PageLinkModOption : ReadOnlyModOption
    {
        /*********
        ** Accessors
        *********/
        public string NewPage { get; }


        /*********
        ** Public methods
        *********/
        public PageLinkModOption(string name, string desc, string newPage, ModConfig mod)
            : base(name, desc, mod)
        {
            this.NewPage = newPage;
        }
    }
}
