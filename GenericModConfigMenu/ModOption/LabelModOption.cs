using StardewModdingAPI;

namespace GenericModConfigMenu.ModOption
{
    internal class LabelModOption : BaseModOption
    {

        public override void SyncToMod()
        {
        }

        public override void Save()
        {
        }

        public LabelModOption(string name, string desc, IManifest mod)
            : base(name, desc, name, mod) { }
    }
}
