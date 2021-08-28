using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class HatPackData : CommonPackData
    {
        public enum HairStyleType
        {
            Full = 0,
            Obscured = 1,
            Hide = 2,
        }

        [JsonIgnore]
        public string Name => parent.smapiPack.Translation.Get( $"hat.{ID}.name" );
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get( $"hat.{ID}.description" );


        public string Texture { get; set; }
        [DefaultValue( HairStyleType.Full )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public HairStyleType HairStyle { get; set; }
        [DefaultValue( false )]
        public bool IgnoreHairstyleOffset { get; set; }

        public override TexturedRect GetTexture()
        {
            return parent.GetTexture( Texture, 20, 80 );
        }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomHat chat )
                {
                    if ( chat.SourcePack == parent.smapiPack.Manifest.UniqueID && chat.Id == ID )
                        return null;
                }
                return item;
            } );
        }

        public override Item ToItem()
        {
            return new CustomHat( this );
        }
    }
}
