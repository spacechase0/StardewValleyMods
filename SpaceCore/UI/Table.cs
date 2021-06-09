using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace SpaceCore.UI
{
    public class Table : Container
    {
        private List<Element[]> rows = new List<Element[]>();

        private Vector2 size;
        public Vector2 Size
        {
            get { return this.size; }
            set
            {
                this.size = new Vector2(value.X, ((int)value.Y) / this.RowHeight * this.RowHeight);
                this.UpdateScrollbar();
            }
        }

        public const int RowPadding = 16;
        private int rowHeight;
        public int RowHeight
        {
            get { return this.rowHeight; }
            set
            {
                this.rowHeight = value + Table.RowPadding;
                this.UpdateScrollbar();
            }
        }

        public int RowCount { get { return this.rows.Count; } }

        public Scrollbar Scrollbar { get; }

        public Table()
        {
            this.Scrollbar = new Scrollbar();
            this.Scrollbar.LocalPosition = new Vector2(0, 0);
            this.AddChild(this.Scrollbar);
        }

        public void AddRow(Element[] elements)
        {
            this.rows.Add(elements);
            foreach (var child in elements)
            {
                this.AddChild(child);
            }
            this.UpdateScrollbar();
        }

        private void UpdateScrollbar()
        {
            this.Scrollbar.LocalPosition = new Vector2(this.Size.X + 48, this.Scrollbar.LocalPosition.Y);
            this.Scrollbar.RequestHeight = (int)this.Size.Y;
            this.Scrollbar.Rows = this.rows.Count;
            this.Scrollbar.FrameSize = (int)(this.Size.Y / this.RowHeight);
        }

        public override int Width => (int)this.Size.X;
        public override int Height => (int)this.Size.Y;

        public override void Update(bool hidden = false)
        {
            //base.Update(hidden);
            if (hidden) return;

            int ir = 0;
            foreach (var row in this.rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * this.RowHeight - this.Scrollbar.TopRow * this.RowHeight);
                    if (!(element is Label) && // Labels must update anyway to get rid of hovertext on scrollwheel
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
            foreach (var row in this.rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * this.RowHeight - this.Scrollbar.ScrollPercent * this.rows.Count * this.RowHeight);
                    element.Update(hidden || element.Position.Y < this.Position.Y || element.Position.Y + this.RowHeight - Table.RowPadding > this.Position.Y + this.Size.Y);
                }
                ++ir;
            }
            this.Scrollbar.Update(hidden);
        }

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, (int)this.Position.X - 32, (int)this.Position.Y - 32, (int)this.Size.X + 64, (int)this.Size.Y + 64, Color.White);

            foreach (var row in this.rows)
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

            if (this.RenderLast != null)
                this.RenderLast.Draw(b);

            this.Scrollbar.Draw(b);
        }
    }
}
