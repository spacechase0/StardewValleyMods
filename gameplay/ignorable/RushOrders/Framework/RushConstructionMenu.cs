using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace RushOrders.Framework
{
    internal class RushConstructionMenu : IClickableMenu
    {
        private readonly int X;
        private readonly int Y;
        private readonly int HeightForQuestions;
        private readonly IClickableMenu Old;
        private string Question;
        private readonly string[] Responses;
        private int SelectedResponse = -1;
        private bool ShowingBroke;

        public RushConstructionMenu(IClickableMenu oldMenu)
        {
            this.Old = oldMenu;
            this.width = 800;
            this.height = 400;
            this.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height).X;
            this.Y = Game1.viewport.Height - this.height - Game1.tileSize;

            this.Question = I18n.Robin_RushQuestion(daysLeft: Mod.GetBuildingDaysLeft());
            this.Responses = new[]
            {
                I18n.Robin_RushAnswerYes(price: Mod.GetBuildingRushPrice()),
                I18n.Robin_RushAnswerNo()
            };

            this.HeightForQuestions = SpriteText.getHeightOfString(this.Question, this.width - Game1.pixelZoom * 4);
            foreach (string rs in this.Responses)
                this.HeightForQuestions += SpriteText.getHeightOfString(rs, this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 4;
            this.HeightForQuestions += Game1.pixelZoom * 10;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (this.ShowingBroke)
            {
                Game1.activeClickableMenu = this.Old;
                return;
            }

            int num = this.Y - (this.HeightForQuestions - this.height) + SpriteText.getHeightOfString(this.Question, this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 12;
            for (int i = 0; i < this.Responses.Length; i++)
            {
                Rectangle rect = new Rectangle(this.X + Game1.pixelZoom * 2, num, this.width, SpriteText.getHeightOfString(this.Responses[i]));
                if (rect.Contains(x, y))
                {
                    if (i == 0)
                    {
                        int cost = Mod.GetBuildingRushPrice();
                        if (Game1.player.Money < cost)
                        {
                            this.Question = I18n.Robin_NotEnoughMoney();
                            this.ShowingBroke = true;
                            return;
                        }
                        else
                        {
                            Game1.player.Money -= cost;
                            Game1.playSound("coin");
                            Mod.RushBuilding();
                            if (Mod.GetBuildingDaysLeft() > 1)
                            {
                                Game1.activeClickableMenu = new RushConstructionMenu(this.Old);
                                return;
                            }
                        }
                    }

                    Game1.activeClickableMenu = this.Old;
                    return;
                }
                num += SpriteText.getHeightOfString(this.Responses[i], this.width) + Game1.pixelZoom * 4;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            this.DrawBox(b, this.X, this.Y - (this.HeightForQuestions - this.height), this.width, this.HeightForQuestions);
            SpriteText.drawString(b, this.Question, this.X + Game1.pixelZoom * 2, this.Y + Game1.pixelZoom * 3 - (this.HeightForQuestions - this.height), 999999999, this.width - Game1.pixelZoom * 4);

            if (this.ShowingBroke)
                return;

            int num = this.Y - (this.HeightForQuestions - this.height) + SpriteText.getHeightOfString(this.Question, this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 12;
            for (int i = 0; i < this.Responses.Length; i++)
            {
                Rectangle rect = new Rectangle(this.X + Game1.pixelZoom * 2, num, this.width, SpriteText.getHeightOfString(this.Responses[i]));
                if (rect.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    this.SelectedResponse = i;
                if (i == this.SelectedResponse)
                {
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.X + Game1.pixelZoom, num - Game1.pixelZoom * 2, this.width - Game1.pixelZoom * 2, SpriteText.getHeightOfString(this.Responses[i], this.width - Game1.pixelZoom * 4) + Game1.pixelZoom * 4, Color.White, Game1.pixelZoom, false);
                }
                SpriteText.drawString(b, this.Responses[i], this.X + Game1.pixelZoom * 2, num, 999999, this.width, 999999, (this.SelectedResponse == i) ? 1f : 0.6f);
                num += SpriteText.getHeightOfString(this.Responses[i], this.width) + Game1.pixelZoom * 4;
            }

            this.drawMouse(b);
        }

        private void DrawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
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
