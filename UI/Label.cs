using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace GenericModConfigMenu.UI
{
    class Label : Element
    {
        public SpriteFont Font { get; set; } = Game1.dialogueFont;
        public Color IdleTextColor { get; set; } = Color.Black;
        public Color HoverTextColor { get; set; } = Color.DarkGoldenrod;
        public string String { get; set; }

        public Action<Element> Callback { get; set; }

        public bool Hover { get; private set; } = false;

        public override void Update()
        {
            var size = Font.MeasureString(String);
            var bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)size.X, (int)size.Y);
            Hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (Hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && Callback != null)
                Callback.Invoke(this);
        }

        public override void Draw(SpriteBatch b)
        {
            b.DrawString(Font, String, Position, Hover ? HoverTextColor : IdleTextColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }
}
