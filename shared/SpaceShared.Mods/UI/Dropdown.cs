#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
        class Dropdown : Element, ISingleTexture
    {
        /*********
        ** Accessors
        *********/
        public int RequestWidth { get; set; }
        public int MaxValuesAtOnce { get; set; }
        public Texture2D Texture { get; set; } = Game1.mouseCursors;
        public Rectangle BackgroundTextureRect { get; set; } = OptionsDropDown.dropDownBGSource;
        public Rectangle ButtonTextureRect { get; set; } = OptionsDropDown.dropDownButtonSource;

        public string Value
        {
            get => this.Choices[this.ActiveChoice];
            set
            {
                if (!this.Choices.Contains(value))
                    return;

                this.ActiveChoice = Array.IndexOf(this.Choices, value);
                ScreenReaderText = Labels[ActiveChoice];
            }
        }

        public string Label => this.Labels[this.ActiveChoice];

        public int ActiveChoice { get; set; }

        public int ActivePosition { get; set; }
        public string[] Choices { get; set; } = new[] { "null" };

        public string[] Labels { get; set; } = new[] { "null" };

        public bool Dropped;

        public Action<Element>? Callback;

        /// <inheritdoc />
        public override int Width => Math.Max(300, Math.Min(500, this.RequestWidth));

        /// <inheritdoc />
        public override int Height => 44;

        /// <inheritdoc />
        public override string ClickedSound => "shwip";


        /*********
        ** Public methods
        *********/
        public override bool LeftClick(Point mousePos, bool pressed)
        {
            if (pressed)
            {
                if (Dropped)
                {
                    Game1.playSound("drumkit6");
                    Dropped = false;
                    if (Root.RenderLast == this)
                        Root.RenderLast = null;

                    int tall = Math.Min(this.MaxValuesAtOnce, this.Choices.Length - this.ActivePosition) * this.Height;
                    int drawY = Math.Min((int)Position.Y, Game1.uiViewport.Height - tall);
                    int choiceRelativeMouseY = (int)Position.Y + mousePos.Y - drawY;
                    if (new Rectangle(0, 0, Width, Height * MaxValuesAtOnce).Contains(mousePos.X, choiceRelativeMouseY))
                    {
                        this.ActiveChoice = (choiceRelativeMouseY - drawY) / Height + this.ActivePosition;
                        Callback?.Invoke(this);

                        ScreenReaderText = Labels[ActiveChoice];
                    }
                }
                else
                {
                    Game1.playSound("shwip");
                    Dropped = true;
                    Root.RenderLast = this;
                }
                Root.GamepadMovementRegionsDirty = true;
            }
            return true;
        }

        public override bool VerticalScroll(int amount)
        {
            base.VerticalScroll(amount);
            if (Dropped)
            {
                ActivePosition = Math.Min(Math.Max(ActivePosition - (amount / 120), 0), Choices.Length - MaxValuesAtOnce);
            }
            return true;
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            if (!Dropped)
                ActivePosition = Math.Min(ActiveChoice, Choices.Length - MaxValuesAtOnce);
        }

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this.Texture, this.BackgroundTextureRect, (int)this.Position.X, (int)this.Position.Y, this.Width - 48, this.Height, Color.White, 4, false);
            b.DrawString(Game1.smallFont, this.Label, new Vector2(this.Position.X + 4, this.Position.Y + 8), Game1.textColor);
            b.Draw(this.Texture, new Vector2(this.Position.X + this.Width - 48, this.Position.Y), this.ButtonTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);

            if (this.Dropped)
            {
                int maxValues = this.MaxValuesAtOnce;
                int start = this.ActivePosition;
                int end = Math.Min(this.Choices.Length, start + maxValues);
                int tall = Math.Min(maxValues, this.Choices.Length - this.ActivePosition) * this.Height;
                int drawY = Math.Min((int)this.Position.Y, Game1.uiViewport.Height - tall);
                IClickableMenu.drawTextureBox(b, this.Texture, this.BackgroundTextureRect, (int)this.Position.X, drawY, this.Width - 48, tall, Color.White, 4, false);
                for (int i = start; i < end; ++i)
                {
                    if (i == this.ActiveChoice)
                        b.Draw(Game1.staminaRect, new Rectangle((int)this.Position.X + 4, drawY + (i - this.ActivePosition) * this.Height, this.Width - 48 - 8, this.Height), null, Color.Wheat, 0, Vector2.Zero, SpriteEffects.None, 0.98f);
                    b.DrawString(Game1.smallFont, this.Labels[i], new Vector2(this.Position.X + 4, drawY + (i - this.ActivePosition) * this.Height + 8), Game1.textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                }
            }
        }

        public override IEnumerable<ClickableComponent> GetGamepadMovementRegions()
        {
            if (!Dropped)
            {
                foreach (var val in base.GetGamepadMovementRegions())
                    yield return val;
                yield break;
            }

            int maxValues = MaxValuesAtOnce;
            int start = ActivePosition;
            int end = Math.Min(Choices.Length, start + maxValues);
            int tall = Math.Min(maxValues, this.Choices.Length - this.ActivePosition) * this.Height;
            int drawY = Math.Min((int)this.Position.Y, Game1.uiViewport.Height - tall);
            for (int i = Math.Max(start - 1, 0); i < Math.Min(end + 1, Choices.Length); ++i)
            {
                yield return new ElementClickableComponent(this, new Rectangle((int)this.Position.X + 4, drawY + (i - this.ActivePosition) * this.Height, this.Width - 48 - 8, this.Height), Choices[i])
                {
                    leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                    rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                    upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                    downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                    ScreenReaderText = Labels[i],
                };
            }
        }

        public override bool CurrentlyUsingGamepadMovement(out bool allowSnappyMovement)
        {
            allowSnappyMovement = true;
            return Dropped;
        }
    }
}
#endif
