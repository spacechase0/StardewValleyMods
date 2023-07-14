using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
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
         class Slider : Element
    {
        /*********
        ** Fields
        *********/
        protected bool Dragging;


        /*********
        ** Accessors
        *********/
        public int RequestWidth { get; set; }

        public Action<Element> Callback { get; set; }

        /// <inheritdoc />
        public override int Width => this.RequestWidth;

        /// <inheritdoc />
        public override int Height => 24;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Draw(SpriteBatch b) { }
    }

    internal class Slider<T> : Slider
    {
        /*********
        ** Accessors
        *********/
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Value { get; set; }

        public T Interval { get; set; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            if (this.Clicked)
                this.Dragging = true;
            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Released && Game1.input.GetGamePadState().Buttons.A == ButtonState.Released)
                    this.Dragging = false;
            }
            else
            {
                if (Game1.input.GetMouseState().LeftButton == ButtonState.Released && Game1.input.GetGamePadState().Buttons.A == ButtonState.Released)
                    this.Dragging = false;
            }


            if (this.Dragging)
            {
                float perc = (Game1.getOldMouseX() - this.Position.X) / this.Width;
                this.Value = Util.Adjust(this.Value, this.Interval);
                this.Value = this.Value switch
                {
                    int => Util.Clamp<T>(this.Minimum, (T)(object)(int)(perc * ((int)(object)this.Maximum - (int)(object)this.Minimum) + (int)(object)this.Minimum), this.Maximum),
                    float => Util.Clamp<T>(this.Minimum, (T)(object)(perc * ((float)(object)this.Maximum - (float)(object)this.Minimum) + (float)(object)this.Minimum), this.Maximum),
                    _ => this.Value
                };

                this.Callback?.Invoke(this);
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            float perc = this.Value switch
            {
                int => ((int)(object)this.Value - (int)(object)this.Minimum) / (float)((int)(object)this.Maximum - (int)(object)this.Minimum),
                float => ((float)(object)this.Value - (float)(object)this.Minimum) / ((float)(object)this.Maximum - (float)(object)this.Minimum),
                _ => 0
            };

            Rectangle back = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
            Rectangle front = new Rectangle((int)(this.Position.X + perc * (this.Width - 40)), (int)this.Position.Y, 40, this.Height);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, new Vector2(front.X, front.Y), new Rectangle(420, 441, 10, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        }
    }
}
