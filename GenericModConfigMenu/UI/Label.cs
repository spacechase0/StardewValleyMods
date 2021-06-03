using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace GenericModConfigMenu.UI
{
    class Label : Element
    {
        public bool Bold { get; set; } = false;
        public float NonBoldScale { get; set; } = 1f; // Only applies when Bold = false
        public bool NonBoldShadow { get; set; } = true; // Only applies when Bold = false
        public Color IdleTextColor { get; set; } = Game1.textColor;
        public Color HoverTextColor { get; set; } = Game1.unselectedOptionColor;
        public string String { get; set; }

        public Action<Element> Callback { get; set; }

        public override int Width => (int)Measure().X;
        public override int Height => (int)Measure().Y;
        public override string HoveredSound => (Callback != null) ? "shiny4" : null;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);            

            if (Clicked && Callback != null)
                Callback.Invoke(this);
        }

        public Vector2 Measure()
        {
            if (Bold)
                return new Vector2(SpriteText.getWidthOfString(String), SpriteText.getHeightOfString(String));
            else
                return Game1.dialogueFont.MeasureString(String) * NonBoldScale;
        }

        public override void Draw(SpriteBatch b)
        {
            bool altColor = Hover && Callback != null;
            if (Bold)
                SpriteText.drawString(b, String, (int)Position.X, (int)Position.Y, layerDepth: 1, color: altColor ? SpriteText.color_Gray : -1);
            else if ( NonBoldShadow )
                Utility.drawTextWithShadow( b, String, Game1.dialogueFont, Position, altColor ? HoverTextColor : IdleTextColor, NonBoldScale );
            else
                b.DrawString( Game1.dialogueFont, String, Position, altColor ? HoverTextColor : IdleTextColor, 0f, Vector2.Zero, NonBoldScale, SpriteEffects.None, 1 );
        }
    }
}
