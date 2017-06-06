using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Interface
{
    public class TabbedMenu : IClickableMenu
    {
        protected int currentTab = 0;
        protected TabMenu[] tabs;

        public TabbedMenu( int w, int h ) : base((Game1.viewport.Width - w) / 2, (Game1.viewport.Height - h) / 2, w, h, true)
        {
        }

        public override void update(GameTime time)
        {
            base.update(time);
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null, null, Matrix.CreateTranslation(xPositionOnScreen, yPositionOnScreen, 0));
            {
                int tabArea = width / tabs.Length;
                
                for ( int i = 0; i < tabs.Length; ++i )
                {
                    TabMenu tab = tabs[i];
                    
                    int ix = i * tabArea;
                    ix += (tabArea - SpriteText.getWidthOfString(tab.Name)) / 2;
                    int iy = IClickableMenu.borderWidth;
                    SpriteText.drawString(b, tabs[i].Name, ix, iy, color: i == currentTab ? SpriteText.color_Orange : -1);
                }

                tabs[currentTab].draw(b);
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            
            base.draw(b);
            drawMouse(b);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            tabs[currentTab].mouseMove(x - xPositionOnScreen, y - yPositionOnScreen);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            int tabArea = width / tabs.Length;
            for (int i = 0; i < tabs.Length; ++i)
            {
                TabMenu tab = tabs[i];

                int ix = xPositionOnScreen + i * tabArea;
                ix += (tabArea - SpriteText.getWidthOfString(tab.Name)) / 2;
                int iy = yPositionOnScreen + IClickableMenu.borderWidth;

                if ( x >= ix && y >= iy && x < ix + SpriteText.getWidthOfString(tab.Name) && y < iy + SpriteText.getHeightOfString( tab.Name ) )
                {
                    currentTab = i;
                    return;
                }
            }

            tabs[currentTab].leftClick(x - xPositionOnScreen, y - yPositionOnScreen);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
    }
}
