using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public PageLabelModOption( string name, string desc, string newPage, IManifest mod )
        :   base( name, desc, name, mod )
        {
            NewPage = newPage;
        }
    }
}
