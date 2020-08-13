using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.UI
{
    class Slider : Element
    {
        public int RequestWidth { get; set; }

        public Action<Element> Callback { get; set; }

        protected bool dragging = false;

        public override int Width => RequestWidth;
        public override int Height => 24;

        public override void Draw(SpriteBatch b)
        {
        }

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);
        }
    }

    class Slider<T> : Slider
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Value { get; set; }

        public T Interval { get; set; }

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (Clicked)
                dragging = true;
            if (Mouse.GetState().LeftButton == ButtonState.Released)
                dragging = false;

            if (dragging)
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

                Value = Util.Adjust(Value, Interval);

                if (Callback != null)
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

            Rectangle back = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Rectangle front = new Rectangle((int)(Position.X + perc * (Width - 40)), (int)Position.Y, 40, Height);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, new Vector2(front.X, front.Y), new Rectangle(420, 441, 10, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        }
    }
}
