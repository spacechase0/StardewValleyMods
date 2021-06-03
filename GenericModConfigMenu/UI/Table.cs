using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace GenericModConfigMenu.UI
{
    public class Table : Container
    {
        private List<Element[]> rows = new List<Element[]>();

        private Vector2 size;
        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = new Vector2(value.X, ((int) value.Y) / RowHeight * RowHeight);
                UpdateScrollbar();
            }
        }

        public const int RowPadding = 16;
        private int rowHeight;
        public int RowHeight
        {
            get { return rowHeight; }
            set
            {
                rowHeight = value + RowPadding;
                UpdateScrollbar();
            }
        }

        public int RowCount { get { return rows.Count; } }

        public Scrollbar Scrollbar { get; }
        
        public Table()
        {
            Scrollbar = new Scrollbar();
            Scrollbar.LocalPosition = new Vector2(0, 0);
            AddChild(Scrollbar);
        }

        public void AddRow( Element[] elements )
        {
            rows.Add(elements);
            foreach ( var child in elements )
            {
                AddChild(child);
            }
            UpdateScrollbar();
        }

        private void UpdateScrollbar()
        {
            Scrollbar.LocalPosition = new Vector2(Size.X + 48, Scrollbar.LocalPosition.Y);
            Scrollbar.RequestHeight = (int)Size.Y;
            Scrollbar.Rows = rows.Count;
            Scrollbar.FrameSize = (int)(Size.Y / RowHeight);
        }

        public override int Width => (int)Size.X;
        public override int Height => (int)Size.Y;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);
            if (hidden) return;

            int ir = 0;
            foreach (var row in rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * RowHeight - Scrollbar.TopRow * RowHeight);
                    if (!(element is Label) && // Labels must update anyway to get rid of hovertext on scrollwheel
                            (element.Position.Y < Position.Y || element.Position.Y + RowHeight - RowPadding > Position.Y + Size.Y))
                        continue;
                    element.Update();
                }
                ++ir;
            }
            Scrollbar.Update();
        }

        public void ForceUpdateEvenHidden(bool hidden = false)
        {
            int ir = 0;
            foreach (var row in rows)
            {
                foreach (var element in row)
                {
                    element.LocalPosition = new Vector2(element.LocalPosition.X, ir * RowHeight - Scrollbar.ScrollPercent * rows.Count * RowHeight);
                    element.Update(hidden || element.Position.Y < Position.Y || element.Position.Y + RowHeight - RowPadding > Position.Y + Size.Y);
                }
                ++ir;
            }
            Scrollbar.Update(hidden);
        }

        public override void Draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, (int) Position.X - 32, (int) Position.Y - 32, (int) Size.X + 64, (int) Size.Y + 64, Color.White);
            
            foreach (var row in rows)
            {
                foreach (var element in row)
                {
                    if (element.Position.Y < Position.Y || element.Position.Y + RowHeight - RowPadding > Position.Y + Size.Y)
                        continue;
                    if (element == RenderLast)
                        continue;
                    element.Draw(b);
                }
            }
            
            if (RenderLast != null)
                RenderLast.Draw(b);

            Scrollbar.Draw(b);
        }
    }
}
