using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI
{
    internal class Table : Container
    {
        /*********
        ** Fields
        *********/
        private readonly List<Element[]> Rows = new();

        private Vector2 SizeImpl;

        private const int RowPadding = 16;
        private int RowHeightImpl;


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
        public Table()
        {
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
            foreach (var child in elements)
            {
                this.AddChild(child);
            }
            this.UpdateScrollbar();
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);
            if (this.IsHidden(isOffScreen))
                return;

            int ir = 0;
            foreach (var row in this.Rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * this.RowHeight - this.Scrollbar.TopRow * this.RowHeight);
                    if (element is not Label && // Labels must update anyway to get rid of hovertext on scrollwheel
                            ElementIsOffscreen(element))
                        continue;
                    element.Update();
                }
                ++ir;
            }
            this.Scrollbar.Update();
        }

        public void ForceUpdateEvenHidden(bool isOffScreen = false)
        {
            int ir = 0;
            foreach (var row in this.Rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * this.RowHeight - this.Scrollbar.ScrollPercent * this.Rows.Count * this.RowHeight);
                    element.Update(isOffScreen || ElementIsOffscreen(element));
                }
                ++ir;
            }
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

            // draw table contents
            // This uses a scissor rectangle to clip content taller than one row that might be
            // drawn past the bottom of the UI, like images or complex options.
            this.InScissorRectangle(b, contentArea, contentBatch =>
            {
                foreach (var row in this.Rows)
                {
                    foreach (var element in row)
                    {
                        if (ElementIsOffscreen(element))
                            continue;
                        if (element == this.RenderLast)
                            continue;
                        element.Draw(contentBatch);
                    }
                }

                this.RenderLast?.Draw(contentBatch);
            });

            this.Scrollbar.Draw(b);
        }


        /*********
        ** Private methods
        *********/
        private bool ElementIsOffscreen(Element element) {
            return element.Position.Y + element.Height < this.Position.Y || element.Position.Y + this.RowHeight - Table.RowPadding > this.Position.Y + this.Size.Y;
        }

        private void UpdateScrollbar()
        {
            this.Scrollbar.LocalPosition = new Vector2(this.Size.X + 48, this.Scrollbar.LocalPosition.Y);
            this.Scrollbar.RequestHeight = (int)this.Size.Y;
            this.Scrollbar.Rows = this.Rows.Count;
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
    }
}
