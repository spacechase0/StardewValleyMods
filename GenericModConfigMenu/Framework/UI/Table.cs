using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework.UI
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
        public override void Update(bool hidden = false)
        {
            base.Update(hidden);
            if (hidden) return;

            int ir = 0;
            foreach (var row in this.Rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * this.RowHeight - this.Scrollbar.TopRow * this.RowHeight);
                    if (element is not Label && // Labels must update anyway to get rid of hovertext on scrollwheel
                            (element.Position.Y < this.Position.Y || element.Position.Y + this.RowHeight - Table.RowPadding > this.Position.Y + this.Size.Y))
                        continue;
                    element.Update();
                }
                ++ir;
            }
            this.Scrollbar.Update();
        }

        public void ForceUpdateEvenHidden(bool hidden = false)
        {
            int ir = 0;
            foreach (var row in this.Rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * this.RowHeight - this.Scrollbar.ScrollPercent * this.Rows.Count * this.RowHeight);
                    element.Update(hidden || element.Position.Y < this.Position.Y || element.Position.Y + this.RowHeight - Table.RowPadding > this.Position.Y + this.Size.Y);
                }
                ++ir;
            }
            this.Scrollbar.Update(hidden);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, (int)this.Position.X - 32, (int)this.Position.Y - 32, (int)this.Size.X + 64, (int)this.Size.Y + 64, Color.White);

            foreach (var row in this.Rows)
            {
                foreach (var element in row)
                {
                    if (element.Position.Y < this.Position.Y || element.Position.Y + this.RowHeight - Table.RowPadding > this.Position.Y + this.Size.Y)
                        continue;
                    if (element == this.RenderLast)
                        continue;
                    element.Draw(b);
                }
            }

            this.RenderLast?.Draw(b);

            this.Scrollbar.Draw(b);
        }


        /*********
        ** Private methods
        *********/
        private void UpdateScrollbar()
        {
            this.Scrollbar.LocalPosition = new Vector2(this.Size.X + 48, this.Scrollbar.LocalPosition.Y);
            this.Scrollbar.RequestHeight = (int)this.Size.Y;
            this.Scrollbar.Rows = this.Rows.Count;
            this.Scrollbar.FrameSize = (int)(this.Size.Y / this.RowHeight);
        }
    }
}
