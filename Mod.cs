using System;
using System.Linq;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using Microsoft.Xna.Framework.Input;

// TODO: Render on skills page?

namespace ExperienceBars
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static readonly int[] expNeededForLevel = new int[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

        public static Configuration Config;

        public static bool renderLuck = false;
        public static int expBottom = 0;
        public static bool show = true;
        private static bool stopLevelExtenderCompat = false;

        public override void Entry( IModHelper helper )
        {
            Config = helper.ReadConfig<Configuration>();

            helper.Events.Display.RenderedHud += onRenderedHud;
            helper.Events.Input.ButtonPressed += onButtonPressed;
        }

        public override object GetApi()
        {
            return new Api();
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if ( e.Button == Config.ToggleBars )
            {
                if (show && (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.GetKeyboardState().IsKeyDown(Keys.RightShift)))
                {
                    Config.X = (int)e.Cursor.ScreenPixels.X;
                    Config.Y = (int)e.Cursor.ScreenPixels.Y;
                    Helper.WriteConfig(Config);
                }
                else
                {
                    show = !show;
                }
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void onRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // renderExpBars

            if (!show || Game1.activeClickableMenu != null)
                return;
            
            int[] skills = new int[]
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
            if (Helper.ModRegistry.IsLoaded("Devin Lematty.Level Extender") && !stopLevelExtenderCompat)
            {
                try
                {
                    var instance = Type.GetType("LevelExtender.ModEntry, LevelExtender").GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    var extLevels = Helper.Reflection.GetField<int[]>(instance, "sLevs").GetValue();
                    var extExp = Helper.Reflection.GetField<int[]>(instance, "addedXP").GetValue();
                    exp = (int[]) exp.Clone();
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
                    Monitor.Log("Exception during level extender compat: " + ex, LogLevel.Error);
                    stopLevelExtenderCompat = true;
                }
            }

            int x = Config.X;
            int y = Config.Y; ;
            if (Game1.player.currentLocation != null && Game1.player.currentLocation is MineShaft &&
                x <= 25 && y <= 75)
                y += 75;
            for ( int i = 0; i < (renderLuck ? 6 : 5); ++i )
            {
                int prevReq = 0, nextReq = 1;
                if ( skills[ i ] == 0 )
                {
                    nextReq = expNeededForLevel[ 0 ];
                }
                else if ( skills[ i ] < 10 )
                {
                    prevReq = expNeededForLevel[ skills[ i ] - 1 ];
                    nextReq = expNeededForLevel[ skills[ i ] ];
                }
                else if ( foundLevelExtender )
                {
                    prevReq = 0;
                    nextReq = (int)(skills[i] * 1000 + (skills[i] * skills[i] * skills[i] * 0.3));
                }

                int haveExp = exp[ i ] - prevReq;
                int needExp = nextReq - prevReq;
                float progress = ( float ) haveExp / needExp;
                if ( skills[ i ] == 10 && !foundLevelExtender || skills[ i ] == 100 )
                {
                    progress = -1;
                }
                
                renderSkillBar( x, y, Game1.buffsIcons, getSkillRect( i ), skills[ i ], progress, getSkillColor( i ) );
                
                y += 40;
            }
            expBottom = y;
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
        private Color getSkillColor( int skill )
        {
            if (skillColors != null)
                return skillColors[skill];

            // Taken from the icons
            skillColors = new Color[]
            {
                new Color( 115, 255, 56 ),
                new Color( 117, 225, 255 ),
                new Color( 0xCD, 0x7F, 0x32 ),//new Color( 78, 183, 0 ),
                new Color( 247, 31, 0 ),
                new Color( 178, 255, 211 ),
                new Color( 255, 255, 84 ),
            };

            return skillColors[skill];
        }

        private const int BAR_WIDTH = 102;
        private const int BAR_HEIGHT = 10;
        private static readonly Color BAR_BORDER = Color.DarkGoldenrod;
        private static readonly Color BAR_BG = Color.Black;
        private static readonly Color BAR_FG = new Color(150, 150, 150);
        private static readonly Color BAR_BG_TICK = new Color(50, 50, 50);
        private static readonly Color BAR_FG_TICK = new Color(120, 120, 120);
        private static Texture2D skillBg, skillFg;

        public static void renderSkillBar( int x, int y, Texture2D iconTex, Rectangle icon, int level, float progress, Color skillCol )
        {
            if (!show) return;

            var b = Game1.spriteBatch;

            if ( skillBg == null || skillFg == null )
                setupExpBars();

            b.Draw( iconTex, new Rectangle( x, y, 32, 32 ), icon, Color.White );

            int extra = 0;
            if ( level > 9  ) extra += 16;
            if ( level > 99 ) extra += 20;
            NumberSprite.draw(level, b, new Vector2(x + 32 + 4 + 16 + extra, y + 16), Color.White, 0.75f, 0, 1, 0);

            if (progress < 0 || progress > 100)
                return;

            Rectangle barRect = new Rectangle( x + 32 + 4 + 32 + extra + 4, y + ( 32 - BAR_HEIGHT ) / 2 - 1, BAR_WIDTH * 2, BAR_HEIGHT * 2 );
            b.Draw(skillBg, barRect, new Rectangle(0, 0, BAR_WIDTH, BAR_HEIGHT), Color.White);
            barRect.Width = (int)(barRect.Width * progress);
            b.Draw(skillFg, barRect, new Rectangle(0, 0, barRect.Width / 2, BAR_HEIGHT), skillCol);
        }

        private static void setupExpBars()
        {
            skillBg = new Texture2D(Game1.graphics.GraphicsDevice, BAR_WIDTH, BAR_HEIGHT);
            skillFg = new Texture2D(Game1.graphics.GraphicsDevice, BAR_WIDTH, BAR_HEIGHT);
            Color[] emptyColors = new Color[BAR_WIDTH * BAR_HEIGHT];
            Color[] fillColors = new Color[BAR_WIDTH * BAR_HEIGHT];
            for (int ix = 0; ix < BAR_WIDTH; ++ix)
            {
                for (int iy = 0; iy < BAR_HEIGHT; ++iy)
                {
                    Color e = BAR_BG;
                    Color f = BAR_FG;
                    if (ix == 0 || iy == 0 || ix == BAR_WIDTH - 1 || iy == BAR_HEIGHT - 1)
                    {
                        e = BAR_BORDER;
                        f = Color.Transparent;

                        // Corners
                        if (ix == 0 && iy == 0 || ix == BAR_WIDTH - 1 && iy == 0 ||
                             ix == 0 && iy == BAR_HEIGHT - 1 || ix == BAR_WIDTH - 1 && iy == BAR_HEIGHT - 1)
                        {
                            e = Color.Transparent;
                        }
                    }
                    else if ((ix - 1) % 10 == 0)
                    {
                        e = BAR_BG_TICK;
                        f = BAR_FG_TICK;
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



                    emptyColors[ix + iy * BAR_WIDTH] = e;
                    fillColors[ix + iy * BAR_WIDTH] = f;
                }
            }
            skillBg.SetData(emptyColors);
            skillFg.SetData(fillColors);
        }
    }
}
