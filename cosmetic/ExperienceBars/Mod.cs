using System;
using System.Linq;
using System.Reflection;
using ExperienceBars.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
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
        private static readonly Color DefaultBarForeground = new(150, 150, 150);

        public static bool RenderLuck = false;
        public static int ExpBottom;
        public static bool Show = true;
        private static bool StopLevelExtenderCompat;

        private const int BarWidth = 102;
        private const int BarHeight = 10;
        private static Texture2D SkillBackground;
        private static Texture2D SkillForeground;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
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
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_PositionX_Name,
                    tooltip: I18n.Config_PositionX_Tooltip,
                    getValue: () => Mod.Config.Position.X,
                    setValue: value => Mod.Config.Position = new(value, Mod.Config.Position.Y)
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_PositionY_Name,
                    tooltip: I18n.Config_PositionY_Tooltip,
                    getValue: () => Mod.Config.Position.Y,
                    setValue: value => Mod.Config.Position = new(Mod.Config.Position.X, value)
                );
                configMenu.AddKeybind(
                    mod: this.ModManifest,
                    name: I18n.Config_ToggleKey_Name,
                    tooltip: I18n.Config_ToggleKey_Tooltip,
                    getValue: () => Mod.Config.ToggleBars,
                    setValue: value => Mod.Config.ToggleBars = value
                );
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
                    Mod.Config.Position = new Point(
                        (int)e.Cursor.ScreenPixels.X,
                        (int)e.Cursor.ScreenPixels.Y
                    );
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
                    object instance = Type.GetType("LevelExtender.ModEntry, LevelExtender").GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
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

            int x = Mod.Config.Position.X;
            int y = Mod.Config.Position.Y;
            if (Game1.player.currentLocation is MineShaft && x <= 25 && y <= 75)
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
            return skill switch
            {
                0 => new Rectangle(0, 0, 16, 16),
                1 => new Rectangle(16, 0, 16, 16),
                2 => new Rectangle(80, 0, 16, 16),
                3 => new Rectangle(32, 0, 16, 16),
                4 => new Rectangle(128, 16, 16, 16),
                5 => new Rectangle(64, 0, 16, 16),
                _ => new Rectangle(32, 16, 16, 16) // The eye thing
            };
        }

        private Color GetSkillColor(int skill)
        {
            var colors = Mod.Config.SkillColors;
            return skill switch
            {
                Farmer.combatSkill => colors.Combat,
                Farmer.farmingSkill => colors.Farming,
                Farmer.fishingSkill => colors.Fishing,
                Farmer.foragingSkill => colors.Foraging,
                Farmer.luckSkill => colors.Luck,
                Farmer.miningSkill => colors.Mining,
                _ => Mod.DefaultBarForeground
            };
        }

        public static void RenderSkillBar(int x, int y, Texture2D iconTex, Rectangle icon, int level, float progress, Color skillCol)
        {
            if (!Mod.Show || Game1.activeClickableMenu != null || Game1.eventUp || !Context.IsPlayerFree)
                return;

            var b = Game1.spriteBatch;

            if (Mod.SkillBackground == null || Mod.SkillForeground == null)
                Mod.SetupExpBars();

            b.Draw(iconTex, new Rectangle(x, y, 32, 32), icon, Color.White);

            int extra = 0;
            if (level > 9)
                extra += 16;
            if (level > 99)
                extra += 20;

            NumberSprite.draw(level, b, new Vector2(x + 32 + 4 + 16 + extra, y + 16), Color.White, 0.75f, 0, 1, 0);

            if (progress is < 0 or > 100)
                return;

            Rectangle barRect = new Rectangle(x + 32 + 4 + 32 + extra + 4, y + (32 - Mod.BarHeight) / 2 - 1, Mod.BarWidth * 2, Mod.BarHeight * 2);
            b.Draw(Mod.SkillBackground, barRect, new Rectangle(0, 0, Mod.BarWidth, Mod.BarHeight), Color.White);
            barRect.Width = (int)(barRect.Width * progress);
            b.Draw(Mod.SkillForeground, barRect, new Rectangle(0, 0, barRect.Width / 2, Mod.BarHeight), skillCol);
        }

        private static void SetupExpBars()
        {
            var colors = Mod.Config.BaseColors;

            Mod.SkillBackground = new Texture2D(Game1.graphics.GraphicsDevice, Mod.BarWidth, Mod.BarHeight);
            Mod.SkillForeground = new Texture2D(Game1.graphics.GraphicsDevice, Mod.BarWidth, Mod.BarHeight);
            Color[] emptyColors = new Color[Mod.BarWidth * Mod.BarHeight];
            Color[] fillColors = new Color[Mod.BarWidth * Mod.BarHeight];
            for (int x = 0; x < Mod.BarWidth; ++x)
            {
                for (int y = 0; y < Mod.BarHeight; ++y)
                {
                    Color background = colors.BarBackground;
                    Color foreground = Mod.DefaultBarForeground;
                    if (x == 0 || y == 0 || x == Mod.BarWidth - 1 || y == Mod.BarHeight - 1)
                    {
                        background = colors.BarBorder;
                        foreground = Color.Transparent;

                        // Corners
                        if (x == 0 && y == 0 || x == Mod.BarWidth - 1 && y == 0 || x == 0 && y == Mod.BarHeight - 1 || x == Mod.BarWidth - 1 && y == Mod.BarHeight - 1)
                            background = Color.Transparent;
                    }
                    else if ((x - 1) % 10 == 0)
                    {
                        background = colors.BarBackgroundTick;
                        foreground = colors.BarForegroundTick;
                    }

                    float colorMultiply = y switch
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
                    background = Color.Multiply(background, colorMultiply);
                    foreground = Color.Multiply(foreground, colorMultiply);



                    emptyColors[x + y * Mod.BarWidth] = background;
                    fillColors[x + y * Mod.BarWidth] = foreground;
                }
            }
            Mod.SkillBackground.SetData(emptyColors);
            Mod.SkillForeground.SetData(fillColors);
        }
    }
}
