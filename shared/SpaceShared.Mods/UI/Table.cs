#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
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
            Scrollbar.OnScrolled += (_, _, _) => GetRoot().GamepadMovementRegionsDirty = true;
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

        private ConditionalWeakTable<ClickableComponent, SpaceShared.Holder<bool>> modifiedRegions = new();
        public override IEnumerable<ClickableComponent> GetGamepadMovementRegions()
        {
            {
                var scroll = Scrollbar.GetGamepadMovementRegions().ToArray();
                int idCounter = 0;
                foreach (var entry in scroll)
                {
                    entry.myID = idCounter++;
                    yield return entry;
                }
            }

            int rowCounter = 0;
            List<ClickableComponent> prevRow = new();
            List<ClickableComponent> currRow = new();
            foreach (var row in Rows)
            {
                ++rowCounter;
                currRow.Clear();

                int idCounter = 0;
                foreach (var elem in row)
                {
                    var regions = elem.GetGamepadMovementRegions().ToArray();
                    foreach (var entry in regions)
                    {
                        var didMod = modifiedRegions.GetOrCreateValue(entry);
                        if (!didMod.Value)
                        {
                            didMod.Value = true;
                            entry.myID = rowCounter * 10 + idCounter++; // TODO: This won't work right if a refresh makes new ones appear
                        }

                        // TODO: May not work right with some row configurations
                        if (entry.leftNeighborID == ClickableComponent.SNAP_AUTOMATIC)
                            entry.leftNeighborID = (idCounter > 1) ? (entry.myID - 1) : -1;
                        if (entry.rightNeighborID == ClickableComponent.SNAP_AUTOMATIC)
                            entry.rightNeighborID = (elem != row.Last() || entry != regions.Last()) ? (entry.myID + 1) : -1;

                        if (entry.upNeighborID == ClickableComponent.SNAP_AUTOMATIC && rowCounter > 1)
                        {
                            int bestOverlapScore = int.MinValue;
                            int bestDiffScore = int.MaxValue;
                            foreach (var prevEntry in prevRow)
                            {
                                Rectangle a = new(prevEntry.bounds.Left, 0, prevEntry.bounds.Width, 10);
                                Rectangle b = new(entry.bounds.Left, 0, entry.bounds.Width, 10);

                                Rectangle overlap = Rectangle.Intersect(a, b);
                                if (overlap.Width > 0)
                                {
                                    if (overlap.Width > bestOverlapScore)
                                    {
                                        bestOverlapScore = overlap.Width;
                                        entry.upNeighborID = prevEntry.myID;
                                    }
                                    continue;
                                }
                                if (bestOverlapScore > 0)
                                    continue;

                                int diff = Math.Max(0, prevEntry.bounds.Left - entry.bounds.Right);
                                diff = Math.Min(diff, Math.Max(0, entry.bounds.Left - prevEntry.bounds.Right));

                                if (diff < bestDiffScore)
                                {
                                    bestDiffScore = diff;
                                    entry.upNeighborID = prevEntry.myID;
                                }
                            }
                        }

                        foreach (var prevEntry in prevRow)
                        {
                            ClickableComponent existing = currRow.FirstOrDefault(c => c.myID == prevEntry.downNeighborID);
                            if (prevEntry.downNeighborID != ClickableComponent.SNAP_AUTOMATIC && existing != null)
                                continue;

                            if (existing != null)
                            {
                                Rectangle a = new(prevEntry.bounds.Left, 0, prevEntry.bounds.Width, 10);
                                Rectangle b = new(existing.bounds.Left, 0, existing.bounds.Width, 10);
                                int overlapScore = Rectangle.Intersect(a, b).Width;
                                if (overlapScore == 0) overlapScore = int.MinValue;

                                Rectangle c = new(entry.bounds.Left, 0, entry.bounds.Width, 10);
                                int newOverlapScore = Rectangle.Intersect(a, c).Width;
                                if (newOverlapScore == 0) overlapScore = int.MinValue;

                                if (newOverlapScore > overlapScore)
                                {
                                    prevEntry.downNeighborID = entry.myID;
                                    continue;
                                }
                                if (newOverlapScore > 0 || overlapScore > 0)
                                    continue;

                                int diffScore = Math.Max(0, prevEntry.bounds.Left - existing.bounds.Right);
                                diffScore = Math.Min(diffScore, Math.Max(0, existing.bounds.Left - prevEntry.bounds.Right));

                                int newDiffScore = Math.Max(0, prevEntry.bounds.Left - entry.bounds.Right);
                                newDiffScore = Math.Min(newDiffScore, Math.Max(0, entry.bounds.Left - prevEntry.bounds.Right));

                                if (newDiffScore > diffScore)
                                {
                                    prevEntry.downNeighborID = entry.myID;
                                }
                            }
                            else
                            {
                                prevEntry.downNeighborID = entry.myID;
                            }
                        }

                        currRow.Add(entry);
                        yield return entry;
                    }
                }

                prevRow.Clear();
                prevRow.AddRange(currRow);
            }
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
                    foreach (var region in element.GetGamepadMovementRegions())
                        region.visible = !isChildOffScreen;
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
                    foreach (var region in element.GetGamepadMovementRegions())
                        region.visible = !isChildOffScreen;
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
#endif
