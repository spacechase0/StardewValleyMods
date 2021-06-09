using System;
using SpaceShared;
using StardewValley;

namespace RushOrders
{
    public interface IApi
    {
        event EventHandler<Tool> ToolRushed;
        event EventHandler BuildingRushed;
    }

    public class Api : IApi
    {
        public event EventHandler<Tool> ToolRushed;
        internal void InvokeToolRushed(Tool tool)
        {
            Log.trace("Event: ToolRushed");
            if (this.ToolRushed == null)
                return;
            Util.invokeEvent("RushOrders.Api.ToolRushed", this.ToolRushed.GetInvocationList(), null, tool);
        }

        public event EventHandler BuildingRushed;
        internal void InvokeBuildingRushed()
        {
            Log.trace("Event: BuildingRushed");
            if (this.BuildingRushed == null)
                return;
            Util.invokeEvent("RushOrders.Api.BuildingRushed", this.BuildingRushed.GetInvocationList(), null);
        }
    }
}
