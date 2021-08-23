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
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAShirt")]
    public class CustomShirt : Clothing, IDGAItem
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
        public ShirtPackData Data => Mod.Find( FullId ) as ShirtPackData;

        public CustomShirt()
        {
            base.NetFields.AddFields( _sourcePack, _id );
        }

        public CustomShirt( ShirtPackData data )
        :   this()
        {
            _sourcePack.Value = data.parent.smapiPack.Manifest.UniqueID;
            _id.Value = data.ID;

            this.dyeable.Value = data.Dyeable;
            this.clothesType.Value = ( int ) ClothesType.SHIRT;
            if ( data.Sleeveless )
                this.otherData.Value = "Sleeveless";
            this.clothesColor.Value = data.DefaultColor;

            this.indexInTileSheetMale.Value = FullId.GetHashCode();
            this.indexInTileSheetFemale.Value = data.TextureFemale != null ? ( FullId.GetHashCode() + 1 ) : FullId.GetHashCode();
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

            float dye_portion_layer_offset = 1E-07f;
            if ( layerDepth >= 1f - dye_portion_layer_offset )
            {
                layerDepth = 1f - dye_portion_layer_offset;
            }

            var texMale = Data.parent.GetTexture( Data.TextureMale, 8, 32);
            var texMaleColor = Data.TextureMaleColor == null ? null : Data.parent.GetTexture( Data.TextureMaleColor, 8, 32);

            texMale.Rect ??= new Rectangle( 0, 0, texMale.Texture.Width, texMale.Texture.Height );
            spriteBatch.Draw( texMale.Texture, location + new Vector2( 32f, 32f ), new Rectangle( texMale.Rect.Value.X+ 0 * 8 % 128, texMale.Rect.Value.Y + 0 * 8 / 128 * 32, 8, 8 ), color * transparency, 0f, new Vector2( 4f, 4f ), scaleSize * 4f, SpriteEffects.None, layerDepth );
            if ( texMaleColor != null )
            {
                texMaleColor.Rect ??= new Rectangle( 0, 0, texMaleColor.Texture.Width, texMaleColor.Texture.Height );
                spriteBatch.Draw( texMaleColor.Texture, location + new Vector2( 32f, 32f ), new Rectangle( texMaleColor.Rect.Value.X + 0 * 8 % 128 + 128, texMaleColor.Rect.Value.X + 0 * 8 / 128 * 32, 8, 8 ), Utility.MultiplyColor( clothes_color, color ) * transparency, 0f, new Vector2( 4f, 4f ), scaleSize * 4f, SpriteEffects.None, layerDepth + dye_portion_layer_offset );
            }
        }

        public override Item getOne()
        {
            var ret = new CustomShirt( Data );
            ret.clothesColor.Value = clothesColor.Value;
            ret._GetOneFrom( this );
            return ret;
        }
    }
}
