using System;

using Microsoft.Xna.Framework;

using StardewValley;

using xTile;

namespace MoonMisadventures.Game.Locations.DungeonLevelGenerators
{
    public class RoomDungeonLevelGenerator : BaseDungeonLevelGenerator
    {
        public override void Generate( AsteroidsDungeon location, ref Vector2 warpFromPrev, ref Vector2 warpFromNext )
        {
            Random rand = new Random( location.genSeed.Value );
            location.isIndoorLevel = true;

            var caveMap = Game1.game1.xTileContent.Load<Map>( Mod.instance.Helper.ModContent.GetInternalAssetName("assets/maps/MoonDungeonRoom.tmx").BaseName );

            int x = ( location.Map.Layers[ 0 ].LayerWidth - caveMap.Layers[ 0 ].LayerWidth ) / 2;
            int y = ( location.Map.Layers[ 0 ].LayerHeight - caveMap.Layers[ 0 ].LayerHeight ) / 2;

            location.ApplyMapOverride( caveMap, "actual_map", null, new Rectangle( x, y, caveMap.Layers[ 0 ].LayerWidth, caveMap.Layers[ 0 ].LayerHeight ) );

            for ( int ix = x + 4; ix <= x + 14; ++ix )
            {
                for ( int iy = y + 4; iy <= y + 8; ++iy )
                {
                    if ( rand.NextDouble() < 0.175 )
                        PlaceBreakableAt( location, rand, ix, iy );
                }
            }

            PlaceChestAt( location, rand, x + caveMap.Layers[ 0 ].LayerWidth / 2 - 1, y + caveMap.Layers[ 0 ].LayerHeight / 2, rand.Next( 3 ) == 0 );

            warpFromPrev = new Vector2( x + 9, y + 10 );
            location.warps.Add( new Warp( x + 9, y + 11, "Custom_MM_MoonAsteroidsDungeon" + location.level.Value / 100, 1, location.level.Value % 100, false ) );
        }
    }
}
