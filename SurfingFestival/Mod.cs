using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using SurfingFestival.Framework;
using SurfingFestival.Patches;
using xTile;
using xTile.Layers;
using xTile.Tiles;
using SObject = StardewValley.Object;

namespace SurfingFestival
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        private static IJsonAssetsApi Ja;

        public const int SurfSpeed = 8;

        public static BonfireState PlayerDidBonfire = BonfireState.NotDone;
        public static List<string> Racers;
        public static Dictionary<string, RacerState> RacerState = new();
        public static string RaceWinner;
        public static List<Obstacle> Obstacles = new();

        public static Texture2D SurfboardTex;
        public static Texture2D SurfboardWaterTex;
        public static Texture2D StunTex;
        public static Texture2D ObstaclesTex;

        public static string FestivalName = "Surfing Festival";

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Mod.SurfboardTex = helper.ModContent.Load<Texture2D>("assets/surfboards.png");
            Mod.SurfboardWaterTex = helper.ModContent.Load<Texture2D>("assets/surfboard-water.png");
            Mod.StunTex = helper.ModContent.Load<Texture2D>("assets/net-stun.png");
            Mod.ObstaclesTex = helper.ModContent.Load<Texture2D>("assets/obstacles.png");

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            //helper.Events.Display.RenderedWorld += onRenderedWorld;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
            helper.Events.Multiplayer.ModMessageReceived += this.OnMessageReceived;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            SpaceEvents.ActionActivated += this.OnActionActivated;

            HarmonyPatcher.Apply(this,
                new CharacterPatcher(),
                new EventPatcher(),
                new FarmerPatcher()
            );
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data\\Festivals\\summer5"))
            {
                e.LoadFrom(() =>
                {
                    var data = this.BuildFestivalData();
                    Mod.FestivalName = data["name"];
                    return data;
                }, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps\\Beach-Surfing"))
                e.LoadFromModFile<Map>("assets/Beach.tbin", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps\\surfing"))
                e.LoadFromModFile<Texture2D>("assets/surfing.png", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo("Data\\Festivals\\FestivalDates"))
                e.Edit(static (asset) =>
                {
                    asset.AsDictionary<string, string>().Data.Add("summer5", Mod.FestivalName);
                });
        }

        /// <summary>Build the festival data file from the mod translations.</summary>
        private IDictionary<string, string> BuildFestivalData()
        {
            // base data
            var data = new Dictionary<string, string>
            {
                ["name"] = I18n.Festival_Name(),
                ["conditions"] = "Beach/900 1400",
                ["set-up"] = "event2/-1000 -1000/farmer 38 3 2/changeToTemporaryMap Beach-Surfing/loadActors Set-Up/animate Robin false true 500 20 21 20 22/animate Demetrius false true 500 24 25 24 26/viewport 38 2 clamp true/pause 1000/playerControl surfing",
                ["mainEvent"] = $@"pause 500/playMusic none/pause 500/globalFade/viewport -1000 -1000/loadActors MainEvent/warpSurfingRacers/viewport 18 57 true unfreeze/pause 2000/message ""{I18n.Race_Instructions()}""/speak Lewis ""{I18n.Race_LewisStart_0()}""/speak Lewis ""{I18n.Race_LewisStart_1()}""/speak Lewis ""{I18n.Race_LewisStart_2()}""/waitForOtherPlayers actualRace/playSound whistle/playMusic cowboy_outlawsong/playerControl surfingRace",
                ["afterSurfingRace"] = "pause 100/playSound whistle/waitForOtherPlayers endContest/pause 1000/globalFade/viewport -1000 -1000/playMusic event1/loadActors PostEvent/warpSurfingRacersFinish/pause 1000/viewport 34 12 true/pause 2000/speak Lewis \"{{winDialog}}\"/awardSurfingPrize/pause 600/viewport move 1 0 5000/pause 2000/globalFade/viewport -1000 -1000/waitForOtherPlayers festivalEnd/end",
                ["HarveyWin"] = I18n.Race_Winner_Harvey(),
                ["EmilyWin"] = I18n.Race_Winner_Emily(),
                ["MaruWin"] = I18n.Race_Winner_Maru(),
                ["ShaneWin"] = I18n.Race_Winner_Shane()
            };

            // NPC dialogue strings
            foreach (var translation in this.Helper.Translation.GetTranslations())
            {
                const string prefix = "npc.";
                if (translation.Key.StartsWith(prefix))
                    data[translation.Key.Substring(prefix.Length)] = translation.ToString();
            }

            return data;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spacecore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spacecore.AddEventCommand("warpSurfingRacers", PatchHelper.RequireMethod<Mod>(nameof(Mod.EventCommand_WarpSurfingRacers)));
            spacecore.AddEventCommand("warpSurfingRacersFinish", PatchHelper.RequireMethod<Mod>(nameof(Mod.EventCommand_WarpSurfingRacersFinish)));
            spacecore.AddEventCommand("awardSurfingPrize", PatchHelper.RequireMethod<Mod>(nameof(Mod.EventCommand_AwardSurfingPrize)));

            Mod.Ja = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            Mod.Ja.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets", "json-assets"), this.Helper.Translation);
        }

        private Event PrevEvent;
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (++Mod.SurfboardWaterAnimTimer >= 5)
            {
                Mod.SurfboardWaterAnimTimer = 0;
                if (++Mod.SurfboardWaterAnim >= 3)
                    Mod.SurfboardWaterAnim = 0;
            }
            if (++this.ItemBobbleTimer >= 25)
            {
                this.ItemBobbleTimer = 0;
                if (++this.ItemBobbleFrame >= 4)
                    this.ItemBobbleFrame = 0;
            }
            ++this.NetBobTimer;

            if (Game1.CurrentEvent?.FestivalName != Mod.FestivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace")
            {
                this.PrevEvent = Game1.CurrentEvent;
                return;
            }

            if (this.PrevEvent == null)
                Mod.PlayerDidBonfire = BonfireState.NotDone;
            this.PrevEvent = Game1.CurrentEvent;

            var rand = new Random();
            foreach (var actor in Game1.CurrentEvent.actors)
            {
                if (Mod.Racers.Contains(actor.Name))
                    continue;
                if (rand.Next(30 * Game1.CurrentEvent.actors.Count / 2) == 0)
                    actor.jumpWithoutSound();
            }

            foreach (var obstacle in Mod.Obstacles)
            {
                if (obstacle.Type is ObstacleType.HomingProjectile or ObstacleType.FirstPlaceProjectile)
                {
                    var targetRect = Game1.CurrentEvent.getCharacterByName(obstacle.HomingTarget).GetBoundingBox().Center;
                    var target = new Vector2(targetRect.X, targetRect.Y);
                    var current = obstacle.Position;

                    int speed = 15;
                    if (obstacle.Type == ObstacleType.FirstPlaceProjectile)
                        speed = 25;

                    if (Vector2.Distance(target, current) < speed)
                    {
                        current = target;
                    }
                    else
                    {
                        var unit = (target - current);
                        unit.Normalize();

                        current += unit * speed;
                    }
                    obstacle.Position = current;
                }
            }

            Vector2[][] switchDirs = new[]
            {
                new Vector2[]
                {
                    new(16, 60),
                    new(15, 61),
                    new(14, 62),
                    new(13, 63),
                    new(12, 64),
                    new(11, 65),
                    new(10, 66),
                    new(9, 67),
                    new(8, 68),
                    new(7, 69)
                },
                new Vector2[]
                {
                    new(16, 58),
                    new(15, 57),
                    new(14, 56),
                    new(13, 55),
                    new(12, 54),
                    new(11, 53),
                    new(10, 52),
                    new(9, 51),
                    new(8, 50),
                    new(7, 49)
                },
                new Vector2[]
                {
                    new(133, 58),
                    new(134, 57),
                    new(135, 56),
                    new(136, 55),
                    new(137, 54),
                    new(138, 53),
                    new(139, 52),
                    new(140, 51),
                    new(141, 50),
                    new(142, 49)
                },
                new Vector2[]
                {
                    new(133, 60),
                    new(134, 61),
                    new(135, 62),
                    new(136, 63),
                    new(137, 64),
                    new(138, 65),
                    new(139, 66),
                    new(140, 67),
                    new(141, 68),
                    new(142, 69)
                }
            };

            foreach (string racerName in Mod.Racers)
            {
                var state = Mod.RacerState[racerName];
                var racer = Game1.CurrentEvent.getCharacterByName(racerName);

                for (int i = Mod.Obstacles.Count - 1; i >= 0; --i)
                {
                    var obstacle = Mod.Obstacles[i];
                    if (obstacle.GetBoundingBox().Intersects(racer.GetBoundingBox()))
                    {
                        switch (obstacle.Type)
                        {
                            case ObstacleType.Item:
                                if (!state.CurrentItem.HasValue && state.ItemObtainTimer == -1 && state.ItemUsageTimer == -1)
                                {
                                    state.ItemObtainTimer = 120;
                                }
                                else continue;
                                break;
                            case ObstacleType.Net:
                                if (!(state.CurrentItem == SurfItem.Invincibility && state.ItemUsageTimer >= 0))
                                {
                                    state.StunTimer = 90;
                                }
                                break;
                            case ObstacleType.Rock:
                                if (!(state.CurrentItem == SurfItem.Invincibility && state.ItemUsageTimer >= 0))
                                {
                                    if (state.SlowdownTimer == -1)
                                        state.Speed /= 2;
                                    state.SlowdownTimer = 150;
                                }
                                // spawn particles
                                break;
                            case ObstacleType.FirstPlaceProjectile:
                            case ObstacleType.HomingProjectile:
                                if (racerName != obstacle.HomingTarget)
                                    continue;
                                if (!(state.CurrentItem == SurfItem.Invincibility && state.ItemUsageTimer >= 0))
                                {
                                    if (state.SlowdownTimer == -1)
                                        state.Speed /= 2;
                                    state.SlowdownTimer = obstacle.Type == ObstacleType.HomingProjectile ? 90 : 180;
                                }
                                if (obstacle.Type == ObstacleType.FirstPlaceProjectile)
                                    Game1.playSound("thunder");
                                if (obstacle.Type == ObstacleType.HomingProjectile)
                                    Game1.CurrentEvent.underwaterSprites.Remove(obstacle.UnderwaterSprite);
                                break;
                        }
                        Mod.Obstacles.Remove(obstacle);
                    }
                }

                if (state.ItemObtainTimer >= 0)
                {
                    --state.ItemObtainTimer;
                    if (state.ItemObtainTimer != 0 && state.ItemObtainTimer % 5 == 0)
                    {
                        if (racer == Game1.player)
                            Game1.playSound("shiny4");
                    }
                    else if (state.ItemObtainTimer == -1)
                    {
                        while (true)
                        {
                            state.CurrentItem = (SurfItem)Game1.recentMultiplayerRandom.Next(Enum.GetValues(typeof(SurfItem)).Length);
                            if (Mod.GetRacePlacement()[Mod.GetRacePlacement().Count - 1] == racerName && state.CurrentItem == SurfItem.FirstPlaceProjectile)
                            { }
                            else break;
                        }
                    }
                }
                if (state.ItemUsageTimer >= 0)
                {
                    if (--state.ItemUsageTimer < 0)
                    {
                        if (state.CurrentItem.Value == SurfItem.Boost)
                        {
                            state.Speed /= 2;
                            racer.stopGlowing();
                        }
                        else if (state.CurrentItem.Value == SurfItem.Invincibility)
                        {
                            state.AddedSpeed -= 3;
                            racer.stopGlowing();
                        }
                        state.CurrentItem = null;
                    }
                    else
                    {
                        if (state.CurrentItem == SurfItem.Invincibility)
                            racer.glowingColor = Mod.MyGetPrismaticColor();
                    }
                }
                if (state.SlowdownTimer >= 0)
                {
                    if (--state.SlowdownTimer < 0)
                    {
                        state.Speed *= 2;
                    }
                }
                if (state.StunTimer >= 0)
                {
                    --state.StunTimer;
                    if (racer == Game1.player)
                    {
                        Game1.player.controller = new PathFindController(Game1.player, Game1.currentLocation, new Point((int)Game1.player.getTileLocation().X, (int)Game1.player.getTileLocation().Y), Game1.player.FacingDirection)
                        {
                            pathToEndPoint = null
                        };
                        Game1.player.Halt();
                    }
                    continue;
                }

                if (racer is Farmer)
                {
                    if (racer == Game1.player)
                    {
                        int opposite = state.Facing switch
                        {
                            Game1.up => Game1.down,
                            Game1.down => Game1.up,
                            Game1.left => Game1.right,
                            Game1.right => Game1.left,
                            _ => 0
                        };

                        if (Game1.player.FacingDirection != state.Facing && Game1.player.FacingDirection != opposite)
                        {
                            racer.faceDirection(Game1.player.FacingDirection);

                            int wasSpeed = racer.speed;
                            racer.speed = (state.Speed + state.AddedSpeed) / 2;
                            racer.tryToMoveInDirection(racer.FacingDirection, racer is Farmer, 0, false);
                            racer.speed = wasSpeed;
                        }

                        Game1.player.controller = new PathFindController(Game1.player, Game1.currentLocation, new Point((int)Game1.player.getTileLocation().X, (int)Game1.player.getTileLocation().Y), Game1.player.FacingDirection)
                        {
                            pathToEndPoint = null
                        };
                        Game1.player.Halt();
                    }
                }
                else if (racer is NPC npc)
                {
                    npc.CurrentDialogue.Clear();

                    int checkDirX = 0, checkDirY = 0;
                    int inDir = 0, outDir = 0;
                    switch (state.Facing)
                    {
                        case Game1.up: checkDirY = -1; inDir = Game1.right; outDir = Game1.left; break;
                        case Game1.down: checkDirY = 1; inDir = Game1.left; outDir = Game1.right; break;
                        case Game1.left: checkDirX = -1; inDir = Game1.up; outDir = Game1.down; break;
                        case Game1.right: checkDirX = 1; inDir = Game1.down; outDir = Game1.up; break;
                    }

                    bool foundObstacle = false;
                    for (int i = 0; i < 7; ++i)
                    {
                        var bb = racer.GetBoundingBox();
                        bb.X += checkDirX * Game1.tileSize;
                        bb.Y += checkDirY * Game1.tileSize;

                        foreach (var obstacle in Mod.Obstacles)
                        {
                            if ((obstacle.Type is ObstacleType.Net or ObstacleType.Rock) &&
                                 obstacle.GetBoundingBox().Intersects(bb))
                            {
                                foundObstacle = true;
                                break;
                            }
                        }

                        if (foundObstacle)
                            break;
                    }

                    var r = new Random(((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed) ^ racerName.GetHashCode() + (int)racer.getTileLocation().X / 15);
                    int facingDir = -1;
                    if (foundObstacle)
                        facingDir = (r.Next(2) == 0) ? inDir : outDir;
                    else
                    {
                        switch (r.Next(3))
                        {
                            case 0: facingDir = inDir; break;
                            case 1: break;
                            case 2: facingDir = outDir; break;
                        }
                    }

                    facingDir = state.Facing switch
                    {
                        // Fix some times they get stuck on the inner wall
                        Game1.up when racer.Position.X >= 16 * Game1.tileSize + 1 => Game1.left,
                        Game1.down when racer.Position.X <= 133 * Game1.tileSize => Game1.right,
                        Game1.left when racer.Position.Y <= 60 * Game1.tileSize => Game1.down,
                        Game1.right when racer.Position.Y >= 58 * Game1.tileSize + 1 => Game1.up,
                        _ => facingDir
                    };

                    if (facingDir != -1)
                    {
                        racer.faceDirection(facingDir);

                        int oldSpeed = racer.speed;
                        racer.speed = (state.Speed + state.AddedSpeed) / 2;
                        racer.tryToMoveInDirection(racer.FacingDirection, racer is Farmer, 0, false);
                        racer.speed = oldSpeed;
                    }

                    if (state.CurrentItem.HasValue && state.ItemObtainTimer == -1 && state.ItemUsageTimer == -1)
                    {
                        state.ShouldUseItem = true;
                    }
                }

                if (state.ShouldUseItem)
                {
                    state.ShouldUseItem = false;
                    if (racer == Game1.player)
                    {
                        var msg = new UseItemMessage { ItemUsed = state.CurrentItem.Value };
                        this.Helper.Multiplayer.SendMessage(msg, UseItemMessage.Type, new[] { this.ModManifest.UniqueID });
                    }
                    switch (state.CurrentItem.Value)
                    {
                        case SurfItem.Boost:
                            state.Speed *= 2;
                            state.ItemUsageTimer = 80;
                            racer.startGlowing(Color.DarkViolet, false, 0.05f);
                            Game1.playSound("wand");
                            break;
                        case SurfItem.HomingProjectile:
                            string target = null;
                            bool next = false;
                            foreach (string other in Mod.GetRacePlacement())
                            {
                                if (other == racerName)
                                    next = true;
                                else if (next)
                                {
                                    target = other;
                                    break;
                                }
                            }
                            target ??= Mod.GetRacePlacement()[Mod.GetRacePlacement().Count - 2];

                            state.CurrentItem = null;
                            TemporaryAnimatedSprite tas = new TemporaryAnimatedSprite(128, 0, 0, 0, new Vector2(), false, false);
                            Mod.Obstacles.Add(new Obstacle
                            {
                                Type = ObstacleType.HomingProjectile,
                                Position = new Vector2(racer.GetBoundingBox().Center.X, racer.GetBoundingBox().Center.Y),
                                HomingTarget = target,
                                UnderwaterSprite = tas
                            });
                            Game1.CurrentEvent.underwaterSprites ??= new List<TemporaryAnimatedSprite>();
                            Game1.CurrentEvent.underwaterSprites.Add(tas);
                            Game1.playSound("throwDownITem");
                            break;
                        case SurfItem.FirstPlaceProjectile:
                            state.CurrentItem = null;
                            Mod.Obstacles.Add(new Obstacle
                            {
                                Type = ObstacleType.FirstPlaceProjectile,
                                Position = new Vector2(racer.GetBoundingBox().Center.X, racer.GetBoundingBox().Center.Y),
                                HomingTarget = Mod.GetRacePlacement()[Mod.GetRacePlacement().Count - 1]
                            });
                            Game1.playSound("fishEscape");
                            break;
                        case SurfItem.Invincibility:
                            state.ItemUsageTimer = 150;
                            if (state.SlowdownTimer > 0)
                                state.SlowdownTimer = 0;
                            if (state.StunTimer > 0)
                                state.StunTimer = 0;
                            racer.startGlowing(Mod.MyGetPrismaticColor(), false, 0);
                            racer.glowingTransparency = 1;
                            state.AddedSpeed += 3;
                            Game1.playSound("yoba");
                            break;
                    }
                }

                // Fix some times they get stuck on the inner wall
                int go = state.Facing switch
                {
                    Game1.up when racer.Position.X >= 16 * Game1.tileSize + 1 => Game1.left,
                    Game1.down when racer.Position.X <= 133 * Game1.tileSize => Game1.right,
                    Game1.left when racer.Position.Y <= 60 * Game1.tileSize => Game1.down,
                    Game1.right when racer.Position.Y >= 58 * Game1.tileSize + 1 => Game1.up,
                    _ => -1
                };

                if (go != -1)
                {
                    racer.faceDirection(go);

                    int oldSpeed = racer.speed;
                    racer.speed = (state.Speed + state.AddedSpeed) / 2;
                    racer.tryToMoveInDirection(racer.FacingDirection, racer is Farmer, 0, false);
                    racer.speed = oldSpeed;
                }

                racer.faceDirection(state.Facing);

                {
                    int wasSpeed = racer.speed;
                    racer.speed = state.Speed + state.AddedSpeed;
                    racer.tryToMoveInDirection(racer.FacingDirection, racer is Farmer, 0, false);
                    racer.speed = wasSpeed;
                }

                for (int i = 0; i < switchDirs.Length; ++i)
                {
                    var switchDir = switchDirs[i];
                    foreach (var tile in switchDir)
                    {
                        if (racer.GetBoundingBox().Intersects(new Rectangle((int)tile.X * Game1.tileSize, (int)tile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize)))
                        {
                            racer.faceDirection(i);
                            state.Facing = i;
                        }
                    }
                }

                if (racer.getTileLocation().X >= 132 && racer.Position.Y >= 59 * Game1.tileSize)
                {
                    state.ReachedHalf = true;
                }
                if (state.ReachedHalf && racer.getTileLocation().X >= 17 && racer.Position.Y <= 59 * Game1.tileSize - 1)
                {
                    ++state.LapsDone;
                    state.ReachedHalf = false;

                    if (state.LapsDone >= 2 && Mod.RaceWinner == null)
                    {
                        Mod.RaceWinner = racerName;

                        Game1.CurrentEvent.playerControlSequence = false;
                        Game1.CurrentEvent.playerControlSequenceID = null;
                        var festData = Mod.Instance.Helper.Reflection.GetField<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData").GetValue();

                        if (!festData.TryGetValue($"{Mod.RaceWinner}Win", out string winDialog))
                            winDialog = null;
                        winDialog ??= I18n.Race_Winner_Player(name: racer.Name);

                        Game1.CurrentEvent.eventCommands = festData["afterSurfingRace"].Replace("{{winDialog}}", winDialog).Split('/');
                        Game1.CurrentEvent.currentCommand = 0;

                        foreach (string curRacerName in Mod.Racers)
                            Game1.CurrentEvent.getCharacterByName(curRacerName).stopGlowing();
                    }
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.CurrentEvent?.FestivalName != Mod.FestivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace")
                return;

            var state = Mod.RacerState["farmer" + Utility.getFarmerNumberFromFarmer(Game1.player)];
            if (e.Button.IsActionButton())
            {
                if (state.CurrentItem.HasValue && state.ItemObtainTimer == -1 && state.ItemUsageTimer == -1)
                {
                    state.ShouldUseItem = true;
                }
            }
        }

        private int ItemBobbleFrame;
        private int ItemBobbleTimer;
        private uint NetBobTimer;
        public void DrawObstacles(SpriteBatch b)
        {
            if (Game1.CurrentEvent?.FestivalName != Mod.FestivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace")
                return;

            foreach (var obstacle in Mod.Obstacles)
            {
                Texture2D srcTex = null;
                Rectangle srcRect = new Rectangle();
                Vector2 origin = new Vector2();
                Vector2 offset = new Vector2();
                switch (obstacle.Type)
                {
                    case ObstacleType.Item:
                        srcTex = Mod.ObstaclesTex;
                        srcRect = new Rectangle(48 + 16 * this.ItemBobbleFrame, 0, 16, 16);
                        break;
                    case ObstacleType.Net:
                        srcTex = Mod.ObstaclesTex;
                        srcRect = new Rectangle(0, 48, 48, 32);
                        origin = new Vector2(0, 16);
                        offset = new Vector2(0, (float)Math.Sin(this.NetBobTimer / 10) * 3);
                        break;
                    case ObstacleType.Rock:
                        srcTex = Mod.ObstaclesTex;
                        srcRect = new Rectangle(0, 0, 48, 48);
                        origin = new Vector2(0, 32);
                        break;
                    case ObstacleType.HomingProjectile:
                        // These are rendered differently, underneath the water
                        obstacle.UnderwaterSprite.Position = new Vector2(obstacle.GetBoundingBox().Center.X, obstacle.GetBoundingBox().Center.Y);
                        /*
                        srcTex = Game1.objectSpriteSheet;
                        srcRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 128, 16, 16);
                        origin = new Vector2(8, 8);
                        */
                        break;
                    case ObstacleType.FirstPlaceProjectile:
                        srcTex = Game1.mouseCursors;
                        srcRect = new Rectangle(643, 1043, 61, 92);
                        origin = new Vector2(662 - 643, 1134 - 1043);

                        var target = Game1.CurrentEvent.getCharacterByName(obstacle.HomingTarget);
                        if (Vector2.Distance(new Vector2(obstacle.GetBoundingBox().Center.X, obstacle.GetBoundingBox().Center.Y),
                                               new Vector2(target.GetBoundingBox().Center.X, target.GetBoundingBox().Center.Y))
                             >= Game1.tileSize * 2)
                            srcRect.Height = 35;
                        break;

                }
                float depth = (obstacle.Position.Y + srcRect.Height - origin.Y) / 10000f;

                if (srcTex == null)
                    continue;

                b.Draw(srcTex, Game1.GlobalToLocal(obstacle.Position + offset), srcRect, Color.White, 0, origin, Game1.pixelZoom, SpriteEffects.None, depth);
                //e.SpriteBatch.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, obstacle.GetBoundingBox()), Color.Red);
            }
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.CurrentEvent?.FestivalName != Mod.FestivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace")
                return;

            var b = e.SpriteBatch;
            var state = Mod.RacerState["farmer" + Utility.getFarmerNumberFromFarmer(Game1.player)];

            var pos = new Vector2(Game1.viewport.Width - (74 + 14) * 2 - 25, 25);
            b.Draw(Game1.mouseCursors, pos, new Rectangle(603, 414, 74, 74), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            b.Draw(Game1.mouseCursors, new Vector2(pos.X - 14 * 2, pos.Y + 74 * 2), new Rectangle(589, 488, 102, 18), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            if (state.CurrentItem.HasValue || state.ItemObtainTimer >= 0)
            {
                int displayItem = state.ItemObtainTimer / 5 % Enum.GetValues(typeof(SurfItem)).Length;
                if (state.CurrentItem.HasValue)
                    displayItem = (int)state.CurrentItem.Value;

                Texture2D displayTex = null;
                Rectangle displayRect = new Rectangle();
                Color displayColor = Color.White;
                string displayName = null;
                switch (displayItem)
                {
                    case (int)SurfItem.Boost:
                        displayTex = Game1.objectSpriteSheet;
                        displayRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 434, 16, 16);
                        displayName = I18n.Item_Boost();
                        break;

                    case (int)SurfItem.HomingProjectile:
                        displayTex = Game1.objectSpriteSheet;
                        displayRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 128, 16, 16);
                        displayName = I18n.Item_HomingProjectile();
                        break;

                    case (int)SurfItem.FirstPlaceProjectile:
                        displayTex = Game1.mouseCursors;
                        displayRect = new Rectangle(643, 1043, 61, 61);
                        displayName = I18n.Item_FirstPlaceProjectile();
                        break;

                    case (int)SurfItem.Invincibility:
                        displayTex = Game1.content.Load<Texture2D>("Characters\\Junimo"); // TODO: Cache this
                        displayRect = new Rectangle(80, 80, 16, 16);
                        displayColor = Mod.MyGetPrismaticColor();
                        displayName = I18n.Item_Invincibility();
                        break;
                }

                b.Draw(displayTex, new Rectangle((int)pos.X + 42, (int)pos.Y + 42, 64, 64), displayRect, displayColor);
                b.DrawString(Game1.smallFont, displayName, new Vector2((int)pos.X + 74, (int)pos.Y + 74 * 2 + 6), Game1.textColor, 0, new Vector2(Game1.smallFont.MeasureString(displayName).X / 2, 0), 0.85f, SpriteEffects.None, 0.88f);
            }

            string lapsStr = I18n.Ui_Laps(laps: state.LapsDone);
            SpriteText.drawStringHorizontallyCenteredAt(b, lapsStr, (int)pos.X + 74, (int)pos.Y + 74 * 2 + 18 * 2 + 8);

            string str = I18n.Ui_Ranking();
            SpriteText.drawStringHorizontallyCenteredAt(b, str, (int)pos.X + 74, Game1.viewport.Height - 128 - (Mod.Racers.Count - 1) / 5 * 40);

            int i = 0;
            var sortedRacers = Mod.GetRacePlacement();
            sortedRacers.Reverse();
            foreach (string racerName in sortedRacers)
            {
                var racer = Game1.CurrentEvent.getCharacterByName(racerName);
                int x = (int)pos.X + 74 - SpriteText.getWidthOfString(str) / 2 + i % 5 * 40 - 20;
                int y = Game1.viewport.Height - 64 + i / 5 * 50;

                if (racer is NPC)
                {
                    var rect = new Rectangle(0, 3, 16, 16);
                    b.Draw(racer.Sprite.Texture, new Vector2(x, y), rect, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1);
                }
                else if (racer is Farmer farmer)
                {
                    farmer.FarmerRenderer.drawMiniPortrat(b, new Vector2(x, y), 0, 2, 0, farmer);
                }
                ++i;
            }
        }

        private void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != this.ModManifest.UniqueID)
                return;
            switch (e.Type)
            {
                case UseItemMessage.Type:
                    {
                        var msg = e.ReadAs<UseItemMessage>();
                        string racerName = "farmer" + Utility.getFarmerNumberFromFarmer(Game1.getFarmer(e.FromPlayerID));
                        if (!Mod.Racers.Contains(racerName))
                            return;
                        var state = Mod.RacerState[racerName];

                        state.CurrentItem = msg.ItemUsed;
                        state.ShouldUseItem = true;
                    }
                    break;
            }
        }

        private void OnActionActivated(object sender, EventArgsAction e)
        {
            void PlaceBonfire(Map map, int x, int y, bool purple)
            {
                int bw = 48 / 16, bh = 80 / 16;
                TileSheet ts = map.GetTileSheet("surfing");
                int baseY = (purple ? 272 : 112) / 16 * ts.SheetWidth;
                Layer buildings = map.GetLayer("Buildings");
                Layer front = map.GetLayer("Front");
                for (int ix = 0; ix < bw; ++ix)
                {
                    for (int iy = 0; iy < bh; ++iy)
                    {
                        var layer = iy < bh - 2 ? front : buildings;

                        var frames = new List<StaticTile>();
                        for (int i = 0; i < 8; ++i)
                        {
                            int toThisTile = ix + iy * ts.SheetWidth;
                            int toThisFrame = (i % 4) * 3 + (i / 4) * (ts.SheetWidth * bh);
                            frames.Add(new StaticTile(layer, ts, BlendMode.Alpha, baseY + toThisTile + toThisFrame));
                        }

                        layer.Tiles[x + ix, y + iy] = new AnimatedTile(layer, frames.ToArray(), 75);
                        if (layer == buildings)
                            layer.Tiles[x + ix, y + iy].Properties.Add("Action", "SurfingBonfire");
                    }
                }
            }

            if (e.Action == "SurfingBonfire" && Mod.PlayerDidBonfire == BonfireState.NotDone)
            {
                bool Highlight(StardewValley.Item item) => (item is SObject obj && !obj.bigCraftable.Value && ((obj.ParentSheetIndex == 388 && obj.Stack >= 50) || obj.ParentSheetIndex is 71 or 789));
                void BehaviorOnSelect(StardewValley.Item item, Farmer farmer)
                {
                    if (item == null)
                        return;

                    if (item.ParentSheetIndex == 388 && item.Stack >= 50)
                    {
                        item.Stack -= 50;
                        if (item.Stack == 0)
                            farmer.removeItemFromInventory(item);
                        foreach (NPC character in Game1.CurrentEvent.actors)
                        {
                            if (character != null)
                                farmer.changeFriendship(50, character);
                        }

                        Mod.PlayerDidBonfire = BonfireState.Normal;
                        Game1.drawObjectDialogue(I18n.Dialog_Wood());
                        Game1.playSound("fireball");
                        PlaceBonfire(Game1.currentLocation.Map, 30, 5, false);
                    }
                    else if (item.ParentSheetIndex is 71 or 789)
                    {
                        farmer.removeItemFromInventory(item);
                        Mod.PlayerDidBonfire = BonfireState.Shorts;

                        Game1.drawDialogue(Game1.getCharacterFromName("Lewis"), I18n.Dialog_Shorts());
                        Game1.playSound("fireball");
                        PlaceBonfire(Game1.currentLocation.Map, 30, 5, true);
                    }
                }

                var menu = new ItemGrabMenu(null, true, false, Highlight, BehaviorOnSelect, I18n.Ui_Wood(), BehaviorOnSelect);
                Game1.activeClickableMenu = menu;

                e.Cancel = true;
            }
            else if (e.Action == "SurfingFestival.SecretOffering" && sender == Game1.player && !Game1.player.hasOrWillReceiveMail("SurfingFestivalOffering"))
            {
                var answers = new Response[]
                {
                    new("MakeOffering", I18n.Secret_Yes()),
                    new("Leave", I18n.Secret_No())
                };

                void AfterQuestion(Farmer who, string choice)
                {
                    if (choice == "MakeOffering")
                    {
                        if (Game1.player.Money >= 100000)
                        {
                            Game1.player.mailReceived.Add("SurfingFestivalOffering");
                            Game1.drawObjectDialogue(I18n.Secret_Purchased());
                        }
                        else
                        {
                            Game1.drawObjectDialogue(I18n.Secret_Broke());
                        }
                    }
                }

                Game1.currentLocation.createQuestionDialogue(Game1.parseText(I18n.Secret_Text()), answers, AfterQuestion);

                e.Cancel = true;
            }
        }

        private static int SurfboardWaterAnim;
        private static int SurfboardWaterAnimTimer;
        private static int PrevRacerFrame = -1;
        public static void DrawSurfboard(Character instance, SpriteBatch b)
        {
            var npc = instance as NPC;
            var farmer = instance as Farmer;

            if ((npc != null && !Mod.Racers.Contains(instance.Name)) || (farmer != null && !Mod.Racers.Contains($"farmer{Utility.getFarmerNumberFromFarmer(farmer)}")))
                return;

            bool player = instance is Farmer;
            int ox = 0, oy = 0;

            var state = Mod.RacerState[instance is NPC ? instance.Name : ("farmer" + Utility.getFarmerNumberFromFarmer(instance as Farmer))];
            var rect = new Rectangle(state.Surfboard % 2 * 32, state.Surfboard / 2 * 16, 32, 16);
            var rect2 = new Rectangle(Mod.SurfboardWaterAnim * 64, 0, 64, 48);
            var origin = new Vector2(16, 8);
            var origin2 = new Vector2(32, 24);
            switch (state.Facing)
            {
                case Game1.up:
                    ox = 8;
                    b.Draw(Mod.SurfboardTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect, Color.White, 90 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0002f);
                    b.Draw(Mod.SurfboardWaterTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect2, Color.White, -90 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0001f);
                    break;
                case Game1.down:
                    ox = player ? -8 : -4;
                    b.Draw(Mod.SurfboardTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect, Color.White, -90 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0002f);
                    b.Draw(Mod.SurfboardWaterTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect2, Color.White, 90 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0001f);
                    break;
                case Game1.left:
                    oy = player ? 0 : 8;
                    b.Draw(Mod.SurfboardTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect, Color.White, 180 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0002f);
                    b.Draw(Mod.SurfboardWaterTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect2, Color.White, 180 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0001f);
                    break;
                case Game1.right:
                    oy = player ? -8 : 0;
                    b.Draw(Mod.SurfboardTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect, Color.White, 0 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0002f);
                    b.Draw(Mod.SurfboardWaterTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + 8 * Game1.pixelZoom + ox, instance.Position.Y + 8 * Game1.pixelZoom + oy)), rect2, Color.White, 0 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f - 0.0001f);
                    break;
            }

            if (state.StunTimer >= 0)
            {
                if (npc != null)
                {
                    var shockedFrames = new Dictionary<string, int>
                    {
                        ["Shane"] = 18,
                        ["Harvey"] = 30,
                        ["Maru"] = 27,
                        ["Emily"] = 26
                    };

                    Mod.PrevRacerFrame = npc.Sprite.CurrentFrame;
                    if (shockedFrames.TryGetValue(instance.Name, out int frame))
                        npc.Sprite.CurrentFrame = frame;
                }
                else if (farmer != null)
                {
                    Mod.PrevRacerFrame = farmer.FarmerSprite.CurrentFrame;
                    farmer.FarmerSprite.setCurrentSingleFrame(94, 1);
                }
            }
        }

        public static void DrawSurfingStatuses(Character instance, SpriteBatch b)
        {
            NPC npc = instance as NPC;
            Farmer farmer = instance as Farmer;

            if ((npc != null && !Mod.Racers.Contains(instance.Name)) || (farmer != null && !Mod.Racers.Contains("farmer" + Utility.getFarmerNumberFromFarmer(farmer))))
                return;

            var state = Mod.RacerState[instance is NPC ? instance.Name : ("farmer" + Utility.getFarmerNumberFromFarmer(instance as Farmer))];
            if (state.StunTimer >= 0)
            {
                int ox = 0, oy = 0;
                if (farmer != null)
                {
                    oy = -6 * Game1.pixelZoom;
                }
                b.Draw(Mod.StunTex, Game1.GlobalToLocal(new Vector2(instance.Position.X + ox, instance.Position.Y - 17 * Game1.pixelZoom + oy)), null, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, instance.GetBoundingBox().Center.Y / 10000f + 0.0003f);

                if (npc != null)
                {
                    npc.Sprite.CurrentFrame = Mod.PrevRacerFrame;
                    Mod.PrevRacerFrame = -1;
                }
                else if (farmer != null)
                {
                    //(__instance as Farmer).FarmerSprite.CurrentFrame = prevRacerFrame;
                    Mod.PrevRacerFrame = -1;
                }
            }
        }

        public static void EventCommand_WarpSurfingRacers(Event instance, GameLocation location, GameTime time, string[] split)
        {
            // Generate obstacles
            Mod.Obstacles.Clear();
            Point obstaclesStart = new Point(6, 48);
            Point obstaclesEnd = new Point(143, 70);
            var obstaclesLayer = Game1.currentLocation.Map.GetLayer("RaceObstacles");
            for (int ix = obstaclesStart.X; ix <= obstaclesEnd.X; ++ix)
            {
                for (int iy = obstaclesStart.Y; iy <= obstaclesEnd.Y; ++iy)
                {
                    var tile = obstaclesLayer.Tiles[ix, iy];
                    if (tile?.TileIndex == 3)
                        Mod.Obstacles.Add(new Obstacle
                        {
                            Type = ObstacleType.Item,
                            Position = new Vector2(ix * Game1.tileSize, iy * Game1.tileSize)
                        });
                    else if (tile?.TileIndex == 64)
                        Mod.Obstacles.Add(new Obstacle
                        {
                            Type = ObstacleType.Net,
                            Position = new Vector2(ix * Game1.tileSize, iy * Game1.tileSize)
                        });
                    else if (tile?.TileIndex == 32)
                        Mod.Obstacles.Add(new Obstacle
                        {
                            Type = ObstacleType.Rock,
                            Position = new Vector2(ix * Game1.tileSize, iy * Game1.tileSize)
                        });
                }
            }

            // Add racers
            Mod.Racers = new List<string>
            {
                "Shane",
                "Harvey",
                "Maru",
                "Emily"
            };
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                Mod.Racers.Add("farmer" + Utility.getFarmerNumberFromFarmer(farmer));
                farmer.CanMove = false;
            }

            // Shuffle them
            var r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
            for (int i = 0; i < Mod.Racers.Count; ++i)
            {
                int ni = r.Next(Mod.Racers.Count);
                string old = Mod.Racers[ni];
                Mod.Racers[ni] = Mod.Racers[i];
                Mod.Racers[i] = old;
            }

            // Set states and surfboards
            Mod.RacerState.Clear();
            foreach (string racerName in Mod.Racers)
            {
                Mod.RacerState.Add(racerName, new RacerState
                {
                    Surfboard = r.Next(6)
                });

                // NPCs get a buff since they're dumb
                if (!racerName.StartsWith("farmer"))
                    Mod.RacerState[racerName].AddedSpeed += 1;
                // Farmer's do if they paid the secret offering
                else if (Utility.getFarmerFromFarmerNumberString(racerName, Game1.player)?.hasOrWillReceiveMail("SurfingFestivalOffering") ?? false)
                    Mod.RacerState[racerName].AddedSpeed += 2;
            }

            // Move them to their start
            var startPos = new Vector2(18, 57);
            if (Mod.Racers.Count <= 6)
            {
                startPos.X += 1;
                startPos.Y -= 1;
            }
            var actualPos = startPos;
            foreach (string racerName in Mod.Racers)
            {
                var racer = instance.getCharacterByName(racerName);

                racer.position.X = actualPos.X * Game1.tileSize + 4;
                racer.position.Y = actualPos.Y * Game1.tileSize;
                racer.faceDirection(Game1.right);

                actualPos.X += 1;
                actualPos.Y -= 1;

                // If a more than 4 players mod is used, things might go out of bounds.
                if (actualPos.Y < 50)
                    actualPos.Y = 57;
            }

            // Go to next command
            ++instance.CurrentCommand;
            instance.checkForNextCommand(location, time);
        }

        public static void EventCommand_WarpSurfingRacersFinish(Event instance, GameLocation location, GameTime time, string[] split)
        {
            // Move the racers
            var startPos = new Vector2(32, 12);
            if (Mod.Racers.Count <= 6)
                ++startPos.X;
            var actualPos = startPos;
            foreach (string racerName in Mod.Racers)
            {
                var racer = instance.getCharacterByName(racerName);

                racer.position.X = actualPos.X * Game1.tileSize + 4;
                racer.position.Y = actualPos.Y * Game1.tileSize;
                racer.faceDirection(Game1.up);

                actualPos.X += 1;

                // If a more than 4 players mod is used, things might go out of bounds.
                if (actualPos.X > 39)
                {
                    actualPos.X = 32;
                    ++actualPos.Y;
                }
            }

            // Go to next command
            ++instance.CurrentCommand;
            instance.checkForNextCommand(location, time);
        }

        public static void EventCommand_AwardSurfingPrize(Event instance, GameLocation location, GameTime time, string[] split)
        {
            if (Mod.RaceWinner == "farmer" + Utility.getFarmerNumberFromFarmer(Game1.player))
            {
                if (!Game1.player.mailReceived.Contains("SurfingFestivalWinner"))
                {
                    Game1.player.mailReceived.Add("SurfingFestivalWinner");
                    Game1.player.addItemByMenuIfNecessary(new SObject(Vector2.Zero, Mod.Ja.GetBigCraftableId("Surfing Trophy")));
                }

                Game1.playSound("money");
                Game1.player.Money += 1500;
                Game1.drawObjectDialogue(I18n.Dialog_PrizeMoney());
            }

            instance.CurrentCommand++;
            if (Game1.activeClickableMenu == null)
                ++instance.CurrentCommand;
        }

        public static List<string> GetRacePlacement()
        {
            List<string> ret = new List<string>(Mod.Racers);
            var cmp = new RacerPlacementComparer();
            ret.Sort(cmp);

            return ret;
        }

        public static Color MyGetPrismaticColor(int offset = 0)
        {
            float interval = 250f;
            int currentIndex = ((int)((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / interval) + offset) % Utility.PRISMATIC_COLORS.Length;
            int nextIndex = (currentIndex + 1) % Utility.PRISMATIC_COLORS.Length;
            float position = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / interval % 1f;
            Color prismaticColor = default(Color);
            prismaticColor.R = (byte)(Utility.Lerp(Utility.PRISMATIC_COLORS[currentIndex].R / 255f, Utility.PRISMATIC_COLORS[nextIndex].R / 255f, position) * 255f);
            prismaticColor.G = (byte)(Utility.Lerp(Utility.PRISMATIC_COLORS[currentIndex].G / 255f, Utility.PRISMATIC_COLORS[nextIndex].G / 255f, position) * 255f);
            prismaticColor.B = (byte)(Utility.Lerp(Utility.PRISMATIC_COLORS[currentIndex].B / 255f, Utility.PRISMATIC_COLORS[nextIndex].B / 255f, position) * 255f);
            prismaticColor.A = (byte)(Utility.Lerp(Utility.PRISMATIC_COLORS[currentIndex].A / 255f, Utility.PRISMATIC_COLORS[nextIndex].A / 255f, position) * 255f);
            return prismaticColor;
        }
    }
}
