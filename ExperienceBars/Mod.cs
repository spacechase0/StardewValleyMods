using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

// TODO: Render on skills page?

namespace ExperienceBars
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static readonly int[] expNeededForLevel = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

        public static Configuration Config;

        public static bool renderLuck = false;
        public static int expBottom = 0;
        public static bool show = true;
        private static bool stopLevelExtenderCompat = false;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Display.RenderedHud += this.onRenderedHud;
            helper.Events.Input.ButtonPressed += this.onButtonPressed;
        }

        public override object GetApi()
        {
            return new Api();
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "UI X", "The X position of the UI on-screen.", () => Mod.Config.X, (int val) => Mod.Config.X = val);
                capi.RegisterSimpleOption(this.ModManifest, "UI Y", "The Y position of the UI on-screen.", () => Mod.Config.Y, (int val) => Mod.Config.Y = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Toggle Display", "Press this key to toggle the display.\nHolding Shift lets you move the display as well. ", () => Mod.Config.ToggleBars, (SButton val) => Mod.Config.ToggleBars = val);
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Mod.Config.ToggleBars)
            {
                if (Mod.show && (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.GetKeyboardState().IsKeyDown(Keys.RightShift)))
                {
                    Mod.Config.X = (int)e.Cursor.ScreenPixels.X;
                    Mod.Config.Y = (int)e.Cursor.ScreenPixels.Y;
                    this.Helper.WriteConfig(Mod.Config);
                }
                else
                {
                    Mod.show = !Mod.show;
                }
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void onRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // renderExpBars

            if (!Mod.show || Game1.activeClickableMenu != null || Game1.eventUp || !Context.IsPlayerFree)
                return;

            int[] skills = new[]
            {
                Game1.player.farmingLevel.Value,
                Game1.player.fishingLevel.Value,
                Game1.player.foragingLevel.Value,
                Game1.player.miningLevel.Value,
                Game1.player.combatLevel.Value,
                Game1.player.luckLevel.Value
            };
            int[] exp = Game1.player.experiencePoints.ToArray();

            bool foundLevelExtender = false;
            if (this.Helper.ModRegistry.IsLoaded("Devin Lematty.Level Extender") && !Mod.stopLevelExtenderCompat)
            {
                try
                {
                    var instance = Type.GetType("LevelExtender.ModEntry, LevelExtender").GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    var extLevels = this.Helper.Reflection.GetField<int[]>(instance, "sLevs").GetValue();
                    var extExp = this.Helper.Reflection.GetField<int[]>(instance, "addedXP").GetValue();
                    exp = (int[])exp.Clone();
                    for (int i = 0; i < 5; ++i)
                    {
                        if (skills[i] < extLevels[i])
                            continue;
                        skills[i] = extLevels[i];
                        exp[i] = skills[i] < 10 ? exp[i] : extExp[i];
                    }
                    foundLevelExtender = true;
                }
                catch (Exception ex)
                {
                    Log.error("Exception during level extender compat: " + ex);
                    Mod.stopLevelExtenderCompat = true;
                }
            }

            int x = Mod.Config.X;
            int y = Mod.Config.Y; ;
            if (Game1.player.currentLocation != null && Game1.player.currentLocation is MineShaft &&
                x <= 25 && y <= 75)
                y += 75;
            for (int i = 0; i < (Mod.renderLuck ? 6 : 5); ++i)
            {
                int prevReq = 0, nextReq = 1;
                if (skills[i] == 0)
                {
                    nextReq = Mod.expNeededForLevel[0];
                }
                else if (skills[i] < 10)
                {
                    prevReq = Mod.expNeededForLevel[skills[i] - 1];
                    nextReq = Mod.expNeededForLevel[skills[i]];
                }
                else if (foundLevelExtender)
                {
                    prevReq = 0;
                    nextReq = (int)(skills[i] * 1000 + (skills[i] * skills[i] * skills[i] * 0.3));
                }

                int haveExp = exp[i] - prevReq;
                int needExp = nextReq - prevReq;
                float progress = (float)haveExp / needExp;
                if (skills[i] == 10 && !foundLevelExtender || skills[i] == 100)
                {
                    progress = -1;
                }

                Mod.renderSkillBar(x, y, Game1.buffsIcons, this.getSkillRect(i), skills[i], progress, this.getSkillColor(i));

                y += 40;
            }
            Mod.expBottom = y;
        }

        private Rectangle getSkillRect(int skill)
        {
            switch (skill)
            {
                case 0: return new Rectangle(0, 0, 16, 16);
                case 1: return new Rectangle(16, 0, 16, 16);
                case 2: return new Rectangle(80, 0, 16, 16);
                case 3: return new Rectangle(32, 0, 16, 16);
                case 4: return new Rectangle(128, 16, 16, 16);
                case 5: return new Rectangle(64, 0, 16, 16);
            }

            return new Rectangle(32, 16, 16, 16); // The eye thing
        }

        private Color[] skillColors;
        private Color getSkillColor(int skill)
        {
            if (this.skillColors != null)
                return this.skillColors[skill];

            // Taken from the icons
            this.skillColors = new Color[]
            {
                new(115, 255, 56),
                new(117, 225, 255),
                new(0xCD, 0x7F, 0x32),//new Color(78, 183, 0),
                new(247, 31, 0),
                new(178, 255, 211),
                new(255, 255, 84),
            };

            return this.skillColors[skill];
        }

        private const int BAR_WIDTH = 102;
        private const int BAR_HEIGHT = 10;
        private static readonly Color BAR_BORDER = Color.DarkGoldenrod;
        private static readonly Color BAR_BG = Color.Black;
        private static readonly Color BAR_FG = new(150, 150, 150);
        private static readonly Color BAR_BG_TICK = new(50, 50, 50);
        private static readonly Color BAR_FG_TICK = new(120, 120, 120);
        private static Texture2D skillBg, skillFg;

        public static void renderSkillBar(int x, int y, Texture2D iconTex, Rectangle icon, int level, float progress, Color skillCol)
        {
            if (!Mod.show) return;
            if (Game1.activeClickableMenu != null || Game1.eventUp || !Context.IsPlayerFree) return;

            var b = Game1.spriteBatch;

            if (Mod.skillBg == null || Mod.skillFg == null)
                Mod.setupExpBars();

            b.Draw(iconTex, new Rectangle(x, y, 32, 32), icon, Color.White);

            int extra = 0;
            if (level > 9) extra += 16;
            if (level > 99) extra += 20;
            NumberSprite.draw(level, b, new Vector2(x + 32 + 4 + 16 + extra, y + 16), Color.White, 0.75f, 0, 1, 0);

            if (progress < 0 || progress > 100)
                return;

            Rectangle barRect = new Rectangle(x + 32 + 4 + 32 + extra + 4, y + (32 - Mod.BAR_HEIGHT) / 2 - 1, Mod.BAR_WIDTH * 2, Mod.BAR_HEIGHT * 2);
            b.Draw(Mod.skillBg, barRect, new Rectangle(0, 0, Mod.BAR_WIDTH, Mod.BAR_HEIGHT), Color.White);
            barRect.Width = (int)(barRect.Width * progress);
            b.Draw(Mod.skillFg, barRect, new Rectangle(0, 0, barRect.Width / 2, Mod.BAR_HEIGHT), skillCol);
        }

        private static void setupExpBars()
        {
            Mod.skillBg = new Texture2D(Game1.graphics.GraphicsDevice, Mod.BAR_WIDTH, Mod.BAR_HEIGHT);
            Mod.skillFg = new Texture2D(Game1.graphics.GraphicsDevice, Mod.BAR_WIDTH, Mod.BAR_HEIGHT);
            Color[] emptyColors = new Color[Mod.BAR_WIDTH * Mod.BAR_HEIGHT];
            Color[] fillColors = new Color[Mod.BAR_WIDTH * Mod.BAR_HEIGHT];
            for (int ix = 0; ix < Mod.BAR_WIDTH; ++ix)
            {
                for (int iy = 0; iy < Mod.BAR_HEIGHT; ++iy)
                {
                    Color e = Mod.BAR_BG;
                    Color f = Mod.BAR_FG;
                    if (ix == 0 || iy == 0 || ix == Mod.BAR_WIDTH - 1 || iy == Mod.BAR_HEIGHT - 1)
                    {
                        e = Mod.BAR_BORDER;
                        f = Color.Transparent;

                        // Corners
                        if (ix == 0 && iy == 0 || ix == Mod.BAR_WIDTH - 1 && iy == 0 ||
                             ix == 0 && iy == Mod.BAR_HEIGHT - 1 || ix == Mod.BAR_WIDTH - 1 && iy == Mod.BAR_HEIGHT - 1)
                        {
                            e = Color.Transparent;
                        }
                    }
                    else if ((ix - 1) % 10 == 0)
                    {
                        e = Mod.BAR_BG_TICK;
                        f = Mod.BAR_FG_TICK;
                    }

                    float s = 1;
                    if (iy == 1) s = 1.3f;
                    if (iy == 2) s = 1.7f;
                    if (iy == 3) s = 2.0f;
                    if (iy == 4) s = 1.9f;
                    if (iy == 5) s = 1.5f;
                    if (iy == 6) s = 1.3f;
                    if (iy == 7) s = 1.0f;
                    if (iy == 8) s = 0.8f;
                    if (iy == 9) s = 0.4f;
                    e = Color.Multiply(e, s);
                    f = Color.Multiply(f, s);



                    emptyColors[ix + iy * Mod.BAR_WIDTH] = e;
                    fillColors[ix + iy * Mod.BAR_WIDTH] = f;
                }
            }
            Mod.skillBg.SetData(emptyColors);
            Mod.skillFg.SetData(fillColors);
        }
    }
}
