using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace Designs
{
    public class DesignMenu : IClickableMenu
    {
        private Color[] pixels = new Color[ 16 * 16 ];
        private Texture2D pixelDisplay;

        private Color leftColor = Color.Black, rightColor = Color.White;
        private Dictionary<Rectangle, Color> palette = MakePalette(100, 100);
        private int lastSat = 100, lastVal = 100;

        private bool showGrid = true;
        private static Texture2D grid = MakeGrid();

        private static readonly Vector2 BigPixelDisplay = new(44, 44);
        private static readonly Rectangle LeftColorRect = new(512 + (800 - 512) / 4 + 18 + 8, 150, 64, 64);
        private static readonly Rectangle RightColorRect = new(512 + (800 - 512) / 4 + 18 + 8 + 64 + 12, 150, 64, 64);

        private bool wasLeft = false, wasRight = false;
        private Point prevMousePos;

        public DesignMenu()
        :   base( ( Game1.uiViewport.Width - 800 ) / 2, ( Game1.uiViewport.Height - 600 ) / 2 , 800, 600 )
        {
            Array.Fill(pixels, Color.White);
            pixelDisplay = new(Game1.graphics.GraphicsDevice, 16, 16);
            pixelDisplay.SetData(pixels);
        }

        public override void update(GameTime time)
        {
            base.update(time);

            bool left = Game1.input.GetMouseState().LeftButton == ButtonState.Pressed;
            bool right = Game1.input.GetMouseState().RightButton == ButtonState.Pressed;

            Rectangle bigPixelDisplay = new(xPositionOnScreen + (int) BigPixelDisplay.X, yPositionOnScreen + (int) BigPixelDisplay.Y, 512, 512);
            if (bigPixelDisplay.Contains(Game1.getMousePosition()))
            {
                var pixelPos = new Point(Game1.getMouseX() - bigPixelDisplay.X, Game1.getMouseY() - bigPixelDisplay.Y);
                pixelPos = new Point(pixelPos.X / (512 / 16), pixelPos.Y / (512 / 16));

                bool modified = false;
                if (left)
                {
                    pixels[pixelPos.X + pixelPos.Y * 16] = leftColor;
                    modified = true;
                }
                if (right)
                {
                    pixels[pixelPos.X + pixelPos.Y * 16] = rightColor;
                    modified = true;
                }

                if (modified)
                    pixelDisplay.SetData(pixels);
            }
            else if (left || right)
            {
                foreach (var key in palette.Keys)
                {
                    var r = key;
                    r.Offset(xPositionOnScreen, yPositionOnScreen);

                    if (r.Contains(Game1.getMousePosition()))
                    {
                        if (left)
                            leftColor = palette[key];
                        if (right)
                            rightColor = palette[key];
                    }
                }

                bool modified = false;
                Rectangle rm = new Rectangle(xPositionOnScreen + 512 + 12 + 64, yPositionOnScreen + 484, 7 * 4, 8 * 4);
                Rectangle rp = rm; rp.X = xPositionOnScreen + 800 - 32 - 32;

                if (rm.Contains(Game1.getMousePosition()) && left && !wasLeft)
                {
                    lastSat = Math.Clamp(lastSat - 10, 0, 100);
                    modified = true;
                }
                if (rp.Contains(Game1.getMousePosition()) && left && !wasLeft)
                {
                    lastSat = Math.Clamp(lastSat + 10, 0, 100);
                    modified = true;
                }
                rm.Offset(0, 40); rp.Offset(0, 40);
                if (rm.Contains(Game1.getMousePosition()) && left && !wasLeft)
                {
                    lastVal = Math.Clamp(lastVal - 10, 0, 100);
                    modified = true;
                }
                if (rp.Contains(Game1.getMousePosition()) && left && !wasLeft)
                {
                    lastVal = Math.Clamp(lastVal + 10, 0, 100);
                    modified = true;
                }

                if ( modified )
                    palette = MakePalette(lastSat, lastVal);
            }

            wasLeft = left;
            wasRight = right;
            prevMousePos = Game1.getMousePosition();
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            IClickableMenu.drawTextureBox(b, xPositionOnScreen + (int) BigPixelDisplay.X - 12, yPositionOnScreen + (int) BigPixelDisplay.Y - 12, 512+24, 512+24, Color.White);
            b.Draw(pixelDisplay, new Rectangle(xPositionOnScreen + (int) BigPixelDisplay.X, yPositionOnScreen + (int) BigPixelDisplay.Y, 512, 512), Color.White);
            if (showGrid)
            {
                b.Draw(grid, new Vector2(xPositionOnScreen + (int)BigPixelDisplay.X, yPositionOnScreen + (int)BigPixelDisplay.Y), Color.White);
            }

            IClickableMenu.drawTextureBox(b, xPositionOnScreen + width - 64 - 48 - 32 - 12, yPositionOnScreen + 48 - 12, 64+24, 64+24, Color.White);
            b.Draw(pixelDisplay, new Rectangle(xPositionOnScreen + width - 64 - 48 - 32, yPositionOnScreen + 48, 64, 64), Color.White);

            IClickableMenu.drawTextureBox(b, xPositionOnScreen + RightColorRect.X - 12, yPositionOnScreen + RightColorRect.Y - 12, RightColorRect.Width + 24, RightColorRect.Height + 24, Color.White);
            IClickableMenu.drawTextureBox(b, xPositionOnScreen + LeftColorRect.X - 12, yPositionOnScreen + LeftColorRect.Y - 12, LeftColorRect.Width+24, LeftColorRect.Height+24, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + LeftColorRect.X, yPositionOnScreen + LeftColorRect.Y, LeftColorRect.Width, LeftColorRect.Height), leftColor);
            b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + RightColorRect.X, yPositionOnScreen + RightColorRect.Y, RightColorRect.Width, RightColorRect.Height), rightColor);

            var keys = palette.Keys.OrderBy( k => k.Y * 100 - k.X ); // so shadows don't draw over each other
            foreach (var key in keys)
            {
                var col = palette[key];
                var k = key;
                k.Offset(xPositionOnScreen, yPositionOnScreen);
                IClickableMenu.drawTextureBox(b, k.X - 12, k.Y - 12, k.Width + 12, k.Height + 12, Color.White);
                b.Draw(Game1.staminaRect, new Rectangle(k.X, k.Y, k.Width - 12, k.Height - 12), col);
            }

            b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 512 + 12 + 64, yPositionOnScreen + 484), OptionsPlusMinus.minusButtonSource, Color.White, 0, Vector2.Zero, Vector2.One * 4, SpriteEffects.None, 1);
            b.DrawString(Game1.smallFont, "Sat: " + lastSat, new Vector2(xPositionOnScreen + 512 + 12 + 64 + 32, yPositionOnScreen + 484), Color.Black);
            b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 800 - 32 - 32, yPositionOnScreen + 484), OptionsPlusMinus.plusButtonSource, Color.White, 0, Vector2.Zero, Vector2.One * 4, SpriteEffects.None, 1);

            b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 512 + 12 + 64, yPositionOnScreen + 484 + 40), OptionsPlusMinus.minusButtonSource, Color.White, 0, Vector2.Zero, Vector2.One * 4, SpriteEffects.None, 1);
            b.DrawString(Game1.smallFont, "Val: " + lastVal, new Vector2(xPositionOnScreen + 512 + 12 + 64 + 32, yPositionOnScreen + 484 + 40), Color.Black);
            b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 800 - 32 - 32, yPositionOnScreen + 484 + 40), OptionsPlusMinus.plusButtonSource, Color.White, 0, Vector2.Zero, Vector2.One * 4, SpriteEffects.None, 1);

            drawMouse(b);

            // buttons and stuff
        }

        private static Texture2D MakeGrid()
        {
            Color[] cols = new Color[512 * 512];
            Array.Fill(cols, Color.Transparent);
            for (int ix = 0; ix < 512; ++ix)
            {
                for (int iy = 0; iy < 512; ++iy)
                {
                    if (ix % (512 / 16) != 0 && iy % (512 / 16) != 0)
                        continue;
                    cols[ix + iy * 512] = new Color(128, 128, 128, 255);
                }
            }

            Texture2D tex = new(Game1.graphics.GraphicsDevice, 512, 512);
            tex.SetData(cols);
            return tex;
        }

        private static Dictionary<Rectangle, Color> MakePalette(int saturation, int value)
        {
            Point basePoint = new(512 + 12 + 64, 250);
            Dictionary<Rectangle, Color> ret = new();
            for (int i = 0; i < 36; ++i)
            {
                var p = basePoint + new Point(i % 6 * 32, i / 6 * 32);
                ret.Add(new Rectangle(p.X, p.Y, 32, 32), Util.ColorFromHsv(i * 10, saturation / 100f, value / 100f));
            }
            for (int i = 0; i < 6; ++i)
            {
                var p = basePoint + new Point(i % 6 * 32,6 * 32);
                ret.Add(new Rectangle(p.X, p.Y, 32, 32), new Color(255 / 5 * i, 255 / 5 * i, 255 / 5 * i));
            }
            return ret;
        }
    }
}
