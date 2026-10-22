#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
         class Textbox : Element
    {
        /*********
        ** Fields
        *********/
        private readonly TextBox StardewTextBox;
        private bool lastPressed = false;


        /*********
        ** Accessors
        *********/
        public virtual string String
        {
            get
            {
                StringBuilder sb = new();
                foreach (var c in StardewTextBox.Contents)
                    c.ToText(sb, 0, c.CharacterCount);
                return sb.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    StardewTextBox.ClearContents();
                else
                    StardewTextBox.SetContents(new TextBoxTextSnippet(StardewTextBox.Font, StardewTextBox.TextColor, value));
            }
        }

        public bool Selected
        {
            get => this.StardewTextBox.Selected;
            set => this.StardewTextBox.Selected = value;
        }

        public Action<Element>? Callback { get; set; }

        /// <inheritdoc />
        public override int Width => 192;

        /// <inheritdoc />
        public override int Height => 48;


        /*********
        ** Public methods
        *********/
        public Textbox()
        {
            this.StardewTextBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor);
            this.StardewTextBox.FilterText += FilterInputText;
            this.StardewTextBox.OnTextChanged += OnInputTextChanged;
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            StardewTextBox.X = (int)Position.X;
            StardewTextBox.Y = (int)Position.Y;
            StardewTextBox.Width = Width;
            StardewTextBox.Height = Height;
            StardewTextBox.Hover(Game1.getMouseX(), Game1.getMouseY());

            bool pressed = false;
            if (!IsHidden(isOffScreen))
            {
                pressed = pressed || Game1.input.GetMouseState().LeftButton == ButtonState.Pressed;
                pressed = pressed || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.A));

                if (!lastPressed && pressed)
                    StardewTextBox.receiveLeftClick();
                else if (lastPressed && !pressed)
                    StardewTextBox.releaseLeftClick();
                else if (lastPressed && pressed)
                    StardewTextBox.leftClickHeld();
            }
            lastPressed = pressed;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            StardewTextBox.Draw(b);
        }


        /*********
        ** Protected methods
        *********/
        protected virtual void FilterInputText(TextBox _, List<TextBoxSnippet> contents)
        {
        }
        protected virtual void OnInputTextChanged(TextBox _, List<TextBoxSnippet> oldContents, List<TextBoxSnippet> newContents)
        {
            this.Callback?.Invoke(this);
            ScreenReaderText = String;
        }
    }
}
#endif
