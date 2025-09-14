using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley;
using xTile;
using xTile.Tiles;

namespace MoonMisadventures.Game.Locations.DungeonLevelGenerators
{
    public class BeltDungeonLevelGenerator : BaseDungeonLevelGenerator
    {
        private class Isle
        {
            public List< Vector2 > tiles = new();
            public List< Vector2 > spots = new();
        }

        public override void Generate( AsteroidsDungeon location, ref Vector2 warpFromPrev, ref Vector2 warpFromNext )
        {
            int ts = location.Map.TileSheets.IndexOf( location.Map.GetTileSheet( "tf_darkdimension_sheet" ) );
            Random rand = new Random( location.genSeed.Value );

            // First pass - a line across the map
            // y = mx + b
            // Center at 75, 75 for template I have right now, but don't have to hard-code that I guess...
            double w = location.map.Layers[ 0 ].LayerWidth;
            double h = location.map.Layers[ 0 ].LayerHeight;
            double cx = w / 2;
            double cy = h / 2;
            // Slope should cross 70% of the map? Sure.
            double slopeMax = h * 0.7 / w;
            double m = rand.NextDouble() * slopeMax * 2 - slopeMax;
            // b can be 0, don't mind it centering on center - not like you can see the whole map in-game anyways (without Realtime Minimap)
            double b = 0;
            Func<double, double> pass1 = (x) => m * x + b;

            // Second pass - sin wave for curve on line
            // How much curve? Uh... 40%? 
            // I don't know the 'proper' variable names for these :P
            double curveMax = h * 0.4 / w;
            double c = rand.NextDouble() * curveMax * 2 - curveMax;
            // But don't want to go off the edge of the map
            if ( Math.Abs( m ) + Math.Abs( c ) >= 0.90 )
                m = ( 0.90 - Math.Abs( c ) ) * Math.Sign( m );
            // Period - tweaked a lot, don't remember what I originally wanted :P
            double p = ( 1 + rand.NextDouble() ) * w / 2;
            // Randomly start in an offset in the sin wave
            double o = rand.NextDouble() * Math.PI * 2;
            Func< double, double > pass2 = (x) => c * h / 2 * Math.Sin( Math.PI / p * x + o );

            // Which coordinate to use as "base" for the line
            bool relToX = rand.NextDouble() < 0.5;

            double beltWidth = 23 + rand.NextDouble() * 20;

            Log.Debug( $"Belt params: {m} {b} {c} {p} {relToX} {beltWidth}" );

            // Big block with copied code because faster than checking relToX every iteration
            // Also, don't need every single point. Speeds it up some, and makes things "chunkier"
            List<Vector2> points = new();
            if ( relToX )
            {
                for ( int ix = 12; ix < w - 12; ix += 4 )
                {
                    double y = pass1( ix - cx ) + pass2( ix - cx );
                    points.Add( new Vector2( ix, ( int ) ( y + cy ) ) );
                }
            }
            else
            {
                for ( int ix = 12; ix < w - 12; ix += 4 )
                {
                    double y = pass1( ix - cx ) + pass2( ix - cx );
                    points.Add( new Vector2( ( int ) ( y + cy ), ix ) );
                }
            }


            List< Vector2 > tiles = new();
            List< Vector2 > chunkSpots = new(); // The placement point of each chunk, used later
            for ( int ip = 1; ip < points.Count - 1; ++ip)
            {
                for ( int ib = 0; ib < beltWidth / 6; ++ib )
                {
                    Vector2 pt = points[ ip ];
                    Vector2 diffAdjacent = points[ ip + 1 ] - points[ ip - 1 ];
                    diffAdjacent.Normalize();
                    pt += new Vector2( diffAdjacent.Y, diffAdjacent.X ) * ( float ) ( rand.NextDouble() * 2 - 1 ) * ( float ) beltWidth; ;

                    if ( pt.X < 10 || pt.Y < 10 || pt.X >= w - 10 || pt.Y >= h - 10 )
                        continue;
                    if ( chunkSpots.Contains( new Vector2( ( int ) pt.X, ( int ) pt.Y ) ) )
                        continue;
                    if ( rand.NextDouble() <= 0.15 )
                        continue;

                    List< Vector2 > localTiles = MakeSmallAsteroid( rand, ( int ) pt.X, ( int ) pt.Y, 19 + rand.Next( 26 ) );

                    foreach ( var tile in localTiles )
                    {
                        location.setMapTile( ( int ) tile.X, ( int ) tile.Y, 237, "Back", null, ts );
                    }

                    tiles.AddRange( localTiles );
                    chunkSpots.Add( new Vector2( ( int ) pt.X, ( int ) pt.Y ) );
                }
            }

            // Do a cleanup pass or two
            foreach ( var tile in tiles.ToList() )
            {
                int tx = ( int ) tile.X;
                int ty = ( int ) tile.Y;

                bool lu = location.getTileIndexAt( tx - 1, ty - 1, "Back" ) == 237;
                bool  u = location.getTileIndexAt( tx + 0, ty - 1, "Back" ) == 237;
                bool ru = location.getTileIndexAt( tx + 1, ty - 1, "Back" ) == 237;
                bool l  = location.getTileIndexAt( tx - 1, ty + 0, "Back" ) == 237;
                bool r  = location.getTileIndexAt( tx + 1, ty + 0, "Back" ) == 237;
                bool ld = location.getTileIndexAt( tx - 1, ty + 1, "Back" ) == 237;
                bool  d = location.getTileIndexAt( tx + 0, ty + 1, "Back" ) == 237;
                bool rd = location.getTileIndexAt( tx + 1, ty + 1, "Back" ) == 237;

                // Fix corridors that will end up not walkable once buildings layer tiles are added
                if ( u && ( lu ^ ru ) && ( l ^ r ) )
                {
                    if ( !lu )
                    {
                        location.setMapTile( tx - 1, ty - 1, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx - 1, ty - 1 ) );
                    }
                    else
                    {
                        location.setMapTile( tx + 1, ty - 1, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx + 1, ty - 1 ) );
                    }
                    if ( !l )
                    {
                        location.setMapTile( tx - 1, ty, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx - 1, ty ) );
                    }
                    if ( !r )
                    {
                        location.setMapTile( tx + 1, ty, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx + 1, ty ) );
                    }
                }
                if ( l && ( lu ^ ld ) && ( u ^ d ) )
                {
                    if ( !lu )
                    {
                        location.setMapTile( tx - 1, ty - 1, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx - 1, ty - 1 ) );
                    }
                    else
                    {
                        location.setMapTile( tx - 1, ty + 1, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx - 1, ty + 1 ) );
                    }
                    if ( !u )
                    {
                        location.setMapTile( tx, ty - 1, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx, ty - 1 ) );
                    }
                    if ( !d )
                    {
                        location.setMapTile( tx, ty + 1, 237, "Back", null, ts );
                        tiles.Add( new Vector2( tx, ty + 1 ) );
                    }
                }

                // probably more I should do
            }
            foreach ( var tile in tiles.ToList() )
            {
                int tx = ( int ) tile.X;
                int ty = ( int ) tile.Y;

                bool ld = location.getTileIndexAt( tx - 1, ty + 1, "Back" ) == 237;
                bool rd = location.getTileIndexAt( tx + 1, ty + 1, "Back" ) == 237;
                bool dd = location.getTileIndexAt( tx + 0, ty + 2, "Back" ) == 237;

                // Fill 1 tile holes
                if ( ld && rd && dd )
                {
                    location.setMapTile( tx, ty + 1, 237, "Back", null, ts );
                    tiles.Add( new Vector2( tx, ty + 1 ) );
                }
            }

            List<Vector2> pendingClearBack = new();
            Action< int, int, int > setBuildingsOrAlt = ( tx, ty, t ) =>
            {
                int ot = location.getTileIndexAt( tx, ty, "Buildings" );
                if ( ot == -1 )
                    location.setMapTile( tx, ty, t, "Buildings", null, ts );
                else
                {
                    location.setMapTile( tx, ty, ot, "Buildings1", null, ts );
                    location.setMapTile( tx, ty, t, "Buildings", null, ts );
                }
            };
            Action< int, int > doTile = null;
            doTile = ( tx, ty ) =>
            {
                if ( !tiles.Contains( new Vector2( tx, ty ) ) )
                    return;

                // Should I change these checks to tiles.Contains ??? Would it be faster?
                bool lu = location.getTileIndexAt( tx - 1, ty - 1, "Back" ) is 237 or 210 or 211 or 239 or 240;
                bool  u = location.getTileIndexAt( tx + 0, ty - 1, "Back" ) is 237 or 210 or 211 or 239 or 240;
                bool ru = location.getTileIndexAt( tx + 1, ty - 1, "Back" ) is 237 or 210 or 211 or 239 or 240;
                bool l  = location.getTileIndexAt( tx - 1, ty + 0, "Back" ) is 237 or 210 or 211 or 239 or 240;
                bool r  = location.getTileIndexAt( tx + 1, ty + 0, "Back" ) is 237 or 210 or 211 or 239 or 240;
                bool ld = location.getTileIndexAt( tx - 1, ty + 1, "Back" ) is 237 or 210 or 211 or 239 or 240;
                bool  d = location.getTileIndexAt( tx + 0, ty + 1, "Back" ) is 237 or 210 or 211 or 239 or 240;
                bool rd = location.getTileIndexAt( tx + 1, ty + 1, "Back" ) is 237 or 210 or 211 or 239 or 240;
                /*
                bool lu = tiles.Contains( new Vector2( tx - 1, ty - 1 ) );
                bool  u = tiles.Contains( new Vector2( tx + 0, ty - 1 ) );
                bool ru = tiles.Contains( new Vector2( tx + 1, ty - 1 ) );
                bool l  = tiles.Contains( new Vector2( tx - 1, ty + 0 ) );
                bool r  = tiles.Contains( new Vector2( tx + 1, ty + 0 ) );
                bool ld = tiles.Contains( new Vector2( tx - 1, ty + 1 ) );
                bool  d = tiles.Contains( new Vector2( tx + 0, ty + 1 ) );
                bool rd = tiles.Contains( new Vector2( tx + 1, ty + 1 ) );
                */

                // TODO: Optimize, maybe use bitflags/masks?
                if ( !d )
                {
                    bool dd = location.getTileIndexAt( tx + 0, ty + 2, "Back" ) is 237 or 210 or 211 or 239 or 240;
                    bool ddd = location.getTileIndexAt( tx + 0, ty + 3, "Back" ) is 237 or 210 or 211 or 239 or 240;
                    if ( u )
                    {
                        pendingClearBack.Add( new Vector2( tx, ty ) );
                        if ( l && r )
                        {
                            location.setMapTile( tx, ty, 266, "Buildings", null, ts );
                            location.setMapTile( tx, ty + 1, 295, "Back", null, ts );
                            if ( !dd )
                                location.setMapTile( tx, ty + 2, 382, "Back", null, ts );
                        }
                        else if ( !l && r )
                        {
                            location.setMapTile( tx, ty, 265, "Buildings", null, ts );
                            location.setMapTile( tx, ty + 1, 294, "Buildings", null, ts );
                            if ( !dd )
                                location.setMapTile( tx, ty + 2, 381, "Back", null, ts );
                        }
                        else if ( l && !r )
                        {
                            location.setMapTile( tx, ty, 267, "Buildings", null, ts );
                            location.setMapTile( tx, ty + 1, 296, "Buildings", null, ts );
                            if ( !dd )
                                location.setMapTile( tx, ty + 2, 383, "Back", null, ts );
                        }
                        else // !l && !r
                        {
                            // U shape
                            location.removeTile( tx, ty, "Back" );
                            doTile( tx - 1, ty - 1 );
                            doTile( tx + 0, ty - 1 );
                            doTile( tx - 1, ty + 0 );
                        }
                    }
                    else
                    {
                        // = shape (or < or > or nothing)
                        location.removeTile( tx, ty, "Back" );
                        doTile( tx - 1, ty - 1 );
                        doTile( tx + 0, ty - 1 );
                        doTile( tx - 1, ty + 0 );
                    }
                }
                else
                {
                    if ( !u )
                    {
                        pendingClearBack.Add( new Vector2( tx, ty ) );
                        if ( l && r )
                            setBuildingsOrAlt( tx, ty, 208 );
                        else if ( !l && r )
                            setBuildingsOrAlt( tx, ty, 207 );
                        else if ( l && !r )
                            setBuildingsOrAlt( tx, ty, 209 );
                        else // if ( !l && !r )
                        {
                            // upside down U shape
                            location.removeTile( tx, ty, "Back" );
                            doTile( tx - 1, ty - 1 );
                            doTile( tx + 0, ty - 1 );
                            doTile( tx - 1, ty + 0 );
                        }
                    }
                    else
                    {
                        if ( !( l && r ) )
                            pendingClearBack.Add( new Vector2( tx, ty ) );
                        if ( l && r )
                        {
                            // full tile, at least orthogonally (is that the right word?)
                            if ( !lu && ru && ld && rd )
                                location.setMapTile( tx, ty, 210, "Back", null, ts );
                            else if ( lu && !ru && ld && rd )
                                location.setMapTile( tx, ty, 211, "Back", null, ts );
                            else if ( lu && ru && !ld && rd )
                                location.setMapTile( tx, ty, 239, "Back", null, ts );
                            else if ( lu && ru && ld && !rd )
                                location.setMapTile( tx, ty, 240, "Back", null, ts );
                        }
                        else if ( !l && r )
                            setBuildingsOrAlt( tx, ty, 236 );
                        else if ( l && !r )
                            setBuildingsOrAlt( tx, ty, 238 );
                        else // if ( !l && !r )
                        {
                            // || shape
                            location.removeTile( tx, ty, "Back" );
                            doTile( tx - 1, ty - 1 );
                            doTile( tx + 0, ty - 1 );
                            doTile( tx - 1, ty + 0 );
                        }
                    }
                }
            };

            tiles.Sort( ( a, b ) => Math.Sign(b.X + b.Y * w - a.X + a.Y * w ) );
            foreach ( var tile in tiles )
            {
                int tx = ( int ) tile.X;
                int ty = ( int ) tile.Y;

                doTile( tx, ty );
            }

            foreach ( var tile in pendingClearBack )
            {
                if ( location.getTileIndexAt( ( int ) tile.X, ( int ) tile.Y, "Back" ) != 295 ) // some holes were popping up
                    location.removeTile( ( int ) tile.X, ( int ) tile.Y, "Back" );
            }

            location.PlaceSpaceTiles(); // Need these to check asteroid connectivity

            // Gather tiles into traversable "islands"
            // An island can contain multiple asteroids if you can get to them by pickaxing an edge
            List<Vector2> checkTiles = new( tiles );
            List<Isle> islands = new();
            while ( checkTiles.Count > 0 )
            {
                Vector2 spot = checkTiles[ 0 ];
                checkTiles.RemoveAt( 0 );
                var isle = new Isle();

                Queue< Vector2 > pending = new();
                pending.Enqueue( spot );

                while ( pending.Count > 0 )
                {
                    var curr = pending.Dequeue();
                    if ( chunkSpots.Contains( curr ) )
                    {
                        isle.spots.Add( curr );
                        chunkSpots.Remove( curr );
                    }

                    Action<Vector2> checkAdjacency = ( tv ) =>
                    {
                        if ( checkTiles.Contains( tv ) )
                        {
                            int t = location.getTileIndexAt( ( int ) tv.X, ( int ) tv.Y, "Back" );
                            if ( t == 237 )
                            {
                                curr = curr;
                                // Plain tile is fine
                                isle.tiles.Add( tv );
                                pending.Enqueue( tv );
                                checkTiles.Remove( tv );
                            }
                            else
                            {
                                // Check for pickaxe-able directions
                                // (with the minimum distance of 3 - higher upgrades might be able to do further though)
                                Vector2 dir = Vector2.Zero;
                                switch ( location.getTileIndexAt( ( int ) tv.X, ( int ) tv.Y, "Buildings" ) )
                                {
                                    case 208: dir = new Vector2( 0, -1 ); break;
                                    case 236: dir = new Vector2( -1, 0 ); break;
                                    case 238: dir = new Vector2( 1, 0 ); break;
                                    case 266: dir = new Vector2( 0, 1 ); break;
                                }

                                if ( dir != Vector2.Zero )
                                {
                                    Vector2? found = null;
                                    for ( int i = 1; i <= 3; ++i )
                                    {
                                        Vector2 sd = tv + dir * i;
                                        int td = location.getTileIndexAt( ( int ) sd.X, ( int ) sd.Y, "Buildings" );
                                        if ( td == LunarLocation.SpaceTileIndex )
                                        {
                                            found = sd + dir;
                                        }
                                    }

                                    if ( found.HasValue &&
                                         location.getTileIndexAt( ( int ) found.Value.X, ( int ) found.Value.Y, "Buildings" ) != -1 &&
                                         location.getTileIndexAt( ( int )( found.Value.X + dir.X ), ( int )( found.Value.Y + dir.Y ), "Back" ) == 237 &&
                                         checkTiles.Contains( found.Value ) )
                                    {
                                        isle.tiles.Add( found.Value );
                                        pending.Enqueue( found.Value );
                                        checkTiles.Remove( found.Value );
                                    }
                                }
                            }
                        }
                    };

                    checkAdjacency( new Vector2( curr.X - 1, curr.Y + 0 ) );
                    checkAdjacency( new Vector2( curr.X + 1, curr.Y + 0 ) );
                    checkAdjacency( new Vector2( curr.X + 0, curr.Y - 1 ) );
                    checkAdjacency( new Vector2( curr.X + 0, curr.Y + 1 ) );
                }

                if ( isle.tiles.Count > 5 )
                    islands.Add( isle );
            }

            islands.Sort( (a, b) => b.tiles.Count - a.tiles.Count );
            Log.Debug( islands.Count + " islands (" + chunkSpots.Count + " chunks): " );
            foreach ( var isle in islands )
                Log.Debug( "\t" + isle.tiles.Count + " w/ " + isle.spots.Count + " spots, first @ " + isle.tiles[ 0 ] );

            var bigIslands = islands.ToList();
            bigIslands.RemoveAll( isle => isle.tiles.Count < 100 );

            bool hubIsland = false;
            if ( bigIslands.Count > 1 )
            {
                if ( rand.NextDouble() < 0.5 )
                {
                    // Chain of teleportable islands
                    for ( int i = 0; i < bigIslands.Count - 1; ++i )
                    {
                        int ix = rand.Next( bigIslands[ i ].spots.Count );
                        int iy = rand.Next( bigIslands[ i + 1 ].spots.Count );

                        Vector2 x = bigIslands[ i ].spots[ ix ];
                        bigIslands[ i ].spots.RemoveAt( ix );
                        Vector2 y = bigIslands[ i + 1 ].spots[ iy ];
                        bigIslands[ i + 1 ].spots.RemoveAt( iy );

                        PlaceRandomTeleporterPair( location, rand, ( int ) x.X, ( int ) x.Y, ( int ) y.X, ( int ) y.Y, canInactive: false );
                    }
                }
                else
                {
                    // Central island with all teleporters
                    hubIsland = true;
                    for ( int i = 1; i < bigIslands.Count; ++i )
                    {
                        int ix = rand.Next( bigIslands[ 0 ].spots.Count );
                        int iy = rand.Next( bigIslands[ i ].spots.Count );

                        Vector2 x = bigIslands[ 0 ].spots[ ix ];
                        bigIslands[ 0 ].spots.RemoveAt( ix );
                        Vector2 y = bigIslands[ i ].spots[ iy ];
                        bigIslands[ i ].spots.RemoveAt( iy );

                        PlaceRandomTeleporterPair( location, rand, ( int ) x.X, ( int ) x.Y, ( int ) y.X, ( int ) y.Y );
                    }
                }
            }

            // Place level warps
            for (int i = 0; i < 10; ++i)
            {
                int bi = rand.Next( bigIslands.Count );
                if ( hubIsland )
                    bi = 0;
                if (bigIslands[bi].spots.Count <= 0) continue;
                int bis = rand.Next( bigIslands[ bi ].spots.Count );
                warpFromPrev = bigIslands[ bi ].spots[ bis ];
                PlacePreviousWarp( location, ( int ) warpFromPrev.X, ( int ) warpFromPrev.Y - 1 );
                if (bigIslands[bi].spots.Count > 1)
                    bigIslands[ bi ].spots.RemoveAt( bis );

                break;
            }
            for (int i = 0; i < 10; ++i)
            {
                int bi = rand.Next( bigIslands.Count );
                int bis = rand.Next( bigIslands[ bi ].spots.Count );
                if (bigIslands[bi].spots.Count <= 0) continue;
                warpFromNext = bigIslands[ bi ].spots[ bis ];
                PlaceNextWarp( location, ( int ) warpFromNext.X, ( int ) warpFromNext.Y - 1 );
                if (bigIslands[bi].spots.Count > 1)
                    bigIslands[ bi ].spots.RemoveAt( bis );

                break;
            }

            // Place features
            int[] pieceSizes = new int[] { 3, 5, 11 };
            Map[] pieceMaps = new Map[ pieceSizes.Length ];
            for ( int i = 0; i < pieceSizes.Length; ++i )
            {
                pieceMaps[ i ] = Game1.game1.xTileContent.Load<Map>( Mod.instance.Helper.ModContent.GetInternalAssetName( "assets/maps/MoonPieces" + pieceSizes[ i ] + ".tmx" ).BaseName );
            }
            int featureCounter = 0;
            List<Vector2> stoneSpots = new();
            foreach ( var island in islands )
            {
                foreach ( var spot in island.spots )
                {
                    if ( rand.NextDouble() < 0.3 )
                        continue;

                    int sx = ( int ) spot.X;
                    int sy = ( int ) spot.Y;

                    int size = -1;
                    for ( int i = -1; i < pieceSizes.Length; ++i, ++size )
                    {
                        //i = 2;size = 2;
                        int d = -1; // Most features have a border of no tiles anyways
                        if ( i >= 0 )
                            d += pieceSizes[ i ] / 2 + 1;

                        if ( d == -1 )
                            d = 0;

                        bool allEmpty = true;
                        for ( int ix = -d; ix <= d; ++ix )
                        {
                            for ( int iy = -d; iy <= d; ++iy )
                            {
                                if ( !( location.getTileIndexAt( sx + ix, sy + iy, "Back" ) == 237 &&
                                        location.getTileIndexAt( sx + ix, sy + iy, "Buildings" ) == -1 &&
                                        location.getTileIndexAt( sx + ix, sy + iy, "Front" ) == -1 &&
                                        !location.netObjects.ContainsKey( new Vector2( sx + ix, sy + iy ) ) ) )
                                {
                                    allEmpty = false;
                                    break;
                                }
                            }

                            if ( !allEmpty )
                                break;
                        }

                        if ( !allEmpty )
                        {
                            --size;
                            break;
                        }
                    }

                    if ( size == -2 )
                        continue;

                    // Chance of downsizing even if a larger piece works here
                    //if(false)
                    while ( size >= 0 && ( size >= pieceSizes.Length || rand.NextDouble() < Math.Min( 0.5, 0.1 + size * 0.05 ) ) )
                        --size;
                    //while ( size >= pieceSizes.Length ) --size;
                    int actualSize = size >= 0 ? pieceSizes[ size ] : 1;
                    if ( actualSize == 1 )
                    {
                        stoneSpots.Add( new Vector2( sx, sy ) );
                    }
                    else
                    {
                        if ( actualSize >= 11 )
                            Log.Debug( "Placing size " + actualSize + " feature @ " + sx + " " + sy );
                        Map pieceMap = pieceMaps[ size ];
                        int entriesW = pieceMap.Layers[ 0 ].LayerWidth / actualSize;
                        int entries = entriesW * pieceMap.Layers[ 0 ].LayerHeight / actualSize;
                        int entry = rand.Next( entries );
                        //entry = 6;
                        int sourceX = entry % entriesW * actualSize;
                        int sourceY = entry / entriesW * actualSize;

                        Rectangle sr = new Rectangle( sx - actualSize / 2, sy - actualSize / 2, actualSize, actualSize );
                        location.ApplyMapOverride( pieceMap, "feature_" + featureCounter++, new Rectangle( sourceX, sourceY, actualSize, actualSize ), sr );

                        for ( int ix = sr.X; ix <= sr.Right; ++ix )
                        {
                            for ( int iy = sr.Y; iy <= sr.Bottom; ++iy )
                            {
                                int path = location.getTileIndexAt( ix, iy, "Paths" );
                                if ( path == 7 ) // chest
                                {
                                    if ( Game1.IsMasterGame )
                                    {
                                        double chance = 1;
                                        string chanceProp = location.doesTileHaveProperty( ix, iy, "Chance", "Paths" );
                                        if ( chanceProp != null && double.TryParse( chanceProp, out double chanceVal ) )
                                            chance = chanceVal;

                                        if ( chance == 1 || rand.NextDouble() < chance )
                                            PlaceChestAt( location, rand, ix, iy, location.doesTileHaveProperty( ix, iy, "ChestType", "Paths" ) == "1" );
                                    }
                                }
                                else if ( path == 27 ) // barrel
                                {
                                    if ( Game1.IsMasterGame )
                                    {
                                        double chance = 1;
                                        string chanceProp = location.doesTileHaveProperty( ix, iy, "Chance", "Paths" );
                                        if ( chanceProp != null && double.TryParse( chanceProp, out double chanceVal ) )
                                            chance = chanceVal;

                                        if ( chance == 1 || rand.NextDouble() < chance )
                                            PlaceBreakableAt( location, rand, ix, iy );
                                    }
                                }

                                string prop = location.doesTileHaveProperty( ix, iy, "Action", "Buildings" ) ?? "";
                                if ( string.IsNullOrEmpty( prop ) )
                                    prop = location.doesTileHaveProperty( ix, iy, "TouchAction", "Back" ) ?? "";
                                if ( prop.StartsWith( "LunarLock " ) || prop.StartsWith( "LunarDoor " ) || prop.StartsWith( "LunarCave " ) )
                                {
                                    prop = prop.Replace( "{l}", location.level.Value.ToString() );
                                    prop = prop.Replace( "{c}", ( featureCounter - 1 ).ToString() );
                                    location.setTileProperty( ix, iy, prop.StartsWith( "LunarCave " ) ? "Back" : "Buildings", prop.StartsWith( "LunarCave " ) ? "TouchAction" : "Action", prop );

                                    location.lunarDoors.Add( featureCounter - 1, new Vector2( ix, iy ) );

                                    if ( prop.StartsWith( "LunarLock " ) )
                                    {
                                        TileSheet ts_ = location.map.GetTileSheet( "zz_volcano_dungeon" );
                                        if ( ts_ == null )
                                        {
                                            ts_ = new TileSheet( location.map, "Maps/Mines/volcano_dungeon", new xTile.Dimensions.Size( 16, 36 ), new xTile.Dimensions.Size( 16, 16 ) );
                                            ts_.Id = "zz_volcano_dungeon";
                                            location.map.AddTileSheet( ts_ );
                                            location.map.LoadTileSheets( Game1.mapDisplayDevice );
                                        }
                                        //int next = island.spots.IndexOf( spot ) + 1; // This makes them always close to the building
                                        int next = island.spots.IndexOf( spot ) + rand.Next( island.spots.Count - island.spots.IndexOf( spot ) - 1 ) + 1;
                                        Vector2 nextSpot = island.spots[ next ];
                                        Log.Debug( "Placing switch @ " + nextSpot );
                                        var t = new StaticTile( location.map.GetLayer( "Back" ), ts_, BlendMode.Alpha, 496 );
                                        t.Properties.Add( "TouchAction", "LunarSwitch " + ix + " " + iy );
                                        location.map.GetLayer( "Back" ).Tiles[ ( int ) nextSpot.X, ( int ) nextSpot.Y ] = t;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ( Game1.IsMasterGame )
            {
                foreach ( var spot in stoneSpots )
                {
                    int sx = ( int ) spot.X;
                    int sy = ( int ) spot.Y;

                    if ( ( location.getTileIndexAt( sx, sy, "Back" ) == 237 &&
                            location.getTileIndexAt( sx, sy, "Buildings" ) == -1 &&
                            location.getTileIndexAt( sx, sy, "Front" ) == -1 &&
                            !location.netObjects.ContainsKey( new Vector2( sx, sy ) ) ) )
                    {
                        PlaceMinableAt( location, rand, sx, sy );
                    }
                }

                foreach ( var isle in islands )
                {
                    foreach ( var spot in isle.spots )
                    {
                        if ( rand.NextDouble() < 0.65 )
                            continue;

                        Vector2 offset = new Vector2( rand.Next( 5 ) - 2, rand.Next( 5 ) - 2 );
                        int sx = ( int )( spot.X + offset.X );
                        int sy = ( int )( spot.Y + offset.Y );

                        if ( ( location.getTileIndexAt( sx, sy, "Back" ) == 237 &&
                                location.getTileIndexAt( sx, sy, "Buildings" ) == -1 &&
                                location.getTileIndexAt( sx, sy, "Front" ) == -1 &&
                                !location.netObjects.ContainsKey( new Vector2( sx, sy ) ) ) )
                        {
                            PlaceMonsterAt( location, rand, sx, sy );
                        }
                    }
                }
            }

            location.asteroidChance.Value = 0.1f + ( float ) rand.NextDouble() * 0.3f;

            location.loadLights();
        }
    }
}
