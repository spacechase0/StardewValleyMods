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

        /// <inheritdoc />
        public override int Width => 192;

        /// <inheritdoc />
        public override int Height => 48;


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

            b.Draw(this.Tex, this.Position, Color.White);

            // Copied from game code - caret
            string text = this.String;
            Vector2 vector2;
            for (vector2 = this.Font.MeasureString(text); vector2.X > 192f; vector2 = this.Font.MeasureString(text))
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
