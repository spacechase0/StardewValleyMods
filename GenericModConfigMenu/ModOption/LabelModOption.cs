using GenericModConfigMenu.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class LabelModOption : BaseModOption
    {
        /*********
        ** Public methods
        *********/
        public LabelModOption(string name, string desc, ModConfig mod)
            : base(name, desc, name, mod) { }

        /// <inheritdoc />
        public override void SyncToMod() { }

        /// <inheritdoc />
        public override void Save() { }
    }
}
