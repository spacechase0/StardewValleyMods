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
    class Slider<T> : Element
    {
        public int Width { get; set; }

        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Value { get; set; }

        public Action<Element> Callback { get; set; }

        private bool dragging = false;

        public override void Update()
        {
            var bounds = new Rectangle((int)Position.X, (int)Position.Y, Width, 30);
            bool hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)
                dragging = true;
            if (Mouse.GetState().LeftButton == ButtonState.Released)
                dragging = false;

            if ( dragging )
            {
                float perc = (Game1.getOldMouseX() - Position.X) / (float)Width;
                if (Value is int)
                {
                    Value = Util.Clamp<T>(Minimum, (T)(object)(int)(perc * ((int)(object)Maximum - (int)(object)Minimum) + (int)(object)Minimum), Maximum);
                }
                else if (Value is float)
                {
                    Value = Util.Clamp<T>(Minimum, (T)(object)(float)(perc * ((float)(object)Maximum - (float)(object)Minimum) + (float)(object)Minimum), Maximum);
                }
                if ( Callback != null )
                    Callback.Invoke(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            float perc = 0;
            if (Value is int)
            {
                perc = ((int)(object)Value - (int)(object)Minimum) / (float)((int)(object)Maximum - (int)(object)Minimum);
            }
            else if (Value is float)
            {
                perc = ((float)(object)Value - (float)(object)Minimum) / (float)((float)(object)Maximum - (float)(object)Minimum);
            }

            Rectangle back = new Rectangle((int)Position.X, (int)Position.Y + 10, Width, 10);
            Rectangle front = new Rectangle((int)(Position.X + perc * Width) - 5, (int)Position.Y, 10, 30);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.DarkGoldenrod, Game1.pixelZoom, false);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), front.X, front.Y, front.Width, front.Height, Color.Gold, Game1.pixelZoom, false);
        }
    }
}
