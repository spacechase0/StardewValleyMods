using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using static Microsoft.Xna.Framework.Input.ButtonState;
using Microsoft.Xna.Framework.Input;

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

        public SelectDisplayMenu( string theType, string theActive )
        {
            type = theType;
            active = theActive;

            choices.Add("/", null);
            foreach ( var choice in CustomizeExteriorMod.config.choices[ type ] )
            {
                choices.Add(choice, CustomizeExteriorMod.content.Load<Texture2D>(type + "/" + choice));
            }

            size = Game1.viewport.Size.Height - PADDING_OUTER * 2;
            x = ( Game1.viewport.Size.Width - size ) / 2;
            entrySize = ( size - PADDING_INNER * 4 ) / 3;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            justClicked = true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        bool justClicked = false;
        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, x, PADDING_OUTER, size, size, Color.White);

            int i = 0;
            foreach ( var entry in choices )
            {
                int ix = x + PADDING_INNER + (entrySize + PADDING_INNER) * (i % 3);
                int iy = PADDING_OUTER + PADDING_INNER + (entrySize + PADDING_INNER) * (i / 3);

                Color col = entry.Key == active ? Color.Goldenrod : Color.White;
                if ( new Rectangle(ix, iy, entrySize, entrySize).Contains(Game1.getMousePosition()) )
                {
                    col = Color.Wheat;
                    if ( justClicked )
                    {
                        active = entry.Key;
                        if (onSelected != null)
                        {
                            onSelected.Invoke(type, active);
                        }
                        Game1.exitActiveMenu();
                    }
                }
                drawTextureBox(b, ix, iy, entrySize, entrySize, col);

                if (entry.Value != null)
                    b.Draw(entry.Value, new Rectangle(ix + PADDING_IN, iy + PADDING_IN, entrySize - PADDING_IN * 2, entrySize - PADDING_IN * 2), new Rectangle(0, 0, entry.Value.Width, entry.Value.Height), Color.White);
                else
                    SpriteText.drawString(b, entry.Key, ix + PADDING_IN + (entrySize - PADDING_IN * 2 - SpriteText.getWidthOfString(entry.Key)) / 2, iy + PADDING_IN + (entrySize - PADDING_IN * 2 - SpriteText.getHeightOfString(entry.Key)) / 2);
                
                ++i;
            }

            base.draw(b);
            drawMouse(b);

            justClicked = false;
        }
    }
}
