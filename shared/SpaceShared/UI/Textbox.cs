using System;
using System.Collections.Generic;
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

        private float DrawPositionStart;
        private KeyValuePair<Keys, long>? PreviousKeyState;


        /*********
        ** Accessors
        *********/
        private string _string;
        public virtual string String
        {
            get => _string;
            set
            {
                int diff = _string?.Length ?? 0;
                _string = value;
                int curr = _string?.Length ?? 0;
                diff = curr - diff;
                if (Caret + diff < 0) Caret = 0;
                else if (Caret + diff > curr) Caret = curr;
                else Caret += diff;
            }
        }

        public int Caret { get; set; }

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
            Update_KeyDown();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            b.Draw(this.Tex, this.Position, Color.White);

            int maxTextWidth = Width - 33;
            string text = this.String;
            Vector2 textWidth = this.Font.MeasureString(text);
            Vector2 vectorCaret = this.Font.MeasureString(text[..Caret]);
            if (textWidth.X > maxTextWidth)
            {
                float viewDist = DrawPositionStart - vectorCaret.X;
                if (viewDist < 0f) //start is before Caret
                {
                    viewDist = -viewDist;
                    if (viewDist > maxTextWidth)
                    {
                        DrawPositionStart += viewDist - maxTextWidth;
                    }
                }
                else //start is after Caret
                {
                    DrawPositionStart = vectorCaret.X;
                }
                float drawPositionEnd = DrawPositionStart + maxTextWidth;

                text = "";
                string tmp = "";
                for (int i = 0; i < this.String.Length; i++)
                {
                    char c = this.String[i];
                    tmp += c;

                    float tmpLength = this.Font.MeasureString(tmp).X;
                    if (tmpLength >= DrawPositionStart)
                    {
                        if (tmpLength <= drawPositionEnd)
                        {
                            text += c;
                            if (i + 1 == Caret) vectorCaret.X = this.Font.MeasureString(text).X;
                        }
                        else break;
                    }
                }

                //bool caretInView = maxTextWidth > vectorCaret.X;
                //int index = 0;
                //for (Vector2 vectorWidth = textWidth; vectorWidth.X > maxTextWidth; vectorWidth = this.Font.MeasureString(text))
                //{
                //    index++;
                //    if (!caretInView)
                //    {
                //        text = this.String[index..Caret];
                //        vectorCaret = this.Font.MeasureString(text);
                //        caretInView = maxTextWidth > vectorCaret.X;
                //    }
                //    else text = text[..^1];
                //}
            }
            else DrawPositionStart = 0f;
            // Copied from game code
            if (this.Selected && (DateTime.UtcNow.Millisecond % 1000 >= 500 || PreviousKeyState?.Value + 1600000 > DateTime.UtcNow.Ticks))
                b.Draw(Game1.staminaRect, new Rectangle((int)this.Position.X + 16 + (int)vectorCaret.X - 1, (int)this.Position.Y + 8, 3, 32), Game1.textColor);

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
            if (command == '\b' && this.String.Length > 0 && Caret > 0)
            {
                Game1.playSound("tinyWhip");
                this.String = this.String[..(Caret - 1)] + this.String[Caret..];
                this.Callback?.Invoke(this);
            }
        }
        private void Update_KeyDown()
        {
            if (Selected)
            {
                var state = Keyboard.GetState();
                var keys = state.GetPressedKeys();
                if (keys.Length != 0)
                {
                    Keys key = keys[0];
                    if (PreviousKeyState is null || PreviousKeyState.Value.Key != key || PreviousKeyState.Value.Value + 1600000 < DateTime.UtcNow.Ticks)
                    {
                        if (key == Keys.End || key == Keys.NumPad1 && !state.NumLock) Caret = this.String.Length;
                        else if (key == Keys.Home || key == Keys.NumPad7 && !state.NumLock) Caret = 0;
                        else if (key == Keys.Left)
                        {
                            if (Caret > 0) Caret--;
                        }
                        else if (key == Keys.Right)
                        {
                            if (Caret < this.String.Length) Caret++;
                        }
                        else if (key == Keys.Delete && Caret < this.String.Length)
                        {
                            Game1.playSound("tinyWhip");
                            int prev = Caret;
                            this.String = this.String[..Caret] + this.String[(Caret + 1)..];
                            if (prev != 0) Caret++;
                            this.Callback?.Invoke(this);
                        }
                        PreviousKeyState = new(key, DateTime.UtcNow.Ticks);
                    }
                }
            }
        }


        /// <inheritdoc />
        public void RecieveSpecialInput(Keys key) { }


        /*********
        ** Protected methods
        *********/
        protected virtual void ReceiveInput(string str)
        {
            this.String = this.String[..Caret] + str + this.String[Caret..];
            this.Callback?.Invoke(this);
        }
    }
}
