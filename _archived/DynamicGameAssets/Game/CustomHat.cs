using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAHat")]
    public partial class CustomHat : Hat
    {
        partial void DoInit()
        {
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        partial void DoInit(HatPackData data)
        {
            this.Name = this.Id;
            this.which.Value = this.FullId.GetDeterministicHashCode();

            this.hairDrawType.Value = (int)data.HairStyle;
            this.ignoreHairstyleOffset.Value = data.IgnoreHairstyleOffset;
            this.Category = -95;
        }

        public override string DisplayName { get => this.Data.Name; set { } }

        public override string getDescription()
        {
            return Game1.parseText(this.Data.Description, Game1.smallFont, this.getDescriptionWidth());
        }

        public override Item getOne()
        {
            var ret = new CustomHat(this.Data);
            // TODO: All the other fields objects does??
            ret.Stack = 1;
            ret._GetOneFrom( this );
            return ret;
        }

        /*
        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            var currTex = Data.GetTexture();
            if ( !currTex.Rect.HasValue )
                currTex.Rect = new Rectangle( 0, 0, currTex.Texture.Width, currTex.Texture.Height );

            float originalScale = scaleSize;
            scaleSize *= 0.75f;
            spriteBatch.Draw( currTex.Texture, location + new Vector2( 32f, 32f ), new Rectangle( currTex.Rect.Value.X, currTex.Rect.Value.Y, 20, 20 ), this.isPrismatic ? ( Utility.GetPrismaticColor() * transparency ) : ( color * transparency ), 0f, new Vector2( 10f, 10f ), 4f * scaleSize, SpriteEffects.None, layerDepth );
            if ( ( ( drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1 ) || drawStackNumber == StackDrawType.Draw_OneInclusive ) && ( double ) scaleSize > 0.3 && this.Stack != int.MaxValue )
            {
                Utility.drawTinyDigits( this.Stack, spriteBatch, location + new Vector2( ( float ) ( 64 - Utility.getWidthOfTinyDigitString( this.Stack, 3f * originalScale ) ) + 3f * originalScale, 64f - 18f * originalScale + 2f ), 3f * originalScale, 1f, color );
            }
        }

        public void Draw( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction )
        {
            SpaceShared.Log.Debug( "meow!" );
            var currTex = Data.GetTexture();
            if ( !currTex.Rect.HasValue )
                currTex.Rect = new Rectangle( 0, 0, currTex.Texture.Width, currTex.Texture.Height );

            switch ( direction )
            {
                case 0:
                    direction = 3;
                    break;
                case 2:
                    direction = 0;
                    break;
                case 3:
                    direction = 2;
                    break;
            }
            spriteBatch.Draw( currTex.Texture, location + new Vector2( 10f, 10f ), new Rectangle( currTex.Rect.Value.X, currTex.Rect.Value.Y + direction * 20, 20, 20 ), this.isPrismatic ? ( Utility.GetPrismaticColor() * transparency ) : ( Color.White * transparency ), 0f, new Vector2( 3f, 3f ), 3f * scaleSize, SpriteEffects.None, layerDepth );
        }*/
    }
}
