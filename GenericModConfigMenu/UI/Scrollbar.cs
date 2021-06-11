using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.UI
{
    public class Scrollbar : Element
    {
        public int RequestHeight { get; set; }

        public int Rows { get; set; }
        public int FrameSize { get; set; }

        public int TopRow { get; private set; }
        public int MaxTopRow => Math.Max(0, this.Rows - this.FrameSize);

        public float ScrollPercent => (this.MaxTopRow > 0) ? this.TopRow / (float)this.MaxTopRow : 0f;

        private bool dragScroll;

        public void ScrollBy(int amount)
        {
            int row = Util.Clamp(0, this.TopRow + amount, this.MaxTopRow);
            if (row != this.TopRow)
            {
                Game1.playSound("shwip");
                this.TopRow = row;
            }
        }

        public void ScrollTo(int row)
        {
            if (this.TopRow != row)
            {
                Game1.playSound("shiny4");
                this.TopRow = Util.Clamp(0, row, this.MaxTopRow);
            }
        }

        public override int Width => 24;
        public override int Height => this.RequestHeight;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (this.Clicked)
                this.dragScroll = true;
            if (this.dragScroll && Mouse.GetState().LeftButton == ButtonState.Released)
                this.dragScroll = false;

            if (this.dragScroll)
            {
                int my = Game1.getMouseY();
                int relY = (int)(my - this.Position.Y - 40 / 2);
                this.ScrollTo((int)Math.Round(relY / (float)(this.Height - 40) * this.MaxTopRow));
            }
        }

        public override void Draw(SpriteBatch b)
        {
            Rectangle back = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
            Vector2 front = new Vector2(back.X, back.Y + (this.Height - 40) * this.ScrollPercent);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, front, new Rectangle(435, 463, 6, 12), Color.White, 0f, new Vector2(), Game1.pixelZoom, SpriteEffects.None, 0.77f);
        }
    }
}
