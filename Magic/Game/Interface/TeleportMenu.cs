using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewValley;
using StardewValley.Menus;

namespace Magic.Game.Interface
{
    public class TeleportMenu : IClickableMenu
    {
        public const int WINDOW_WIDTH = 640;
        public const int WINDOW_HEIGHT = 480;

        public const int ELEM_HEIGHT = 50;
        public const int EDGE_PAD = 16;

        private List<string> locs = new();
        private string warpTo = null;

        private int scroll = 0;
        private Rectangle scrollbarBack;
        private Rectangle scrollbar;

        private bool dragScroll = false;

        public TeleportMenu()
            : base((Game1.viewport.Width - TeleportMenu.WINDOW_WIDTH) / 2, (Game1.viewport.Height - TeleportMenu.WINDOW_HEIGHT) / 2, TeleportMenu.WINDOW_WIDTH, TeleportMenu.WINDOW_HEIGHT)
        {
            foreach (var loc in Game1.locations)
            {
                if (loc.IsOutdoors && !(loc.Name.StartsWith("SDM") && loc.Name.EndsWith("Farm")))
                    this.locs.Add(loc.Name);
            }

            int x = this.xPositionOnScreen + 12, y = this.yPositionOnScreen + 12, w = TeleportMenu.WINDOW_WIDTH - 24, h = TeleportMenu.WINDOW_HEIGHT - 24;
            this.scrollbarBack = new Rectangle(x + w - Game1.pixelZoom * 6, y, Game1.pixelZoom * 6, h);
            this.scrollbar = new Rectangle(this.scrollbarBack.X + 2, this.scrollbarBack.Y + 2, 6 * Game1.pixelZoom - 4, (int)((5.0 / this.locs.Count) * this.scrollbarBack.Height) - 4);
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if (this.warpTo != null)
            {
                var locObj = Game1.getLocationFromName(this.warpTo);
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
                Game1.warpFarmer(this.warpTo, (int)cloud.getTileLocation().X, (int)cloud.getTileLocation().Y, false);
                Game1.player.consumeObject(Mod.ja.GetObjectId("Travel Core"), 1);
                Game1.player.AddCustomSkillExperience(Magic.Skill, 25);
            }

            if (this.dragScroll)
            {
                int my = Game1.getMouseY();
                int relY = my - (this.scrollbarBack.Y + 2 + this.scrollbar.Height / 2);
                relY = Math.Max(0, relY);
                relY = Math.Min(relY, this.scrollbarBack.Height - 4 - this.scrollbar.Height);
                float percY = relY / (this.scrollbarBack.Height - 4f - this.scrollbar.Height);
                int totalY = this.locs.Count * TeleportMenu.ELEM_HEIGHT - (TeleportMenu.WINDOW_HEIGHT - 24) + 16;
                this.scroll = -(int)(totalY * percY);
            }
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, TeleportMenu.WINDOW_WIDTH, TeleportMenu.WINDOW_HEIGHT, Color.White);

            int x = this.xPositionOnScreen + 12, y = this.yPositionOnScreen + 12, w = TeleportMenu.WINDOW_WIDTH - 24, h = TeleportMenu.WINDOW_HEIGHT - 24;

            b.End();
            RasterizerState state = new RasterizerState();
            state.ScissorTestEnable = true;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, state);
            b.GraphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);
            {
                int iy = y + TeleportMenu.EDGE_PAD;
                iy += this.scroll;
                foreach (var loc in this.locs)
                {
                    Rectangle area = new Rectangle(x, iy - 4, w - this.scrollbarBack.Width, TeleportMenu.ELEM_HEIGHT);
                    if (area.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        b.Draw(Game1.staminaRect, area, new Color(200, 32, 32, 64));
                        if (this.justClicked)
                        {
                            this.warpTo = loc;
                        }
                    }

                    b.DrawString(Game1.dialogueFont, loc, new Vector2(x + TeleportMenu.EDGE_PAD, iy), Color.Black);

                    iy += TeleportMenu.ELEM_HEIGHT;
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            if (this.locs.Count > h / TeleportMenu.ELEM_HEIGHT)
            {
                this.scrollbar.Y = this.scrollbarBack.Y + 2 + (int)(((this.scroll / (float)-TeleportMenu.ELEM_HEIGHT) / (this.locs.Count - (h - 20) / (float)TeleportMenu.ELEM_HEIGHT)) * (this.scrollbarBack.Height - this.scrollbar.Height));

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollbarBack.X, this.scrollbarBack.Y, this.scrollbarBack.Width, this.scrollbarBack.Height, Color.DarkGoldenrod, (float)Game1.pixelZoom, false);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollbar.X, this.scrollbar.Y, this.scrollbar.Width, this.scrollbar.Height, Color.Gold, (float)Game1.pixelZoom, false);
            }

            this.justClicked = false;

            base.draw(b);
            this.drawMouse(b);
        }

        private bool justClicked = false;

        public object ReflectGame1 { get; private set; }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (this.scrollbarBack.Contains(x, y))
            {
                this.dragScroll = true;
            }
            else
            {
                this.justClicked = true;
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);

            this.dragScroll = false;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            this.scroll += direction;
            if (this.scroll > 0)
                this.scroll = 0;

            int cap = this.locs.Count * 50 - (TeleportMenu.WINDOW_HEIGHT - 24) + 16;
            if (this.scroll < -cap)
                this.scroll = -cap;
        }
    }
}
