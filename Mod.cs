using LuckSkill.Other;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Events;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuckSkill
{
    public class Mod : StardewModdingAPI.Mod
    {
        public const int PROFESSION_DAILY_LUCK = 5 * 6;
        public const int PROFESSION_MOREQUESTS = 5 * 6 + 1;// 4;
        public const int PROFESSION_A1 = 5 * 6 + 2;
        public const int PROFESSION_A2 = 5 * 6 + 3;
        public const int PROFESSION_NIGHTLY_EVENTS = 5 * 6 + 4;// 1;
        public const int PROFESSION_B2 = 5 * 6 + 5;

        public static Mod instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            
            helper.Events.Display.MenuChanged += onMenuChanged;
            helper.Events.Player.Warped += onWarped;
            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
            helper.Events.Display.RenderedActiveMenu += onRenderedActiveMenu;
            helper.Events.GameLoop.DayStarted += onDayStarted;
            helper.Events.GameLoop.GameLaunched += onGameLaunched;

            SpaceEvents.ChooseNightlyFarmEvent += changeFarmEvent;

            checkForAllProfessions();
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onDayStarted( object sender, DayStartedEventArgs args )
        {
            gainLuckExp((int)(Game1.player.team.sharedDailyLuck.Value * 750));

            if (Game1.player.professions.Contains(PROFESSION_DAILY_LUCK))
            {
                Game1.player.team.sharedDailyLuck.Value += 0.01;
            }
            if (Game1.player.professions.Contains(PROFESSION_MOREQUESTS) && Game1.questOfTheDay == null)
            {
                if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
                {
                    // Vanilla code doesn't put quests on these days.
                }
                else
                {
                    Quest quest = null;
                    for (uint i = 0; i < 2 && quest == null; ++i)
                    {
                        Game1.stats.daysPlayed += i * 999999; // To rig the rng to not just give the same results.
                        try // Just in case. Want to make sure stats.daysPlayed gets fixed
                        {
                            quest = Utility.getQuestOfTheDay();
                        }
                        catch (Exception) { }
                        Game1.stats.daysPlayed -= i * 999999;
                    }

                    if (quest != null)
                    {
                        Log.info($"Applying quest {quest} for today, due to having PROFESSION_MOREQUESTS.");
                        Game1.questOfTheDay = quest;
                    }
                }
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged( object sender, MenuChangedEventArgs e )
        {
            // fishingExp
            if (!(e.OldMenu is BobberBar fishing) || e.NewMenu != null)
                return;

            float diff = Helper.Reflection.GetField<float>(fishing, "difficulty").GetValue();
            bool perfect = Helper.Reflection.GetField<bool>(fishing, "perfect").GetValue();
            bool treasure = Helper.Reflection.GetField<bool>(fishing, "treasureCaught").GetValue();
            if ( perfect )
            {
                //gainLuckExp((int)(diff / 7) + 1);
            }
            if ( treasure )
            {
                gainLuckExp((int)(diff));
            }
        }
        
        private bool hadGeode = false;

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            /*
            if (Game1.isEating != wasEating)
            {
                Log.Async("Eating:" + Game1.isEating);
                Log.Async(Game1.player.itemToEat + " " + ((Game1.player.itemToEat != null) ? Game1.player.itemToEat.getStack() : -1));
                Log.Async(Game1.player.ActiveObject + " " +( (Game1.player.ActiveObject != null) ? Game1.player.ActiveObject.getStack() : -1));
            }
            wasEating = Game1.isEating;
            */

            if (Game1.activeClickableMenu != null)
            {
                if (Game1.activeClickableMenu is GeodeMenu)
                {
                    GeodeMenu menu = Game1.activeClickableMenu as GeodeMenu;
                    if (menu.geodeSpot.item != null & !hadGeode)
                    {
                        gainLuckExp(10);
                    }
                    hadGeode = (menu.geodeSpot.item != null);
                }
                else if (Game1.activeClickableMenu is LevelUpMenu)
                {
                    LevelUpMenu menu = Game1.activeClickableMenu as LevelUpMenu;
                    int skill = Helper.Reflection.GetField<int>(menu, "currentSkill").GetValue();
                    if (skill == 5)
                    {
                        int level = Helper.Reflection.GetField<int>(menu, "currentLevel").GetValue();
                        Game1.activeClickableMenu = new LuckLevelUpMenu(skill, level);
                    }
                }
            }
        }

        private bool didInitSkills = false;

        /// <summary>When a menu is open (<see cref="Game1.activeClickableMenu"/> isn't null), raised after that menu is drawn to the sprite batch but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onRenderedActiveMenu( object sender, RenderedActiveMenuEventArgs e )
        {
            if ( Game1.activeClickableMenu is GameMenu )
            {
                GameMenu menu = Game1.activeClickableMenu as GameMenu;
                if ( menu.currentTab == GameMenu.skillsTab )
                {
                    var tabs = Helper.Reflection.GetField< List < IClickableMenu > >(menu, "pages" ).GetValue();
                    var skills = tabs[GameMenu.skillsTab] as SkillsPage;

                    if (skills == null)
                        return;

                    if ( !didInitSkills )
                    {
                        initLuckSkill(skills);
                        didInitSkills = true;
                    }
                    drawLuckSkill( skills );
                }
            }
            else didInitSkills = false;
        }

        private void initLuckSkill( SkillsPage skills )
        {
            // Bunch of stuff from the constructor
            int num2 = 0;
            int num3 = skills.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 * Game1.tileSize - Game1.pixelZoom;
            int num4 = skills.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.pixelZoom * 3;
            for (int i = 4; i < 10; i += 5)
            {
                int j = 5;

                string text = "";
                string text2 = "";
                bool flag = false;
                int num5 = -1;

                flag = (Game1.player.LuckLevel > i);
                num5 = getLuckProfessionForSkill( i + 1 );//Game1.player.getProfessionForSkill(5, i + 1);
                object[] args = new object[] { text, text2, LuckLevelUpMenu.getProfessionDescription(num5) };
                Helper.Reflection.GetMethod(skills, "parseProfessionDescription").Invoke(args);
                text = (string)args[0];
                text2 = (string)args[1];

                if (flag && (i + 1) % 5 == 0)
                {
                    var skillBars = Helper.Reflection.GetField< List < ClickableTextureComponent > >(skills, "skillBars" ).GetValue();
                    skillBars.Add(new ClickableTextureComponent(string.Concat(num5), new Rectangle(num2 + num3 - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom), num4 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6), 14 * Game1.pixelZoom, 9 * Game1.pixelZoom), null, text, Game1.mouseCursors, new Rectangle(159, 338, 14, 9), (float)Game1.pixelZoom, true));
                }
                num2 += Game1.pixelZoom * 6;
            }
            int k = 5;
            int num6 = k;
            if (num6 == 1)
            {
                num6 = 3;
            }
            else if (num6 == 3)
            {
                num6 = 1;
            }
            string text3 = "";
            if (Game1.player.LuckLevel > 0)
            {
                text3 = "Luck Increased";
            }
            var skillAreas = Helper.Reflection.GetField<List<ClickableTextureComponent>>(skills, "skillAreas").GetValue();
            skillAreas.Add(new ClickableTextureComponent(string.Concat(num6), new Rectangle(num3 - Game1.tileSize * 2 - Game1.tileSize * 3 / 4, num4 + k * (Game1.tileSize / 2 + Game1.pixelZoom * 6), Game1.tileSize * 2 + Game1.pixelZoom * 5, 9 * Game1.pixelZoom), string.Concat(num6), text3, null, Rectangle.Empty, 1f, false));
        }

        private void drawLuckSkill( SkillsPage skills )
        {
            SpriteBatch b = Game1.spriteBatch;
            int j = 5;

            int num;
            int num2;

            num = skills.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 * Game1.tileSize - 8;
            num2 = skills.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.pixelZoom * 2;
			
			int num3 = 0;
			for (int i = 0; i < 10; i++)
            {
                bool flag = false;
                bool flag2 = false;
                string text = "";
                int num4 = 0;
                Rectangle empty = Rectangle.Empty;

                flag = (Game1.player.LuckLevel > i);
                if (i == 0)
                {
                    text = "Luck";
                }
                num4 = Game1.player.LuckLevel;
                flag2 = (Game1.player.addedLuckLevel.Value > 0);
                empty = new Rectangle(50, 428, 10, 10);

                if (!text.Equals(""))
                {
                    b.DrawString(Game1.smallFont, text, new Vector2((float)num - Game1.smallFont.MeasureString(text).X - (float)(Game1.pixelZoom * 4) - (float)Game1.tileSize, (float)(num2 + Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), Game1.textColor);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num - Game1.pixelZoom * 16), (float)(num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(empty), Color.Black * 0.3f, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.85f);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num - Game1.pixelZoom * 15), (float)(num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(empty), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
                }
                if (!flag && (i + 1) % 5 == 0)
                {
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(145, 338, 14, 9)), Color.Black * 0.35f, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(145 + (flag ? 14 : 0), 338, 14, 9)), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
                }
                else if ((i + 1) % 5 != 0)
                {
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(129, 338, 8, 9)), Color.Black * 0.35f, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.85f);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(129 + (flag ? 8 : 0), 338, 8, 9)), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
                }
                if (i == 9)
                {
                    NumberSprite.draw(num4, b, new Vector2((float)(num3 + num + (i + 2) * (Game1.tileSize / 2 + Game1.pixelZoom) + Game1.pixelZoom * 3 + ((num4 >= 10) ? (Game1.pixelZoom * 3) : 0)), (float)(num2 + Game1.pixelZoom * 4 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), Color.Black * 0.35f, 1f, 0.85f, 1f, 0, 0);
                    NumberSprite.draw(num4, b, new Vector2((float)(num3 + num + (i + 2) * (Game1.tileSize / 2 + Game1.pixelZoom) + Game1.pixelZoom * 4 + ((num4 >= 10) ? (Game1.pixelZoom * 3) : 0)), (float)(num2 + Game1.pixelZoom * 3 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), (flag2 ? Color.LightGreen : Color.SandyBrown) * ((num4 == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0, 0);
                }
                if ((i + 1) % 5 == 0)
                {
                    num3 += Game1.pixelZoom * 6;
                }
            }

            skills.drawMouse(b);
        }
        
        private void changeFarmEvent(object sender, EventArgsChooseNightlyFarmEvent args)
        {
            if ( Game1.player.professions.Contains( PROFESSION_NIGHTLY_EVENTS ) && !Game1.weddingToday &&
                    ( args.NightEvent == null || (args.NightEvent is SoundInTheNightEvent &&
                    Helper.Reflection.GetField<int>(args.NightEvent, "behavior").GetValue() == 2 ) ) )
            {
                //Log.Async("Doing event check");
                FarmEvent ev = null;
                //for (uint i = 0; i < 100 && ev == null; ++i) // Testing purposes.
                {
                    Game1.stats.daysPlayed += 999999; // To rig the rng to not just give the same results.
                    try // Just in case. Want to make sure stats.daysPlayed gets fixed
                    {
                        ev = Utility.pickFarmEvent();
                    }
                    catch (Exception) { }
                    Game1.stats.daysPlayed -= 999999;
                    //if (ev != null) Log.Async("ev=" + ev + " " + (ev is SoundInTheNightEvent ? (Util.GetInstanceField(typeof(SoundInTheNightEvent), ev, "behavior") + " " + Util.GetInstanceField(typeof(SoundInTheNightEvent), ev, "soundName")) : "?"));
                    if (ev != null && ev.setUp())
                    {
                        ev = null;
                    }
                }

                if ( ev != null )
                {
                    Log.info($"Applying {ev} as tonight's nightly event, due to having PROFESSION_NIGHTLY_EVENTS");
                    args.NightEvent = ev;
                }
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            if ( HAS_ALL_PROFESSIONS )
            {
                // This is where AllProfessions does it.
                // This is that mod's code, too (from ILSpy, anyways). Just trying to give credit where credit is due. :P
                // Except this only applies for luck professions. Since they don't exist in vanilla AllProfessions doesn't take care of it.
                var professions = Game1.player.professions;
                List<List<int>> list = new List<List<int>> { luckProfessions5, luckProfessions10, };
                foreach (List<int> current in list)
                {
                    bool flag = professions.Intersect(current).Any<int>();
                    if (flag)
                    {
                        foreach (int current2 in current)
                        {
                            bool flag2 = !professions.Contains(current2);
                            if (flag2)
                            {
                                professions.Add(current2);
                            }
                        }
                    }
                }
                //End of AllProfessions code.
            }

            // I wanna see this "SpiritAltar"
            /*
            var ev = args as EventArgsCurrentLocationChanged;
            if (ev.NewLocation.name != "SeedShop")
                return;

            GameLocation loc = ev.NewLocation;
            foreach (var layer in loc.map.Layers)
            {
                //var layer = loc.map.GetLayer("Buildings");
                Log.Async("Layer: " + layer.Id + " " + layer.LayerWidth + " " + layer.LayerHeight + " " + layer.LayerSize);
                for (int ix = 0; ix < layer.LayerWidth; ++ix)
                {
                    for (int iy = 0; iy < layer.LayerHeight; ++iy)
                    {
                        var tile = layer.Tiles[ix, iy];
                        try
                        {
                            if (tile.Properties["Action"] == "Yoba" )
                            {
                                Log.Async("Changing action from " + tile.Properties["Action"] + " " + ix + " " + iy);
                                tile.Properties["Action"] = new xTile.ObjectModel.PropertyValue("SpiritAltar");
                            }
                        }
                        catch (Exception e) { }
                    }
                }
            }*/
        }

        // Copied from vanilla Farmer.gainExperience
        public static void gainLuckExp(int howMuch)
        {
            int which = 5;

            if (/*which == 5 ||*/ howMuch <= 0)
            {
                return;
            }
            int num = Farmer.checkForLevelGain(Game1.player.experiencePoints[which], Game1.player.experiencePoints[which] + howMuch);
            Game1.player.experiencePoints[which] += howMuch;
            int num2 = -1;
            if (num != -1)
            {
                switch (which)
                {/*
                    case 0:
                        num2 = this.farmingLevel;
                        this.farmingLevel = num;
                        break;
                    case 1:
                        num2 = this.fishingLevel;
                        this.fishingLevel = num;
                        break;
                    case 2:
                        num2 = this.foragingLevel;
                        this.foragingLevel = num;
                        break;
                    case 3:
                        num2 = this.miningLevel;
                        this.miningLevel = num;
                        break;
                    case 4:
                        num2 = this.combatLevel;
                        this.combatLevel = num;
                        break;*/
                    case 5:
                        num2 = Game1.player.luckLevel;
                        Game1.player.LuckLevel = num;
                        break;
                }
            }
            if (num > num2)
            {
                for (int i = num2 + 1; i <= num; i++)
                {
                    Game1.player.newLevels.Add(new Point(which, i));
                    Game1.player.newLevels.Count<Point>();
                }
            }
        }

        private int getLuckProfessionForSkill( int level )
        {
            if (level != 5 && level != 10)
                return -1;

            List<int> list = (level == 5 ? luckProfessions5 : luckProfessions10);
            foreach (int prof in list)
            {
                if (Game1.player.professions.Contains(prof))
                    return prof;
            }

            return -1;
        }

        private void onGameLaunched(object sender, EventArgs args)
        {
            // enableLuckSkillBar
            var api = Helper.ModRegistry.GetApi<ExperienceBarsApi>("spacechase0.ExperienceBars");
            Log.trace($"Experience Bars API {(api == null ? "not " : "")}found");
            api?.SetDrawLuck(true);
        }

        private bool HAS_ALL_PROFESSIONS = false;
        private List<int> luckProfessions5 = new List<int>() { PROFESSION_DAILY_LUCK, PROFESSION_MOREQUESTS };
        private List<int> luckProfessions10 = new List<int>() { PROFESSION_A1, PROFESSION_A2, PROFESSION_NIGHTLY_EVENTS, PROFESSION_B2 };

        private void checkForAllProfessions()
        {
            if (!Helper.ModRegistry.IsLoaded("cantorsdust.AllProfessions"))
            {
                Log.info("All Professions not found.");
                return;
            }

            Log.info("All Professions found. You will get every luck profession for your level.");
            HAS_ALL_PROFESSIONS = true;
        }
    }
}
