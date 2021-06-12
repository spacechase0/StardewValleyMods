using System;
using System.Linq;
using System.Reflection;
using ExperienceBars.Framework;
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
    internal class Mod : StardewModdingAPI.Mod
    {
        public static readonly int[] ExpNeededForLevel = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

        public static Configuration Config;

        public static bool RenderLuck = false;
        public static int ExpBottom;
        public static bool Show = true;
        private static bool StopLevelExtenderCompat;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        public override object GetApi()
        {
            return new Api();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
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
        public void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Mod.Config.ToggleBars)
            {
                if (Mod.Show && (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.GetKeyboardState().IsKeyDown(Keys.RightShift)))
                {
                    Mod.Config.X = (int)e.Cursor.ScreenPixels.X;
                    Mod.Config.Y = (int)e.Cursor.ScreenPixels.Y;
                    this.Helper.WriteConfig(Mod.Config);
                }
                else
                {
                    Mod.Show = !Mod.Show;
                }
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // renderExpBars

            if (!Mod.Show || Game1.activeClickableMenu != null || Game1.eventUp || !Context.IsPlayerFree)
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
            if (this.Helper.ModRegistry.IsLoaded("Devin Lematty.Level Extender") && !Mod.StopLevelExtenderCompat)
            {
                try
                {
                    var instance = Type.GetType("LevelExtender.ModEntry, LevelExtender").GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    int[] extLevels = this.Helper.Reflection.GetField<int[]>(instance, "sLevs").GetValue();
                    int[] extExp = this.Helper.Reflection.GetField<int[]>(instance, "addedXP").GetValue();
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
                    Log.Error("Exception during level extender compat: " + ex);
                    Mod.StopLevelExtenderCompat = true;
                }
            }

            int x = Mod.Config.X;
            int y = Mod.Config.Y; ;
            if (Game1.player.currentLocation != null && Game1.player.currentLocation is MineShaft &&
                x <= 25 && y <= 75)
                y += 75;
            for (int i = 0; i < (Mod.RenderLuck ? 6 : 5); ++i)
            {
                int prevReq = 0, nextReq = 1;
                if (skills[i] == 0)
                {
                    nextReq = Mod.ExpNeededForLevel[0];
                }
                else if (skills[i] < 10)
                {
                    prevReq = Mod.ExpNeededForLevel[skills[i] - 1];
                    nextReq = Mod.ExpNeededForLevel[skills[i]];
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

                Mod.RenderSkillBar(x, y, Game1.buffsIcons, this.GetSkillRect(i), skills[i], progress, this.GetSkillColor(i));

                y += 40;
            }
            Mod.ExpBottom = y;
        }

        private Rectangle GetSkillRect(int skill)
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

        private Color[] SkillColors;
        private Color GetSkillColor(int skill)
        {
            if (this.SkillColors != null)
                return this.SkillColors[skill];

            // Taken from the icons
            this.SkillColors = new Color[]
            {
                new(115, 255, 56),
                new(117, 225, 255),
                new(0xCD, 0x7F, 0x32),//new Color(78, 183, 0),
                new(247, 31, 0),
                new(178, 255, 211),
                new(255, 255, 84),
            };

            return this.SkillColors[skill];
        }

        private const int BarWidth = 102;
        private const int BarHeight = 10;
        private static readonly Color BarBorder = Color.DarkGoldenrod;
        private static readonly Color BarBg = Color.Black;
        private static readonly Color BarFg = new(150, 150, 150);
        private static readonly Color BarBgTick = new(50, 50, 50);
        private static readonly Color BarFgTick = new(120, 120, 120);
        private static Texture2D SkillBg, SkillFg;

        public static void RenderSkillBar(int x, int y, Texture2D iconTex, Rectangle icon, int level, float progress, Color skillCol)
        {
            if (!Mod.Show) return;
            if (Game1.activeClickableMenu != null || Game1.eventUp || !Context.IsPlayerFree) return;

            var b = Game1.spriteBatch;

            if (Mod.SkillBg == null || Mod.SkillFg == null)
                Mod.SetupExpBars();

            b.Draw(iconTex, new Rectangle(x, y, 32, 32), icon, Color.White);

            int extra = 0;
            if (level > 9) extra += 16;
            if (level > 99) extra += 20;
            NumberSprite.draw(level, b, new Vector2(x + 32 + 4 + 16 + extra, y + 16), Color.White, 0.75f, 0, 1, 0);

            if (progress < 0 || progress > 100)
                return;

            Rectangle barRect = new Rectangle(x + 32 + 4 + 32 + extra + 4, y + (32 - Mod.BarHeight) / 2 - 1, Mod.BarWidth * 2, Mod.BarHeight * 2);
            b.Draw(Mod.SkillBg, barRect, new Rectangle(0, 0, Mod.BarWidth, Mod.BarHeight), Color.White);
            barRect.Width = (int)(barRect.Width * progress);
            b.Draw(Mod.SkillFg, barRect, new Rectangle(0, 0, barRect.Width / 2, Mod.BarHeight), skillCol);
        }

        private static void SetupExpBars()
        {
            Mod.SkillBg = new Texture2D(Game1.graphics.GraphicsDevice, Mod.BarWidth, Mod.BarHeight);
            Mod.SkillFg = new Texture2D(Game1.graphics.GraphicsDevice, Mod.BarWidth, Mod.BarHeight);
            Color[] emptyColors = new Color[Mod.BarWidth * Mod.BarHeight];
            Color[] fillColors = new Color[Mod.BarWidth * Mod.BarHeight];
            for (int ix = 0; ix < Mod.BarWidth; ++ix)
            {
                for (int iy = 0; iy < Mod.BarHeight; ++iy)
                {
                    Color e = Mod.BarBg;
                    Color f = Mod.BarFg;
                    if (ix == 0 || iy == 0 || ix == Mod.BarWidth - 1 || iy == Mod.BarHeight - 1)
                    {
                        e = Mod.BarBorder;
                        f = Color.Transparent;

                        // Corners
                        if (ix == 0 && iy == 0 || ix == Mod.BarWidth - 1 && iy == 0 ||
                             ix == 0 && iy == Mod.BarHeight - 1 || ix == Mod.BarWidth - 1 && iy == Mod.BarHeight - 1)
                        {
                            e = Color.Transparent;
                        }
                    }
                    else if ((ix - 1) % 10 == 0)
                    {
                        e = Mod.BarBgTick;
                        f = Mod.BarFgTick;
                    }

                    float s = iy switch
                    {
                        1 => 1.3f,
                        2 => 1.7f,
                        3 => 2.0f,
                        4 => 1.9f,
                        5 => 1.5f,
                        6 => 1.3f,
                        7 => 1.0f,
                        8 => 0.8f,
                        9 => 0.4f,
                        _ => 1
                    };
                    e = Color.Multiply(e, s);
                    f = Color.Multiply(f, s);



                    emptyColors[ix + iy * Mod.BarWidth] = e;
                    fillColors[ix + iy * Mod.BarWidth] = f;
                }
            }
            Mod.SkillBg.SetData(emptyColors);
            Mod.SkillFg.SetData(fillColors);
        }
    }
}
