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

namespace MoreBuildings.SpookyShed
{
    public class SpookyShedBuilding : Building, ISaveElement
    {
        private static BluePrint blueprint = new BluePrint("SpookyShed");

        public SpookyShedBuilding()
            : base(blueprint, Vector2.Zero)
        {
            indoors.Value = new SpookyShedLocation();
        }
        public object getReplacement()
        {
            Building building = new Building(new BluePrint("Shed"), new Vector2(tileX, tileY));
            building.indoors.Value = indoors.Value;
            building.daysOfConstructionLeft.Value = daysOfConstructionLeft.Value;
            building.tilesHigh.Value = tilesHigh.Value;
            building.tilesWide.Value = tilesWide.Value;
            building.tileX.Value = tileX.Value;
            building.tileY.Value = tileY.Value;
            return building;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Building building = (Building)replacement;
            indoors.Value = building.indoors.Value;
            daysOfConstructionLeft.Value = building.daysOfConstructionLeft.Value;
            tileX.Value = building.tileX.Value;
            tileY.Value = building.tileY.Value;
        }
    }
}
