using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace Magic.Game.Interface
{
    public class TeleportMenu : IClickableMenu
    {
        public const int WINDOW_WIDTH = 640;
        public const int WINDOW_HEIGHT = 480;

        public const int ELEM_HEIGHT = 50;
        public const int EDGE_PAD = 16;

        private List<string> locs = new List<string>();
        private string warpTo = null;

        private int scroll = 0;
        private Rectangle scrollbarBack;
        private Rectangle scrollbar;

        private bool dragScroll = false;

        public TeleportMenu()
        :   base( ( Game1.viewport.Width - WINDOW_WIDTH ) / 2, ( Game1.viewport.Height - WINDOW_HEIGHT ) / 2, WINDOW_WIDTH, WINDOW_HEIGHT )
        {
            foreach ( var loc in Game1.locations )
            {
                if (loc.IsOutdoors && !( loc.Name.StartsWith( "SDM" ) && loc.Name.EndsWith( "Farm" ) ) )
                    locs.Add(loc.Name);
            }

            int x = xPositionOnScreen + 12, y = yPositionOnScreen + 12, w = WINDOW_WIDTH - 24, h = WINDOW_HEIGHT - 24;
            scrollbarBack = new Rectangle(x + w - Game1.pixelZoom * 6, y, Game1.pixelZoom * 6, h);
            scrollbar = new Rectangle(scrollbarBack.X + 2, scrollbarBack.Y + 2, 6 * Game1.pixelZoom - 4, (int)((5.0 / locs.Count) * scrollbarBack.Height) - 4);
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if ( warpTo != null )
            {
                var locObj = Game1.getLocationFromName(warpTo);
                int mapW = locObj.Map.Layers[0].LayerWidth;
                int mapH = locObj.map.Layers[0].LayerHeight;

                var cloud = new CloudMount();
                cloud.currentLocation = locObj;
                cloud.Position = new Vector2(mapW * Game1.tileSize / 2, mapH * Game1.tileSize / 2);
                Vector2 tileForCharacter = Utility.recursiveFindOpenTileForCharacter(cloud, locObj, cloud.getTileLocation(), 9 * 9);
                cloud.Position = new Vector2(tileForCharacter.X * Game1.tileSize, tileForCharacter.Y * Game1.tileSize);
                locObj.addCharacter(cloud);
                Game1.player.mount = cloud;
                cloud.rider = Game1.player;

                Game1.activeClickableMenu = null;

                Game1.playSound("wand");
                Game1.warpFarmer(warpTo, (int)cloud.getTileLocation().X, (int)cloud.getTileLocation().Y, false);
                Game1.player.consumeObject(Mod.ja.GetObjectId("Travel Core"), 1);
                Game1.player.addMagicExp(25);
            }

            if (dragScroll)
            {
                int my = Game1.getMouseY();
                int relY = my - (scrollbarBack.Y + 2 + scrollbar.Height / 2);
                relY = Math.Max(0, relY);
                relY = Math.Min(relY, scrollbarBack.Height - 4 - scrollbar.Height);
                float percY = relY / (scrollbarBack.Height - 4f - scrollbar.Height);
                int totalY = locs.Count * ELEM_HEIGHT - (WINDOW_HEIGHT - 24) + 16;
                scroll = -(int)(totalY * percY);
            }
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, WINDOW_WIDTH, WINDOW_HEIGHT, Color.White);

            int x = xPositionOnScreen + 12, y = yPositionOnScreen + 12, w = WINDOW_WIDTH - 24, h = WINDOW_HEIGHT - 24;

            b.End();
            RasterizerState state = new RasterizerState();
            state.ScissorTestEnable = true;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, state);
            b.GraphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);
            {
                int iy = y + EDGE_PAD;
                iy += scroll;
                foreach ( var loc in locs )
                {
                    Rectangle area = new Rectangle(x, iy - 4, w - scrollbarBack.Width, ELEM_HEIGHT);
                    if (area.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        b.Draw(Game1.staminaRect, area, new Color(200, 32, 32, 64));
                        if (justClicked)
                        {
                            warpTo = loc;
                        }
                    }

                    b.DrawString(Game1.dialogueFont, loc, new Vector2(x + EDGE_PAD, iy), Color.Black);

                    iy += ELEM_HEIGHT;
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            if (locs.Count > h / ELEM_HEIGHT)
            {
                scrollbar.Y = scrollbarBack.Y + 2 + (int)(((scroll / (float)-ELEM_HEIGHT) / (locs.Count - (h - 20) / (float)ELEM_HEIGHT)) * (scrollbarBack.Height - scrollbar.Height));

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollbarBack.X, scrollbarBack.Y, scrollbarBack.Width, scrollbarBack.Height, Color.DarkGoldenrod, (float)Game1.pixelZoom, false);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollbar.X, scrollbar.Y, scrollbar.Width, scrollbar.Height, Color.Gold, (float)Game1.pixelZoom, false);
            }

            justClicked = false;

            base.draw(b);
            drawMouse(b);
        }

        private bool justClicked = false;

        public object ReflectGame1 { get; private set; }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (scrollbarBack.Contains(x, y))
            {
                dragScroll = true;
            }
            else
            {
                justClicked = true;
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);

            dragScroll = false;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            scroll += direction;
            if (scroll > 0)
                scroll = 0;

            int cap = locs.Count * 50 - (WINDOW_HEIGHT - 24) + 16;
            if (scroll < -cap)
                scroll = -cap;
        }
    }
}
