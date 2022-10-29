using System;
using System.Collections.Generic;
using System.Linq;
using LuckSkill.Framework;
using LuckSkill.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Spacechase.Shared.Patching;
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
using SObject = StardewValley.Object;

namespace LuckSkill
{
    internal class Mod : StardewModdingAPI.Mod 
    {
        /*********
        ** Fields
        *********/
        private bool HasAllProfessions;
        private readonly List<int> LuckProfessions5 = new() { Mod.FortunateProfessionId, Mod.PopularHelperProfessionId };
        private readonly List<int> LuckProfessions10 = new() { Mod.LuckyProfessionId, Mod.UnUnluckyProfessionId, Mod.ShootingStarProfessionId, Mod.SpiritChildProfessionId };


        /*********
        ** Accessors
        *********/
        public const int FortunateProfessionId = 5 * 6;
        public const int PopularHelperProfessionId = 5 * 6 + 1;// 4;
        public const int LuckyProfessionId = 5 * 6 + 2;
        public const int UnUnluckyProfessionId = 5 * 6 + 3;
        public const int ShootingStarProfessionId = 5 * 6 + 4;// 1;
        public const int SpiritChildProfessionId = 5 * 6 + 5;

        public static Mod Instance;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            SpaceEvents.ChooseNightlyFarmEvent += this.ChangeFarmEvent;

            HarmonyPatcher.Apply(this,
                new FarmerPatcher(),
                new LevelUpMenuPatcher()
            );

            this.CheckForAllProfessions();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(@"Strings\UI"))
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    foreach (IProfession profession in this.GetProfessions().Values)
                    {
                        string internalKey = this.Helper.Reflection.GetMethod(typeof(LevelUpMenu), "getProfessionName").Invoke<string>(profession.Id);
                        data.Add($"LevelUp_ProfessionName_{internalKey}", profession.Name);
                        data.Add($"LevelUp_ProfessionDescription_{internalKey}", profession.Description);
                    }
                });
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new LuckSkillApi();
        }

        /// <summary>Get the available Luck professions.</summary>
        public IDictionary<int, IProfession> GetProfessions()
        {
            return new IProfession[]
            {
                new Profession(
                    id: Mod.FortunateProfessionId,
                    defaultName: "Fortunate",
                    defaultDescription:"Better daily luck.",
                    name: I18n.Fortunate_Name,
                    description: I18n.Fortunate_Desc
                ),
                new Profession(
                    id: Mod.ShootingStarProfessionId,
                    defaultName: "Shooting Star",
                    defaultDescription: "Nightly events occur twice as often.",
                    name: I18n.ShootingStar_Name,
                    description: I18n.ShootingStar_Desc
                ),
                new Profession(
                    id: Mod.LuckyProfessionId,
                    defaultName: "Lucky",
                    defaultDescription: "20% chance for max daily luck.",
                    name: I18n.Lucky_Name,
                    description: I18n.Lucky_Desc
                ),
                new Profession(
                    id: Mod.UnUnluckyProfessionId,
                    defaultName: "Un-unlucky",
                    defaultDescription: "Never have bad luck.",
                    name: I18n.UnUnlucky_Name,
                    description: I18n.UnUnlucky_Desc
                ),
                new Profession(
                    id: Mod.PopularHelperProfessionId,
                    defaultName: "Popular Helper",
                    defaultDescription: "Daily quests occur three times as often.",
                    name: I18n.PopularHelper_Name,
                    description: I18n.PopularHelper_Desc
                ),
                new Profession(
                    id: Mod.SpiritChildProfessionId,
                    defaultName: "Spirit Child",
                    defaultDescription: "Giving gifts makes junimos happy. They might help your farm.\n(15% chance for some form of farm advancement.)",
                    name: I18n.SpiritChild_Name,
                    description: I18n.SpiritChild_Desc
                )
            }.ToDictionary(p => p.Id);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs args)
        {
            Game1.player.gainExperience(Farmer.luckSkill, (int)(Game1.player.team.sharedDailyLuck.Value * 750));

            if (Game1.player.professions.Contains(Mod.FortunateProfessionId))
            {
                Game1.player.team.sharedDailyLuck.Value += 0.01;
            }
            if (Game1.player.professions.Contains(Mod.LuckyProfessionId))
            {
                Random r = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed * 3));
                if (r.NextDouble() <= 0.20)
                {
                    Game1.player.team.sharedDailyLuck.Value = 0.12;
                }
            }
            if (Game1.player.professions.Contains(Mod.UnUnluckyProfessionId))
            {
                if (Game1.player.team.sharedDailyLuck.Value < 0)
                    Game1.player.team.sharedDailyLuck.Value = 0;
            }
            if (Game1.player.professions.Contains(Mod.PopularHelperProfessionId) && Game1.questOfTheDay == null)
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
                        Log.Info($"Applying quest {quest} for today, due to having PROFESSION_MOREQUESTS.");
                        Game1.questOfTheDay = quest;
                    }
                }
            }
        }

        private void OnDayEnding(object sender, DayEndingEventArgs args)
        {
            if (Game1.player.professions.Contains(Mod.SpiritChildProfessionId))
            {
                int rolls = 0;
                foreach (string friendKey in Game1.player.friendshipData.Keys)
                {
                    var data = Game1.player.friendshipData[friendKey];
                    if (data.GiftsToday > 0)
                        rolls++;
                }

                Random r = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
                while (rolls-- > 0)
                {
                    if (r.NextDouble() > 0.15)
                        continue;
                    rolls = 0;

                    void AdvanceCrops()
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
                                if (obj is IndoorPot pot)
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

                        Game1.showGlobalMessage(I18n.JunimoRewards_GrowCrops());
                    }

                    void AdvanceBarn(AnimalHouse house)
                    {
                        foreach (var animal in house.Animals.Values)
                        {
                            animal.friendshipTowardFarmer.Value = Math.Min(1000, animal.friendshipTowardFarmer.Value + 100);
                        }

                        Game1.showGlobalMessage(I18n.JunimoRewards_AnimalFriendship());
                    }

                    void GrassAndFences()
                    {
                        var farm = Game1.getFarm();
                        foreach (var entry in farm.terrainFeatures.Values)
                        {
                            if (entry is Grass grass)
                            {
                                grass.numberOfWeeds.Value = 4;
                            }
                        }

                        foreach (var entry in farm.Objects.Values)
                        {
                            if (entry is Fence fence)
                            {
                                fence.repair();
                            }
                        }

                        Game1.showGlobalMessage(I18n.JunimoRewards_GrowGrass());
                    }

                    if (r.Next() <= 0.05 && Game1.player.addItemToInventoryBool(new SObject(SObject.prismaticShardIndex, 1)))
                    {
                        Game1.showGlobalMessage(I18n.JunimoRewards_PrismaticShard());
                        continue;
                    }

                    var animalHouses = new List<AnimalHouse>();
                    foreach (var loc in Game1.locations)
                    {
                        if (loc is BuildableGameLocation bgl)
                        {
                            foreach (var building in bgl.buildings)
                            {
                                if (building.indoors.Value is AnimalHouse ah)
                                {
                                    bool foundAnimalWithRoomForGrowth = false;
                                    foreach (var animal in ah.Animals.Values)
                                    {
                                        if (animal.friendshipTowardFarmer.Value < 1000)
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

                    List<Action> choices = new() { AdvanceCrops, AdvanceCrops, AdvanceCrops };
                    foreach (var ah in animalHouses)
                    {
                        choices.Add(() => AdvanceBarn(ah));
                    }
                    choices.Add(GrassAndFences);

                    choices[r.Next(choices.Count)]();
                }
            }
        }

        private bool DidInitSkills;

        /// <summary>When a menu is open (<see cref="Game1.activeClickableMenu"/> isn't null), raised after that menu is drawn to the sprite batch but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu menu)
            {
                if (menu.currentTab == GameMenu.skillsTab)
                {
                    if (menu.pages[GameMenu.skillsTab] is not SkillsPage page)
                        return;

                    if (!this.DidInitSkills)
                    {
                        this.InitLuckSkill(page);
                        this.DidInitSkills = true;
                    }

                    this.DrawLuckSkill(page);
                }
            }
            else
                this.DidInitSkills = false;
        }

        private void InitLuckSkill(SkillsPage skills)
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
                num5 = this.GetLuckProfessionForSkill(i + 1);//Game1.player.getProfessionForSkill(5, i + 1);
                object[] args = new object[] { text, text2, LevelUpMenu.getProfessionDescription(num5) };
                this.Helper.Reflection.GetMethod(skills, "parseProfessionDescription").Invoke(args);
                text = (string)args[0];
                text2 = (string)args[1];

                if (flag && (i + 1) % 5 == 0)
                {
                    var skillBars = skills.skillBars;
                    skillBars.Add(new ClickableTextureComponent(string.Concat(num5), new Rectangle(num2 + num3 - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom), num4 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6), 14 * Game1.pixelZoom, 9 * Game1.pixelZoom), null, text, Game1.mouseCursors, new Rectangle(159, 338, 14, 9), Game1.pixelZoom, true));
                }
                num2 += Game1.pixelZoom * 6;
            }
            int k = 5;
            int num6 = k;
            num6 = num6 switch
            {
                1 => 3,
                3 => 1,
                _ => num6
            };
            string text3 = "";
            if (Game1.player.LuckLevel > 0)
            {
                text3 = I18n.Skill_LevelUp();
            }
            var skillAreas = skills.skillAreas;
            skillAreas.Add(new ClickableTextureComponent(string.Concat(num6), new Rectangle(num3 - Game1.tileSize * 2 - Game1.tileSize * 3 / 4, num4 + k * (Game1.tileSize / 2 + Game1.pixelZoom * 6), Game1.tileSize * 2 + Game1.pixelZoom * 5, 9 * Game1.pixelZoom), string.Concat(num6), text3, null, Rectangle.Empty, 1f));
        }

        private void DrawLuckSkill(SkillsPage skills)
        {
            SpriteBatch b = Game1.spriteBatch;
            string hoverText = this.Helper.Reflection.GetField<string>(skills, "hoverText").GetValue();
            string hoverTitle = this.Helper.Reflection.GetField<string>(skills, "hoverTitle").GetValue();

            // draw skills bar
            {
                int j = 5;

                int num = skills.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 * Game1.tileSize - 8;
                int num2 = skills.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.pixelZoom * 2;

                int num3 = 0;
                for (int i = 0; i < 10; i++)
                {
                    string text = "";

                    bool flag = (Game1.player.LuckLevel > i);
                    if (i == 0)
                        text = I18n.Skill_Name();
                    int num4 = Game1.player.LuckLevel;
                    bool flag2 = (Game1.player.addedLuckLevel.Value > 0);
                    Rectangle empty = new Rectangle(50, 428, 10, 10);

                    if (!text.Equals(""))
                    {
                        b.DrawString(Game1.smallFont, text, new Vector2(num - Game1.smallFont.MeasureString(text).X - Game1.pixelZoom * 4 - Game1.tileSize, num2 + Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), Game1.textColor);
                        b.Draw(Game1.mouseCursors, new Vector2(num - Game1.pixelZoom * 16, num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), empty, Color.Black * 0.3f, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.85f);
                        b.Draw(Game1.mouseCursors, new Vector2(num - Game1.pixelZoom * 15, num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), empty, Color.White, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
                    }
                    if (!flag && (i + 1) % 5 == 0)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(num3 + num - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom), num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), new Rectangle(145, 338, 14, 9), Color.Black * 0.35f, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
                        b.Draw(Game1.mouseCursors, new Vector2(num3 + num + i * (Game1.tileSize / 2 + Game1.pixelZoom), num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), new Rectangle(145 + (flag ? 14 : 0), 338, 14, 9), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
                    }
                    else if ((i + 1) % 5 != 0)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(num3 + num - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom), num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), new Rectangle(129, 338, 8, 9), Color.Black * 0.35f, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.85f);
                        b.Draw(Game1.mouseCursors, new Vector2(num3 + num + i * (Game1.tileSize / 2 + Game1.pixelZoom), num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), new Rectangle(129 + (flag ? 8 : 0), 338, 8, 9), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.87f);
                    }
                    if (i == 9)
                    {
                        NumberSprite.draw(num4, b, new Vector2(num3 + num + (i + 2) * (Game1.tileSize / 2 + Game1.pixelZoom) + Game1.pixelZoom * 3 + ((num4 >= 10) ? (Game1.pixelZoom * 3) : 0), num2 + Game1.pixelZoom * 4 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
                        NumberSprite.draw(num4, b, new Vector2(num3 + num + (i + 2) * (Game1.tileSize / 2 + Game1.pixelZoom) + Game1.pixelZoom * 4 + ((num4 >= 10) ? (Game1.pixelZoom * 3) : 0), num2 + Game1.pixelZoom * 3 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6)), (flag2 ? Color.LightGreen : Color.SandyBrown) * ((num4 == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);
                    }
                    if ((i + 1) % 5 == 0)
                    {
                        num3 += Game1.pixelZoom * 6;
                    }
                }
            }

            // redraw cursor + tooltip over new UI
            skills.drawMouse(Game1.spriteBatch);
            if (hoverText?.Length > 0)
                IClickableMenu.drawHoverText(Game1.spriteBatch, hoverText, Game1.smallFont, boldTitleText: hoverTitle);
        }

        private void ChangeFarmEvent(object sender, EventArgsChooseNightlyFarmEvent args)
        {
            if (Game1.player.professions.Contains(Mod.ShootingStarProfessionId) && !Game1.weddingToday &&
                    (args.NightEvent == null || (args.NightEvent is SoundInTheNightEvent &&
                    this.Helper.Reflection.GetField<NetInt>(args.NightEvent, "behavior").GetValue().Value == 2)))
            {
                //Log.Async("Doing event check");
                FarmEvent ev = null;
                //for (uint i = 0; i < 100 && ev == null; ++i) // Testing purposes.
                {
                    Game1.stats.daysPlayed += 999999; // To rig the rng to not just give the same results.
                    try // Just in case. Want to make sure stats.daysPlayed gets fixed
                    {
                        ev = this.PickFarmEvent();
                    }
                    catch (Exception) { }
                    Game1.stats.daysPlayed -= 999999;
                    //if (ev != null) Log.Async("ev=" + ev + " " + (ev is SoundInTheNightEvent ? (Util.GetInstanceField(typeof(SoundInTheNightEvent), ev, "behavior") + " " + Util.GetInstanceField(typeof(SoundInTheNightEvent), ev, "soundName")) : "?"));
                    if (ev != null && ev.setUp())
                    {
                        ev = null;
                    }
                }

                if (ev != null)
                {
                    Log.Info($"Applying {ev} as tonight's nightly event, due to having PROFESSION_NIGHTLY_EVENTS");
                    args.NightEvent = ev;
                }
            }
        }

        private FarmEvent PickFarmEvent()
        {
            Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
            if (Game1.weddingToday)
                return null;
            foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
            {
                Friendship spouseFriendship = onlineFarmer.GetSpouseFriendship();
                if (spouseFriendship != null && spouseFriendship.IsMarried() && spouseFriendship.WeddingDate == Game1.Date)
                    return null;
            }
            if (Game1.stats.DaysPlayed == 31U)
                return new SoundInTheNightEvent(4);
            if (Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry"))
                return new WorldChangeEvent(0);
            if (Game1.player.mailForTomorrow.Contains("ccPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("ccPantry"))
                return new WorldChangeEvent(1);
            if (Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault"))
                return new WorldChangeEvent(6);
            if (Game1.player.mailForTomorrow.Contains("ccVault%&NL&%") || Game1.player.mailForTomorrow.Contains("ccVault"))
                return new WorldChangeEvent(7);
            if (Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom"))
                return new WorldChangeEvent(2);
            if (Game1.player.mailForTomorrow.Contains("ccBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccBoilerRoom"))
                return new WorldChangeEvent(3);
            if (Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom"))
                return new WorldChangeEvent(4);
            if (Game1.player.mailForTomorrow.Contains("ccCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccCraftsRoom"))
                return new WorldChangeEvent(5);
            if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaFishTank"))
                return new WorldChangeEvent(8);
            if (Game1.player.mailForTomorrow.Contains("ccFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("ccFishTank"))
                return new WorldChangeEvent(9);
            if (Game1.player.mailForTomorrow.Contains("ccMovieTheaterJoja%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaMovieTheater"))
                return new WorldChangeEvent(10);
            if (Game1.player.mailForTomorrow.Contains("ccMovieTheater%&NL&%") || Game1.player.mailForTomorrow.Contains("ccMovieTheater"))
                return new WorldChangeEvent(11);
            if (Game1.MasterPlayer.eventsSeen.Contains(191393) && (Game1.isRaining || Game1.isLightning) && (!Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible") && !Game1.MasterPlayer.mailReceived.Contains("ccMovieTheater")))
                return new WorldChangeEvent(12);
            if (random.NextDouble() < 0.01 && !Game1.currentSeason.Equals("winter"))
                return new FairyEvent();
            if (random.NextDouble() < 0.01)
                return new WitchEvent();
            if (random.NextDouble() < 0.01)
                return new SoundInTheNightEvent(1);
            if (random.NextDouble() < 0.01 && Game1.year > 1)
                return new SoundInTheNightEvent(0);
            if (random.NextDouble() < 0.01)
                return new SoundInTheNightEvent(3);
            return null;
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            if (this.HasAllProfessions)
            {
                // This is where AllProfessions does it.
                // This is that mod's code, too (from ILSpy, anyways). Just trying to give credit where credit is due. :P
                // Except this only applies for luck professions. Since they don't exist in vanilla AllProfessions doesn't take care of it.
                var professions = Game1.player.professions;
                List<List<int>> list = new List<List<int>> { this.LuckProfessions5, this.LuckProfessions10 };
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

        private int GetLuckProfessionForSkill(int level)
        {
            if (level != 5 && level != 10)
                return -1;

            List<int> list = (level == 5 ? this.LuckProfessions5 : this.LuckProfessions10);
            foreach (int prof in list)
            {
                if (Game1.player.professions.Contains(prof))
                    return prof;
            }

            return -1;
        }

        private void OnGameLaunched(object sender, EventArgs args)
        {
            // enable Luck skill bar
            var api = this.Helper.ModRegistry.GetApi<IExperienceBarsApi>("spacechase0.ExperienceBars");
            api?.SetDrawLuck(true);
        }

        private void CheckForAllProfessions()
        {
            if (this.Helper.ModRegistry.IsLoaded("cantorsdust.AllProfessions"))
            {
                Log.Info("All Professions found. You will get every luck profession for your level.");
                this.HasAllProfessions = true;
            }
        }
    }
}
