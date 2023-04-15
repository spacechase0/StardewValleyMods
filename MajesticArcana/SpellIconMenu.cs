using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace MajesticArcana
{
    public class SpellIconMenu : IClickableMenu
    {
        private const int IconsPerPage = 12;

        private List<Tuple<string, Texture2D>> icons = new();
        private int page = 0;

        public SpellIconMenu()
        : base(Game1.uiViewport.Width / 2 - 300, Game1.uiViewport.Height / 2 - 250, 600, 500, true)
        {
            foreach (var icon in Mod.SpellIcons)
                icons.Add(new(icon.Key, icon.Value));
        }

        private int? leftClickX = null, leftClickY = null;
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            leftClickX = x;
            leftClickY = y;
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            for (int i = page * IconsPerPage; i < Math.Min(icons.Count, (page + 1) * IconsPerPage); ++i)
            {
                string filename = icons[i].Item1;
                Texture2D icon = icons[i].Item2;
                int num = i % IconsPerPage;
                int x = xPositionOnScreen + 50 + (num / 3) * 100 + 32 * (num / 3);
                int y = yPositionOnScreen + 32 + (num % 3) * 100 + 32 * (num % 3);

                b.Draw(icon, new Rectangle(x, y, 100, 100), Color.White);

                if (leftClickX.HasValue && new Rectangle(x, y, 100, 100).Contains(leftClickX.Value, leftClickY.Value))
                {
                    if (GetParentMenu() is SpellcraftingMenu spellcrafting)
                    {
                        spellcrafting.SetSpellIcon( filename );
                        GetParentMenu().SetChildMenu(null);
                    }
                    leftClickX = leftClickY = null;
                }
            }

            int maxPages = (int)Math.Ceiling((float) icons.Count / IconsPerPage);
            string pageStr = "Page " + (page+1) + "/" + maxPages; // I18n.SpellStash_Page(page + 1, maxPages);
            b.DrawString(Game1.smallFont, pageStr, new Vector2(xPositionOnScreen + (width - Game1.smallFont.MeasureString( pageStr ).X) / 2, yPositionOnScreen + height - 50), Color.Black);

            if (page > 0)
            {
                SpriteText.drawString(b, "@", xPositionOnScreen + 32, yPositionOnScreen + height - 64);
                if (leftClickX.HasValue && new Rectangle(xPositionOnScreen + 32 - 8, yPositionOnScreen + height - 64 - 8, 48, 48).Contains(leftClickX.Value, leftClickY.Value))
                {
                    --page;
                    leftClickX = leftClickY = null;
                }
            }
            if (page + 1 < maxPages)
            {
                SpriteText.drawString(b, ">", xPositionOnScreen + width - 64, yPositionOnScreen + height - 64);
                if (leftClickX.HasValue && new Rectangle(xPositionOnScreen + width - 64 - 8, yPositionOnScreen + height - 64 - 8, 48, 48).Contains(leftClickX.Value, leftClickY.Value))
                {
                    ++page;
                    leftClickX = leftClickY = null;
                }
            }

            drawMouse(b);
        }
    }
}
