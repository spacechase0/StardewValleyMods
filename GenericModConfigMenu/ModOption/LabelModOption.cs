using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class LabelModOption : ReadOnlyModOption
    {
        /*********
        ** Public methods
        *********/
        public LabelModOption(string name, string desc, ModConfig mod)
            : base(name, desc, mod) { }
    }
}
