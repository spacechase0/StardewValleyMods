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
    [XmlType( "Mods_DGAHat" )]
    public class CustomHat : Hat, IDGAItem
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
        public HatPackData Data => Mod.Find( FullId ) as HatPackData;

        public CustomHat()
        {
            base.NetFields.AddFields( _sourcePack, _id );
        }

        public CustomHat( HatPackData data )
        :   this()
        {
            _sourcePack.Value = data.parent.smapiPack.Manifest.UniqueID;
            _id.Value = data.ID;

            this.Name = Id;
            this.which.Value = FullId.GetHashCode();

            this.hairDrawType.Value = ( int ) data.HairStyle;
            this.ignoreHairstyleOffset.Value = data.IgnoreHairstyleOffset;
            base.Category = -95;
        }

        public override string DisplayName { get => Data.Name; set { } }

        public override string getDescription()
        {
            return Game1.parseText( Data.Description, Game1.smallFont, this.getDescriptionWidth() );
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
