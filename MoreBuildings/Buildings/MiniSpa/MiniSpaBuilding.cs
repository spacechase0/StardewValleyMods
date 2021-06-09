using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Buildings;

namespace MoreBuildings.Buildings.MiniSpa
{
    public class MiniSpaBuilding : Building, ISaveElement
    {
        private static readonly BluePrint blueprint = new BluePrint("MiniSpa");

        public MiniSpaBuilding()
            : base(blueprint, Vector2.Zero) { }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new MiniSpaLocation();
        }

        public object getReplacement()
        {
            Mill building = new Mill(new BluePrint("Mill"), new Vector2(this.tileX, this.tileY));
            building.indoors.Value = this.indoors.Value;
            building.daysOfConstructionLeft.Value = this.daysOfConstructionLeft.Value;
            building.tileX.Value = this.tileX.Value;
            building.tileY.Value = this.tileY.Value;
            return building;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Mill building = (Mill)replacement;
            this.indoors.Value = building.indoors.Value;
            this.daysOfConstructionLeft.Value = building.daysOfConstructionLeft.Value;
            this.tileX.Value = building.tileX.Value;
            this.tileY.Value = building.tileY.Value;

            this.indoors.Value.map = Game1.content.Load<xTile.Map>("Maps\\MiniSpa");
            this.indoors.Value.updateWarps();
            this.updateInteriorWarps();
        }
    }
}
