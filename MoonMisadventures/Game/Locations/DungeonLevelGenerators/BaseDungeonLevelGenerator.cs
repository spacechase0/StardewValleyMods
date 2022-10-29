using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using MoonMisadventures.Game.Items;
using MoonMisadventures.Game.Monsters;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace MoonMisadventures.Game.Locations.DungeonLevelGenerators
{
    public abstract class BaseDungeonLevelGenerator
    {
        public abstract void Generate( AsteroidsDungeon location, ref Vector2 warpFromPrev, ref Vector2 warpFromNext );

        protected string GetNextLocationName( AsteroidsDungeon location )
        {
            return AsteroidsDungeon.BaseLocationName + ( location.level.Value + 1 );
        }

        protected string GetPreviousLocationName( AsteroidsDungeon location )
        {
            if ( location.level.Value == 1 )
                return "Custom_MM_MoonAsteroidsEntrance";
            return AsteroidsDungeon.BaseLocationName + ( location.level.Value - 1 );
        }

        protected void PlacePreviousWarp( AsteroidsDungeon location, int centerX, int groundY )
        {
            int ts = location.Map.TileSheets.IndexOf( location.Map.GetTileSheet( "tf_darkdimension_sheet" ) );

            Log.Debug( "Placing previous warp @ " + centerX + ", " + groundY );
            location.setMapTile( centerX + -1, groundY - 2, 503, "Front", null, ts );
            location.setMapTile( centerX +  0, groundY - 2, 504, "Front", null, ts );
            location.setMapTile( centerX +  1, groundY - 2, 505, "Front", null, ts );
            location.setMapTile( centerX + -1, groundY - 1, 532, "Front", null, ts );
            location.setMapTile( centerX +  0, groundY - 1, 533, "Front", null, ts );
            location.setMapTile( centerX +  1, groundY - 1, 534, "Front", null, ts );
            location.setMapTile( centerX + -1, groundY - 0, 561, "Buildings", "AsteroidsWarpPrevious", ts );
            location.setMapTile( centerX +  0, groundY - 0, 562, "Buildings", "AsteroidsWarpPrevious", ts );
            location.setMapTile( centerX +  1, groundY - 0, 563, "Buildings", "AsteroidsWarpPrevious", ts );
        }

        protected void PlaceNextWarp( AsteroidsDungeon location, int centerX, int groundY )
        {
            int ts = location.Map.TileSheets.IndexOf( location.Map.GetTileSheet( "tf_darkdimension_sheet" ) );

            Log.Debug( "Placing next warp @ " + centerX + ", " + groundY );
            location.setMapTile( centerX + -1, groundY - 2, 503+9, "Front", null, ts );
            location.setMapTile( centerX + 0, groundY - 2, 504+9, "Front", null, ts );
            location.setMapTile( centerX + 1, groundY - 2, 505+9, "Front", null, ts );
            location.setMapTile( centerX + -1, groundY - 1, 532+9, "Front", null, ts );
            location.setMapTile( centerX + 0, groundY - 1, 533+9, "Front", null, ts );
            location.setMapTile( centerX + 1, groundY - 1, 534+9, "Front", null, ts );
            location.setMapTile( centerX + -1, groundY - 0, 561+9, "Buildings", "AsteroidsWarpNext", ts );
            location.setMapTile( centerX + 0, groundY - 0, 562+9, "Buildings", "AsteroidsWarpNext", ts );
            location.setMapTile( centerX + 1, groundY - 0, 563+9, "Buildings", "AsteroidsWarpNext", ts );
        }

        protected void PlaceRandomTeleporterPair( AsteroidsDungeon location, Random rand, int centerX1, int groundY1, int centerX2, int groundY2, bool canInactive = true )
        {
            int num = rand.Next( 3 );
            bool active = true;
            if ( canInactive && rand.NextDouble() < 0.5 )
                active = false;

            PlaceTeleporter( location, rand, num, active, centerX1, groundY1, centerX2, groundY2 + 1 );
            PlaceTeleporter( location, rand, num, true, centerX2, groundY2, centerX1, groundY1 + 1 );
        }
        protected void PlaceTeleporter( AsteroidsDungeon location, Random rand, int num, bool active, int centerX, int groundY, int targetX, int targetY )
        {
            int offset = num * 27;
            int ts = location.Map.TileSheets.IndexOf( location.Map.GetTileSheet( "z_moon-teleporters" ) );
            int ts2 = location.Map.TileSheets.IndexOf( location.Map.GetTileSheet( "P" ) );

            Log.Debug( "Placing teleporter " + location.teleports.Count + " @ " + centerX + ", " + groundY + " to " + targetX + " " + targetY );
            if ( !active )
            {
                location.setMapTile( centerX, groundY - 2, offset + 0 + 8, "Front", null, ts );
                location.setMapTile( centerX, groundY - 1, offset + 9 + 8, "Front", null, ts );
                location.setMapTile( centerX, groundY - 0, offset + 18 + 8, "Buildings", "LunarTeleporterOffline " + location.teleports.Count, ts );
            }
            else
            {
                int[] a = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] b = new int[] { 9, 10, 11, 12, 13, 14, 15, 16 };
                int[] c = new int[] { 18, 19, 20, 21, 22, 23, 24, 25 };

                a = a.Select( x => x + offset ).ToArray();
                b = b.Select( x => x + offset ).ToArray();
                c = c.Select( x => x + offset ).ToArray();

                location.setMapTile( centerX, groundY - 2, 8, "Paths", null, ts2 );
                location.setAnimatedMapTile( centerX, groundY - 2, a, 300, "Front", null, ts );
                location.setAnimatedMapTile( centerX, groundY - 1, b, 300, "Front", null, ts );
                location.setAnimatedMapTile( centerX, groundY - 0, c, 300, "Buildings", "LunarTeleporter " + location.teleports.Count, ts );
            }
            location.teleports.Add( new Vector2( targetX * Game1.tileSize, targetY * Game1.tileSize ) );
        }

        protected List< Vector2 > MakeSmallAsteroid( Random r, int centerX, int centerY, int size )
        {
            // This isn't completely accurate or efficient, but good enough for now
            List< Vector2 > ret = new();

            List< Vector2 > outerTiles = new();
            ret.Add( new Vector2( centerX, centerY ) );
            outerTiles.Add( new Vector2( centerX, centerY ) );
            List< Vector2 > open = new();
            for ( --size; size > 0; )
            {
                var tile = outerTiles[ r.Next( outerTiles.Count ) ];
                var check = tile + new Vector2( -1, 0 );
                if ( !ret.Contains( check ) )
                    open.Add( check );
                check = tile + new Vector2( 1, 0 );
                if ( !ret.Contains( check ) )
                    open.Add( check );
                check = tile + new Vector2( 0, -1 );
                if ( !ret.Contains( check ) )
                    open.Add( check );
                check = tile + new Vector2( 0, 1 );
                if ( !ret.Contains( check ) )
                    open.Add( check );

                if ( open.Count == 0 )
                {
                    outerTiles.Remove( tile );
                    ++size;
                    continue;
                }

                ret.AddRange( open );
                size -= open.Count;
                outerTiles.AddRange( open );
                open.Clear();
            }

            return ret;
        }

        protected void PlaceMinableAt( AsteroidsDungeon location, Random rand, int sx, int sy )
        {
            double r = rand.NextDouble();
            if ( r < 0.65 )
            {
                location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), rand.NextDouble() < 0.5 ? 846 : 847, 1 )
                {
                    Name = "Stone",
                    MinutesUntilReady = 12
                } );
            }
            else if ( r < 0.85 )
            {
                int[] ores = new int[] { 95, 95, 849, 850, 764, 765, int.MaxValue, int.MaxValue, int.MaxValue };
                int[] breaks = new int[] { 15, 15, 6, 8, 10, 12 };
                int ore_ = rand.Next( ores.Length );
                int ore = ores[ ore_ ];
                if ( ore == int.MaxValue )
                {
                    var obj = new DynamicGameAssets.Game.CustomObject( ( DynamicGameAssets.PackData.ObjectPackData ) DynamicGameAssets.Mod.Find( ItemIds.MythiciteOreMinable ) );
                    obj.Name = "Stone";
                    obj.MinutesUntilReady = 24;
                    location.netObjects.Add( new Vector2( sx, sy ), obj );
                }
                else
                {
                    location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), ore, 1 )
                    {
                        Name = "Stone",
                        MinutesUntilReady = breaks[ ore_ ]
                    } );
                }
            }
            else if ( r < 0.95 )
            {
                int[] gems = new int[] { 2, 4, 6, 8, 10, 12, 14, 44, 44, 44, 46, 46 };
                int gem_ = rand.Next( gems.Length );
                int gem = gems[ gem_ ];
                location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), gem, 1 )
                {
                    Name = "Stone",
                    MinutesUntilReady = 10
                } );
            }
            else
            {
                location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), 819, 1 )
                {
                    Name = "Stone",
                    MinutesUntilReady = 10
                } );
            }
        }

        protected void PlaceMonsterAt(AsteroidsDungeon location, Random rand, int tx, int ty)
        {
            switch (rand.Next(3))
            {
                case 0:
                    location.characters.Add(new BoomEye(new Vector2(tx * Game1.tileSize, ty * Game1.tileSize)));
                    break;
                case 1:
                case 2:
                    location.characters.Add(new LunarSlime(new Vector2(tx * Game1.tileSize, ty * Game1.tileSize)));
                    break;
            }
        }

        protected void PlaceBreakableAt( AsteroidsDungeon location, Random rand, int tx, int ty )
        {
            Vector2 position = new Vector2( tx, ty );
            if ( location.netObjects.ContainsKey( position ) )
                return;

            BreakableContainer bcontainer = new BreakableContainer( position, true );
            bcontainer.setHealth( 6 );

            location.netObjects.Add( position, bcontainer );
        }

        protected void PlaceChestAt( AsteroidsDungeon location, Random rand, int tx, int ty, bool rare )
        {
            Vector2 position = new Vector2( tx, ty);
            Chest chest = new Chest(playerChest: false, position);
            chest.dropContents.Value = true;
            chest.synchronized.Value = true;
            chest.Type = "interactive";
            if ( rare )
            {
                chest.SetBigCraftableSpriteIndex( 227 );
                switch ( rand.Next( 7 ) )
                {
                    case 0:
                    case 1:
                    case 2:
                        chest.addItem( new Necklace( Necklace.Type.Lunar ) );
                        break;
                    case 3:
                    case 4:
                        chest.addItem( new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.SoulSapphire ) as ObjectPackData ) );
                        break;
                    case 5:
                        chest.addItem( new DynamicGameAssets.Game.CustomBoots( DynamicGameAssets.Mod.Find( ItemIds.CosmosBoots ) as BootsPackData ) );
                        break;
                    case 6:
                        var item = new AnimalGauntlets();
                        var mp = Mod.instance.Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();
                        switch ( rand.Next( 5 ) )
                        {
                            case 0:
                            case 1:
                            case 2:
                                break;
                            case 3:
                                item.holding.Value = new LunarAnimal( LunarAnimalType.Cow, Vector2.Zero, mp.getNewID() );
                                break;
                            case 4:
                                item.holding.Value = new LunarAnimal( LunarAnimalType.Chicken, Vector2.Zero, mp.getNewID() );
                                break;
                        }
                        chest.addItem( item );
                        break;
                }
            }
            else
            {
                chest.SetBigCraftableSpriteIndex( 223 );
                switch ( rand.Next( 6 ) )
                {
                    case 0:
                    case 1:
                    case 2:
                        chest.addItem( new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.MythiciteOre ) as ObjectPackData ) { Stack = 3 + rand.Next( 12 ) } );
                        break;
                    case 3:
                        chest.addItem( new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.MythiciteBar ) as ObjectPackData ) { Stack = 1 + rand.Next( 7 ) } );
                        break;
                    case 4:
                        chest.addItem( new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.StellarEssence ) as ObjectPackData ) { Stack = 4 + rand.Next( 9 ) } );
                        break;
                    case 5:
                        chest.addItem( new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.PersistiumDust ) as ObjectPackData ) { Stack = 2 + rand.Next( 6 ) } );
                        break;

                }
            }
            if ( location.netObjects.ContainsKey( position ) )
                location.netObjects.Remove( position );
            location.netObjects.Add( position, chest );
        }
    }
}
