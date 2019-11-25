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
    public class Scrollbar : Element
    {
        public Vector2 BackSize { get; set; }
        public int FrontSize { get; set; }

        private float scrollPerc = 0;
        public float ScrollPercent { get { return scrollPerc; } }
        private bool dragScroll = false;

        public void Scroll(float amt)
        {
            scrollPerc = Util.Clamp(0, scrollPerc + amt, 1 - (FrontSize / BackSize.Y));
        }

        public override void Update()
        {
            var bounds = new Rectangle((int)Position.X, (int)Position.Y, (int) BackSize.X, (int)BackSize.Y);
            bool hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                dragScroll = true;
            }
            if (dragScroll && Mouse.GetState().LeftButton == ButtonState.Released)
            {
                dragScroll = false;
            }

            if (dragScroll)
            {
                int my = Game1.getMouseY();
                int relY = (int)(my - Position.Y - 2 - FrontSize / 2);
                relY = Math.Min(relY, (int)BackSize.Y - 2 - FrontSize);
                relY = Math.Max(0, relY);
                scrollPerc = relY / (BackSize.Y - 4);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            Rectangle back = new Rectangle((int)Position.X, (int)Position.Y, (int)BackSize.X, (int)BackSize.Y);
            Rectangle front = new Rectangle(back.X + 2, back.Y + 2 + (int)((back.Height - 4) * scrollPerc), 6 * Game1.pixelZoom - 4, FrontSize);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.DarkGoldenrod, Game1.pixelZoom, false);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), front.X, front.Y, front.Width, front.Height, Color.Gold, Game1.pixelZoom, false);
        }
    }
}
