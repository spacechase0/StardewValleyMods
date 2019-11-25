using StardewValley.Menus;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;

namespace RushOrders
{
    class RushConstructionMenu : IClickableMenu
    {
        private readonly int x;
        private readonly int y;
        private readonly string[] r = { "Yes", "No" };
        private readonly int heightForQuestions;
        private readonly IClickableMenu old;
        private string q = "Rush your building construction?";
        private int selectedResponse = -1;
        bool showingBroke = false;

        public RushConstructionMenu( IClickableMenu oldMenu )
        {
            old = oldMenu;
            width = 800;
            height = 400;
            x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
            y = Game1.viewport.Height - height - Game1.tileSize;

            q += $" ({Mod.getBuildingDaysLeft()} days left)";
            r[0] += $" ({Mod.getBuildingRushPrice()}g)";

            heightForQuestions = SpriteText.getHeightOfString(q, width - Game1.pixelZoom * 4);
            foreach ( string rs in r )
                heightForQuestions += SpriteText.getHeightOfString(rs, width - Game1.pixelZoom * 4) + Game1.pixelZoom * 4;
            heightForQuestions += Game1.pixelZoom * 10;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if ( showingBroke )
            {
                Game1.activeClickableMenu = old;
                return;
            }

            int num = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(q, this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 12;
            for (int i = 0; i < this.r.Length; i++)
            {
                Rectangle rect = new Rectangle(this.x + Game1.pixelZoom * 2, num, width, SpriteText.getHeightOfString(r[i]));
                if ( rect.Contains( x, y ) )
                {
                    if ( i == 0 )
                    {
                        int cost = Mod.getBuildingRushPrice();
                        if (Game1.player.Money < cost)
                        {
                            q = "You do not have enough money.";
                            showingBroke = true;
                            return;
                        }
                        else
                        {
                            Game1.player.Money -= cost;
                            Game1.playSound("coin");
                            Mod.rushBuilding();
                            if (Mod.getBuildingDaysLeft() > 1)
                            {
                                Game1.activeClickableMenu = new RushConstructionMenu(old);
                                return;
                            }
                        }
                    }

                    Game1.activeClickableMenu = old;
                    return;
                }
                num += SpriteText.getHeightOfString(this.r[i], this.width) + Game1.pixelZoom * 4;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            this.drawBox(b, this.x, this.y - (this.heightForQuestions - this.height), this.width, this.heightForQuestions);
            SpriteText.drawString(b, q, this.x + Game1.pixelZoom * 2, this.y + Game1.pixelZoom * 3 - (this.heightForQuestions - this.height), 999999999, this.width - Game1.pixelZoom * 4);
            
            if (showingBroke)
                return;
            
            int num = this.y - (this.heightForQuestions - this.height) + SpriteText.getHeightOfString(q, this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 12;
            for (int i = 0; i < this.r.Count(); i++)
            {
                Rectangle rect = new Rectangle(this.x + Game1.pixelZoom * 2, num, width, SpriteText.getHeightOfString(r[i]));
                if (rect.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    selectedResponse = i;
                if (i == this.selectedResponse)
                {
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.x + Game1.pixelZoom, num - Game1.pixelZoom * 2, this.width - Game1.pixelZoom * 2, SpriteText.getHeightOfString(this.r[i], this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 4, Color.White, (float)Game1.pixelZoom, false);
                }
                SpriteText.drawString(b, this.r[i], this.x + Game1.pixelZoom * 2, num, 999999, this.width, 999999, (this.selectedResponse == i) ? 1f : 0.6f);
                num += SpriteText.getHeightOfString(this.r[i], this.width) + Game1.pixelZoom * 4;
            }

            drawMouse( b );
        }

        private void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
        {
            if (xPos > 0)
            {
                b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(306, 320, 16, 16), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 5 * Game1.pixelZoom, boxWidth, 6 * Game1.pixelZoom), new Rectangle(275, 313, 1, 6), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos + 3 * Game1.pixelZoom, yPos + boxHeight, boxWidth - 5 * Game1.pixelZoom, 8 * Game1.pixelZoom), new Rectangle(275, 328, 1, 8), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos - 8 * Game1.pixelZoom, yPos + 6 * Game1.pixelZoom, 8 * Game1.pixelZoom, boxHeight - 7 * Game1.pixelZoom), new Rectangle(264, 325, 8, 1), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 7 * Game1.pixelZoom, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
                b.Draw(Game1.mouseCursors, new Vector2(xPos - 11 * Game1.pixelZoom, yPos - 7 * Game1.pixelZoom), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - Game1.pixelZoom * 2, yPos - 7 * Game1.pixelZoom), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - Game1.pixelZoom * 2, yPos + boxHeight - 2 * Game1.pixelZoom), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos - 11 * Game1.pixelZoom, yPos + boxHeight - Game1.pixelZoom), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
            }
        }
    }
}
