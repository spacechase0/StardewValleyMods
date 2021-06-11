using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace CustomizeExterior
{
    internal class SelectDisplayMenu : IClickableMenu
    {
        private const int PADDING_OUTER = 100;
        private const int PADDING_INNER = 50;
        private const int PADDING_IN = 20;

        public Action<string, string> onSelected = null;

        private string type;
        private string active;
        private Dictionary<string, Texture2D> choices = new();

        private int x;
        private int size;
        private int entrySize;

        private int scroll = 0;

        public SelectDisplayMenu(string theType, string theActive)
        {
            this.type = theType;
            this.active = theActive;

            this.choices.Add("/", null);
            foreach (var choice in Mod.choices[this.type])
            {
                this.choices.Add(choice, Mod.getTextureForChoice(this.type, choice));
            }

            this.size = Game1.viewport.Size.Height - SelectDisplayMenu.PADDING_OUTER * 2;
            this.x = (Game1.viewport.Size.Width - this.size) / 2;
            this.entrySize = (this.size - SelectDisplayMenu.PADDING_INNER * 4) / 3;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            int i = 0;
            foreach (var entry in this.choices)
            {
                int ix = this.x + SelectDisplayMenu.PADDING_INNER + (this.entrySize + SelectDisplayMenu.PADDING_INNER) * (i % 3);
                int iy = SelectDisplayMenu.PADDING_OUTER + SelectDisplayMenu.PADDING_INNER + (this.entrySize + SelectDisplayMenu.PADDING_INNER) * (i / 3);
                iy += this.scroll;

                if (new Rectangle(ix, iy, this.entrySize, this.entrySize).Contains(x, y))
                {
                    this.active = entry.Key;
                    this.onSelected?.Invoke(this.type, this.active);
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
            this.scroll += dir;
            if (this.scroll > 0)
                this.scroll = 0;

            int cap = SelectDisplayMenu.PADDING_OUTER + SelectDisplayMenu.PADDING_INNER + (this.entrySize + SelectDisplayMenu.PADDING_INNER) * (this.choices.Count / 3 - 3) + SelectDisplayMenu.PADDING_INNER * 2;
            if (this.scroll <= -cap)
                this.scroll = -cap;
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this.x, SelectDisplayMenu.PADDING_OUTER, this.size, this.size, Color.White);

            int edge = (int)(10 * Game1.options.zoomLevel);

            b.End();
            RasterizerState state = new RasterizerState();
            state.ScissorTestEnable = true;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, state);
            b.GraphicsDevice.ScissorRectangle = new Rectangle(this.x + edge, SelectDisplayMenu.PADDING_OUTER + edge, this.size - edge * 2, this.size - edge * 2);
            {
                int i = 0;
                foreach (var entry in this.choices)
                {
                    int ix = this.x + SelectDisplayMenu.PADDING_INNER + (this.entrySize + SelectDisplayMenu.PADDING_INNER) * (i % 3);
                    int iy = SelectDisplayMenu.PADDING_OUTER + SelectDisplayMenu.PADDING_INNER + (this.entrySize + SelectDisplayMenu.PADDING_INNER) * (i / 3);
                    iy += this.scroll;

                    Color col = entry.Key == this.active ? Color.Goldenrod : Color.White;
                    if (new Rectangle(ix, iy, this.entrySize, this.entrySize).Contains(Game1.getMousePosition()))
                        col = Color.Wheat;
                    IClickableMenu.drawTextureBox(b, ix, iy, this.entrySize, this.entrySize, col);

                    if (entry.Value != null)
                        b.Draw(entry.Value, new Rectangle(ix + SelectDisplayMenu.PADDING_IN, iy + SelectDisplayMenu.PADDING_IN, this.entrySize - SelectDisplayMenu.PADDING_IN * 2, this.entrySize - SelectDisplayMenu.PADDING_IN * 2), new Rectangle(0, 0, entry.Value.Width, entry.Value.Height), Color.White);
                    else
                        SpriteText.drawString(b, entry.Key, ix + SelectDisplayMenu.PADDING_IN + (this.entrySize - SelectDisplayMenu.PADDING_IN * 2 - SpriteText.getWidthOfString(entry.Key)) / 2, iy + SelectDisplayMenu.PADDING_IN + (this.entrySize - SelectDisplayMenu.PADDING_IN * 2 - SpriteText.getHeightOfString(entry.Key)) / 2);

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
