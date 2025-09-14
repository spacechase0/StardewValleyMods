using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
         class Table : Container
    {
        /*********
        ** Fields
        *********/
        private readonly List<Element[]> Rows = new();

        private Vector2 SizeImpl;

        private const int RowPadding = 16;
        private int RowHeightImpl;
        private bool FixedRowHeight;
        private int ContentHeight;


        /*********
        ** Accessors
        *********/
        public Vector2 Size
        {
            get => this.SizeImpl;
            set
            {
                this.SizeImpl = new Vector2(value.X, ((int)value.Y) / this.RowHeight * this.RowHeight);
                this.UpdateScrollbar();
            }
        }

        public int RowHeight
        {
            get => this.RowHeightImpl;
            set
            {
                this.RowHeightImpl = value + Table.RowPadding;
                this.UpdateScrollbar();
            }
        }

        public int RowCount => this.Rows.Count;

        public Scrollbar Scrollbar { get; }

        /// <inheritdoc />
        public override int Width => (int)this.Size.X;

        /// <inheritdoc />
        public override int Height => (int)this.Size.Y;


        /*********
        ** Public methods
        *********/
        public Table(bool fixedRowHeight = true)
        {
            this.FixedRowHeight = fixedRowHeight;
            this.UpdateChildren = false; // table will update children itself
            this.Scrollbar = new Scrollbar
            {
                LocalPosition = new Vector2(0, 0)
            };
            this.AddChild(this.Scrollbar);
        }

        public void AddRow(Element[] elements)
        {
            this.Rows.Add(elements);
            int maxElementHeight = 0;
            foreach (var child in elements)
            {
                this.AddChild(child);
                maxElementHeight = Math.Max(maxElementHeight, child.Height);
            }
            this.ContentHeight += this.FixedRowHeight ? this.RowHeight : maxElementHeight + RowPadding;
            this.UpdateScrollbar();
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);
            if (this.IsHidden(isOffScreen))
                return;

            int topPx = 0;
            foreach (var row in this.Rows)
            {
                int maxElementHeight = 0;
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, topPx - this.Scrollbar.TopRow * this.RowHeight);
                    bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(element);

                    if (!isChildOffScreen || element is Label) // Labels must update anyway to get rid of hovertext on scrollwheel
                        element.Update(isOffScreen: isChildOffScreen);
                    maxElementHeight = Math.Max(maxElementHeight, element.Height);
                }
                topPx += this.FixedRowHeight ? this.RowHeight : maxElementHeight + RowPadding;
            }

            if (topPx != this.ContentHeight) {
                this.ContentHeight = topPx;
                this.Scrollbar.Rows = PxToRow(this.ContentHeight);
            }

            this.Scrollbar.Update();
        }

        public void ForceUpdateEvenHidden(bool isOffScreen = false)
        {
            int topPx = 0;
            foreach (var row in this.Rows)
            {
                int maxElementHeight = 0;
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, topPx - this.Scrollbar.ScrollPercent * this.Rows.Count * this.RowHeight);
                    bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(element);

                    element.Update(isOffScreen: isChildOffScreen);
                    maxElementHeight = Math.Max(maxElementHeight, element.Height);
                }
                topPx += this.FixedRowHeight ? this.RowHeight : maxElementHeight + RowPadding;
            }
            this.ContentHeight = topPx;
            this.Scrollbar.Update(isOffScreen);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            // calculate draw area
            var backgroundArea = new Rectangle((int)this.Position.X - 32, (int)this.Position.Y - 32, (int)this.Size.X + 64, (int)this.Size.Y + 64);
            int contentPadding = 12;
            var contentArea = new Rectangle(backgroundArea.X + contentPadding, backgroundArea.Y + contentPadding, backgroundArea.Width - contentPadding * 2, backgroundArea.Height - contentPadding * 2);

            // draw background
            IClickableMenu.drawTextureBox(b, backgroundArea.X, backgroundArea.Y, backgroundArea.Width, backgroundArea.Height, Color.White);
            b.Draw(Game1.menuTexture, contentArea, new Rectangle(64, 128, 64, 64), Color.White); // Smoother gradient for the content area.

            // draw table contents
            // This uses a scissor rectangle to clip content taller than one row that might be
            // drawn past the bottom of the UI, like images or complex options.
            Element? renderLast = null;
            this.InScissorRectangle(b, contentArea, contentBatch =>
            {
                foreach (var row in this.Rows)
                {
                    foreach (var element in row)
                    {
                        if (this.IsElementOffScreen(element))
                            continue;
                        if (element == this.RenderLast) {
                            renderLast = element;
                            continue;
                        }
                        element.Draw(contentBatch);
                    }
                }
            });
            renderLast?.Draw(b);

            this.Scrollbar.Draw(b);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a child element is outside the table's current display area.</summary>
        /// <param name="element">The child element to check.</param>
        private bool IsElementOffScreen(Element element)
        {
            return
                element.Position.Y + element.Height < this.Position.Y
                || element.Position.Y > this.Position.Y + this.Size.Y;
        }

        private void UpdateScrollbar()
        {
            this.Scrollbar.LocalPosition = new Vector2(this.Size.X + 48, this.Scrollbar.LocalPosition.Y);
            this.Scrollbar.RequestHeight = (int)this.Size.Y;
            this.Scrollbar.Rows = PxToRow(this.ContentHeight);
            this.Scrollbar.FrameSize = (int)(this.Size.Y / this.RowHeight);
        }

        private void InScissorRectangle(SpriteBatch spriteBatch, Rectangle area, Action<SpriteBatch> draw)
        {
            // render the current sprite batch to the screen
            spriteBatch.End();

            // start temporary sprite batch
            using SpriteBatch contentBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
            GraphicsDevice device = Game1.graphics.GraphicsDevice;
            Rectangle prevScissorRectangle = device.ScissorRectangle;

            // render in scissor rectangle
            try
            {
                device.ScissorRectangle = area;
                contentBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);

                draw(contentBatch);

                contentBatch.End();
            }
            finally
            {
                device.ScissorRectangle = prevScissorRectangle;
            }

            // resume previous sprite batch
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        }

        private int PxToRow(int px)
        {
            return (px + this.RowHeight - 1) / this.RowHeight;
        }
    }
}
