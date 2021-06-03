using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
