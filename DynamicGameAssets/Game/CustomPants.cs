using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAPants")]
    [Mixin( typeof( CustomItemMixin<PantsPackData> ) )]
    public partial class CustomPants : Clothing
    {
        partial void DoInit()
        {
            base.NetFields.AddFields( _sourcePack, _id );
        }

        partial void DoInit( PantsPackData data )
        {
            this.dyeable.Value = data.Dyeable;
            this.clothesType.Value = ( int ) ClothesType.PANTS;
            this.clothesColor.Value = data.DefaultColor;

            this.indexInTileSheetMale.Value = FullId.GetDeterministicHashCode();
        }

        public override string DisplayName { get => Data.Name; set { } }

        public override string getDescription()
        {
            return Game1.parseText( Data.Description, Game1.smallFont, this.getDescriptionWidth() );
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            Color clothes_color = this.clothesColor;
            if ( this.isPrismatic.Value )
            {
                clothes_color = Utility.GetPrismaticColor();
            }

            var currTex = Data.GetTexture();

            spriteBatch.Draw( currTex.Texture, location + new Vector2( 32f, 32f ), currTex.Rect, Utility.MultiplyColor( clothes_color, color ) * transparency, 0f, new Vector2( 8f, 8f ), scaleSize * 4f, SpriteEffects.None, layerDepth );
        }

        public override Item getOne()
        {
            var ret = new CustomPants( Data );
            ret.clothesColor.Value = clothesColor.Value;
            ret._GetOneFrom( this );
            return ret;
        }
    }
}
