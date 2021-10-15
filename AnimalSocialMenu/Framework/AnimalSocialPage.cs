using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AnimalSocialMenu.Framework
{
    internal class AnimalSocialPage : IClickableMenu
    {
        private string HoverText = "";
        private readonly ClickableTextureComponent UpButton;
        private readonly ClickableTextureComponent DownButton;
        private readonly ClickableTextureComponent ScrollBar;
        private readonly Rectangle ScrollBarRunner;
        private readonly List<object> Names;
        private readonly List<ClickableTextureComponent> Sprites;
        private int SlotPosition;
        private readonly Dictionary<long, FarmAnimal> Animals = new();
        private bool Scrolling;

        public AnimalSocialPage(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            foreach (FarmAnimal fa in Game1.getFarm().getAllFarmAnimals())
            {
                this.Animals[fa.myID.Value] = fa;
            }
            this.Names = new List<object>();
            this.Sprites = new List<ClickableTextureComponent>();
            foreach (var kvp in this.Animals.OrderBy(p => p.Value.type.Value))
            {
                this.Names.Add(kvp.Key);
                //this.sprites.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.borderWidth + 4, 0, width, 64), (string)null, "", Game1.objectSpriteSheet, new Rectangle(0, 0, 24, 24), 4f, false));
                this.Sprites.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.borderWidth + 4 + (kvp.Value.Sprite.SourceRect.Width == 16 ? 16 : 0), 0, width, 64), null, "", kvp.Value.Sprite.Texture, kvp.Value.Sprite.SourceRect, 2));
            }
            this.UpButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            this.DownButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            this.ScrollBar = new ClickableTextureComponent(new Rectangle(this.UpButton.bounds.X + 12, this.UpButton.bounds.Y + this.UpButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            this.ScrollBarRunner = new Rectangle(this.ScrollBar.bounds.X, this.UpButton.bounds.Y + this.UpButton.bounds.Height + 4, this.ScrollBar.bounds.Width, height - 128 - this.UpButton.bounds.Height - 8);
            this.UpdateSlots();
        }

        public void UpdateSlots()
        {
            int num1 = 0;
            for (int slotPosition = this.SlotPosition; slotPosition < this.SlotPosition + 5; ++slotPosition)
            {
                if (this.Sprites.Count > slotPosition)
                {
                    int num2 = this.yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * num1 + 32;
                    if (this.Animals[(long)this.Names[slotPosition]].Sprite.SourceRect.Height == 16)
                        num2 += 16;
                    this.Sprites[slotPosition].bounds.Y = num2;
                }
                ++num1;
            }
        }

        public override void applyMovementKey(int direction)
        {
            if (direction == 0 && this.SlotPosition > 0)
                this.UpArrowPressed();
            else if (direction == 2 && this.SlotPosition < this.Sprites.Count - 5)
            {
                this.DownArrowPressed();
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
            if (!this.Scrolling)
                return;
            int y1 = this.ScrollBar.bounds.Y;
            this.ScrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.ScrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.UpButton.bounds.Height + 20));
            this.SlotPosition = Math.Min(this.Sprites.Count - 5, Math.Max(0, (int)(this.Sprites.Count * (double)((y - this.ScrollBarRunner.Y) / (float)this.ScrollBarRunner.Height))));
            this.SetScrollBarToCurrentIndex();
            int y2 = this.ScrollBar.bounds.Y;
            if (y1 == y2)
                return;
            Game1.playSound("shiny4");
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            this.Scrolling = false;
        }

        private void SetScrollBarToCurrentIndex()
        {
            if (this.Sprites.Count > 0)
            {
                this.ScrollBar.bounds.Y = this.ScrollBarRunner.Height / Math.Max(1, this.Sprites.Count - 5 + 1) * this.SlotPosition + this.UpButton.bounds.Bottom + 4;
                if (this.SlotPosition == this.Sprites.Count - 5)
                    this.ScrollBar.bounds.Y = this.DownButton.bounds.Y - this.ScrollBar.bounds.Height - 4;
            }
            this.UpdateSlots();
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.SlotPosition > 0)
            {
                this.UpArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (direction >= 0 || this.SlotPosition >= Math.Max(0, this.Sprites.Count - 5))
                    return;
                this.DownArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        public void UpArrowPressed()
        {
            --this.SlotPosition;
            this.UpdateSlots();
            this.UpButton.scale = 3.5f;
            this.SetScrollBarToCurrentIndex();
        }

        public void DownArrowPressed()
        {
            ++this.SlotPosition;
            this.UpdateSlots();
            this.DownButton.scale = 3.5f;
            this.SetScrollBarToCurrentIndex();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.UpButton.containsPoint(x, y) && this.SlotPosition > 0)
            {
                this.UpArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.DownButton.containsPoint(x, y) && this.SlotPosition < this.Sprites.Count - 5)
            {
                this.DownArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.ScrollBar.containsPoint(x, y))
                this.Scrolling = true;
            else if (!this.DownButton.containsPoint(x, y) && x > this.xPositionOnScreen + this.width - 96 && (x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.Scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            this.SlotPosition = Math.Max(0, Math.Min(this.Sprites.Count - 5, this.SlotPosition));
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            this.HoverText = "";
            this.UpButton.tryHover(x, y);
            this.DownButton.tryHover(x, y);
        }

        private void DrawNpcSlot(SpriteBatch b, int i)
        {
            this.Sprites[i].draw(b);
            var animal = this.Animals[(long)this.Names[i]];
            string name = animal.Name;
            int heartLevelForNpc = animal.friendshipTowardFarmer.Value;
            float y = Game1.smallFont.MeasureString("W").Y;
            float num = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? (float)(-(double)y / 2.0) : 0.0f;

            int n = 0;
            if (animal.Sprite.SourceRect.Height == 16)
                n = 16;

            double loveLevel = heartLevelForNpc / 1000.0;
            int num3 = loveLevel * 1000.0 % 200.0 >= 100.0 ? (int)(loveLevel * 1000.0 / 200.0) : -100;
            b.DrawString(Game1.dialogueFont, name, new Vector2(this.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96 - Game1.dialogueFont.MeasureString(name).X / 2f, (float)(this.Sprites[i].bounds.Y + 48 + (double)num - 20.0)), Game1.textColor);
            for (int index = 0; index < 5; ++index)
            {
                b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + 320 - 8 + index * 32, this.Sprites[i].bounds.Y - n + 64 - 28), new Rectangle(211 + (loveLevel * 1000.0 <= ((index + 1) * 195) ? 7 : 0), 428, 7, 6), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
                if (num3 == index)
                    b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + 320 - 8 + index * 32, this.Sprites[i].bounds.Y - n + 64 - 28), new Rectangle(211, 428, 4, 6), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.891f);
            }

            if (!animal.wasPet.Value)
                b.DrawString(Game1.dialogueFont, I18n.NeedsPetting(), new Vector2(this.xPositionOnScreen + 250 + 264, this.Sprites[i].bounds.Y + 32 - 12), Game1.textColor);
        }

        private int RowPosition(int i)
        {
            int num1 = i - this.SlotPosition;
            int num2 = 112;
            return this.yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + num1 * num2;
        }

        public override void draw(SpriteBatch b)
        {
            using RasterizerState rasterizerState = new RasterizerState
            {
                ScissorTestEnable = true
            };

            b.End();
            b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, true);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, true);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, true);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, true);
            Rectangle scissorRectangle = b.GraphicsDevice.ScissorRectangle;
            Rectangle rectangle = scissorRectangle;
            rectangle.Y = Math.Max(0, this.RowPosition(0 - 1));
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
            for (int slotPosition = this.SlotPosition; slotPosition < this.SlotPosition + 5; ++slotPosition)
            {
                if (slotPosition < this.Sprites.Count)
                {
                    if (this.Names[slotPosition] is long)
                        this.DrawNpcSlot(b, slotPosition);
                }
            }
            this.UpButton.draw(b);
            this.DownButton.draw(b);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.ScrollBarRunner.X, this.ScrollBarRunner.Y, this.ScrollBarRunner.Width, this.ScrollBarRunner.Height, Color.White, 4f);
            this.ScrollBar.draw(b);
            if (!this.HoverText.Equals(""))
                IClickableMenu.drawHoverText(b, this.HoverText, Game1.smallFont);
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }
    }
}
