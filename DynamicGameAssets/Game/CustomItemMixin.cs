using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Netcode;

namespace DynamicGameAssets.Game
{
    public partial class CustomItemMixin<TPackData> : IDGAItem where TPackData : CommonPackData, new()
    {
        public readonly NetString _sourcePack = new NetString();
        public readonly NetString _id = new NetString();

        [XmlIgnore]
        public string SourcePack => _sourcePack.Value;
        [XmlIgnore]
        public string Id => _id.Value;
        [XmlIgnore]
        public string FullId => $"{SourcePack}/{Id}";
        [XmlIgnore]
        public TPackData Data => Mod.Find( FullId ) as TPackData ?? new TPackData() { parent = Mod.DummyContentPack };

        public CustomItemMixin()
        {
            DoInit();
        }

        public CustomItemMixin( TPackData data )
        :   this()
        {
            _sourcePack.Value = data.parent.smapiPack.Manifest.UniqueID;
            _id.Value = data.ID;

            DoInit( data );
        }

        partial void DoInit();
        partial void DoInit( TPackData data );
    }
}
