using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class ShirtPackData : CommonPackData
    {
        [JsonIgnore]
        public string Name => pack.smapiPack.Translation.Get( $"shirt.{ID}.name" );
        [JsonIgnore]
        public string Description => pack.smapiPack.Translation.Get( $"shirt.{ID}.description" );

        public string TextureMale { get; set; }
        [DefaultValue( null )]
        public string TextureMaleColor { get; set; }
        [DefaultValue( null )]
        public string TextureFemale { get; set; }
        [DefaultValue( null )]
        public string TextureFemaleColor { get; set; }

        public Color DefaultColor { get; set; } = Color.White;
        [DefaultValue( false )]
        public bool Dyeable { get; set; } = false;

        public bool ShouldSerializeDefaultColor() { return DefaultColor != Color.White; }

        [DefaultValue( false )]
        public bool Sleeveless { get; set; } = false;

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomShirt cshirt )
                {
                    if ( cshirt.SourcePack == pack.smapiPack.Manifest.UniqueID && cshirt.Id == ID )
                        return null;
                }
                return item;
            } );
        }

        public override Item ToItem()
        {
            return new CustomShirt( this );
        }

        public override TexturedRect GetTexture()
        {
            return pack.GetTexture( TextureMale, 8, 32 );
        }
    }
}
