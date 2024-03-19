using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game.Locations.DungeonLevelGenerators;
using MoonMisadventures.VirtualProperties;
using Netcode;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Tiles;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventuress_AsteroidsDungeon" )]
    public class AsteroidsDungeon : LunarLocation
    {
        public const string BaseLocationName = "Custom_MM_MoonAsteroidsDungeon";
        public const string LocationRoomInfix = "Room";
        public const string LocationCaveInfix = "Cave";
        public const int BossLevel = 10;

        public enum LevelType
        {
            Outside,
            Room,
            Cave,
        }

        public static List<BaseDungeonLevelGenerator> NormalDungeonGenerators = new()
        {
            //new BlankDungeonLevelGenerator(),
            new BeltDungeonLevelGenerator(),
        };
        public static List<BaseDungeonLevelGenerator> RoomDungeonGenerators = new()
        {
            new RoomDungeonLevelGenerator(),
        };
        public static List<BaseDungeonLevelGenerator> CaveDungeonGenerators = new()
        {
            new CaveDungeonLevelGenerator(),
        };
        public static List<BaseDungeonLevelGenerator> BossDungeonGenerators = new()
        {
            new BossIslandDungeonLevelGenerator(),
        };
        internal static List<AsteroidsDungeon> activeLevels = new();
        public static AsteroidsDungeon GetLevelInstance( string locName )
        {
            foreach ( var level in activeLevels )
            {
                if ( level.Name == locName )
                    return level;
            }

            LevelType t = LevelType.Outside;
            int l = 0;
            if ( char.IsNumber( locName[ BaseLocationName.Length ] ) )
            {
                t = LevelType.Outside;
                l = int.Parse( locName.Substring( BaseLocationName.Length ) );
            }
            else if ( locName.StartsWith( BaseLocationName + LocationRoomInfix ) )
            {
                t = LevelType.Room;
                string[] parts = locName.Substring( ( BaseLocationName + LocationRoomInfix ).Length ).Split( '_' );
                l = int.Parse( parts[ 0 ] ) * 100 + int.Parse( parts[ 1 ] );
            }
            else if ( locName.StartsWith( BaseLocationName + LocationCaveInfix ) )
            {
                t = LevelType.Cave;
                string[] parts = locName.Substring( ( BaseLocationName + LocationCaveInfix ).Length ).Split( '_' );
                l = int.Parse( parts[ 0 ] ) * 100 + int.Parse( parts[ 1 ] );
            }

            AsteroidsDungeon newLevel = new AsteroidsDungeon( t, l, locName );
            activeLevels.Add( newLevel );

            if ( Game1.IsMasterGame )
                newLevel.Generate();
            else
                newLevel.reloadMap();

            return newLevel;
        }

        public static void UpdateLevels( GameTime time )
        {
            foreach ( var level in activeLevels.ToList() )
            {
                if ( level.farmers.Count > 0 )
                    level.UpdateWhenCurrentLocation( time );
                level.updateEvenIfFarmerIsntHere( time );
            }
        }

        public static void UpdateLevels10Minutes( int time )
        {
            if ( Game1.IsClient )
                return;

            foreach ( var level in activeLevels.ToList() )
            {
                if ( level.farmers.Count > 0 )
                    level.performTenMinuteUpdate( time );
            }
        }

        public static void ClearAllLevels()
        {
            foreach ( var level in activeLevels )
            {
                level.mapContent.Dispose();
            }
            activeLevels.Clear();
        }

        private bool generated = false;
        private LocalizedContentManager mapContent;
        public readonly NetEnum<LevelType> levelType = new();
        public readonly NetInt level = new();
        public readonly NetInt genSeed = new();
        public readonly NetList< Point, NetPoint > fixedTeleporters = new();
        [XmlIgnore]
        public readonly NetEvent1Field<Point, NetPoint> fixTeleporter = new();
        public readonly NetList< Point, NetPoint > doneSwitches = new();
        [XmlIgnore]
        public readonly NetEvent1Field<Point, NetPoint> doSwitch = new();
        private readonly NetVector2 warpFromPrev = new();
        private readonly NetVector2 warpFromNext = new();
        private Random genRandom;
        // These should be fine not synced since they are generated based on seed... right?
        public List<Vector2> teleports = new(); 
        public SerializableDictionary<int, Vector2> lunarDoors = new();
        public bool isIndoorLevel = false;

        public AsteroidsDungeon()
        {
            mapContent = Game1.game1.xTileContent.CreateTemporary();
            mapPath.Value = Mod.instance.Helper.ModContent.GetInternalAssetName("assets/maps/MoonAsteroidsTemplate.tmx").BaseName;
        }

        public AsteroidsDungeon( LevelType type, int level, string name )
        :   this()
        {
            levelType.Value = type;
            this.level.Value = level;
            this.name.Value = name;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(level, "level");
            NetFields.AddField(genSeed, "genSeed");
            NetFields.AddField(fixedTeleporters, "fixedTeleporters");
            NetFields.AddField(fixTeleporter, "fixTeleporter");
            NetFields.AddField(doneSwitches, "doneSwitches");
            NetFields.AddField(doSwitch, "doSwitch");
            NetFields.AddField(warpFromPrev, "warpFromPrev");
            NetFields.AddField(warpFromNext, "warpFromNext");
            fixTeleporter.onEvent += OnFixTeleporter;
            doSwitch.onEvent += DoSwitch;
        }

        private void OnFixTeleporter( Point tile )
        {
            int tx = tile.X;
            int ty = tile.Y;

            int[] a = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            int[] b = new int[] { 9, 10, 11, 12, 13, 14, 15, 16 };
            int[] c = new int[] { 18, 19, 20, 21, 22, 23, 24, 25 };

            int tileBase = getTileIndexAt( tx, ty, "Buildings" ) - 8 - 18;
            a = a.Select( x => x + tileBase ).ToArray();
            b = b.Select( x => x + tileBase ).ToArray();
            c = c.Select( x => x + tileBase ).ToArray();

            int pathsTs = Map.TileSheets.IndexOf( Map.GetTileSheet( "P" ) );
            int teleportTs = Map.TileSheets.IndexOf( Map.GetTileSheet( "z_moon-teleporters" ) );

            setMapTile( tx, ty - 2, 8, "Paths", null, pathsTs );
            setAnimatedMapTile( tx, ty - 2, a, 300, "Front", null, teleportTs );
            setAnimatedMapTile( tx, ty - 1, b, 300, "Front", null, teleportTs );
            Tile t = this.map.GetLayer("Buildings").Tiles[ tx, ty ];
            string actionStr = t.Properties[ "Action" ].ToString();
            setAnimatedMapTile( tx, ty - 0, c, 300, "Buildings", actionStr.Replace( "Offline", "" ), teleportTs );

            if ( !fixedTeleporters.Contains( tile ) )
                fixedTeleporters.Add( tile );
        }

        private void DoSwitch( Point arg )
        {
            int tx = arg.X;
            int ty = arg.Y;

            Tile t = this.map.GetLayer("Back").Tiles[ tx, ty ];
            string taction = t.Properties[ "TouchAction" ].ToString();
            string[] split = taction.Split( ' ' );

            int lockX = int.Parse( split[ 1 ] );
            int lockY = int.Parse( split[ 2 ] );

            setMapTile( lockX, lockY - 2, getTileIndexAt( lockX, lockY - 2, "Buildings" ) - 116, "Buildings", null, map.TileSheets.IndexOf( map.GetTileSheet( getTileSheetIDAt( lockX, lockY - 2, "Buildings" ) ) ) );
            setMapTile( lockX, lockY - 1, getTileIndexAt( lockX, lockY - 1, "Buildings" ) - 116, "Buildings", null, map.TileSheets.IndexOf( map.GetTileSheet( getTileSheetIDAt( lockX, lockY - 1, "Buildings" ) ) ) );
            string prop = doesTileHaveProperty( lockX, lockY, "Action", "Buildings" ).Replace( "LunarLock", "LunarDoor" );
            setMapTile( lockX, lockY - 0, getTileIndexAt( lockX, lockY - 0, "Buildings" ) - 116, "Buildings", prop, map.TileSheets.IndexOf( map.GetTileSheet( getTileSheetIDAt( lockX, lockY - 0, "Buildings" ) ) ) );

            //playSoundAt( "switch", tileLocation );

            setMapTile( tx, ty, getTileIndexAt( tx, ty, "Back" ) + 1, "Back", null, map.TileSheets.IndexOf( map.TileSheets.First( t => t.Id == getTileSheetIDAt( tx, ty, "Back" ) ) ) );

            if ( !doneSwitches.Contains( arg ) )
                doneSwitches.Add( arg );
        }

        protected override LocalizedContentManager getMapLoader()
        {
            return mapContent;
        }

        protected override void resetLocalState()
        {
            if ( !generated )
                Generate();

            base.resetLocalState();

            if ( !Game1.IsMasterGame )
            {
                foreach ( var point in fixedTeleporters )
                    OnFixTeleporter( point );

                foreach ( var point in doneSwitches )
                    DoSwitch( point );
            }

            if ( Game1.player.Tile.X == 0 )
            {
                if ( Game1.player.Tile.Y == 0 )
                {
                    Game1.player.Position = new Vector2( warpFromPrev.X * Game1.tileSize, warpFromPrev.Y * Game1.tileSize );
                }
                else if ( Game1.player.Tile.Y == 1 )
                {
                    Game1.player.Position = new Vector2( warpFromNext.X * Game1.tileSize, warpFromNext.Y * Game1.tileSize );
                }
            }
            else if ( Game1.player.Tile.X == 1 )
            {
                if ( lunarDoors.TryGetValue( (int)Game1.player.Tile.Y, out Vector2 doorPos ) )
                {
                    Game1.player.Position = ( doorPos + new Vector2( 0, 1 ) ) * Game1.tileSize;
                }
            }

            if ( isIndoorLevel )
                Game1.background = null;

            if ( level.Value == 10 )
            {
                Game1.addHUDMessage(new HUDMessage("Boss TO BE IMPLEMENTED, sorry! Enjoy the freebies in the chest!"));
                Game1.addHUDMessage(new HUDMessage("You also obtained the lunar key!"));
                Game1.player.team.get_hasLunarKey().Value = true;
            }
        }

        protected override bool breakStone( string indexOfStone, int x, int y, Farmer who, Random r )
        {
            if ( objects[ new Vector2( x, y ) ].Name == "Stone" &&
                 objects[ new Vector2( x, y ) ].ItemId == ItemIds.MythiciteOreMinable )
            {
                Game1.createItemDebris( new StardewValley.Object( ItemIds.MythiciteOre, 1 )
                {
                    Stack = r.Next( 2, 5 ),
                }, new Vector2( x * Game1.tileSize, y * Game1.tileSize ), 0, this );
            }
            return base.breakStone( indexOfStone, x, y, who, r );
        }

        public void Generate()
        {
            generated = true;

            BaseDungeonLevelGenerator gen = null;
            if ( levelType.Value == LevelType.Outside )
            {
                if ( level != BossLevel )
                {
                    Random r = new Random( ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed + level.Value  );
                    /*var gens = new List<BaseDungeonLevelGenerator>();
                    for ( int i = 0; i < BossLevel; ++i )
                    {
                        gens.Add( NormalDungeonGenerators[ r.Next( NormalDungeonGenerators.Count ) ] );
                    }
                    gen = gens[ level - 1 ];
                    */
                    gen = NormalDungeonGenerators[ r.Next( NormalDungeonGenerators.Count ) ];
                }
                else
                {
                    Random r = new Random( ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed / 7 + level.Value  );
                    gen = BossDungeonGenerators[ r.Next( BossDungeonGenerators.Count ) ];
                }
            }
            else if ( levelType.Value == LevelType.Room )
            {
                Random r = new Random( ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed / 3+ level.Value  );
                gen = RoomDungeonGenerators[ r.Next( RoomDungeonGenerators.Count ) ];
            }
            else if ( levelType.Value == LevelType.Cave )
            {
                Random r = new Random( ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed / 4 + level.Value );
                gen = CaveDungeonGenerators[ r.Next( CaveDungeonGenerators.Count ) ];
            }

            if ( Game1.IsMasterGame )
            {
                genSeed.Value = ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed * level.Value * SDate.Now().DaysSinceStart; 
            }

            Vector2 warpPrev = Vector2.Zero, warpNext = Vector2.Zero;
            reloadMap();
            map.Id = BaseLocationName;

            if ( Map.GetLayer( "Buildings1" ) == null )
            {
                Map.AddLayer( new xTile.Layers.Layer( "Buildings1", Map, Map.Layers[ 0 ].LayerSize, Map.Layers[ 0 ].TileSize ) );
            }

            var ts = new xTile.Tiles.TileSheet( Map, Mod.instance.Helper.ModContent.GetInternalAssetName( "assets/maps/moon-teleporters.png" ).BaseName, new xTile.Dimensions.Size( 9, 9 ), new xTile.Dimensions.Size( 16, 16 ) );
            ts.Id = "z_moon-teleporters";
            Map.AddTileSheet( ts );
            Map.LoadTileSheets( Game1.mapDisplayDevice );

            try
            {
                gen.Generate( this, ref warpPrev, ref warpNext );
                warpFromPrev.Value = warpPrev;
                warpFromNext.Value = warpNext;
            }
            catch ( Exception e )
            {
                SpaceShared.Log.Error( "Exception generating dungeon: " + e );
            }
        }

        public override void UpdateWhenCurrentLocation( GameTime time )
        {
            base.UpdateWhenCurrentLocation( time );
            fixTeleporter.Poll();
            doSwitch.Poll();
            foreach ( KeyValuePair<long, FarmAnimal> kvp in this.Animals.Pairs )
            {
                kvp.Value.updateWhenCurrentLocation( time, this );
            }
        }

        public override bool performAction( string actionStr, Farmer who, xTile.Dimensions.Location tileLocation )
        {
            string[] split = actionStr.Split( ' ' );
            string action = split[ 0 ];
            int tx = tileLocation.X;
            int ty = tileLocation.Y;

            if ( action == "AsteroidsWarpPrevious" )
            {
                string prev = AsteroidsDungeon.BaseLocationName + ( level.Value - 1 );
                if ( level.Value == 1 )
                    prev = "Custom_MM_MoonAsteroidsEntrance";

                performTouchAction( "MagicWarp " + prev + " 0 1", Game1.player.Tile );
            }
            else if ( action == "AsteroidsWarpNext" )
            {
                if (warpFromPrev == warpFromNext) // boss level
                    performTouchAction("MagicWarp Custom_MM_MoonFarm 7 11", Game1.player.Tile);
                else
                {
                    string next = AsteroidsDungeon.BaseLocationName + (level.Value + 1);
                    performTouchAction("MagicWarp " + next + " 0 0", Game1.player.Tile);
                }
            }
            else if ( action == "LunarTeleporterOffline" )
            {
                if ( Game1.player.ActiveObject?.ItemId == ItemIds.MythiciteOre )
                {
                    who.reduceActiveItemByOne();
                    Game1.playSound( "questcomplete" );

                    fixTeleporter.Fire( new Point( tx, ty ) );
                }
                else
                {
                    Game1.drawObjectDialogue(I18n.Message_LunarTeleporterOffline());
                }
            }
            else if ( action == "LunarTeleporter" )
            {
                playSound( "wand" );
                who.Position = teleports[ int.Parse( split[ 1 ] ) ];
            }
            else if ( action == "LunarDoor" )
            {
                Game1.warpFarmer( split[ 1 ], 0, 0, 0 );
            }
            return base.performAction( action, who, tileLocation );
        }

        public override void performTouchAction( string actionStr, Vector2 tileLocation )
        {
            string[] split = actionStr.Split( ' ' );
            string action = split[ 0 ];
            int tx = (int) tileLocation.X;
            int ty = (int) tileLocation.Y;

            if ( action == "LunarSwitch" )
            {
                doSwitch.Fire( new Point( tx, ty ) );
                Game1.playSound( "shiny4" );
            }
            else if ( action == "LunarCave" )
            {
                Game1.warpFarmer( split[ 1 ], 0, 0, 0 );
            }

            base.performTouchAction( actionStr, tileLocation );
        }

        public override bool CanPlaceThisFurnitureHere( Furniture furniture )
        {
            return false;
        }
    }
}
