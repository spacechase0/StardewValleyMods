using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Buildings;
using xTile;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using PyTK.CustomElementHandler;

namespace MoreBuildings.BigShed
{
    public class BigShedBuilding : Building, ISaveElement
    {
        public object getReplacement()
        {
            return new Shed();
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
        }
    }
}
