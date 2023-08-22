using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MoonMisadventures.Game.Items;
using MoonMisadventures.Game.Monsters;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

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
                location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object(rand.NextDouble() < 0.5 ? "846" : "847", 1 )
                {
                    TileLocation = new Vector2(sx, sy),
                    Name = "Stone",
                    MinutesUntilReady = 12
                } );
            }
            else if ( r < 0.85 )
            {
                string[] ores = new string[] { "95", "95", "849", "850", "764", "765", null, null, null };
                int[] breaks = new int[] { 15, 15, 6, 8, 10, 12 };
                int ore_ = rand.Next( ores.Length );
                string ore = ores[ ore_ ];
                if ( ore == null )
                {
                    var obj = new StardewValley.Object(ItemIds.MythiciteOreMinable, 1)
                    {
                        TileLocation = new Vector2(sx, sy)
                    };
                    obj.Name = "Stone";
                    obj.MinutesUntilReady = 24;
                    location.netObjects.Add( new Vector2( sx, sy ), obj );
                }
                else
                {
                    location.netObjects.Add(new Vector2(sx, sy), new StardewValley.Object(ore, 1 )
                    {
                        TileLocation = new Vector2(sx, sy),
                        Name = "Stone",
                        MinutesUntilReady = breaks[ ore_ ]
                    } );
                }
            }
            else if ( r < 0.95 )
            {
                string[] gems = new[] { "2", "4", "6", "8", "10", "12", "14", "44", "44", "44", "46", "46" };
                int gem_ = rand.Next( gems.Length );
                string gem = gems[ gem_ ];
                location.netObjects.Add(new Vector2(sx, sy), new StardewValley.Object(gem, 1 )
                {
                    TileLocation = new Vector2(sx, sy),
                    Name = "Stone",
                    MinutesUntilReady = 10
                } );
            }
            else
            {
                location.netObjects.Add(new Vector2(sx, sy), new StardewValley.Object( "819", 1 )
                {
                    TileLocation = new Vector2(sx, sy),
                    Name = "Stone",
                    MinutesUntilReady = 10
                } );
            }
        }

        protected void PlaceMonsterAt(AsteroidsDungeon location, Random rand, int tx, int ty)
        {
            switch (rand.Next(8))
            {
                case 0:
                    location.characters.Add(new CrystalBehemoth(new Vector2(tx * Game1.tileSize, ty * Game1.tileSize)));
                    break;
                case 1:
                case 2:
                case 3:
                    location.characters.Add(new BoomEye(new Vector2(tx * Game1.tileSize, ty * Game1.tileSize)));
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                    location.characters.Add(new LunarSlime(new Vector2(tx * Game1.tileSize, ty * Game1.tileSize)));
                    break;
            }
        }

        protected void PlaceBreakableAt( AsteroidsDungeon location, Random rand, int tx, int ty )
        {
            Vector2 position = new Vector2( tx, ty );
            if ( location.netObjects.ContainsKey( position ) )
                return;

            BreakableContainer bcontainer = BreakableContainer.GetBarrelForVolcanoDungeon(position);
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
                        chest.addItem( new StardewValley.Object( ItemIds.SoulSapphire, 1 ) );
                        break;
                    case 5:
                        chest.addItem(new Boots(ItemIds.CosmosBoots));
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
                                item.holding.Value = new FarmAnimal("Lunar Cow", mp.getNewID(), 0);
                                break;
                            case 4:
                                item.holding.Value = new FarmAnimal("Lunar Chicken", mp.getNewID(), 0);
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
                        chest.addItem( new StardewValley.Object( ItemIds.MythiciteOre, 3 + rand.Next( 12 ) ) );
                        break;
                    case 3:
                        chest.addItem( new StardewValley.Object( ItemIds.MythiciteBar, 1 + rand.Next( 7 ) ) );
                        break;
                    case 4:
                        chest.addItem( new StardewValley.Object( ItemIds.StellarEssence, 4 + rand.Next( 9 ) ) );
                        break;
                    case 5:
                        chest.addItem( new StardewValley.Object( ItemIds.PersistiumDust, 2 + rand.Next( 6 ) ) );
                        break;

                }
            }
            if ( location.netObjects.ContainsKey( position ) )
                location.netObjects.Remove( position );
            location.netObjects.Add( position, chest );
        }
    }
}
