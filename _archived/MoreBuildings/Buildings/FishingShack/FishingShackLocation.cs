using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace MoreBuildings.Buildings.FishingShack
{
    [XmlType("Mods_spacechase0_FishingShackLocation")]
    public class FishingShackLocation : GameLocation
    {
        public FishingShackLocation()
            : base("Maps\\FishShack", "FishShack")
        {
            this.waterTiles = new bool[this.map.Layers[0].LayerWidth, this.map.Layers[0].LayerHeight];
            for (int xTile = 0; xTile < this.map.Layers[0].LayerWidth; ++xTile)
            {
                for (int yTile = 0; yTile < this.map.Layers[0].LayerHeight; ++yTile)
                {
                    if (this.doesTileHaveProperty(xTile, yTile, "Water", "Back") != null)
                    {
                        this.waterTiles[xTile, yTile] = true;
                    }
                }
            }
        }

        public override SObject getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        {
            int[] fish = new[] { 128, 129, 130, 131, 132, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 682, 698, 699, 700, 701, 702, 704, 705, 706, 707, 708, 734, 775, 795, 796 };

            return new SObject(Vector2.Zero, fish[Game1.random.Next(fish.Length)], 1);
            //return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, locationName);
        }
    }
}
