using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile;
using MoreBuildings.SpookyShed;
using StardewValley.Monsters;
using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;
using PyTK.CustomElementHandler;

namespace MoreBuildings.MiniSpa
{
    public class MiniSpaLocation : GameLocation, ISaveElement
    {
        public MiniSpaLocation()
        :   base( "Maps\\MiniSpa", "MiniSpa")
        {
        }

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
            if (uniqueName.Value != null)
                data.Add("u", uniqueName.Value);

            return data;
        }

        public object getReplacement()
        {
            Shed shed = new Shed("Maps\\MiniSpa", "MiniSpa");
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
