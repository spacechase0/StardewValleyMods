using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using xTile;

namespace MisappliedPhysicalities.Game.Locations
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_LunarLocation" )]
    public class LunarLocation : GameLocation
    {
        public const int SpaceTileIndex = 608;

        public LunarLocation() { }
        public LunarLocation( IContentHelper content, string mapPath, string mapName )
        :   base( content.GetActualAssetKey( "assets/" + mapPath + ".tmx" ), mapName )
        {
            placeSpaceTiles();
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            Game1.background = new SpaceBackground();
        }
        public override void cleanupBeforePlayerExit()
        {
            Game1.background = null;
        }

        public override string checkForBuriedItem( int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who )
        {
            Random r = new Random( xLocation * 3000 + yLocation + ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed + Name.GetDeterministicHashCode() );
            if ( r.NextDouble() < 0.03 )
            {
                Game1.createObjectDebris( 424 /* cheese */, xLocation, yLocation, this );
            }
            return base.checkForBuriedItem( xLocation, yLocation, explosion, detectOnly, who );
        }

        public override bool performToolAction( Tool t, int tileX, int tileY )
        {
            if ( t is Pickaxe )
            {
                int tile = getTileIndexAt( tileX, tileY, "Buildings" );

                Vector2 dir = Vector2.Zero;
                switch ( tile )
                {
                    case 208: dir = new Vector2( 0, -1 ); break;
                    case 236: dir = new Vector2( -1, 0 ); break;
                    case 238: dir = new Vector2( 1, 0 ); break;
                    case 266: dir = new Vector2( 0, 1 ); break;
                }

                SpaceShared.Log.Debug( "meow? " + tile + " " + dir );

                if ( dir != Vector2.Zero )
                {
                    int i = 1;
                    bool placedTiles = false;
                    for ( ; i <= 3; ++i )
                    {
                        int ii = i;
                        var newTile = new Vector2( tileX, tileY ) + dir * i;
                        if ( /*getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Back" ) == -1 &&
                             */getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" ) == SpaceTileIndex )
                        {
                            placedTiles = true;
                            DelayedAction.functionAfterDelay( () =>
                            {
                                int tileIndex = Game1.random.Next( 3 ) * 4 + ( dir.Y != 0 ? 0 : 12 );
                                List< int > tiles = new List<int> { tileIndex, tileIndex + 1, tileIndex + 2, tileIndex + 3 };
                                int shift = Game1.random.Next( tiles.Count );
                                int[] tilesArr = tiles.OrderBy( x => ( ( tiles.IndexOf( x ) + shift ) % tiles.Count ) ).ToArray(); // Randomize where it starts animating from

                                Game1.playSound( "boulderBreak" );
                                removeTile( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" );
                                setAnimatedMapTile( ( int ) newTile.X, ( int ) newTile.Y, tilesArr, 450, "Back1", null, 0 );
                            }, 100 * ii );
                        }
                        else break;
                    }

                    if ( placedTiles )
                    {
                        setMapTileIndex( tileX, tileY, tile, "Back", 2 );
                        removeTile( tileX, tileY, "Buildings" );

                        var newTile = new Vector2( tileX, tileY ) + dir * i;

                        int newBtile = getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" );
                        if ( newBtile != -1 && newBtile != SpaceTileIndex )
                        {
                            DelayedAction.functionAfterDelay( () =>
                            {
                                setMapTileIndex( ( int ) newTile.X, ( int ) newTile.Y, getTileIndexAt( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" ), "Back", 2 );
                                removeTile( ( int ) newTile.X, ( int ) newTile.Y, "Buildings" );
                            }, 100 * ( i - 1 ) );
                        }

                        return true;
                    }
                }
            }

            return base.performToolAction( t, tileX, tileY );
        }

        protected void placeSpaceTiles()
        {
            if ( map.GetLayer( "Back1" ) == null )
            {
                map.AddLayer( new xTile.Layers.Layer( "Back1", Map, Map.Layers[ 0 ].LayerSize, Map.Layers[ 0 ].TileSize ) );
            }
            for ( int ix = 0; ix < Map.Layers[ 0 ].LayerWidth; ++ix )
            {
                for ( int iy = 0; iy < Map.Layers[ 0 ].LayerHeight; ++iy )
                {
                    if ( ( getTileIndexAt( ix, iy, "Back" ) == -1 || doesTileHaveProperty( ix, iy, "CanSpace", "Back" ) == "T" ) && getTileIndexAt( ix, iy, "Buildings" ) == -1 )
                    {
                        setMapTileIndex( ix, iy, SpaceTileIndex, "Buildings", 2 );
                    }
                }
            }
        }
    }
}
