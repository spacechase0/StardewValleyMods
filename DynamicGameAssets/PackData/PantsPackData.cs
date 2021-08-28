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
    public class PantsPackData : CommonPackData
    {
        [JsonIgnore]
        public string Name => parent.smapiPack.Translation.Get( $"pants.{ID}.name" );
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get( $"pants.{ID}.description" );

        public string Texture { get; set; }

        public Color DefaultColor { get; set; } = Color.White;
        [DefaultValue( false )]
        public bool Dyeable { get; set; } = false;

        public bool ShouldSerializeDefaultColor() { return DefaultColor != Color.White; }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomPants cpants )
                {
                    if ( cpants.SourcePack == parent.smapiPack.Manifest.UniqueID && cpants.Id == ID )
                        return null;
                }
                return item;
            } );
        }

        public override Item ToItem()
        {
            return new CustomPants( this );
        }

        public override TexturedRect GetTexture()
        {
            var ret = parent.GetTexture( Texture, 192, 688 );
            ret.Rect ??= new Rectangle( 0, 0, ret.Texture.Width, ret.Texture.Height );
            ret.Rect = new Rectangle( ret.Rect.Value.X, ret.Rect.Value.Y + 672, 16, 16 );
            return ret;
        }
    }
}
