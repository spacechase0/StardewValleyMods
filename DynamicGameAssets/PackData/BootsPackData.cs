using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Tools;

namespace DynamicGameAssets.PackData
{
    public class BootsPackData : CommonPackData
    {
        [JsonIgnore]
        public string Name => parent.smapiPack.Translation.Get( $"boots.{ID}.name" );
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get( $"boots.{ID}.description" );

        public string Texture { get; set; }
        public string FarmerColors { get; set; }

        [DefaultValue( 0 )]
        public int Defense { get; set; }
        [DefaultValue( 0 )]
        public int Immunity { get; set; }

        [DefaultValue(0)]
        public int SellPrice { get; set; }


        public override TexturedRect GetTexture()
        {
            return parent.GetTexture( Texture, 16, 16 );
        }

        public override void OnDisabled()
        {
            MyUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomBoots cboots )
                {
                    if ( cboots.SourcePack == parent.smapiPack.Manifest.UniqueID && cboots.Id == ID )
                        return null;
                }
                return item;
            } );
        }

        public override Item ToItem()
        {
            return new CustomBoots( this );
        }
    }
}
