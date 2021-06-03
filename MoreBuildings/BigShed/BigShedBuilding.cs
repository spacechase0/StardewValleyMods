using System.Collections.Generic;
using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using System.Reflection;

namespace MoreBuildings.BigShed
{
    public class BigShedBuilding : Building, ISaveElement
    {
        private static readonly BluePrint blueprint = new BluePrint("Shed2");

        public BigShedBuilding()
            : base(blueprint,Vector2.Zero)
        {
        }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new BigShedLocation();
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

            indoors.Value.map = Game1.content.Load<xTile.Map>("Maps\\Shed2");
            indoors.Value.updateWarps();
            updateInteriorWarps();
        }
    }
}
