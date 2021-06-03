using StardewModdingAPI;

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

        public ParagraphModOption( string paragraph, IManifest mod )
        :   base( paragraph, "", paragraph, mod )
        {
        }
    }
}
