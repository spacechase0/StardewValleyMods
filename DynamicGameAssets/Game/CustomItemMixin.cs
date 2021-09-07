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
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public TPackData Data => Mod.Find(this.FullId ) as TPackData ?? new TPackData() { pack = Mod.DummyContentPack };

        public CustomItemMixin()
        {
            this.DoInit();
        }

        public CustomItemMixin( TPackData data )
        :   this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;

            this.DoInit( data );
        }

        partial void DoInit();
        partial void DoInit( TPackData data );
    }
}
