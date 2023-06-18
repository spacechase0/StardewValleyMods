using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile;

namespace MoonMisadventures.Game.Locations.DungeonLevelGenerators
{
    public class CaveDungeonLevelGenerator : BaseDungeonLevelGenerator
    {
        public override void Generate( AsteroidsDungeon location, ref Vector2 warpFromPrev, ref Vector2 warpFromNext )
        {
            Random rand = new Random( location.genSeed.Value );
            location.isIndoorLevel = true;

            var caveMap = Game1.game1.xTileContent.Load<Map>( Mod.instance.Helper.ModContent.GetInternalAssetName("assets/maps/MoonDungeonCave.tmx").BaseName );

            int x = ( location.Map.Layers[ 0 ].LayerWidth - caveMap.Layers[ 0 ].LayerWidth ) / 2;
            int y = ( location.Map.Layers[ 0 ].LayerHeight - caveMap.Layers[ 0 ].LayerHeight ) / 2;

            location.ApplyMapOverride( caveMap, "actual_map", null, new Rectangle( x, y, caveMap.Layers[ 0 ].LayerWidth, caveMap.Layers[ 0 ].LayerHeight ) );

            var mp = Mod.instance.Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();
            long id = mp.getNewID();
            string type = rand.Next( 2 ) == 0 ? "Lunar Cow" : "Lunar Chicken";
            location.Animals.Add( id, new FarmAnimal(type, id, 0) { Position = new Vector2(x + caveMap.Layers[0].LayerWidth / 2, y + caveMap.Layers[0].LayerHeight / 2) * Game1.tileSize } );

            warpFromPrev = new Vector2( x + 6, y + 10 );
            location.warps.Add( new Warp( x + 6, y + 11, "Custom_MM_MoonAsteroidsDungeon" + location.level.Value / 100, 1, location.level.Value % 100, false ) );
        }
    }
}
