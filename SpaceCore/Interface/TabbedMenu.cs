using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace SpaceCore.Interface
{
    public class TabbedMenu : IClickableMenu
    {
        protected int currentTab;
        protected TabMenu[] tabs;

        public TabbedMenu(int w, int h)
            : base((Game1.viewport.Width - w) / 2, (Game1.viewport.Height - h) / 2, w, h, true) { }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null, null, Matrix.CreateTranslation(this.xPositionOnScreen, this.yPositionOnScreen, 0));
            {
                int tabArea = this.width / this.tabs.Length;

                for (int i = 0; i < this.tabs.Length; ++i)
                {
                    TabMenu tab = this.tabs[i];

                    int ix = i * tabArea;
                    ix += (tabArea - SpriteText.getWidthOfString(tab.Name)) / 2;
                    int iy = IClickableMenu.borderWidth;
                    SpriteText.drawString(b, this.tabs[i].Name, ix, iy, color: i == this.currentTab ? SpriteText.color_Orange : -1);
                }

                this.tabs[this.currentTab].draw(b);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);

            base.draw(b);
            this.drawMouse(b);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.tabs[this.currentTab].mouseMove(x - this.xPositionOnScreen, y - this.yPositionOnScreen);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            int tabArea = this.width / this.tabs.Length;
            for (int i = 0; i < this.tabs.Length; ++i)
            {
                TabMenu tab = this.tabs[i];

                int ix = this.xPositionOnScreen + i * tabArea;
                ix += (tabArea - SpriteText.getWidthOfString(tab.Name)) / 2;
                int iy = this.yPositionOnScreen + IClickableMenu.borderWidth;

                if (x >= ix && y >= iy && x < ix + SpriteText.getWidthOfString(tab.Name) && y < iy + SpriteText.getHeightOfString(tab.Name))
                {
                    this.currentTab = i;
                    return;
                }
            }

            this.tabs[this.currentTab].leftClick(x - this.xPositionOnScreen, y - this.yPositionOnScreen);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
    }
}
