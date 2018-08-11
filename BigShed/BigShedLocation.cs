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
using PyTK.CustomElementHandler;

namespace MoreBuildings.BigShed
{
    public class BigShedLocation : Shed, ISaveElement
    {
        public BigShedLocation()
        :   base("Maps\\Shed2", "Shed2" )
        {
        }

        public override List<Rectangle> getFloors()
        {
            return new List<Rectangle> { new Rectangle(1, 3, 21, 20) };
        }

        
        public override List<Rectangle> getWalls()
        {
            return new List<Rectangle>{ new Rectangle(1, 1, 21, 3) };
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public object getReplacement()
        {
            Shed shed = new Shed();
            foreach (Vector2 key in objects.Keys)
                shed.objects.Add(key, objects[key]);
            foreach (Vector2 key in terrainFeatures.Keys)
                shed.terrainFeatures.Add(key, terrainFeatures[key]);
            return shed;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Shed shed = (Shed)replacement;
            foreach (Vector2 key in shed.objects.Keys)
                objects.Add(key, shed.objects[key]);
            foreach (Vector2 key in terrainFeatures.Keys)
                terrainFeatures.Add(key, shed.terrainFeatures[key]);
        }
    }
}
