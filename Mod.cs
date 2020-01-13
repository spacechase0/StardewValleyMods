using Harmony;
using LuckSkill.Overrides;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LuckSkill
{
    public class Mod : StardewModdingAPI.Mod, IAssetEditor
    {
        public const int PROFESSION_DAILY_LUCK = 5 * 6;
        public const int PROFESSION_MORE_QUESTS = 5 * 6 + 1;// 4;
        public const int PROFESSION_CHANCE_MAX_LUCK = 5 * 6 + 2;
        public const int PROFESSION_NO_BAD_LUCK = 5 * 6 + 3;
        public const int PROFESSION_NIGHTLY_EVENTS = 5 * 6 + 4;// 1;
        public const int PROFESSION_JUNIMO_HELP = 5 * 6 + 5;

        public static Mod instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            
            helper.Events.Player.Warped += onWarped;
            helper.Events.Display.RenderedActiveMenu += onRenderedActiveMenu;
            helper.Events.GameLoop.DayStarted += onDayStarted;
            helper.Events.GameLoop.DayEnding += onDayEnding;
            helper.Events.GameLoop.GameLaunched += onGameLaunched;

            SpaceEvents.ChooseNightlyFarmEvent += changeFarmEvent;

            try
            {
                var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
                Log.trace("Doing harmony patches...");
                harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.getProfessionForSkill)), postfix: new HarmonyMethod(AccessTools.Method(typeof(FarmerGetProfessionHook), nameof(LevelUpMenuProfessionNameHook.Postfix))));
                harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)), prefix: new HarmonyMethod(AccessTools.Method(typeof(OverpoweredGeodeFix), nameof(OverpoweredGeodeFix.Prefix))));
                harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(ExperienceGainFix), nameof(ExperienceGainFix.Transpiler))));
                harmony.Patch(AccessTools.Constructor(typeof(LevelUpMenu), new Type[] { typeof(int), typeof(int) }), transpiler: new HarmonyMethod( AccessTools.Method(typeof(LevelUpMenuLuckProfessionConstructorFix), nameof(LevelUpMenuLuckProfessionConstructorFix.Transpiler))) );
                harmony.Patch(AccessTools.Method(typeof(LevelUpMenu), "getProfessionName"), postfix: new HarmonyMethod( AccessTools.Method(typeof(LevelUpMenuProfessionNameHook), nameof(LevelUpMenuProfessionNameHook.Postfix))) );
                harmony.Patch(AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.AddMissedProfessionChoices)), transpiler: new HarmonyMethod( AccessTools.Method(typeof(LevelUpMenuMissedStuffPatch), nameof(LevelUpMenuMissedStuffPatch.Transpiler))) );
                harmony.Patch(AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.AddMissedLevelRecipes)), transpiler: new HarmonyMethod( AccessTools.Method(typeof(LevelUpMenuMissedStuffPatch), nameof(LevelUpMenuMissedStuffPatch.Transpiler))) );
            }
            catch ( Exception e )
            {
                Log.trace("Exception doing harmony: " + e);
            }

            checkForAllProfessions();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Strings\\UI");
        }

        public void Edit<T>(IAssetData asset)
        {
            Func<int, string> getProfName = (id) => Helper.Reflection.GetMethod(typeof(LevelUpMenu), "getProfessionName").Invoke<string>(id);

            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionName_" + getProfName(PROFESSION_DAILY_LUCK), "Fortunate");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionDescription_" + getProfName(PROFESSION_DAILY_LUCK), "Better daily luck.");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionName_" + getProfName(PROFESSION_NIGHTLY_EVENTS), "Shooting Star");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionDescription_" + getProfName(PROFESSION_NIGHTLY_EVENTS), "Nightly events occur twice as often.");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionName_" + getProfName(PROFESSION_CHANCE_MAX_LUCK), "Lucky");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionDescription_" + getProfName(PROFESSION_CHANCE_MAX_LUCK), "20% chance for max daily luck.");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionName_" + getProfName(PROFESSION_NO_BAD_LUCK), "Un-unlucky");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionDescription_" + getProfName(PROFESSION_NO_BAD_LUCK), "Never have bad luck.");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionName_" + getProfName(PROFESSION_MORE_QUESTS), "Popular Helper");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionDescription_" + getProfName(PROFESSION_MORE_QUESTS), "Daily quests occur three times as often.");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionName_" + getProfName(PROFESSION_JUNIMO_HELP), "Spirit Child");
            asset.AsDictionary<string, string>().Data.Add("LevelUp_ProfessionDescription_" + getProfName(PROFESSION_JUNIMO_HELP), "Giving fits makes junimos happy. They might help your farm.\n(15% chance for some form of farm advancement.)");
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onDayStarted( object sender, DayStartedEventArgs args )
        {
            Game1.player.gainExperience(Farmer.luckSkill, (int)(Game1.player.team.sharedDailyLuck.Value * 750));

            if (Game1.player.professions.Contains(PROFESSION_DAILY_LUCK))
            {
                Game1.player.team.sharedDailyLuck.Value += 0.01;
            }
            if (Game1.player.professions.Contains(PROFESSION_CHANCE_MAX_LUCK))
            {
                Random r = new Random((int)(Game1.uniqueIDForThisGame+Game1.stats.DaysPlayed * 3));
                if (r.NextDouble() <= 0.20)
                {
                    Game1.player.team.sharedDailyLuck.Value = 0.12;
                }
            }
            if (Game1.player.professions.Contains(PROFESSION_NO_BAD_LUCK))
            {
                if (Game1.player.team.sharedDailyLuck.Value < 0)
                    Game1.player.team.sharedDailyLuck.Value = 0;
            }
            if (Game1.player.professions.Contains(PROFESSION_MORE_QUESTS) && Game1.questOfTheDay == null)
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
                        finally
                        {
                            Game1.stats.daysPlayed -= i * 999999;
                        }
                    }

                    if (quest != null)
                    {
                        Log.info($"Applying quest {quest} for today, due to having PROFESSION_MOREQUESTS.");
                        Game1.questOfTheDay = quest;
                    }
                }
            }
        }

        private void onDayEnding(object sender, DayEndingEventArgs args)
        {
            if ( Game1.player.professions.Contains(PROFESSION_JUNIMO_HELP))
            {
                int rolls = 0;
                foreach ( var friendKey in Game1.player.friendshipData.Keys )
                {
                    var data = Game1.player.friendshipData[friendKey];
                    if (data.GiftsToday > 0)
                        rolls++;
                }

                Random r = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
                while ( rolls-- > 0 )
                {
                    if (r.NextDouble() > 0.15)
                        continue;
                    rolls = 0;

                    Action advanceCrops = () =>
                    {
                        List<GameLocation> locs = new List<GameLocation>();
                        locs.AddRange(Game1.locations);
                        for (int i = 0; i < locs.Count; ++i)
                        {
                            GameLocation loc = locs[i];
                            if (loc == null) // From buildings without a valid indoors
                                continue;
                            if (loc is BuildableGameLocation bgl)
                                locs.AddRange(bgl.buildings.Select(b => b.indoors.Value));

                            foreach (var entry in loc.objects.Pairs.ToList())
                            {
                                var obj = entry.Value;
                                if ( obj is IndoorPot pot )
                                {
                                    var dirt = pot.hoeDirt.Value;
                                    if (dirt.crop == null || dirt.crop.fullyGrown.Value)
                                        continue;

                                    dirt.crop.newDay(HoeDirt.watered, dirt.fertilizer.Value, (int)entry.Key.X, (int)entry.Key.Y, loc);
                                }
                            }
                            foreach (var entry in loc.terrainFeatures.Pairs.ToList())
                            {
                                var tf = entry.Value;
                                if (tf is HoeDirt dirt)
                                {
                                    if (dirt.crop == null || dirt.crop.fullyGrown.Value)
                                        continue;

                                    dirt.crop.newDay(HoeDirt.watered, dirt.fertilizer.Value, (int)entry.Key.X, (int)entry.Key.Y, loc);
                                }
                                else if (tf is FruitTree ftree)
                                {
                                    ftree.dayUpdate(loc, entry.Key);
                                }
                                else if (tf is Tree tree)
                                {
                                    tree.dayUpdate(loc, entry.Key);
                                }
                            }
                        }

                        Game1.showGlobalMessage("The junimos advanced your crops!");
                    };
                    Action<AnimalHouse> advanceBarn = (AnimalHouse house) =>
                    {
                        foreach (var animal in house.Animals.Values)
                        {
                            animal.friendshipTowardFarmer.Value = Math.Min( 1000, animal.friendshipTowardFarmer.Value + 100 );
                        }
                        Game1.showGlobalMessage("The junimos made some of your animals more fond of you!");
                    };
                    Action grassAndFences = () =>
                    {
                        var farm = Game1.getFarm();
                        foreach ( var entry in farm.terrainFeatures.Values )
                        {
                            if ( entry is Grass grass )
                            {
                                grass.numberOfWeeds.Value = 4;
                            }
                        }
                        foreach (var entry in farm.Objects.Values)
                        {
                            if ( entry is Fence fence )
                            {
                                fence.repair();
                            }
                        }
                        Game1.showGlobalMessage("The junimos grew your grass and repaired your fences!");
                    };

                    if ( r.Next() <= 0.05 && Game1.player.addItemToInventoryBool(new StardewValley.Object(StardewValley.Object.prismaticShardIndex, 1)) )
                    {
                        Game1.showGlobalMessage("The junimos gave you a prismatic shard!");
                        continue;
                    }

                    var animalHouses = new List<AnimalHouse>();
                    foreach ( var loc in Game1.locations )
                    {
                        if ( loc is BuildableGameLocation bgl )
                        {
                            foreach ( var building in bgl.buildings )
                            {
                                if ( building.indoors.Value is AnimalHouse ah )
                                {
                                    bool foundAnimalWithRoomForGrowth = false;
                                    foreach ( var animal in ah.Animals.Values )
                                    {
                                        if ( animal.friendshipTowardFarmer.Value < 1000 )
                                        {
                                            foundAnimalWithRoomForGrowth = true;
                                            break;
                                        }
                                    }
                                    if (foundAnimalWithRoomForGrowth)
                                        animalHouses.Add(ah);
                                }
                            }
                        }
                    }

                    List<Action> choices = new List<Action>();
                    choices.Add(advanceCrops);
                    choices.Add(advanceCrops);
                    choices.Add(advanceCrops);
                    foreach ( var ah in animalHouses )
                    {
                        choices.Add(() => advanceBarn(ah));
                    }
                    choices.Add(grassAndFences);

                    choices[r.Next(choices.Count)]();
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
                object[] args = new object[] { text, text2, LevelUpMenu.getProfessionDescription(num5) };
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
                    Helper.Reflection.GetField<NetInt>(args.NightEvent, "behavior").GetValue().Value == 2 ) ) )
            {
                //Log.Async("Doing event check");
                FarmEvent ev = null;
                //for (uint i = 0; i < 100 && ev == null; ++i) // Testing purposes.
                {
                    Game1.stats.daysPlayed += 999999; // To rig the rng to not just give the same results.
                    try // Just in case. Want to make sure stats.daysPlayed gets fixed
                    {
                        ev = pickFarmEvent();
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

        private FarmEvent pickFarmEvent()
        {
            Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
            if (Game1.weddingToday)
                return (FarmEvent)null;
            foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
            {
                Friendship spouseFriendship = onlineFarmer.GetSpouseFriendship();
                if (spouseFriendship != null && spouseFriendship.IsMarried() && spouseFriendship.WeddingDate == Game1.Date)
                    return (FarmEvent)null;
            }
            if (Game1.stats.DaysPlayed == 31U)
                return (FarmEvent)new SoundInTheNightEvent(4);
            if (Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry"))
                return (FarmEvent)new WorldChangeEvent(0);
            if (Game1.player.mailForTomorrow.Contains("ccPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("ccPantry"))
                return (FarmEvent)new WorldChangeEvent(1);
            if (Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault"))
                return (FarmEvent)new WorldChangeEvent(6);
            if (Game1.player.mailForTomorrow.Contains("ccVault%&NL&%") || Game1.player.mailForTomorrow.Contains("ccVault"))
                return (FarmEvent)new WorldChangeEvent(7);
            if (Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom"))
                return (FarmEvent)new WorldChangeEvent(2);
            if (Game1.player.mailForTomorrow.Contains("ccBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccBoilerRoom"))
                return (FarmEvent)new WorldChangeEvent(3);
            if (Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom"))
                return (FarmEvent)new WorldChangeEvent(4);
            if (Game1.player.mailForTomorrow.Contains("ccCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccCraftsRoom"))
                return (FarmEvent)new WorldChangeEvent(5);
            if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaFishTank"))
                return (FarmEvent)new WorldChangeEvent(8);
            if (Game1.player.mailForTomorrow.Contains("ccFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("ccFishTank"))
                return (FarmEvent)new WorldChangeEvent(9);
            if (Game1.player.mailForTomorrow.Contains("ccMovieTheaterJoja%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaMovieTheater"))
                return (FarmEvent)new WorldChangeEvent(10);
            if (Game1.player.mailForTomorrow.Contains("ccMovieTheater%&NL&%") || Game1.player.mailForTomorrow.Contains("ccMovieTheater"))
                return (FarmEvent)new WorldChangeEvent(11);
            if (Game1.MasterPlayer.eventsSeen.Contains(191393) && (Game1.isRaining || Game1.isLightning) && (!Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible") && !Game1.MasterPlayer.mailReceived.Contains("ccMovieTheater")))
                return (FarmEvent)new WorldChangeEvent(12);
            if (random.NextDouble() < 0.01 && !Game1.currentSeason.Equals("winter"))
                return (FarmEvent)new FairyEvent();
            if (random.NextDouble() < 0.01)
                return (FarmEvent)new WitchEvent();
            if (random.NextDouble() < 0.01)
                return (FarmEvent)new SoundInTheNightEvent(1);
            if (random.NextDouble() < 0.01 && Game1.year > 1)
                return (FarmEvent)new SoundInTheNightEvent(0);
            if (random.NextDouble() < 0.01)
                return (FarmEvent)new SoundInTheNightEvent(3);
            return (FarmEvent)null;
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
            var api = Helper.ModRegistry.GetApi<ExperienceBarsAPI>("spacechase0.ExperienceBars");
            Log.trace($"Experience Bars API {(api == null ? "not " : "")}found");
            api?.SetDrawLuck(true);
        }

        private bool HAS_ALL_PROFESSIONS = false;
        private List<int> luckProfessions5 = new List<int>() { PROFESSION_DAILY_LUCK, PROFESSION_MORE_QUESTS };
        private List<int> luckProfessions10 = new List<int>() { PROFESSION_CHANCE_MAX_LUCK, PROFESSION_NO_BAD_LUCK, PROFESSION_NIGHTLY_EVENTS, PROFESSION_JUNIMO_HELP };

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
