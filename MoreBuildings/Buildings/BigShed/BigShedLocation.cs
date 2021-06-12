using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Locations;

namespace MoreBuildings.Buildings.BigShed
{
    public class BigShedLocation : DecoratableLocation, ISaveElement
    {
        public BigShedLocation()
            : base("Maps\\Shed2_", "Shed2") { }

        public override List<Rectangle> getFloors()
        {
            return new() { new Rectangle(1, 3, 21, 20) };
        }


        public override List<Rectangle> getWalls()
        {
            return new() { new Rectangle(1, 1, 21, 3) };
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
            Shed shed = new Shed("Maps\\Shed2", "Shed2");
            foreach (Vector2 key in this.objects.Keys)
                shed.objects.Add(key, this.objects[key]);
            foreach (Vector2 key in this.terrainFeatures.Keys)
                shed.terrainFeatures.Add(key, this.terrainFeatures[key]);

            return shed;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Shed shed = (Shed)replacement;

            if (additionalSaveData.TryGetValue("u", out string savedName))
                this.uniqueName.Value = savedName;

            foreach (Vector2 key in shed.objects.Keys)
                this.objects.Add(key, shed.objects[key]);
            foreach (Vector2 key in this.terrainFeatures.Keys)
                this.terrainFeatures.Add(key, shed.terrainFeatures[key]);
        }
    }
}
