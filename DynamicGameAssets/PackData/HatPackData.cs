using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
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

        public string Name => parent.smapiPack.Translation.Get( $"hat.{ID}.name" );
        public string Description => parent.smapiPack.Translation.Get( $"hat.{ID}.description" );


        public string Texture { get; set; }
        public HairStyleType HairStyle { get; set; }
        public bool IgnoreHairstyleOffset { get; set; }

        public override TexturedRect GetTexture()
        {
            return parent.GetTexture( Texture, 20, 80 );
        }

        public override void OnDisabled()
        {
            MyUtility.iterateAllItems( ( item ) =>
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
