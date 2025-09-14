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
         class ScrollContainer : StaticContainer
    {
        private int ContentHeight;

        public Scrollbar Scrollbar { get; }

        /// <inheritdoc />
        public override int Width => (int)this.Size.X;

        /// <inheritdoc />
        public override int Height => (int)this.Size.Y;


        /*********
        ** Public methods
        *********/
        public ScrollContainer()
        {
            this.UpdateChildren = false; // table will update children itself
            this.Scrollbar = new Scrollbar
            {
                LocalPosition = new Vector2(0, 0)
            };
            this.AddChild(this.Scrollbar);
        }
        public override void OnChildrenChanged()
        {
            int topPx = 0;
            foreach (var child in Children)
            {
                if (child == Scrollbar)
                    continue;

                topPx = Math.Max(topPx, child.Bounds.Y + child.Bounds.Height);
            }

            if (topPx != this.ContentHeight)
            {
                this.ContentHeight = topPx;
                this.Scrollbar.Rows = PxToRow(this.ContentHeight);
            }

            UpdateScrollbar();
        }

        public int lastScroll = 0; // Feeling lazy, make this public for now and do a proper solution later
        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);
            if (this.IsHidden(isOffScreen))
                return;

            if (lastScroll != Scrollbar.TopRow)
            {
                float diff = (lastScroll * 50) - (Scrollbar.TopRow * 50);
                lastScroll = Scrollbar.TopRow;

                foreach (var child in Children)
                {
                    if (child == Scrollbar)
                        continue;
                    child.LocalPosition = new Vector2(child.LocalPosition.X, child.LocalPosition.Y + diff);
                }
            }

            foreach (var child in Children)
            {
                if (child == Scrollbar)
                    continue;

                bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(child);
                if (!isChildOffScreen || child is Label)
                    child.Update(isOffScreen: isChildOffScreen);
            }

            this.Scrollbar.Update();
        }

        public void ForceUpdateEvenHidden(bool isOffScreen = false)
        {
            if (lastScroll != Scrollbar.TopRow)
            {
                float diff = (lastScroll * 50) - (Scrollbar.TopRow * 50);
                lastScroll = Scrollbar.TopRow;

                foreach (var child in Children)
                {
                    if (child == Scrollbar)
                        continue;
                    child.LocalPosition = new Vector2(child.LocalPosition.X, child.LocalPosition.Y + diff);
                }
            }

            foreach (var child in Children)
            {
                if (child == Scrollbar)
                    continue;

                bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(child);
                if (!isChildOffScreen || child is Label)
                    child.Update(isOffScreen: isChildOffScreen);
            }

            this.Scrollbar.Update(isOffScreen);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            if (this.OutlineColor.HasValue)
            {
                IClickableMenu.drawTextureBox(b, (int)this.Position.X - 12, (int)this.Position.Y - 12, this.Width + 24, this.Height + 24, this.OutlineColor.Value);
            }

            // calculate draw area
            var backgroundArea = new Rectangle((int)this.Position.X - 32, (int)this.Position.Y - 32, (int)this.Size.X + 64, (int)this.Size.Y + 64);
            int contentPadding = 12;
            var contentArea = new Rectangle(backgroundArea.X + 20 + contentPadding, backgroundArea.Y + 20 + contentPadding, backgroundArea.Width - 40 - contentPadding * 2, backgroundArea.Height - 40 - contentPadding * 2);

            // draw table contents
            // This uses a scissor rectangle to clip content taller than one row that might be
            // drawn past the bottom of the UI, like images or complex options.
            Element? renderLast = null;
            this.InScissorRectangle(b, contentArea, contentBatch =>
            {
                foreach (var child in Children)
                {
                    if (child == Scrollbar)
                        continue;
                    if (this.IsElementOffScreen(child))
                        continue;
                    if (child == this.RenderLast)
                    {
                        renderLast = child;
                        continue;
                    }
                    child.Draw(contentBatch);
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
            this.Scrollbar.FrameSize = (int)(this.Size.Y / 50);
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
            return (px + 50 - 1) / 50;
        }
    }
}
