using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MoonMisadventures.Game.Items;
using StardewValley.Objects;
using xTile;

namespace MoonMisadventures.Game.Locations.DungeonLevelGenerators
{
    public class BossIslandDungeonLevelGenerator : BaseDungeonLevelGenerator
    {
        public override void Generate( AsteroidsDungeon location, ref Vector2 warpFromPrev, ref Vector2 warpFromNext )
        {
            Map place = Mod.instance.Helper.ModContent.Load< Map >( "assets/maps/MoonBossIsland.tmx" );
            int offsetX = ( location.Map.Layers[ 0 ].LayerWidth - place.Layers[ 0 ].LayerWidth ) / 2;
            int offsetY = ( location.Map.Layers[ 0 ].LayerHeight - place.Layers[ 0 ].LayerHeight ) / 2;

            location.ApplyMapOverride( place, "island_boss", new Rectangle( 0, 0, place.Layers[ 0 ].LayerWidth, place.Layers[ 0 ].LayerHeight ), new Rectangle( offsetX, offsetY, place.Layers[ 0 ].LayerWidth, place.Layers[ 0 ].LayerHeight ) );

            warpFromPrev = warpFromNext = new Vector2( 24 + offsetX, 43 + offsetY );

            PlaceNextWarp(location, offsetX + 23, offsetY + 17);

            {
                Vector2 position = new Vector2(offsetX + 24, offsetY + 24);
                Chest chest = new Chest(playerChest: false, position);
                chest.dropContents.Value = true;
                chest.synchronized.Value = true;
                chest.type.Value = "interactive";
                chest.SetBigCraftableSpriteIndex(227);
                chest.addItem(new StardewValley.Object(ItemIds.SoulSapphire, 1));
                chest.addItem(new Necklace(Necklace.Type.Lunar));
                if (location.netObjects.ContainsKey(position))
                    location.netObjects.Remove(position);
                location.netObjects.Add(position, chest);
            }

            location.PlaceSpaceTiles();
        }
    }
}
