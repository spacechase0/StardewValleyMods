using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace CustomizeExterior.Framework
{
    internal class SelectDisplayMenu : IClickableMenu
    {
        private const int PaddingOuter = 100;
        private const int PaddingInner = 50;
        private const int PaddingIn = 20;

        public Action<string> OnSelected = null;

        private readonly string Type;
        private string Active;
        private readonly Dictionary<string, Texture2D> Choices = new();

        private readonly int X;
        private readonly int Size;
        private readonly int EntrySize;

        private int Scroll;

        public SelectDisplayMenu(string theType, string theActive, IEnumerable<string> choices, Func<string, string, Texture2D> getBuildingTexture)
        {
            this.Type = theType;
            this.Active = theActive ?? "/";

            this.Choices.Add("/", null);
            foreach (string choice in choices)
                this.Choices.Add(choice, getBuildingTexture(this.Type, choice));

            this.Size = Game1.viewport.Size.Height - SelectDisplayMenu.PaddingOuter * 2;
            this.X = (Game1.viewport.Size.Width - this.Size) / 2;
            this.EntrySize = (this.Size - SelectDisplayMenu.PaddingInner * 4) / 3;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            int i = 0;
            foreach (var entry in this.Choices)
            {
                int ix = this.X + SelectDisplayMenu.PaddingInner + (this.EntrySize + SelectDisplayMenu.PaddingInner) * (i % 3);
                int iy = SelectDisplayMenu.PaddingOuter + SelectDisplayMenu.PaddingInner + (this.EntrySize + SelectDisplayMenu.PaddingInner) * (i / 3);
                iy += this.Scroll;

                if (new Rectangle(ix, iy, this.EntrySize, this.EntrySize).Contains(x, y))
                {
                    this.Active = entry.Key;
                    this.OnSelected?.Invoke(this.Active);
                    Game1.exitActiveMenu();
                }

                ++i;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void receiveScrollWheelAction(int dir)
        {
            this.Scroll += dir;
            if (this.Scroll > 0)
                this.Scroll = 0;

            int cap = SelectDisplayMenu.PaddingOuter + SelectDisplayMenu.PaddingInner + (this.EntrySize + SelectDisplayMenu.PaddingInner) * (this.Choices.Count / 3 - 3) + SelectDisplayMenu.PaddingInner * 2;
            if (this.Scroll <= -cap)
                this.Scroll = -cap;
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this.X, SelectDisplayMenu.PaddingOuter, this.Size, this.Size, Color.White);

            int edge = (int)(10 * Game1.options.zoomLevel);

            b.End();
            using RasterizerState state = new RasterizerState
            {
                ScissorTestEnable = true
            };
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, state);
            b.GraphicsDevice.ScissorRectangle = new Rectangle(this.X + edge, SelectDisplayMenu.PaddingOuter + edge, this.Size - edge * 2, this.Size - edge * 2);
            {
                int i = 0;
                foreach (var entry in this.Choices)
                {
                    int ix = this.X + SelectDisplayMenu.PaddingInner + (this.EntrySize + SelectDisplayMenu.PaddingInner) * (i % 3);
                    int iy = SelectDisplayMenu.PaddingOuter + SelectDisplayMenu.PaddingInner + (this.EntrySize + SelectDisplayMenu.PaddingInner) * (i / 3);
                    iy += this.Scroll;

                    Color col = entry.Key == this.Active ? Color.Goldenrod : Color.White;
                    if (new Rectangle(ix, iy, this.EntrySize, this.EntrySize).Contains(Game1.getMousePosition()))
                        col = Color.Wheat;
                    IClickableMenu.drawTextureBox(b, ix, iy, this.EntrySize, this.EntrySize, col);

                    if (entry.Value != null)
                        b.Draw(entry.Value, new Rectangle(ix + SelectDisplayMenu.PaddingIn, iy + SelectDisplayMenu.PaddingIn, this.EntrySize - SelectDisplayMenu.PaddingIn * 2, this.EntrySize - SelectDisplayMenu.PaddingIn * 2), new Rectangle(0, 0, entry.Value.Width, entry.Value.Height), Color.White);
                    else
                        SpriteText.drawString(b, entry.Key, ix + SelectDisplayMenu.PaddingIn + (this.EntrySize - SelectDisplayMenu.PaddingIn * 2 - SpriteText.getWidthOfString(entry.Key)) / 2, iy + SelectDisplayMenu.PaddingIn + (this.EntrySize - SelectDisplayMenu.PaddingIn * 2 - SpriteText.getHeightOfString(entry.Key)) / 2);

                    ++i;
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            base.draw(b);
            this.drawMouse(b);
        }
    }
}
