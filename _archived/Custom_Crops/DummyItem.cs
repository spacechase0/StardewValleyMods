using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Crops
{
    // Solely for rendering an icon in drawTooltip or similar areas
    // DO NOT PUT IN GAME WORLD, not hooked up to serializer
    public class DummyItem : Item
    {
        private string name;
        private string desc;
        private Texture2D tex;
        private Rectangle? texRect;

        public DummyItem( string name, string desc, Texture2D tex, Rectangle? texRect )
        {
            this.name = name;
            this.desc = desc;
            this.tex = tex;
            this.texRect = texRect;
        }

        public override string DisplayName { get => name; set { } }
        public override int Stack { get => 1; set { } }

        public override int addToStack( Item stack )
        {
            throw new NotImplementedException();
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            int w = tex.Width;
            if (texRect.HasValue)
                w = texRect.Value.Width;
            spriteBatch.Draw( tex, location, texRect, color * transparency, 0, Vector2.Zero, Vector2.One * scaleSize * ( int )( 64 / w ), SpriteEffects.None, layerDepth );
        }

        public override string getDescription()
        {
            return desc;
        }

        public override Item getOne()
        {
            throw new NotImplementedException();
        }

        public override bool isPlaceable()
        {
            throw new NotImplementedException();
        }

        public override int maximumStackSize()
        {
            throw new NotImplementedException();
        }
    }
}
