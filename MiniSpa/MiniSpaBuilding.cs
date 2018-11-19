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
using System.Reflection;

namespace MoreBuildings.MiniSpa
{
    public class MiniSpaBuilding : Building, ISaveElement
    {
        private static BluePrint blueprint = new BluePrint("MiniSpa");

        public MiniSpaBuilding()
            : base(blueprint, Vector2.Zero)
        {
        }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new MiniSpaLocation();
        }

        public object getReplacement()
        {
            Mill building = new Mill(new BluePrint("Mill"), new Vector2(tileX, tileY));
            building.indoors.Value = indoors.Value;
            building.daysOfConstructionLeft.Value = daysOfConstructionLeft.Value;
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
            Mill building = (Mill)replacement;
            indoors.Value = building.indoors.Value;
            daysOfConstructionLeft.Value = building.daysOfConstructionLeft.Value;
            tileX.Value = building.tileX.Value;
            tileY.Value = building.tileY.Value;

            indoors.Value.map = Game1.content.Load<xTile.Map>("Maps\\MiniSpa");
            indoors.Value.GetType().GetMethod("updateWarps", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(indoors.Value, new object[] { });
            updateInteriorWarps();
        }
    }
}
