using System;
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
            set { if (this.Choices.Contains(value)) this.ActiveChoice = Array.IndexOf(this.Choices, value); }
        }

        public string Label => this.Labels[this.ActiveChoice];

        public int ActiveChoice { get; set; }

        public int ActivePosition { get; set; }
        public string[] Choices { get; set; } = new[] { "null" };

        public string[] Labels { get; set; } = new[] { "null" };

        public bool Dropped;

        public Action<Element> Callback;

        public static Dropdown ActiveDropdown;
        public static int SinceDropdownWasActive = 0;

        /// <inheritdoc />
        public override int Width => Math.Max(300, Math.Min(500, this.RequestWidth));

        /// <inheritdoc />
        public override int Height => 44;

        /// <inheritdoc />
        public override string ClickedSound => "shwip";


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            bool justClicked = false;
            if (this.Clicked && ActiveDropdown == null)
            {
                justClicked = true;
                this.Dropped = true;
                this.Parent.RenderLast = this;
            }

            if (this.Dropped)
            {
                //if (Mouse.GetState().LeftButton == ButtonState.Released)
                if (Constants.TargetPlatform != GamePlatform.Android)
                {
                    if ((Mouse.GetState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released ||
                         Game1.input.GetGamePadState().Buttons.A == ButtonState.Pressed && Game1.oldPadState.Buttons.A == ButtonState.Released)
                        && !justClicked)
                    {
                        Game1.playSound("drumkit6");
                        this.Dropped = false;
                        if (this.Parent.RenderLast == this)
                            this.Parent.RenderLast = null;
                    }
                }
                else
                {
                    if ((Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released ||
                         Game1.input.GetGamePadState().Buttons.A == ButtonState.Pressed && Game1.oldPadState.Buttons.A == ButtonState.Released)
                        && !justClicked)
                    {
                        Game1.playSound("drumkit6");
                        this.Dropped = false;
                        if (this.Parent.RenderLast == this)
                            this.Parent.RenderLast = null;
                    }
                }

                int tall = Math.Min(this.MaxValuesAtOnce, this.Choices.Length - this.ActivePosition) * this.Height;
                int drawY = Math.Min((int)this.Position.Y, Game1.uiViewport.Height - tall);
                var bounds2 = new Rectangle((int)this.Position.X, drawY, this.Width, this.Height * this.MaxValuesAtOnce);
                if (bounds2.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                {
                    int choice = (Game1.getOldMouseY() - drawY) / this.Height;
                    this.ActiveChoice = choice + this.ActivePosition;

                    this.Callback?.Invoke(this);
                }
            }

            if (this.Dropped)
            {
                Dropdown.ActiveDropdown = this;
                Dropdown.SinceDropdownWasActive = 3;
            }
            else
            {
                if (Dropdown.ActiveDropdown == this)
                    Dropdown.ActiveDropdown = null;
                this.ActivePosition = Math.Min(this.ActiveChoice, this.Choices.Length - this.MaxValuesAtOnce);
            }
        }

        public void ReceiveScrollWheelAction(int direction)
        {
            if (this.Dropped)
                this.ActivePosition = Math.Min(Math.Max(this.ActivePosition - (direction / 120), 0), this.Choices.Length - this.MaxValuesAtOnce);
            else
                Dropdown.ActiveDropdown = null;
        }

        public void DrawOld(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this.Texture, this.BackgroundTextureRect, (int)this.Position.X, (int)this.Position.Y, this.Width - 48, this.Height, Color.White, 4, false);
            b.DrawString(Game1.smallFont, this.Value, new Vector2(this.Position.X + 4, this.Position.Y + 8), Game1.textColor);
            b.Draw(this.Texture, new Vector2(this.Position.X + this.Width - 48, this.Position.Y), this.ButtonTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);

            if (this.Dropped)
            {
                int tall = this.Choices.Length * this.Height;
                IClickableMenu.drawTextureBox(b, this.Texture, this.BackgroundTextureRect, (int)this.Position.X, (int)this.Position.Y, this.Width - 48, tall, Color.White, 4, false);
                for (int i = 0; i < this.Choices.Length; ++i)
                {
                    if (i == this.ActiveChoice)
                        b.Draw(Game1.staminaRect, new Rectangle((int)this.Position.X + 4, (int)this.Position.Y + i * this.Height, this.Width - 48 - 8, this.Height), null, Color.Wheat, 0, Vector2.Zero, SpriteEffects.None, 0.98f);
                    b.DrawString(Game1.smallFont, this.Choices[i], new Vector2(this.Position.X + 4, this.Position.Y + i * this.Height + 8), Game1.textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                }
            }
        }

        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

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
    }
}
