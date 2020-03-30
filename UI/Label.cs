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
        public Color IdleTextColor { get; set; } = Game1.textColor;
        public Color HoverTextColor { get; set; } = Game1.unselectedOptionColor;
        public string String { get; set; }

        public Action<Element> Callback { get; set; }

        public bool Hover { get; private set; } = false;

        public override void Update()
        {
            var size = Measure();
            var bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)size.X, (int)size.Y);
            Hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) && !GetRoot().Obscured;

            if (Hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && Callback != null)
                Callback.Invoke(this);
        }

        public Vector2 Measure()
        {
            if (Bold)
                return new Vector2(SpriteText.getWidthOfString(String), SpriteText.getHeightOfString(String));
            else
                return Game1.dialogueFont.MeasureString(String);
        }

        public override void Draw(SpriteBatch b)
        {
            bool altColor = Hover && Callback != null;
            if (Bold)
                SpriteText.drawString(b, String, (int)Position.X, (int)Position.Y, layerDepth: 1, color: altColor ? SpriteText.color_Gray : -1);
            else
                Utility.drawTextWithShadow(b, String, Game1.dialogueFont, Position, altColor ? HoverTextColor : IdleTextColor, 1);
        }
    }
}
