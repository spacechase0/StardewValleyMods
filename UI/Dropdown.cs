using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.UI
{
    public class Dropdown : Element
    {
        public Texture2D Texture { get; set; } = Game1.mouseCursors;
        public Rectangle BackgroundTextureRect { get; set; } = OptionsDropDown.dropDownBGSource;
        public Rectangle ButtonTextureRect { get; set; } = OptionsDropDown.dropDownButtonSource;

        public string Value
        {
            get { return Choices[ActiveChoice]; }
            set { if (Choices.Contains(value)) ActiveChoice = Array.IndexOf(Choices, value); }
        }
        public int ActiveChoice { get; set; } = 0;
        public string[] Choices { get; set; } = new string[] { "null" };

        public bool dropped = false;

        public Action<Element> Callback;

        public override void Update()
        {
            var bounds1 = new Rectangle((int)Position.X, (int)Position.Y, 300, 44);
            bool hover = bounds1.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                dropped = true;
                Parent.RenderLast = this;
            }

            if (dropped)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Released)
                {
                    dropped = false;
                    if (Parent.RenderLast == this)
                        Parent.RenderLast = null;
                }

                var bounds2 = new Rectangle((int)Position.X, (int)Position.Y, 300, 44 * Choices.Length);
                hover = bounds2.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());
                if (hover)
                {
                    int choice = (Game1.getOldMouseY() - (int)Position.Y) / 44;
                    ActiveChoice = choice;

                    if (Callback != null)
                        Callback.Invoke(this);
                }
            }
        }

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, Texture, BackgroundTextureRect, (int)Position.X, (int)Position.Y, 300 - 48, 44, Color.White, 4, false);
            b.DrawString(Game1.smallFont, Value, new Vector2(Position.X + 4, Position.Y + 8), Game1.textColor);
            b.Draw(Texture, new Vector2(Position.X + 300 - 48, Position.Y), ButtonTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);

            if ( dropped )
            {
                int tall = Choices.Length * 44;
                IClickableMenu.drawTextureBox(b, Texture, BackgroundTextureRect, (int)Position.X, (int)Position.Y, 300 - 48, tall, Color.White, 4, false);
                for ( int i = 0; i < Choices.Length; ++i )
                {
                    if (i == ActiveChoice)
                        b.Draw(Game1.staminaRect, new Rectangle((int)Position.X + 4, (int)Position.Y + i * 44, 300 - 48 - 8, 44), null, Color.Wheat, 0, Vector2.Zero, SpriteEffects.None, 0.98f );
                    b.DrawString(Game1.smallFont, Choices[i], new Vector2(Position.X + 4, Position.Y + i * 44 + 8), Game1.textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                }
            }
        }
    }
}
