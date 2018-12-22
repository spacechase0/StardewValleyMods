using StardewValley.Menus;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomizeExterior
{
    class SelectDisplayMenu : IClickableMenu
    {
        private const int PADDING_OUTER = 100;
        private const int PADDING_INNER = 50;
        private const int PADDING_IN = 20;

        public Action<string, string> onSelected = null;

        private string type;
        private string active;
        private Dictionary<string, Texture2D> choices = new Dictionary< string, Texture2D >();

        private int x;
        private int size;
        private int entrySize;

        int scroll = 0;

        public SelectDisplayMenu( string theType, string theActive )
        {
            type = theType;
            active = theActive;

            choices.Add("/", null);
            foreach ( var choice in Mod.choices[ type ] )
            {
                choices.Add(choice, Mod.getTextureForChoice( type, choice ));
            }

            size = Game1.viewport.Size.Height - PADDING_OUTER * 2;
            x = ( Game1.viewport.Size.Width - size ) / 2;
            entrySize = ( size - PADDING_INNER * 4 ) / 3;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            int i = 0;
            foreach (var entry in choices)
            {
                int ix = this.x + PADDING_INNER + (entrySize + PADDING_INNER) * (i % 3);
                int iy = PADDING_OUTER + PADDING_INNER + (entrySize + PADDING_INNER) * (i / 3);
                iy += scroll;
                
                if (new Rectangle(ix, iy, entrySize, entrySize).Contains(x, y))
                {
                    active = entry.Key;
                    onSelected?.Invoke(type, active);
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
            scroll += dir;
            if (scroll > 0) scroll = 0;

            int cap = PADDING_OUTER + PADDING_INNER + (entrySize + PADDING_INNER) * (choices.Count / 3 - 3) + PADDING_INNER * 2;
            if (scroll <= -cap) scroll = -cap;
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, x, PADDING_OUTER, size, size, Color.White);

            int edge = (int)(10 * Game1.options.zoomLevel);

            b.End();
            RasterizerState state = new RasterizerState();
            state.ScissorTestEnable = true;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, state );
            b.GraphicsDevice.ScissorRectangle = new Rectangle(x + edge, PADDING_OUTER + edge, size - edge * 2, size - edge * 2);
            {
                int i = 0;
                foreach (var entry in choices)
                {
                    int ix = x + PADDING_INNER + (entrySize + PADDING_INNER) * (i % 3);
                    int iy = PADDING_OUTER + PADDING_INNER + (entrySize + PADDING_INNER) * (i / 3);
                    iy += scroll;

                    Color col = entry.Key == active ? Color.Goldenrod : Color.White;
                    if (new Rectangle(ix, iy, entrySize, entrySize).Contains(Game1.getMousePosition()))
                        col = Color.Wheat;
                    drawTextureBox(b, ix, iy, entrySize, entrySize, col);

                    if (entry.Value != null)
                        b.Draw(entry.Value, new Rectangle(ix + PADDING_IN, iy + PADDING_IN, entrySize - PADDING_IN * 2, entrySize - PADDING_IN * 2), new Rectangle(0, 0, entry.Value.Width, entry.Value.Height), Color.White);
                    else
                        SpriteText.drawString(b, entry.Key, ix + PADDING_IN + (entrySize - PADDING_IN * 2 - SpriteText.getWidthOfString(entry.Key)) / 2, iy + PADDING_IN + (entrySize - PADDING_IN * 2 - SpriteText.getHeightOfString(entry.Key)) / 2);

                    ++i;
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            base.draw(b);
            drawMouse(b);
        }
    }
}
