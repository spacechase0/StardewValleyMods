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
         class Scrollbar : Element
    {
        /*********
        ** Fields
        *********/
        private bool DragScroll;


        /*********
        ** Accessors
        *********/
        public int RequestHeight { get; set; }

        public int Rows { get; set; }
        public int FrameSize { get; set; }

        public int TopRow { get; private set; }
        public int MaxTopRow => Math.Max(0, this.Rows - this.FrameSize);

        public float ScrollPercent => (this.MaxTopRow > 0) ? this.TopRow / (float)this.MaxTopRow : 0f;

        /// <inheritdoc />
        public override int Width => 24;

        /// <inheritdoc />
        public override int Height => this.RequestHeight;


        /*********
        ** Public methods
        *********/
        public void ScrollBy(int amount)
        {
            int row = Util.Clamp(0, this.TopRow + amount, this.MaxTopRow);
            if (row != this.TopRow)
            {
                Game1.playSound("shwip");
                this.TopRow = row;
            }
        }

        public void ScrollTo(int row)
        {
            if (this.TopRow != row)
            {
                Game1.playSound("shiny4");
                this.TopRow = Util.Clamp(0, row, this.MaxTopRow);
            }
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            if (this.Clicked)
                this.DragScroll = true;
            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                if (this.DragScroll && Mouse.GetState().LeftButton == ButtonState.Released)
                    this.DragScroll = false;
            }
            else
            {
                if (this.DragScroll && Game1.input.GetMouseState().LeftButton == ButtonState.Released)
                    this.DragScroll = false;
            }


            if (this.DragScroll)
            {
                int my = Game1.getMouseY();
                int relY = (int)(my - this.Position.Y - 40 / 2);
                this.ScrollTo((int)Math.Round(relY / (float)(this.Height - 40) * this.MaxTopRow));
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            // Don't draw a scrollbar if scrolling is (currently) not possible.
            if (this.MaxTopRow == 0)
                return;

            Rectangle back = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
            Vector2 front = new Vector2(back.X, back.Y + (this.Height - 40) * this.ScrollPercent);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, front, new Rectangle(435, 463, 6, 12), Color.White, 0f, new Vector2(), Game1.pixelZoom, SpriteEffects.None, 0.77f);
        }
    }
}
