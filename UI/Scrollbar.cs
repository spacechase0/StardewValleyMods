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
    public class Scrollbar : Element
    {
        public int RequestHeight { get; set; }

        public int Rows { get; set; }
        public int FrameSize { get; set; }

        public int TopRow { get; private set; }
        public int MaxTopRow => Math.Max(0, Rows - FrameSize);

        public float ScrollPercent => (MaxTopRow > 0) ? TopRow / (float)MaxTopRow : 0f;

        private bool dragScroll = false;

        public void ScrollBy(int amount)
        {
            int row = Util.Clamp(0, TopRow + amount, MaxTopRow);
            if ( row != TopRow )
            {
                Game1.playSound("shwip");
                TopRow = row;
            }
        }

        public void ScrollTo(int row)
        {
            if ( TopRow != row )
            {
                Game1.playSound("shiny4");
                TopRow = Util.Clamp(0, row, MaxTopRow);
            }
        }

        public override int Width => 24;
        public override int Height => RequestHeight;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (Clicked)
                dragScroll = true;
            if (dragScroll && Mouse.GetState().LeftButton == ButtonState.Released)
                dragScroll = false;

            if (dragScroll)
            {
                int my = Game1.getMouseY();
                int relY = (int)(my - Position.Y - 40 / 2);
                ScrollTo((int)Math.Round(relY / (float)(Height - 40) * MaxTopRow));
            }
        }

        public override void Draw(SpriteBatch b)
        {
            Rectangle back = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Vector2 front = new Vector2(back.X, back.Y + (Height - 40) * ScrollPercent);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, front, new Rectangle(435, 463, 6, 12), Color.White, 0f, new Vector2(), (float)Game1.pixelZoom, SpriteEffects.None, 0.77f);
        }
    }
}
