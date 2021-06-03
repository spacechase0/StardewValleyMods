using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley;
using StardewValley.Menus;

namespace AnimalSocialMenu
{
    public class AnimalSocialPage : IClickableMenu
    {
        private string descriptionText = "";
        private string hoverText = "";
        public const int slotsOnPage = 5;
        private ClickableTextureComponent upButton;
        private ClickableTextureComponent downButton;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private List<object> names;
        private List<ClickableTextureComponent> sprites;
        private int slotPosition;
        private Dictionary<long, FarmAnimal> animals = new Dictionary<long, FarmAnimal>();
        private bool scrolling;

        public AnimalSocialPage(int x, int y, int width, int height)
            : base(x, y, width, height, false)
        {
            foreach (FarmAnimal fa in Game1.getFarm().getAllFarmAnimals())
            {
                animals[fa.myID.Value] = fa;
            }
            this.names = new List<object>();
            this.sprites = new List<ClickableTextureComponent>();
            foreach (var kvp in animals.OrderBy(p => p.Value.type.Value))
            {
                this.names.Add((object)kvp.Key);
                //this.sprites.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.borderWidth + 4, 0, width, 64), (string)null, "", Game1.objectSpriteSheet, new Rectangle(0, 0, 24, 24), 4f, false));
                this.sprites.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.borderWidth + 4 + (kvp.Value.Sprite.SourceRect.Width == 16 ? 16 : 0), 0, width, 64), (string)null, "", kvp.Value.Sprite.Texture, kvp.Value.Sprite.SourceRect, 2, false));
            }
            this.upButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
            this.downButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upButton.bounds.X + 12, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, this.scrollBar.bounds.Width, height - 128 - this.upButton.bounds.Height - 8);
            this.updateSlots();
        }

        public void updateSlots()
        {
            int num1 = 0;
            for (int slotPosition = this.slotPosition; slotPosition < this.slotPosition + 5; ++slotPosition)
            {
                if (this.sprites.Count > slotPosition)
                {
                    int num2 = this.yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * num1 + 32;
                    if (animals[(long)names[slotPosition]].Sprite.SourceRect.Height == 16)
                        num2 += 16;
                    this.sprites[slotPosition].bounds.Y = num2;
                }
                ++num1;
            }
        }

        public override void applyMovementKey(int direction)
        {
            if (direction == 0 && this.slotPosition > 0)
                this.upArrowPressed();
            else if (direction == 2 && this.slotPosition < this.sprites.Count - 5)
            {
                this.downArrowPressed();
            }
            else
            {
                if (direction != 3 && direction != 1)
                    return;
                base.applyMovementKey(direction);
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (!this.scrolling)
                return;
            int y1 = this.scrollBar.bounds.Y;
            this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upButton.bounds.Height + 20));
            this.slotPosition = Math.Min(this.sprites.Count - 5, Math.Max(0, (int)((double)this.sprites.Count * (double)((float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height))));
            this.setScrollBarToCurrentIndex();
            int y2 = this.scrollBar.bounds.Y;
            if (y1 == y2)
                return;
            Game1.playSound("shiny4");
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            this.scrolling = false;
        }

        private void setScrollBarToCurrentIndex()
        {
            if (this.sprites.Count > 0)
            {
                this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.sprites.Count - 5 + 1) * this.slotPosition + this.upButton.bounds.Bottom + 4;
                if (this.slotPosition == this.sprites.Count - 5)
                    this.scrollBar.bounds.Y = this.downButton.bounds.Y - this.scrollBar.bounds.Height - 4;
            }
            this.updateSlots();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.slotPosition > 0)
            {
                this.upArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (direction >= 0 || this.slotPosition >= Math.Max(0, this.sprites.Count - 5))
                    return;
                this.downArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        public void upArrowPressed()
        {
            --this.slotPosition;
            this.updateSlots();
            this.upButton.scale = 3.5f;
            this.setScrollBarToCurrentIndex();
        }

        public void downArrowPressed()
        {
            ++this.slotPosition;
            this.updateSlots();
            this.downButton.scale = 3.5f;
            this.setScrollBarToCurrentIndex();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.upButton.containsPoint(x, y) && this.slotPosition > 0)
            {
                this.upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.downButton.containsPoint(x, y) && this.slotPosition < this.sprites.Count - 5)
            {
                this.downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.scrollBar.containsPoint(x, y))
                this.scrolling = true;
            else if (!this.downButton.containsPoint(x, y) && x > this.xPositionOnScreen + this.width - 96 && (x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            this.slotPosition = Math.Max(0, Math.Min(this.sprites.Count - 5, this.slotPosition));
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            this.descriptionText = "";
            this.hoverText = "";
            this.upButton.tryHover(x, y, 0.1f);
            this.downButton.tryHover(x, y, 0.1f);
        }

        private void drawNPCSlot(SpriteBatch b, int i)
        {
            this.sprites[i].draw(b);
            var animal = animals[(long)names[i]];
            string name = animal.Name;
            int heartLevelForNpc = animal.friendshipTowardFarmer.Value;
            float y = Game1.smallFont.MeasureString("W").Y;
            float num = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? (float)(-(double)y / 2.0) : 0.0f;

            int n = 0;
            if (animal.Sprite.SourceRect.Height == 16)
                n = 16;

            double loveLevel = heartLevelForNpc / 1000.0;
            int num3 = loveLevel * 1000.0 % 200.0 >= 100.0 ? (int)(loveLevel * 1000.0 / 200.0) : -100;
            b.DrawString(Game1.dialogueFont, name, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96) - Game1.dialogueFont.MeasureString(name).X / 2f, (float)((double)(this.sprites[i].bounds.Y + 48) + (double)num - (false ? 24.0 : 20.0))), Game1.textColor);
            for (int index = 0; index < 5; ++index)
            {
                b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 320 - 8 + index * 32), (float)(this.sprites[i].bounds.Y - n + 64 - 28)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(211 + (loveLevel * 1000.0 <= (double)((index + 1) * 195) ? 7 : 0), 428, 7, 6)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
                if (num3 == index)
                    b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 320 - 8 + index * 32), (float)(this.sprites[i].bounds.Y - n + 64 - 28)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(211, 428, 4, 6)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.891f);
            }

            if (!animal.wasPet.Value)
                b.DrawString(Game1.dialogueFont, "Needs petting", new Vector2((float)(this.xPositionOnScreen + 250 + 264), (float)(this.sprites[i].bounds.Y + 32 - 12)), Game1.textColor);
        }

        private int rowPosition(int i)
        {
            int num1 = i - this.slotPosition;
            int num2 = 112;
            return this.yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + num1 * num2;
        }

        public override void draw(SpriteBatch b)
        {
            b.End();
            b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, new RasterizerState()
            {
                ScissorTestEnable = true
            });
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, true);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, true);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, true);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, true);
            Rectangle scissorRectangle = b.GraphicsDevice.ScissorRectangle;
            Rectangle rectangle = scissorRectangle;
            rectangle.Y = Math.Max(0, this.rowPosition(0 - 1));
            rectangle.Height -= rectangle.Y;
            b.GraphicsDevice.ScissorRectangle = rectangle;
            try
            {
                this.drawVerticalPartition(b, this.xPositionOnScreen + 256 + 12, true);
            }
            finally
            {
                b.GraphicsDevice.ScissorRectangle = scissorRectangle;
            }
            this.drawVerticalPartition(b, this.xPositionOnScreen + 256 + 12 + 200, true);
            for (int slotPosition = this.slotPosition; slotPosition < this.slotPosition + 5; ++slotPosition)
            {
                if (slotPosition < this.sprites.Count)
                {
                    if (this.names[slotPosition] is long)
                        this.drawNPCSlot(b, slotPosition);
                }
            }
            this.upButton.draw(b);
            this.downButton.draw(b);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, true);
            this.scrollBar.draw(b);
            if (!this.hoverText.Equals(""))
                IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
        }
    }
}
