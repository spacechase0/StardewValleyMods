using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace SurfingFestival
{
    public enum Item
    {
        Boost,
        HomingProjectile,
        FirstPlaceProjectile,
        Invincibility,
    }

    public enum ObstacleType
    {
        Item,
        Net,
        Rock,
        HomingProjectile,
        FirstPlaceProjectile,
    }

    public enum BonfireState
    {
        NotDone,
        Normal,
        Shorts,
    }

    public class Obstacle
    {
        public ObstacleType Type { get; set; }
        public Vector2 Position { get; set; }
        public string HomingTarget { get; set; } = null;

        public TemporaryAnimatedSprite UnderwaterSprite { get; set; } = null;

        public Rectangle GetBoundingBox()
        {
            int w = 48, h = 16;
            int ox = 0, oy = 0;
            if ( Type == ObstacleType.Item || Type == ObstacleType.HomingProjectile || Type == ObstacleType.FirstPlaceProjectile )
                w = 16;
            else if ( Type == ObstacleType.Rock )
            {
                oy = -16 * Game1.pixelZoom;
                h += 16;
            }
            w *= Game1.pixelZoom;
            h *= Game1.pixelZoom;
            return new Rectangle( ( int ) Position.X + ox /*- w / 2*/, ( int ) Position.Y + oy /*- h / 2*/, w, h );
        }
    }
    
    public class RacerState
    {
        public int Speed { get; set; } = Mod.SURF_SPEED;
        public int AddedSpeed { get; set; } = 0;
        public int Surfboard { get; set; } = 0;
        public int Facing { get; set; } = Game1.right;

        public int LapsDone { get; set; } = 0;
        public bool ReachedHalf { get; set; } = false;

        public Item? CurrentItem { get; set; } = null;
        public int ItemObtainTimer { get; set; } = -1;
        public int ItemUsageTimer { get; set; } = -1;
        public int SlowdownTimer { get; set; } = -1;
        public int StunTimer { get; set; } = -1;

        public bool ShouldUseItem { get; set; } = false;
    }

    public class Mod : StardewModdingAPI.Mod, IAssetLoader, IAssetEditor
    {
        public static Mod instance;

        private static JsonAssetsAPI ja;

        public const int SURF_SPEED = 8;

        public static BonfireState playerDidBonfire = BonfireState.NotDone;
        public static List< string > racers = null;
        public static Dictionary< string, RacerState > racerState = new Dictionary<string, RacerState>();
        public static string raceWinner = null;
        public static List<Obstacle> obstacles = new List<Obstacle>();

        public static Texture2D surfboardTex;
        public static Texture2D surfboardWaterTex;
        public static Texture2D stunTex;
        public static Texture2D obstaclesTex;

        public static string festivalName = "Surfing Festival";

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            surfboardTex = helper.Content.Load<Texture2D>( "assets/surfboards.png" );
            surfboardWaterTex = helper.Content.Load<Texture2D>( "assets/surfboard-water.png" );
            stunTex = helper.Content.Load<Texture2D>( "assets/net-stun.png" );
            obstaclesTex = helper.Content.Load<Texture2D>( "assets/obstacles.png" );

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
            helper.Events.Input.ButtonPressed += onButtonPressed;
            //helper.Events.Display.RenderedWorld += onRenderedWorld;
            helper.Events.Display.RenderedHud += onRenderedHud;
            helper.Events.Multiplayer.ModMessageReceived += onMessageReceived;

            SpaceEvents.ActionActivated += onActionActivated;

            var harmony = HarmonyInstance.Create( ModManifest.UniqueID );
            harmony.PatchAll();
        }

        public bool CanLoad<T>( IAssetInfo asset )
        {
            return asset.AssetNameEquals( "Data\\Festivals\\summer5" ) ||
                   asset.AssetNameEquals( "Maps\\Beach-Surfing" ) ||
                   asset.AssetNameEquals( "Maps\\surfing" );
        }

        public T Load<T>( IAssetInfo asset )
        {
            if ( asset.AssetNameEquals( "Data\\Festivals\\summer5" ) )
            {
                var data =  Helper.Content.Load<Dictionary<string, string>>( "assets/festival." + LocalizedContentManager.CurrentLanguageCode + ".json" );
                festivalName = data[ "name" ];
                return ( T ) ( object ) data;
            }
            else if ( asset.AssetNameEquals( "Maps\\Beach-Surfing" ) )
            {
                return ( T ) ( object ) Helper.Content.Load<xTile.Map>( "assets/Beach.tbin" );
            }
            else if ( asset.AssetNameEquals( "Maps\\surfing" ) )
            {
                return ( T ) ( object ) Helper.Content.Load<Texture2D>( "assets/surfing.png" );
            }

            return default( T );
        }

        public bool CanEdit<T>( IAssetInfo asset )
        {
            return asset.AssetNameEquals( "Data\\Festivals\\FestivalDates" );
        }

        public void Edit<T>( IAssetData asset )
        {
            if ( asset.AssetNameEquals( "Data\\Festivals\\FestivalDates" ) )
            {
                asset.AsDictionary<string, string>().Data.Add( "summer5", festivalName );
            }
        }

        private void onGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var spacecore = Helper.ModRegistry.GetApi< SpaceCoreAPI >( "spacechase0.SpaceCore" );
            spacecore.AddEventCommand( "warpSurfingRacers", AccessTools.Method( typeof( Mod ), nameof( Mod.EventCommand_WarpSurfingRacers ) ) );
            spacecore.AddEventCommand( "warpSurfingRacersFinish", AccessTools.Method( typeof( Mod ), nameof( Mod.EventCommand_WarpSurfingRacersFinish ) ) );
            spacecore.AddEventCommand( "awardSurfingPrize", AccessTools.Method( typeof( Mod ), nameof( Mod.EventCommand_AwardSurfingPrize ) ) );

            ja = Helper.ModRegistry.GetApi< JsonAssetsAPI >( "spacechase0.JsonAssets" );
            ja.LoadAssets( Path.Combine( Helper.DirectoryPath, "assets", "ja" ) );
        }

        private Event prevEvent = null;
        private void onUpdateTicked( object sender, UpdateTickedEventArgs e )
        {
            if ( ++surfboardWaterAnimTimer >= 5 )
            {
                surfboardWaterAnimTimer = 0;
                if ( ++surfboardWaterAnim >= 3 )
                    surfboardWaterAnim = 0;
            }
            if ( ++itemBobbleTimer >= 25 )
            {
                itemBobbleTimer = 0;
                if ( ++itemBobbleFrame >= 4 )
                    itemBobbleFrame = 0;
            }
            ++netBobTimer;

            if ( Game1.CurrentEvent?.FestivalName != festivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace" )
            {
                prevEvent = Game1.CurrentEvent;
                return;
            }

            if ( prevEvent == null )
                playerDidBonfire = BonfireState.NotDone;
            prevEvent = Game1.CurrentEvent;

            var rand = new Random();
            foreach ( var actor in Game1.CurrentEvent.actors )
            {
                if ( racers.Contains( actor.Name ) )
                    continue;
                if ( rand.Next( 30 * Game1.CurrentEvent.actors.Count / 2 ) == 0 )
                    actor.jumpWithoutSound();
            }

            foreach ( var obstacle in obstacles )
            {
                if ( obstacle.Type == ObstacleType.HomingProjectile || obstacle.Type == ObstacleType.FirstPlaceProjectile )
                {
                    var target_ = Game1.CurrentEvent.getCharacterByName( obstacle.HomingTarget ).GetBoundingBox().Center;
                    var target = new Vector2( target_.X, target_.Y );
                    var current = obstacle.Position;

                    int speed = 15;
                    if ( obstacle.Type == ObstacleType.FirstPlaceProjectile )
                        speed = 25;

                    if ( Vector2.Distance( target, current ) < speed )
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

            Vector2[][] switchDirs = new Vector2[][]
            {
                new Vector2[]
                {
                    new Vector2( 16, 60 ),
                    new Vector2( 15, 61 ),
                    new Vector2( 14, 62 ),
                    new Vector2( 13, 63 ),
                    new Vector2( 12, 64 ),
                    new Vector2( 11, 65 ),
                    new Vector2( 10, 66 ),
                    new Vector2(  9, 67 ),
                    new Vector2(  8, 68 ),
                    new Vector2(  7, 69 ),
                },
                new Vector2[]
                {
                    new Vector2( 16, 58 ),
                    new Vector2( 15, 57 ),
                    new Vector2( 14, 56 ),
                    new Vector2( 13, 55 ),
                    new Vector2( 12, 54 ),
                    new Vector2( 11, 53 ),
                    new Vector2( 10, 52 ),
                    new Vector2(  9, 51 ),
                    new Vector2(  8, 50 ),
                    new Vector2(  7, 49 ),
                },
                new Vector2[]
                {
                    new Vector2( 133, 58 ),
                    new Vector2( 134, 57 ),
                    new Vector2( 135, 56 ),
                    new Vector2( 136, 55 ),
                    new Vector2( 137, 54 ),
                    new Vector2( 138, 53 ),
                    new Vector2( 139, 52 ),
                    new Vector2( 140, 51 ),
                    new Vector2( 141, 50 ),
                    new Vector2( 142, 49 ),
                },
                new Vector2[]
                {
                    new Vector2( 133, 60 ),
                    new Vector2( 134, 61 ),
                    new Vector2( 135, 62 ),
                    new Vector2( 136, 63 ),
                    new Vector2( 137, 64 ),
                    new Vector2( 138, 65 ),
                    new Vector2( 139, 66 ),
                    new Vector2( 140, 67 ),
                    new Vector2( 141, 68 ),
                    new Vector2( 142, 69 ),
                },
            };

            foreach ( var racerName in racers )
            {
                var state = racerState[ racerName ];
                var racer = Game1.CurrentEvent.getCharacterByName(racerName);
                
                for ( int i = obstacles.Count - 1; i >= 0; --i )
                {
                    var obstacle = obstacles[ i ];
                    if ( obstacle.GetBoundingBox().Intersects( racer.GetBoundingBox() ) )
                    {
                        switch ( obstacle.Type )
                        {
                            case ObstacleType.Item:
                                if ( !state.CurrentItem.HasValue && state.ItemObtainTimer == -1 && state.ItemUsageTimer == -1 )
                                {
                                    state.ItemObtainTimer = 120;
                                }
                                else continue;
                                break;
                            case ObstacleType.Net:
                                if ( !( state.CurrentItem == Item.Invincibility && state.ItemUsageTimer >= 0 ) )
                                {
                                    state.StunTimer = 90;
                                }
                                break;
                            case ObstacleType.Rock:
                                if ( !( state.CurrentItem == Item.Invincibility && state.ItemUsageTimer >= 0 ) )
                                {
                                    if ( state.SlowdownTimer == -1 )
                                        state.Speed /= 2;
                                    state.SlowdownTimer = 150;
                                }
                                // spawn particles
                                break;
                            case ObstacleType.FirstPlaceProjectile:
                            case ObstacleType.HomingProjectile:
                                if ( racerName != obstacle.HomingTarget )
                                    continue;
                                if ( !( state.CurrentItem == Item.Invincibility && state.ItemUsageTimer >= 0 ) )
                                {
                                    if ( state.SlowdownTimer == -1 )
                                        state.Speed /= 2;
                                    state.SlowdownTimer = obstacle.Type == ObstacleType.HomingProjectile ? 90 : 180;
                                }
                                if ( obstacle.Type == ObstacleType.FirstPlaceProjectile )
                                    Game1.playSound( "thunder" );
                                if ( obstacle.Type == ObstacleType.HomingProjectile )
                                    Game1.CurrentEvent.underwaterSprites.Remove( obstacle.UnderwaterSprite );
                                break;
                        }
                        obstacles.Remove( obstacle );
                    }
                }

                if ( state.ItemObtainTimer >= 0 )
                {
                    --state.ItemObtainTimer;
                    if ( state.ItemObtainTimer != 0 && state.ItemObtainTimer % 5 == 0 )
                    {
                        if ( racer == Game1.player )
                            Game1.playSound( "shiny4" );
                    }
                    else if ( state.ItemObtainTimer == -1 )
                    {
                        while ( true )
                        {
                            state.CurrentItem = ( Item ) Game1.recentMultiplayerRandom.Next( Enum.GetValues( typeof( Item ) ).Length );
                            if ( GetRacePlacement()[ GetRacePlacement().Count - 1 ] == racerName && state.CurrentItem == Item.FirstPlaceProjectile )
                            { }
                            else break;
                        }
                    }
                }
                if ( state.ItemUsageTimer >= 0 )
                {
                    if ( --state.ItemUsageTimer < 0 )
                    {
                        if ( state.CurrentItem.Value == Item.Boost )
                        {
                            state.Speed /= 2;
                            racer.stopGlowing();
                        }
                        else if ( state.CurrentItem.Value == Item.Invincibility )
                        {
                            state.AddedSpeed -= 3;
                            racer.stopGlowing();
                        }
                        state.CurrentItem = null;
                    }
                    else
                    {
                        if ( state.CurrentItem == Item.Invincibility )
                            racer.glowingColor = MyGetPrismaticColor();
                    }
                }
                if ( state.SlowdownTimer >= 0 )
                {
                    if ( --state.SlowdownTimer < 0 )
                    {
                        state.Speed *= 2;
                    }
                }
                if ( state.StunTimer >= 0 )
                {
                    --state.StunTimer;
                    if ( racer == Game1.player )
                    {
                        Game1.player.controller = new PathFindController( Game1.player, Game1.currentLocation, new Point( ( int ) Game1.player.getTileLocation().X, ( int ) Game1.player.getTileLocation().Y ), Game1.player.FacingDirection );
                        Game1.player.controller.pathToEndPoint = null;
                        Game1.player.Halt();
                    }
                    continue;
                }
                
                if ( racer is Farmer farmer )
                {
                    if ( racer == Game1.player )
                    {
                        int opposite = 0;
                        switch ( state.Facing )
                        {
                            case Game1.up: opposite = Game1.down; break;
                            case Game1.down: opposite = Game1.up; break;
                            case Game1.left: opposite = Game1.right; break;
                            case Game1.right: opposite = Game1.left; break;
                        }

                        if ( Game1.player.FacingDirection != state.Facing && Game1.player.FacingDirection != opposite )
                        {
                            racer.faceDirection( Game1.player.FacingDirection );

                            int oldSpeed_ = racer.speed;
                            racer.speed = ( state.Speed + state.AddedSpeed ) / 2;
                            racer.tryToMoveInDirection( racer.FacingDirection, racer is Farmer, 0, false );
                            racer.speed = oldSpeed_;
                        }

                        Game1.player.controller = new PathFindController( Game1.player, Game1.currentLocation, new Point( ( int ) Game1.player.getTileLocation().X, ( int ) Game1.player.getTileLocation().Y ), Game1.player.FacingDirection );
                        Game1.player.controller.pathToEndPoint = null;
                        Game1.player.Halt();
                    }
                }
                else if ( racer is NPC npc )
                {
                    npc.CurrentDialogue.Clear();

                    int checkDirX = 0, checkDirY = 0;
                    int inDir = 0, outDir = 0;
                    switch ( state.Facing )
                    {
                        case Game1.up: checkDirY = -1; inDir = Game1.right; outDir = Game1.left; break;
                        case Game1.down: checkDirY = 1; inDir = Game1.left; outDir = Game1.right; break;
                        case Game1.left: checkDirX = -1; inDir = Game1.up; outDir = Game1.down; break;
                        case Game1.right: checkDirX = 1; inDir = Game1.down; outDir = Game1.up; break;
                    }

                    bool foundObstacle = false;
                    for ( int i = 0; i < 7; ++i )
                    {
                        var bb = racer.GetBoundingBox();
                        bb.X += checkDirX * Game1.tileSize;
                        bb.Y += checkDirY * Game1.tileSize;

                        foreach ( var obstacle in obstacles )
                        {
                            if ( ( obstacle.Type == ObstacleType.Net || obstacle.Type == ObstacleType.Rock ) &&
                                 obstacle.GetBoundingBox().Intersects( bb ) )
                            {
                                foundObstacle = true;
                                break;
                            }
                        }

                        if ( foundObstacle )
                            break;
                    }

                    var r = new Random( ( ( int ) Game1.uniqueIDForThisGame + (int) Game1.stats.DaysPlayed ) ^ racerName.GetHashCode() + ( int ) racer.getTileLocation().X / 15 );
                    int go_ = -1;
                    if ( foundObstacle )
                        go_ = (r.Next( 2 ) == 0) ? inDir : outDir;
                    else
                    {
                        switch ( r.Next( 3 ) )
                        {
                            case 0: go_ = inDir; break;
                            case 1: break;
                            case 2: go_ = outDir; break;
                        }
                    }

                    // Fix some times they get stuck on the inner wall
                    if ( state.Facing == Game1.up && racer.Position.X >= 16 * Game1.tileSize + 1 )
                        go_ = Game1.left;
                    if ( state.Facing == Game1.down && racer.Position.X <= 133 * Game1.tileSize )
                        go_ = Game1.right;
                    if ( state.Facing == Game1.left && racer.Position.Y <= 60 * Game1.tileSize )
                        go_ = Game1.down;
                    if ( state.Facing == Game1.right && racer.Position.Y >= 58 * Game1.tileSize + 1 )
                        go_ = Game1.up;

                    if ( go_ != -1 )
                    {
                        racer.faceDirection( go_ );

                        int oldSpeed_ = racer.speed;
                        racer.speed = ( state.Speed + state.AddedSpeed ) / 2;
                        racer.tryToMoveInDirection( racer.FacingDirection, racer is Farmer, 0, false );
                        racer.speed = oldSpeed_;
                    }

                    if ( state.CurrentItem.HasValue && state.ItemObtainTimer == -1 && state.ItemUsageTimer == -1 )
                    {
                        state.ShouldUseItem = true;
                    }
                }

                if ( state.ShouldUseItem )
                {
                    state.ShouldUseItem = false;
                    if ( racer == Game1.player )
                    {
                        var msg = new UseItemMessage() { ItemUsed = state.CurrentItem.Value };
                        Helper.Multiplayer.SendMessage( msg, UseItemMessage.TYPE, new string[] { ModManifest.UniqueID }, null );
                    }
                    switch ( state.CurrentItem.Value )
                    {
                        case Item.Boost:
                            state.Speed *= 2;
                            state.ItemUsageTimer = 80;
                            racer.startGlowing( Color.DarkViolet, false, 0.05f );
                            Game1.playSound( "wand" );
                            break;
                        case Item.HomingProjectile:
                            string target = null;
                            bool next = false;
                            foreach ( var other in GetRacePlacement() )
                            {
                                if ( other == racerName )
                                    next = true;
                                else if ( next )
                                {
                                    target = other;
                                    break;
                                }
                            }
                            if ( target == null )
                                target = GetRacePlacement()[ GetRacePlacement().Count - 2 ];

                            state.CurrentItem = null;
                            TemporaryAnimatedSprite tas = new TemporaryAnimatedSprite(128, 0, 0, 0, new Vector2(), false, false );
                            obstacles.Add( new Obstacle()
                            {
                                Type = ObstacleType.HomingProjectile,
                                Position = new Vector2( racer.GetBoundingBox().Center.X, racer.GetBoundingBox().Center.Y ),
                                HomingTarget = target,
                                UnderwaterSprite = tas,
                            } );
                            if ( Game1.CurrentEvent.underwaterSprites == null )
                                Game1.CurrentEvent.underwaterSprites = new List<TemporaryAnimatedSprite>();
                            Game1.CurrentEvent.underwaterSprites.Add( tas );
                            Game1.playSound( "throwDownITem" );
                            break;
                        case Item.FirstPlaceProjectile:
                            state.CurrentItem = null;
                            obstacles.Add( new Obstacle()
                            {
                                Type = ObstacleType.FirstPlaceProjectile,
                                Position = new Vector2( racer.GetBoundingBox().Center.X, racer.GetBoundingBox().Center.Y ),
                                HomingTarget = GetRacePlacement()[ GetRacePlacement().Count - 1 ],
                            } );
                            Game1.playSound( "fishEscape" );
                            break;
                        case Item.Invincibility:
                            state.ItemUsageTimer = 150;
                            if ( state.SlowdownTimer > 0 )
                                state.SlowdownTimer = 0;
                            if ( state.StunTimer > 0 )
                                state.StunTimer = 0;
                            racer.startGlowing( MyGetPrismaticColor(), false, 0 );
                            racer.glowingTransparency = 1;
                            state.AddedSpeed += 3;
                            Game1.playSound( "yoba" );
                            break;
                    }
                }
                
                // Fix some times they get stuck on the inner wall
                int go = -1;
                if ( state.Facing == Game1.up && racer.Position.X >= 16 * Game1.tileSize + 1 )
                    go = Game1.left;
                if ( state.Facing == Game1.down && racer.Position.X <= 133 * Game1.tileSize )
                    go = Game1.right;
                if ( state.Facing == Game1.left && racer.Position.Y <= 60 * Game1.tileSize )
                    go = Game1.down;
                if ( state.Facing == Game1.right && racer.Position.Y >= 58 * Game1.tileSize + 1 )
                    go = Game1.up;

                if ( go != -1 )
                {
                    racer.faceDirection( go );

                    int oldSpeed_ = racer.speed;
                    racer.speed = ( state.Speed + state.AddedSpeed ) / 2;
                    racer.tryToMoveInDirection( racer.FacingDirection, racer is Farmer, 0, false );
                    racer.speed = oldSpeed_;
                }

                racer.faceDirection( state.Facing );

                int oldSpeed = racer.speed;
                racer.speed = state.Speed + state.AddedSpeed;
                racer.tryToMoveInDirection( racer.FacingDirection, racer is Farmer, 0, false );
                racer.speed = oldSpeed;

                for ( int i = 0; i < switchDirs.Length; ++i )
                {
                    var switchDir = switchDirs[ i ];
                    foreach ( var tile in switchDir )
                    {
                        if ( racer.GetBoundingBox().Intersects( new Rectangle( (int) tile.X * Game1.tileSize, (int) tile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize ) ) )
                        {
                            racer.faceDirection( i );
                            state.Facing = i;
                        }
                    }
                }

                if ( racer.getTileLocation().X >= 132 && racer.Position.Y >= 59 * Game1.tileSize )
                {
                    state.ReachedHalf = true;
                }
                if ( state.ReachedHalf && racer.getTileLocation().X >= 17 && racer.Position.Y <= 59 * Game1.tileSize - 1 )
                {
                    ++state.LapsDone;
                    state.ReachedHalf = false;

                    if ( state.LapsDone >= 2 && raceWinner == null )
                    {
                        raceWinner = racerName;
                        string winnerName = raceWinner;

                        Game1.CurrentEvent.playerControlSequence = false;
                        Game1.CurrentEvent.playerControlSequenceID = null;
                        var festData = Mod.instance.Helper.Reflection.GetField<Dictionary<string, string>>( Game1.CurrentEvent, "festivalData" ).GetValue();
                        string winDialog = festData.ContainsKey( raceWinner + "Win" ) ? festData[ raceWinner + "Win" ] : null;
                        if ( winDialog == null )
                            winDialog = festData[ "FarmerWin" ].Replace( "{{winner}}", racer.Name );
                        Game1.CurrentEvent.eventCommands = festData[ "afterSurfingRace" ].Replace( "{{winDialog}}", winDialog ).Split( '/' );
                        Game1.CurrentEvent.currentCommand = 0;

                        foreach ( var racerName_ in racers )
                        {
                            var racer_ = Game1.CurrentEvent.getCharacterByName( racerName_ );
                            racer_.stopGlowing();
                        }
                    }
                }
            }
        }

        private void onButtonPressed( object sender, ButtonPressedEventArgs e )
        {
            if ( Game1.CurrentEvent?.FestivalName != festivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace" )
                return;

            var state = racerState[ "farmer" + Utility.getFarmerNumberFromFarmer( Game1.player ) ];
            if ( e.Button.IsActionButton() )
            {
                if ( state.CurrentItem.HasValue && state.ItemObtainTimer == -1 && state.ItemUsageTimer == -1 )
                {
                    state.ShouldUseItem = true;
                }
            }
        }

        private int itemBobbleFrame = 0;
        private int itemBobbleTimer = 0;
        private uint netBobTimer = 0;
        public void DrawObstacles( SpriteBatch b )
        {
            if ( Game1.CurrentEvent?.FestivalName != festivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace" )
                return;

            foreach ( var obstacle in obstacles )
            {
                Texture2D srcTex = null;
                Rectangle srcRect = new Rectangle();
                Vector2 origin = new Vector2();
                Vector2 offset = new Vector2();
                switch ( obstacle.Type )
                {
                    case ObstacleType.Item:
                        srcTex = obstaclesTex;
                        srcRect = new Rectangle( 48 + 16 * itemBobbleFrame, 0, 16, 16 );
                        break;
                    case ObstacleType.Net:
                        srcTex = obstaclesTex;
                        srcRect = new Rectangle( 0, 48, 48, 32 );
                        origin = new Vector2( 0, 16 );
                        offset = new Vector2( 0, ( float ) Math.Sin( netBobTimer / 10 ) * 3 );
                        break;
                    case ObstacleType.Rock:
                        srcTex = obstaclesTex;
                        srcRect = new Rectangle( 0, 0, 48, 48 );
                        origin = new Vector2( 0, 32 );
                        break;
                    case ObstacleType.HomingProjectile:
                        // These are rendered differently, underneath the water
                        obstacle.UnderwaterSprite.Position = new Vector2( obstacle.GetBoundingBox().Center.X, obstacle.GetBoundingBox().Center.Y );
                        /*
                        srcTex = Game1.objectSpriteSheet;
                        srcRect = Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, 128, 16, 16 );
                        origin = new Vector2( 8, 8 );
                        */
                        break;
                    case ObstacleType.FirstPlaceProjectile:
                        srcTex = Game1.mouseCursors;
                        srcRect = new Rectangle( 643, 1043, 61, 92 );
                        origin = new Vector2( 662 - 643, 1134 - 1043 );

                        var target = Game1.CurrentEvent.getCharacterByName( obstacle.HomingTarget );
                        if ( Vector2.Distance( new Vector2( obstacle.GetBoundingBox().Center.X, obstacle.GetBoundingBox().Center.Y ),
                                               new Vector2( target.GetBoundingBox().Center.X, target.GetBoundingBox().Center.Y ) )
                             >= Game1.tileSize * 2 )
                            srcRect.Height = 35;
                        break;

                }
                float depth = ( obstacle.Position.Y + srcRect.Height - origin.Y ) / 10000f;

                if ( srcTex == null )
                    continue;

                b.Draw( srcTex, Game1.GlobalToLocal( obstacle.Position + offset ), srcRect, Color.White, 0, origin, Game1.pixelZoom, SpriteEffects.None, depth );
                //e.SpriteBatch.Draw( Game1.staminaRect, Game1.GlobalToLocal( Game1.viewport, obstacle.GetBoundingBox() ), Color.Red );
            }
        }

        private void onRenderedHud( object sender, RenderedHudEventArgs e )
        {
            if ( Game1.CurrentEvent?.FestivalName != festivalName || Game1.CurrentEvent?.playerControlSequenceID != "surfingRace" )
                return;

            var b = e.SpriteBatch;
            var state = racerState[ "farmer" + Utility.getFarmerNumberFromFarmer( Game1.player ) ];

            var pos = new Vector2( Game1.viewport.Width - ( 74 + 14 ) * 2 - 25, 25 );
            b.Draw( Game1.mouseCursors, pos, new Rectangle( 603, 414, 74, 74 ), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0 );
            b.Draw( Game1.mouseCursors, new Vector2( pos.X - 14 * 2, pos.Y + 74 * 2 ), new Rectangle( 589, 488, 102, 18 ), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0 );
            if ( state.CurrentItem.HasValue || state.ItemObtainTimer >= 0 )
            {
                int displayItem = state.ItemObtainTimer / 5 % Enum.GetValues( typeof( Item ) ).Length;
                if ( state.CurrentItem.HasValue )
                    displayItem = ( int ) state.CurrentItem.Value;

                Texture2D displayTex = null;
                Rectangle displayRect = new Rectangle();
                Color displayColor = Color.White;
                string displayName = null;
                switch ( displayItem )
                {
                    case (int) Item.Boost:
                        displayTex = Game1.objectSpriteSheet;
                        displayRect = Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, 434, 16, 16 );
                        displayName = Helper.Translation.Get( "item.boost" );
                        break;

                    case ( int ) Item.HomingProjectile:
                        displayTex = Game1.objectSpriteSheet;
                        displayRect = Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, 128, 16, 16 );
                        displayName = Helper.Translation.Get( "item.homingprojectile" );
                        break;

                    case ( int ) Item.FirstPlaceProjectile:
                        displayTex = Game1.mouseCursors;
                        displayRect = new Rectangle( 643, 1043, 61, 61 );
                        displayName = Helper.Translation.Get( "item.firstplaceprojectile" );
                        break;

                    case ( int ) Item.Invincibility:
                        displayTex = Game1.content.Load<Texture2D>( "Characters\\Junimo" ); // TODO: Cache this
                        displayRect = new Rectangle( 80, 80, 16, 16 );
                        displayColor = MyGetPrismaticColor();
                        displayName = Helper.Translation.Get( "item.invincibility" );
                        break;
                }

                b.Draw( displayTex, new Rectangle( (int) pos.X + 42, (int) pos.Y + 42, 64, 64 ), displayRect, displayColor );
                b.DrawString( Game1.smallFont, displayName, new Vector2( ( int ) pos.X + 74, ( int ) pos.Y + 74 * 2 + 6 ), Game1.textColor, 0, new Vector2( Game1.smallFont.MeasureString( displayName ).X / 2, 0 ), 0.85f, SpriteEffects.None, 0.88f );
            }

            string lapsStr = Helper.Translation.Get( "ui.laps", new { laps = state.LapsDone } );
            SpriteText.drawStringHorizontallyCenteredAt( b, lapsStr, ( int ) pos.X + 74, ( int ) pos.Y + 74 * 2 + 18 * 2 + 8 );

            string str = Helper.Translation.Get( "ui.ranking" );
            SpriteText.drawStringHorizontallyCenteredAt( b, str, ( int ) pos.X + 74, ( int ) Game1.viewport.Height - 128 - ( racers.Count - 1 ) / 5 * 40 );

            int i = 0;
            var sortedRacers = GetRacePlacement();
            sortedRacers.Reverse();
            foreach ( var racerName in sortedRacers )
            {
                var racer = Game1.CurrentEvent.getCharacterByName( racerName );
                int x = (int) pos.X + 74 - SpriteText.getWidthOfString( str ) / 2 + i % 5 * 40 - 20;
                int y = Game1.viewport.Height - 64 + i / 5 * 50;

                if ( racer is NPC npc )
                {
                    var rect = new Rectangle(0, 3, 16, 16);
                    b.Draw( racer.Sprite.Texture, new Vector2( x, y ), rect, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1 );
                }
                else if ( racer is Farmer farmer )
                {
                    farmer.FarmerRenderer.drawMiniPortrat( b, new Vector2( x, y ), 0, 2, 0, farmer );
                }
                ++i;
            }
        }

        private void onMessageReceived( object sender, ModMessageReceivedEventArgs e )
        {
            if ( e.FromModID != ModManifest.UniqueID )
                return;
            switch ( e.Type )
            {
                case UseItemMessage.TYPE:
                    {
                        var msg = e.ReadAs< UseItemMessage >();
                        var racerName = "farmer" + Utility.getFarmerNumberFromFarmer( Game1.getFarmer( e.FromPlayerID ) );
                        if ( !racers.Contains( racerName ) )
                            return;
                        var racer = Game1.CurrentEvent.getCharacterByName( racerName ) as Farmer;
                        var state = racerState[ racerName ];

                        state.CurrentItem = msg.ItemUsed;
                        state.ShouldUseItem = true;
                    }
                    break;
            }
        }

        private void onActionActivated( object sender, EventArgsAction e )
        {
            Action< Map, int, int, bool > placeBonfire = (map, x, y, purple) =>
            {
                int bw = 48 / 16, bh = 80 / 16;
                TileSheet ts = map.GetTileSheet( "surfing" );
                int baseY = (purple ? 272 : 112) / 16 * ts.SheetWidth;
                Layer buildings = map.GetLayer( "Buildings" );
                Layer front = map.GetLayer( "Front" );
                for ( int ix = 0; ix < bw; ++ix )
                {
                    for ( int iy = 0; iy < bh; ++iy )
                    {
                        var layer = iy < bh - 2 ? front : buildings;

                        var frames = new List<StaticTile>();
                        for ( int i = 0; i < 8; ++i )
                        {
                            int toThisTile = ix + iy * ts.SheetWidth;
                            int toThisFrame = ( i % 4 ) * 3 + ( i / 4 ) * ( ts.SheetWidth * bh );
                            frames.Add( new StaticTile( layer, ts, BlendMode.Alpha, baseY + toThisTile + toThisFrame ) );
                        }

                        layer.Tiles[ x + ix, y + iy ] = new AnimatedTile( layer, frames.ToArray(), 75 );
                        if ( layer == buildings )
                            layer.Tiles[ x + ix, y + iy ].Properties.Add( "Action", "SurfingBonfire" );
                    }
                }
            };

            if ( e.Action == "SurfingBonfire" && playerDidBonfire == BonfireState.NotDone )
            {
                InventoryMenu.highlightThisItem highlight = ( item ) => ( item is StardewValley.Object obj && !obj.bigCraftable.Value && ( ( obj.ParentSheetIndex == 388 && obj.Stack >= 50 ) || obj.ParentSheetIndex == 71 || obj.ParentSheetIndex == 789 ) );
                ItemGrabMenu.behaviorOnItemSelect behaviorOnSelect = ( item, farmer ) =>
                {
                    if ( item == null )
                        return;

                    if ( item.ParentSheetIndex == 388 && item.Stack >= 50 )
                    {
                        item.Stack -= 50;
                        if ( item.Stack == 0 )
                            farmer.removeItemFromInventory( item );
                        foreach ( var character in Game1.CurrentEvent.actors )
                        {
                            if ( character is NPC npc )
                                farmer.changeFriendship( 50, npc );
                        }
                        playerDidBonfire = BonfireState.Normal;
                        Game1.drawObjectDialogue( Helper.Translation.Get( "dialog.wood" ) );
                        Game1.playSound( "fireball" );
                        placeBonfire( Game1.currentLocation.Map, 30, 5, false );
                    }
                    else if ( item.ParentSheetIndex == 71 || item.ParentSheetIndex == 789 )
                    {
                        farmer.removeItemFromInventory( item );
                        playerDidBonfire = BonfireState.Shorts;

                        Game1.drawDialogue( Game1.getCharacterFromName( "Lewis" ), Helper.Translation.Get( "dialog.shorts" ) );
                        Game1.playSound( "fireball" );
                        placeBonfire( Game1.currentLocation.Map, 30, 5, true );
                    }
                };

                var menu = new ItemGrabMenu( null, true, false, highlight, behaviorOnSelect, Helper.Translation.Get( "ui.wood" ), behaviorOnSelect );
                Game1.activeClickableMenu = menu;

                e.Cancel = true;
            }
            else if ( e.Action == "SurfingFestival.SecretOffering" && sender == Game1.player && !Game1.player.hasOrWillReceiveMail( "SurfingFestivalOffering" ) )
            {
                var answers = new Response[]
                {
                    new Response( "MakeOffering", Helper.Translation.Get( "secret.yes" ) ),
                    new Response( "Leave", Helper.Translation.Get( "secret.no" ) ),
                };
                GameLocation.afterQuestionBehavior afterQuestion = ( who, choice ) =>
                {
                    if ( choice == "MakeOffering" )
                    {
                        if ( Game1.player.Money >= 100000 )
                        {
                            Game1.player.mailReceived.Add( "SurfingFestivalOffering" );
                            Game1.drawObjectDialogue( Helper.Translation.Get( "secret.purchased" ) );
                        }
                        else
                        {
                            Game1.drawObjectDialogue( Helper.Translation.Get( "secret.broke" ) );
                        }
                    }
                };
                Game1.currentLocation.createQuestionDialogue( Game1.parseText( Helper.Translation.Get( "secret.text" ) ), answers, afterQuestion );

                e.Cancel = true;
            }
        }

        private static int surfboardWaterAnim = 0;
        private static int surfboardWaterAnimTimer = 0;
        private static int prevRacerFrame = -1;
        public static void DrawSurfboard( Character __instance, SpriteBatch b )
        {
            if ( __instance is NPC npc && !racers.Contains( __instance.Name ) ||
                 __instance is Farmer farmer && !racers.Contains( "farmer" + Utility.getFarmerNumberFromFarmer( farmer ) ) )
                return;

            bool player = __instance is Farmer;
            int ox = 0, oy = 0;

            var state = racerState[ __instance is NPC ? __instance.Name : ( "farmer" + Utility.getFarmerNumberFromFarmer( __instance as Farmer ) ) ];
            var rect = new Rectangle( state.Surfboard % 2 * 32, state.Surfboard / 2 * 16, 32, 16 );
            var rect2 = new Rectangle( surfboardWaterAnim * 64, 0, 64, 48 );
            var origin = new Vector2( 16, 8 );
            var origin2 = new Vector2( 32, 24 );
            switch ( state.Facing )
            {
                case Game1.up:
                    ox = player ? 8 : 8;
                    b.Draw( surfboardTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect, Color.White, 90 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0002f );
                    b.Draw( surfboardWaterTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect2, Color.White, -90 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0001f );
                    break;
                case Game1.down:
                    ox = player ? -8 : -4;
                    b.Draw( surfboardTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect, Color.White, -90 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0002f );
                    b.Draw( surfboardWaterTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect2, Color.White, 90 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0001f );
                    break;
                case Game1.left:
                    oy = player ? 0 : 8;
                    b.Draw( surfboardTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect, Color.White, 180 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0002f );
                    b.Draw( surfboardWaterTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect2, Color.White, 180 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0001f );
                    break;
                case Game1.right:
                    oy = player ? -8 : 0;
                    b.Draw( surfboardTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect, Color.White, 0 * 3.14f / 180, origin, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0002f );
                    b.Draw( surfboardWaterTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + 8 * Game1.pixelZoom + ox, __instance.Position.Y + 8 * Game1.pixelZoom + oy ) ), rect2, Color.White, 0 * 3.14f / 180, origin2, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f - 0.0001f );
                    break;
            }

            if ( state.StunTimer >= 0 )
            {
                if ( __instance is NPC )
                {
                    var shockedFrames = new Dictionary<string, int>();
                    shockedFrames.Add( "Shane", 18 );
                    shockedFrames.Add( "Harvey", 30 );
                    shockedFrames.Add( "Maru", 27 );
                    shockedFrames.Add( "Emily", 26 );

                    prevRacerFrame = ( __instance as NPC ).Sprite.CurrentFrame;
                    if ( shockedFrames.ContainsKey( __instance.Name ) )
                    {
                        ( __instance as NPC ).Sprite.CurrentFrame = shockedFrames[ __instance.Name ];
                    }
                }
                else if ( __instance is Farmer )
                {
                    prevRacerFrame = ( __instance as Farmer ).FarmerSprite.CurrentFrame;
                    ( __instance as Farmer ).FarmerSprite.setCurrentSingleFrame( 94, 1 );
                }
            }
        }

        public static void DrawSurfingStatuses( Character __instance, SpriteBatch b )
        {
            if ( __instance is NPC npc && !racers.Contains( __instance.Name ) ||
                 __instance is Farmer farmer && !racers.Contains( "farmer" + Utility.getFarmerNumberFromFarmer( farmer ) ) )
                return;

            var state = racerState[ __instance is NPC ? __instance.Name : ( "farmer" + Utility.getFarmerNumberFromFarmer( __instance as Farmer ) ) ];
            if ( state.StunTimer >= 0 )
            {
                int ox = 0, oy = 0;
                if ( __instance is Farmer )
                {
                    oy = -6 * Game1.pixelZoom;
                }
                b.Draw( stunTex, Game1.GlobalToLocal( new Vector2( __instance.Position.X + ox, __instance.Position.Y - 17 * Game1.pixelZoom + oy ) ), null, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, __instance.GetBoundingBox().Center.Y / 10000f + 0.0003f );

                if ( __instance is NPC )
                {
                    ( __instance as NPC ).Sprite.CurrentFrame = prevRacerFrame;
                    prevRacerFrame = -1;
                }
                else if ( __instance is Farmer )
                {
                    //( __instance as Farmer ).FarmerSprite.CurrentFrame = prevRacerFrame;
                    prevRacerFrame = -1;
                }
            }
        }

        public static void EventCommand_WarpSurfingRacers( Event __instance, GameLocation location, GameTime time, string[] split )
        {
            // Generate obstacles
            obstacles.Clear();
            Point obstaclesStart = new Point( 6, 48 );
            Point obstaclesEnd = new Point( 143, 70 );
            var obstaclesLayer = Game1.currentLocation.Map.GetLayer( "RaceObstacles" );
            for ( int ix = obstaclesStart.X; ix <= obstaclesEnd.X; ++ix )
            {
                for ( int iy = obstaclesStart.Y; iy <= obstaclesEnd.Y; ++iy )
                {
                    var tile = obstaclesLayer.Tiles[ ix, iy ];
                    if ( tile?.TileIndex == 3 )
                        obstacles.Add( new Obstacle()
                        {
                            Type = ObstacleType.Item,
                            Position = new Vector2( ix * Game1.tileSize, iy * Game1.tileSize )
                        } );
                    else if ( tile?.TileIndex == 64 )
                        obstacles.Add( new Obstacle()
                        {
                            Type = ObstacleType.Net,
                            Position = new Vector2( ix * Game1.tileSize, iy * Game1.tileSize )
                        } );
                    else if ( tile?.TileIndex == 32 )
                        obstacles.Add( new Obstacle()
                        {
                            Type = ObstacleType.Rock,
                            Position = new Vector2( ix * Game1.tileSize, iy * Game1.tileSize )
                        } );
                }
            }

            // Add racers
            racers = new List<string>();
            racers.Add( "Shane" );
            racers.Add( "Harvey" );
            racers.Add( "Maru" );
            racers.Add( "Emily" );
            foreach ( var farmer in Game1.getOnlineFarmers() )
            {
                racers.Add( "farmer" + Utility.getFarmerNumberFromFarmer( farmer ) );
                farmer.CanMove = false;
            }

            // Shuffle them
            var r = new Random( (int) Game1.uniqueIDForThisGame + (int) Game1.stats.DaysPlayed );
            for ( int i = 0; i < racers.Count; ++i )
            {
                int ni = r.Next( racers.Count );
                var old = racers[ ni ];
                racers[ ni ] = racers[ i ];
                racers[ i ] = old;
            }

            // Set states and surfboards
            racerState.Clear();
            foreach ( var racerName in racers )
            {
                racerState.Add( racerName, new RacerState()
                {
                    Surfboard = r.Next( 6 ),
                } );

                // NPCs get a buff since they're dumb
                if ( !racerName.StartsWith( "farmer" ) )
                    racerState[ racerName ].AddedSpeed += 2;
                // Farmer's do if they paid the secret offering
                else if ( Utility.getFarmerFromFarmerNumberString( racerName, Game1.player )?.hasOrWillReceiveMail( "SurfingFestivalOffering" ) ?? false )
                    racerState[ racerName ].AddedSpeed += 3;
            }

            // Move them to their start
            var startPos = new Vector2( 18, 57 );
            if ( racers.Count <= 6 )
            {
                startPos.X += 1;
                startPos.Y -= 1;
            }
            var actualPos = startPos;
            foreach ( var racerName in racers )
            {
                var racer = __instance.getCharacterByName( racerName );

                racer.position.X = actualPos.X * Game1.tileSize + 4;
                racer.position.Y = actualPos.Y * Game1.tileSize;
                racer.faceDirection( Game1.right );

                actualPos.X += 1;
                actualPos.Y -= 1;

                // If a more than 4 players mod is used, things might go out of bounds.
                if ( actualPos.Y < 50 )
                    actualPos.Y = 57;
            }

            // Go to next command
            ++__instance.CurrentCommand;
            __instance.checkForNextCommand( location, time );
        }

        public static void EventCommand_WarpSurfingRacersFinish( Event __instance, GameLocation location, GameTime time, string[] split )
        {
            // Move the racers
            var startPos = new Vector2( 32, 12 );
            if ( racers.Count <= 6 )
                ++startPos.X;
            var actualPos = startPos;
            foreach ( var racerName in racers )
            {
                var racer = __instance.getCharacterByName( racerName );

                racer.position.X = actualPos.X * Game1.tileSize + 4;
                racer.position.Y = actualPos.Y * Game1.tileSize;
                racer.faceDirection( Game1.up );

                actualPos.X += 1;

                // If a more than 4 players mod is used, things might go out of bounds.
                if ( actualPos.X > 39 )
                {
                    actualPos.X = 32;
                    ++actualPos.Y;
                }
            }

            // Go to next command
            ++__instance.CurrentCommand;
            __instance.checkForNextCommand( location, time );
        }

        public static void EventCommand_AwardSurfingPrize( Event __instance, GameLocation location, GameTime time, string[] split )
        {
            if ( raceWinner == "farmer" + Utility.getFarmerNumberFromFarmer( Game1.player ) )
            {
                if ( !Game1.player.mailReceived.Contains( "SurfingFestivalWinner" ) )
                {
                    Game1.player.mailReceived.Add( "SurfingFestivalWinner" );
                    Game1.player.addItemByMenuIfNecessary( new StardewValley.Object( Vector2.Zero, ja.GetBigCraftableId( "Surfing Trophy" ) ) );
                }

                Game1.playSound( "money" );
                Game1.player.Money += 1500;
                Game1.drawObjectDialogue( Mod.instance.Helper.Translation.Get( "dialog.prizemoney" ) );
            }

            __instance.CurrentCommand++;
            if ( Game1.activeClickableMenu == null )
                ++__instance.CurrentCommand;
        }

        private class RacerPlacementComparer : Comparer<string>
        {
            public override int Compare( string x, string y )
            {
                int xLaps = Mod.racerState[ x ].LapsDone;
                int yLaps = Mod.racerState[ y ].LapsDone;
                if ( xLaps != yLaps )
                    return xLaps - yLaps;

                int xPlace = DirectionToProgress( Mod.racerState[ x ].Facing );
                int yPlace = DirectionToProgress( Mod.racerState[ y ].Facing );
                if ( xPlace != yPlace )
                    return xPlace - yPlace;

                int xCoord = (int) GetProgressCoordinate( x );
                int yCoord = (int) GetProgressCoordinate( y );

                // x @ 5, y @ 10
                // right: 5 - 10 = -5, y is greater (same for down)
                // left: -5 - -10 = -5 + 10 = 5, x is greater (same for up)
                return xCoord - yCoord;
            }

            private int DirectionToProgress( int dir )
            {
                switch ( dir )
                {
                    case Game1.up: return 3;
                    case Game1.down: return 1;
                    case Game1.left: return 2;
                    case Game1.right: return 0;
                }
                throw new ArgumentException( "Bad facing direction" );
            }

            private float GetProgressCoordinate( string racerName )
            {
                switch ( Mod.racerState[ racerName ].Facing )
                {
                    case Game1.up: return -Game1.CurrentEvent.getCharacterByName( racerName ).Position.Y;
                    case Game1.down: return Game1.CurrentEvent.getCharacterByName( racerName ).Position.Y;
                    case Game1.left: return -Game1.CurrentEvent.getCharacterByName( racerName ).Position.X;
                    case Game1.right: return Game1.CurrentEvent.getCharacterByName( racerName ).Position.X;
                }
                throw new ArgumentException( "Bad facing direction" );
            }
        };

        public static List<string> GetRacePlacement()
        {
            List<string> ret = new List<string>(racers);
            var cmp = new RacerPlacementComparer();
            ret.Sort( cmp );

            return ret;
        }

        public static Color MyGetPrismaticColor( int offset = 0 )
        {
            float interval = 250f;
            int current_index = ((int)((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / interval) + offset) % Utility.PRISMATIC_COLORS.Length;
            int next_index = (current_index + 1) % Utility.PRISMATIC_COLORS.Length;
            float position = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / interval % 1f;
            Color prismatic_color = default(Color);
            prismatic_color.R = ( byte ) ( Utility.Lerp( ( float ) ( int ) Utility.PRISMATIC_COLORS[ current_index ].R / 255f, ( float ) ( int ) Utility.PRISMATIC_COLORS[ next_index ].R / 255f, position ) * 255f );
            prismatic_color.G = ( byte ) ( Utility.Lerp( ( float ) ( int ) Utility.PRISMATIC_COLORS[ current_index ].G / 255f, ( float ) ( int ) Utility.PRISMATIC_COLORS[ next_index ].G / 255f, position ) * 255f );
            prismatic_color.B = ( byte ) ( Utility.Lerp( ( float ) ( int ) Utility.PRISMATIC_COLORS[ current_index ].B / 255f, ( float ) ( int ) Utility.PRISMATIC_COLORS[ next_index ].B / 255f, position ) * 255f );
            prismatic_color.A = ( byte ) ( Utility.Lerp( ( float ) ( int ) Utility.PRISMATIC_COLORS[ current_index ].A / 255f, ( float ) ( int ) Utility.PRISMATIC_COLORS[ next_index ].A / 255f, position ) * 255f );
            return prismatic_color;
        }
    }
}
