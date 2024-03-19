using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace QiChest
{
    public class QiCodeMenu : IClickableMenu
    {
        public const int sizeOfEachSwatch = 7;

        public Item itemToDrawColored;

        public bool showExample;

        public bool visible = true;

        public int colorSelection;

        public int totalColors;

        public QiCodeMenu(Item itemToDrawColored = null)
        {
            this.totalColors = 16;
            base.width = this.totalColors * 9 * 4 + IClickableMenu.borderWidth;
            base.height = 32 * 3 + IClickableMenu.borderWidth;
            base.xPositionOnScreen = (Game1.uiViewport.Width - width) / 2 ;
            base.yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;
            this.itemToDrawColored = itemToDrawColored;
            if (this.itemToDrawColored is Chest)
            {
                (itemToDrawColored as Chest).resetLidFrame();
            }
            this.visible = Game1.player.showChestColorPicker;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public int getSelectionFromColor(Color c)
        {
            for (int i = 0; i < this.totalColors; i++)
            {
                if (this.getColorFromSelection(i).Equals(c))
                {
                    return i;
                }
            }
            return -1;
        }

        public override void performHoverAction(int x, int y)
        {
        }

        public override void update(GameTime time)
        {
            base.update(time);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!this.visible)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            for (int i = 0; i < 3; ++i)
            {
                Rectangle area = new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth / 2, base.yPositionOnScreen + 32 * i + IClickableMenu.borderWidth / 2, 36 * this.totalColors, 32);
                if (area.Contains(x, y))
                {
                    this.colorSelection = (x - area.X) / 36;
                    try
                    {
                        Game1.playSound("coin");
                    }
                    catch (Exception)
                    {
                    }
                    if (this.itemToDrawColored is Chest c)
                    {
                        int code = int.Parse(c.modData[Mod.ModDataKey]);
                        code = code.AdjustQiCode(i, this.colorSelection);
                        c.modData[Mod.ModDataKey] = code.ToString();
                        c.GlobalInventoryId = "QiChest_" + code.ToString();
                        (this.itemToDrawColored as Chest).resetLidFrame();
                    }
                }
            }
        }

        public Color getColorFromSelection(int selection)
        {
            selection += 1;
            return selection switch
            {
                2 => new Color(119, 191, 255),
                1 => new Color(85, 85, 255),
                3 => new Color(0, 170, 170),
                4 => new Color(0, 234, 175),
                5 => new Color(0, 170, 0),
                6 => new Color(159, 236, 0),
                7 => new Color(255, 234, 18),
                8 => new Color(255, 167, 18),
                9 => new Color(255, 105, 18),
                10 => new Color(255, 0, 0),
                11 => new Color(135, 0, 35),
                12 => new Color(255, 173, 199),
                13 => new Color(255, 117, 195),
                14 => new Color(172, 0, 198),
                15 => new Color(143, 0, 255),
                16 => new Color(89, 11, 142),
                _ => Color.Black,
            };
        }

        public override void draw(SpriteBatch b)
        {
            if (!this.visible)
            {
                return;
            }
            int code = int.Parse(itemToDrawColored.modData[Mod.ModDataKey]);
            IClickableMenu.drawTextureBox(b, base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.LightGray);
            for (int ic = 0; ic < 3; ++ic)
            {
                int thisCode = code.ExtractQiCode(ic);

                for (int i = 0; i < this.totalColors; i++)
                {
                    /*if (i == 0)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth / 2, base.yPositionOnScreen + IClickableMenu.borderWidth / 2), new Rectangle(295, 503, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                    }
                    else*/
                    {
                        b.Draw(Game1.staminaRect, new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4, base.yPositionOnScreen + ic * 32 + IClickableMenu.borderWidth / 2, 28, 28), this.getColorFromSelection(i));
                    }
                    if (i == thisCode)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), base.xPositionOnScreen + IClickableMenu.borderWidth / 2 - 4 + i * 9 * 4, base.yPositionOnScreen + 32 * ic + IClickableMenu.borderWidth / 2 - 4, 36, 36, Color.Black, 4f, drawShadow: false);
                    }
                }
            }
            /*
            if (this.itemToDrawColored != null && this.itemToDrawColored is Chest)
            {
                (this.itemToDrawColored as Chest).draw(b, base.xPositionOnScreen + base.width + IClickableMenu.borderWidth / 2, base.yPositionOnScreen + 16, 1f, local: true);
            }*/

            drawMouse(b);
        }
    }
}
