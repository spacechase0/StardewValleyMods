using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewValley;
using StardewValley.Menus;

namespace Magic.Framework.Game.Interface
{
    internal class TeleportMenu : IClickableMenu
    {
        /*********
        ** Fields
        *********/
        private const int WindowWidth = 640;
        private const int WindowHeight = 480;

        private const int ElemHeight = 50;
        private const int EdgePad = 16;

        private readonly List<string> Locs = new();
        private string WarpTo;

        private int Scroll;
        private Rectangle ScrollbarBack;
        private Rectangle Scrollbar;

        private bool DragScroll;
        private bool JustClicked;


        /*********
        ** Public methods
        *********/
        public TeleportMenu()
            : base((Game1.viewport.Width - TeleportMenu.WindowWidth) / 2, (Game1.viewport.Height - TeleportMenu.WindowHeight) / 2, TeleportMenu.WindowWidth, TeleportMenu.WindowHeight)
        {
            this.gamePadControlsImplemented = false;

            foreach (var loc in Game1.locations)
            {
                if (loc.IsOutdoors && !(loc.Name.StartsWith("SDM") && loc.Name.EndsWith("Farm")))
                    this.Locs.Add(loc.Name);
            }

            int x = this.xPositionOnScreen + 12, y = this.yPositionOnScreen + 12, w = TeleportMenu.WindowWidth - 24, h = TeleportMenu.WindowHeight - 24;
            this.ScrollbarBack = new Rectangle(x + w - Game1.pixelZoom * 6, y, Game1.pixelZoom * 6, h);
            this.Scrollbar = new Rectangle(this.ScrollbarBack.X + 2, this.ScrollbarBack.Y + 2, 6 * Game1.pixelZoom - 4, (int)((5.0 / this.Locs.Count) * this.ScrollbarBack.Height) - 4);
        }

        /// <inheritdoc />
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }

        /// <inheritdoc />
        public override void update(GameTime time)
        {
            base.update(time);

            if (this.WarpTo != null)
            {
                var locObj = Game1.getLocationFromName(this.WarpTo);
                int mapW = locObj.Map.Layers[0].LayerWidth;
                int mapH = locObj.map.Layers[0].LayerHeight;

                var cloud = new CloudMount
                {
                    currentLocation = locObj,
                    Position = new Vector2(mapW * Game1.tileSize / 2, mapH * Game1.tileSize / 2)
                };
                Vector2 tileForCharacter = Utility.recursiveFindOpenTileForCharacter(cloud, locObj, cloud.getTileLocation(), 9 * 9);
                cloud.Position = new Vector2(tileForCharacter.X * Game1.tileSize, tileForCharacter.Y * Game1.tileSize);
                locObj.addCharacter(cloud);
                Game1.player.mount = cloud;
                cloud.rider = Game1.player;

                Game1.activeClickableMenu = null;

                Game1.playSound("wand");
                Game1.warpFarmer(this.WarpTo, (int)cloud.getTileLocation().X, (int)cloud.getTileLocation().Y, false);
                Game1.player.consumeObject(Mod.Ja.GetObjectId("Travel Core"), 1);
                Game1.player.AddCustomSkillExperience(Magic.Skill, 25);
            }

            if (this.DragScroll)
            {
                int my = Game1.getMouseY();
                int relY = my - (this.ScrollbarBack.Y + 2 + this.Scrollbar.Height / 2);
                relY = Math.Max(0, relY);
                relY = Math.Min(relY, this.ScrollbarBack.Height - 4 - this.Scrollbar.Height);
                float percY = relY / (this.ScrollbarBack.Height - 4f - this.Scrollbar.Height);
                int totalY = this.Locs.Count * TeleportMenu.ElemHeight - (TeleportMenu.WindowHeight - 24) + 16;
                this.Scroll = -(int)(totalY * percY);
            }
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, TeleportMenu.WindowWidth, TeleportMenu.WindowHeight, Color.White);

            int x = this.xPositionOnScreen + 12, y = this.yPositionOnScreen + 12, w = TeleportMenu.WindowWidth - 24, h = TeleportMenu.WindowHeight - 24;

            b.End();
            using RasterizerState state = new RasterizerState
            {
                ScissorTestEnable = true
            };
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, state);
            b.GraphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);
            {
                int iy = y + TeleportMenu.EdgePad;
                iy += this.Scroll;
                foreach (string loc in this.Locs)
                {
                    Rectangle area = new Rectangle(x, iy - 4, w - this.ScrollbarBack.Width, TeleportMenu.ElemHeight);
                    if (area.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        b.Draw(Game1.staminaRect, area, new Color(200, 32, 32, 64));
                        if (this.JustClicked)
                        {
                            this.WarpTo = loc;
                        }
                    }

                    b.DrawString(Game1.dialogueFont, loc, new Vector2(x + TeleportMenu.EdgePad, iy), Color.Black);

                    iy += TeleportMenu.ElemHeight;
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            if (this.Locs.Count > h / TeleportMenu.ElemHeight)
            {
                this.Scrollbar.Y = this.ScrollbarBack.Y + 2 + (int)(((this.Scroll / (float)-TeleportMenu.ElemHeight) / (this.Locs.Count - (h - 20) / (float)TeleportMenu.ElemHeight)) * (this.ScrollbarBack.Height - this.Scrollbar.Height));

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.ScrollbarBack.X, this.ScrollbarBack.Y, this.ScrollbarBack.Width, this.ScrollbarBack.Height, Color.DarkGoldenrod, Game1.pixelZoom, false);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.Scrollbar.X, this.Scrollbar.Y, this.Scrollbar.Width, this.Scrollbar.Height, Color.Gold, Game1.pixelZoom, false);
            }

            this.JustClicked = false;

            base.draw(b);
            this.drawMouse(b);
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (this.ScrollbarBack.Contains(x, y))
            {
                this.DragScroll = true;
            }
            else
            {
                this.JustClicked = true;
            }
        }

        /// <inheritdoc />
        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);

            this.DragScroll = false;
        }

        /// <inheritdoc />
        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            this.Scroll += direction;
            if (this.Scroll > 0)
                this.Scroll = 0;

            int cap = this.Locs.Count * 50 - (TeleportMenu.WindowHeight - 24) + 16;
            if (this.Scroll < -cap)
                this.Scroll = -cap;
        }
    }
}
