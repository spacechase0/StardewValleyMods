using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class ShirtPackData : CommonPackData
    {
        public string Name => parent.smapiPack.Translation.Get( $"shirt.{ID}.name" );
        public string Description => parent.smapiPack.Translation.Get( $"shirt.{ID}.description" );

        public string TextureMale { get; set; }
        public string TextureMaleColor { get; set; }
        public string TextureFemale { get; set; }
        public string TextureFemaleColor { get; set; }

        public Color DefaultColor { get; set; } = Color.White;
        public bool Dyeable { get; set; } = false;

        public bool Sleeveless { get; set; } = false;

        public override void OnDisabled()
        {
            MyUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomShirt cshirt )
                {
                    if ( cshirt.SourcePack == parent.smapiPack.Manifest.UniqueID && cshirt.Id == ID )
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
            return parent.GetTexture( TextureMale, 8, 32 );
        }
    }
}
