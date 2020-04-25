using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    public class Textbox : Element, IKeyboardSubscriber
    {
        private Texture2D tex;
        private SpriteFont font;
        
        public virtual string String { get; set; }

        private bool selected = false;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;

                selected = value;
                if ( selected )
                    Game1.keyboardDispatcher.Subscriber = this;
                else
                {
                    if (Game1.keyboardDispatcher.Subscriber == this)
                        Game1.keyboardDispatcher.Subscriber = null;
                }
            }
        }

        public Action<Element> Callback { get; set; }

        public Textbox()
        {
            tex = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            font = Game1.smallFont;
        }

        public override int Width => 192;
        public override int Height => 48;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (ClickGestured && Callback != null)
            {
                if (Hover)
                    Selected = true;
                else
                    Selected = false;
            }
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(tex, Position, Color.White);
            
            // Copied from game code - caret
            string text = String;
            Vector2 vector2;
            for (vector2 = font.MeasureString(text); (double)vector2.X > (double)192; vector2 = font.MeasureString(text))
                text = text.Substring(1);
            if (DateTime.UtcNow.Millisecond % 1000 >= 500 && Selected)
                b.Draw(Game1.staminaRect, new Rectangle((int)Position.X + 16 + (int)vector2.X + 2, (int)Position.Y + 8, 4, 32), Game1.textColor);

            b.DrawString(font, text, Position + new Vector2(16, 12), Game1.textColor);
        }

        protected virtual void receiveInput(string str)
        {
            String += str;
            if (Callback != null)
                Callback.Invoke(this);
        }

        public void RecieveTextInput(char inputChar)
        {
            receiveInput(inputChar.ToString());

            // Copied from game code
            switch (inputChar)
            {
                case '"':
                    return;
                case '$':
                    Game1.playSound("money");
                    break;
                case '*':
                    Game1.playSound("hammer");
                    break;
                case '+':
                    Game1.playSound("slimeHit");
                    break;
                case '<':
                    Game1.playSound("crystal");
                    break;
                case '=':
                    Game1.playSound("coin");
                    break;
                default:
                    Game1.playSound("cowboy_monsterhit");
                    break;
            }
        }

        public void RecieveTextInput(string text)
        {
            receiveInput(text);
        }

        public void RecieveCommandInput(char command)
        {
            if ( command == '\b' && String.Length > 0 )
            {
                Game1.playSound("tinyWhip");
                String = String.Substring(0, String.Length - 1);
                if (Callback != null)
                    Callback.Invoke(this);
            }
        }

        public void RecieveSpecialInput(Keys key)
        {
        }
    }
}
