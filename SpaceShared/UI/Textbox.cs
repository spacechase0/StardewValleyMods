using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         class Textbox : Element, IKeyboardSubscriber
    {
        /*********
        ** Fields
        *********/
        private readonly Texture2D Tex;
        private readonly SpriteFont Font;
        private bool SelectedImpl;


        /*********
        ** Accessors
        *********/
        public virtual string String { get; set; }

        public bool Selected
        {
            get => this.SelectedImpl;
            set
            {
                if (this.SelectedImpl == value)
                    return;

                this.SelectedImpl = value;
                if (this.SelectedImpl)
                    Game1.keyboardDispatcher.Subscriber = this;
                else
                {
                    if (Game1.keyboardDispatcher.Subscriber == this)
                        Game1.keyboardDispatcher.Subscriber = null;
                }
            }
        }

        public Action<Element> Callback { get; set; }

        public int SetWidth { get; set; } = 192;
        public int SetHeight { get; set; } = 48;
        /// <inheritdoc />
        public override int Width => SetWidth;

        /// <inheritdoc />
        public override int Height => SetHeight;


        /*********
        ** Public methods
        *********/
        public Textbox()
        {
            this.Tex = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            this.Font = Game1.smallFont;
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            if (this.ClickGestured && this.Callback != null)
            {
                this.Selected = this.Hover;
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            if (this.Width == Tex.Width && this.Height == Tex.Height)
            {
                b.Draw(this.Tex, this.Position, Color.White);
            }
            else
            {
                var textureWidthMinus16 = this.Tex.Width - 16;
                var textureHeightMinus16 = this.Tex.Height - 16;
                var textureWidthMinus32 = this.Tex.Width - 32;
                var textureHeightMinus32 = this.Tex.Height - 32;
                // Draw the corners
                b.Draw(this.Tex, new Rectangle((int)this.Position.X, (int)this.Position.Y, 16, 16), new Rectangle(0, 0, 16, 16), Color.White);
                b.Draw(this.Tex, new Rectangle((int)this.Position.X + this.Width - 16, (int)this.Position.Y, 16, 16), new Rectangle(textureWidthMinus16, 0, 16, 16), Color.White);
                b.Draw(this.Tex, new Rectangle((int)this.Position.X, (int)this.Position.Y + this.Height - 16, 16, 16), new Rectangle(0, textureHeightMinus16, 16, 16), Color.White);
                b.Draw(this.Tex, new Rectangle((int)this.Position.X + this.Width - 16, (int)this.Position.Y + this.Height - 16, 16, 16), new Rectangle(textureWidthMinus16, textureHeightMinus16, 16, 16), Color.White);
                // Draw the edges
                b.Draw(this.Tex, new Rectangle((int)this.Position.X + 16, (int)this.Position.Y, this.Width - 32, 16), new Rectangle(16, 0, textureWidthMinus32, 16), Color.White);
                b.Draw(this.Tex, new Rectangle((int)this.Position.X + 16, (int)this.Position.Y + this.Height - 16, this.Width - 32, 16), new Rectangle(16, textureHeightMinus16, textureWidthMinus32, 16), Color.White);
                b.Draw(this.Tex, new Rectangle((int)this.Position.X, (int)this.Position.Y + 16, 16, this.Height - 32), new Rectangle(0, 16, 16, textureHeightMinus32), Color.White);
                b.Draw(this.Tex, new Rectangle((int)this.Position.X + this.Width - 16, (int)this.Position.Y + 16, 16, this.Height - 32), new Rectangle(textureWidthMinus16, 16, 16, textureHeightMinus32), Color.White);
                // Draw the center
                b.Draw(this.Tex, new Rectangle((int)this.Position.X + 16, (int)this.Position.Y + 16, this.Width - 32, this.Height - 32), new Rectangle(16, 16, textureWidthMinus32, textureHeightMinus32), Color.White);
            }

            // Copied from game code - caret
            string text = this.String;
            Vector2 vector2;
            for (vector2 = this.Font.MeasureString(text); vector2.X > this.Width; vector2 = this.Font.MeasureString(text))
                text = text.Substring(1);
            if (DateTime.UtcNow.Millisecond % 1000 >= 500 && this.Selected)
                b.Draw(Game1.staminaRect, new Rectangle((int)this.Position.X + 16 + (int)vector2.X + 2, (int)this.Position.Y + 8, 4, 32), Game1.textColor);

            b.DrawString(this.Font, text, this.Position + new Vector2(16, 12), Game1.textColor);
        }

        /// <inheritdoc />
        public void RecieveTextInput(char inputChar)
        {
            this.ReceiveInput(inputChar.ToString());

            // Copied from game code
            switch (inputChar)
            {
                case '"':
                    return;
                case '$':
                    Game1.playSound("money");
                    break;
                case '*':
                    Game1.playSound("hammer");
                    break;
                case '+':
                    Game1.playSound("slimeHit");
                    break;
                case '<':
                    Game1.playSound("crystal");
                    break;
                case '=':
                    Game1.playSound("coin");
                    break;
                default:
                    Game1.playSound("cowboy_monsterhit");
                    break;
            }
        }

        /// <inheritdoc />
        public void RecieveTextInput(string text)
        {
            this.ReceiveInput(text);
        }

        /// <inheritdoc />
        public void RecieveCommandInput(char command)
        {
            if (command == '\b' && this.String.Length > 0)
            {
                Game1.playSound("tinyWhip");
                this.String = this.String.Substring(0, this.String.Length - 1);
                this.Callback?.Invoke(this);
            }
        }

        /// <inheritdoc />
        public void RecieveSpecialInput(Keys key) { }


        /*********
        ** Protected methods
        *********/
        protected virtual void ReceiveInput(string str)
        {
            this.String += str;
            this.Callback?.Invoke(this);
        }
    }
}
