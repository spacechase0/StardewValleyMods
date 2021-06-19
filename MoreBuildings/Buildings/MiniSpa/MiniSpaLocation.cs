using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;

namespace MoreBuildings.Buildings.MiniSpa
{
    public class MiniSpaLocation : GameLocation, ISaveElement
    {
        public MiniSpaLocation()
            : base("Maps\\MiniSpa", "MiniSpa") { }

        protected override void resetLocalState()
        {
            Game1.player.changeIntoSwimsuit();
            Game1.player.swimming.Value = true;
        }

        public override int getExtraMillisecondsPerInGameMinuteForThisLocation()
        {
            return 7000;
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
            Shed shed = new Shed("Maps\\MiniSpa", "MiniSpa");
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
