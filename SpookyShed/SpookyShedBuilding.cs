using System.Collections.Generic;
using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using System.Reflection;

namespace MoreBuildings.SpookyShed
{
    public class SpookyShedBuilding : Building, ISaveElement
    {
        private static readonly BluePrint blueprint = new BluePrint("SpookyShed");

        public SpookyShedBuilding()
            : base(blueprint, Vector2.Zero)
        {
        }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new SpookyShedLocation();
        }

        public object getReplacement()
        {
            Mill building = new Mill(new BluePrint("Mill"), new Vector2(tileX, tileY));
            building.indoors.Value = indoors.Value;
            building.daysOfConstructionLeft.Value = daysOfConstructionLeft.Value;
            building.tileX.Value = tileX.Value;
            building.tileY.Value = tileY.Value;
            building.humanDoor.Value = humanDoor.Value;
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

            indoors.Value.map = Game1.content.Load<xTile.Map>("Maps\\SpookyShed");
            indoors.Value.GetType().GetMethod("updateWarps", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(indoors.Value, new object[] { });
            updateInteriorWarps();
        }
    }
}
