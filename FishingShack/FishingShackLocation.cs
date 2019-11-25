using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;
using PyTK.CustomElementHandler;

namespace MoreBuildings.FishingShack
{
    public class FishingShackLocation : GameLocation, ISaveElement
    {
        public FishingShackLocation()
        :   base( "Maps\\FishShack", "FishShack")
        {
            this.waterTiles = new bool[map.Layers[0].LayerWidth, map.Layers[0].LayerHeight];
            for (int xTile = 0; xTile < map.Layers[0].LayerWidth; ++xTile)
            {
                for (int yTile = 0; yTile < map.Layers[0].LayerHeight; ++yTile)
                {
                    if (this.doesTileHaveProperty(xTile, yTile, "Water", "Back") != null)
                    {
                        this.waterTiles[xTile, yTile] = true;
                    }
                }
            }
        }

        public override SObject getFish(float millisecondsAfterNibble, int bait, int waterDepth, StardewValley.Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        {
            var fish = new int[] { 128, 129, 130, 131, 132, 136, 137, 1338, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 682, 698, 699, 700, 701, 702, 704, 705, 706, 707, 708, 734, 775, 795, 796 };

            return new SObject(Vector2.Zero, fish[ Game1.random.Next( fish.Length ) ], 1);
            //return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, locationName);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var data = new Dictionary<string, string>();
            if (uniqueName.Value != null)
                data.Add("u", uniqueName.Value);

            return data;
        }

        public object getReplacement()
        {
            Shed shed = new Shed("Maps\\FishShack", "FishShack");
            foreach (Vector2 key in objects.Keys)
                shed.objects.Add(key, objects[key]);
            foreach (Vector2 key in terrainFeatures.Keys)
                shed.terrainFeatures.Add(key, terrainFeatures[key]);

            return shed;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Shed shed = (Shed)replacement;

            if (additionalSaveData.ContainsKey("u"))
                uniqueName.Value = additionalSaveData["u"];

            foreach (Vector2 key in shed.objects.Keys)
                objects.Add(key, shed.objects[key]);
            foreach (Vector2 key in terrainFeatures.Keys)
                terrainFeatures.Add(key, shed.terrainFeatures[key]);
        }
    }
}
