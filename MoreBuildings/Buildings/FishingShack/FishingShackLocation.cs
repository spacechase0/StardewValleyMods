using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using SObject = StardewValley.Object;

namespace MoreBuildings.Buildings.FishingShack
{
    public class FishingShackLocation : GameLocation, ISaveElement
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
            int[] fish = new[] { 128, 129, 130, 131, 132, 136, 137, 1338, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 682, 698, 699, 700, 701, 702, 704, 705, 706, 707, 708, 734, 775, 795, 796 };

            return new SObject(Vector2.Zero, fish[Game1.random.Next(fish.Length)], 1);
            //return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, locationName);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var data = new Dictionary<string, string>();
            if (this.uniqueName.Value != null)
                data.Add("u", this.uniqueName.Value);

            return data;
        }

        public object getReplacement()
        {
            Shed shed = new Shed("Maps\\FishShack", "FishShack");
            foreach (Vector2 key in this.objects.Keys)
                shed.objects.Add(key, this.objects[key]);
            foreach (Vector2 key in this.terrainFeatures.Keys)
                shed.terrainFeatures.Add(key, this.terrainFeatures[key]);

            return shed;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Shed shed = (Shed)replacement;

            if (additionalSaveData.ContainsKey("u"))
                this.uniqueName.Value = additionalSaveData["u"];

            foreach (Vector2 key in shed.objects.Keys)
                this.objects.Add(key, shed.objects[key]);
            foreach (Vector2 key in this.terrainFeatures.Keys)
                this.terrainFeatures.Add(key, shed.terrainFeatures[key]);
        }
    }
}
