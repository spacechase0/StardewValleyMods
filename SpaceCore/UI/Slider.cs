using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.UI
{
    public class Slider : Element
    {
        public int RequestWidth { get; set; }

        public Action<Element> Callback { get; set; }

        protected bool dragging;

        public override int Width => this.RequestWidth;
        public override int Height => 24;

        public override void Draw(SpriteBatch b)
        {
        }

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);
        }
    }

    internal class Slider<T> : Slider
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Value { get; set; }

        public T Interval { get; set; }

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (this.Clicked)
                this.dragging = true;
            if (Mouse.GetState().LeftButton == ButtonState.Released)
                this.dragging = false;

            if (this.dragging)
            {
                float perc = (Game1.getOldMouseX() - this.Position.X) / this.Width;
                if (this.Value is int)
                {
                    this.Value = Util.Clamp<T>(this.Minimum, (T)(object)(int)(perc * ((int)(object)this.Maximum - (int)(object)this.Minimum) + (int)(object)this.Minimum), this.Maximum);
                }
                else if (this.Value is float)
                {
                    this.Value = Util.Clamp<T>(this.Minimum, (T)(object)(perc * ((float)(object)this.Maximum - (float)(object)this.Minimum) + (float)(object)this.Minimum), this.Maximum);
                }

                this.Value = Util.Adjust(this.Value, this.Interval);

                if (this.Callback != null)
                    this.Callback.Invoke(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            float perc = 0;
            if (this.Value is int)
            {
                perc = ((int)(object)this.Value - (int)(object)this.Minimum) / (float)((int)(object)this.Maximum - (int)(object)this.Minimum);
            }
            else if (this.Value is float)
            {
                perc = ((float)(object)this.Value - (float)(object)this.Minimum) / ((float)(object)this.Maximum - (float)(object)this.Minimum);
            }

            Rectangle back = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
            Rectangle front = new Rectangle((int)(this.Position.X + perc * (this.Width - 40)), (int)this.Position.Y, 40, this.Height);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, new Vector2(front.X, front.Y), new Rectangle(420, 441, 10, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        }
    }
}
