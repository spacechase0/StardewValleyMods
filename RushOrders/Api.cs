using SpaceShared;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (ToolRushed == null)
                return;
            Util.invokeEvent("RushOrders.Api.ToolRushed", ToolRushed.GetInvocationList(), null, tool);
        }

        public event EventHandler BuildingRushed;
        internal void InvokeBuildingRushed()
        {
            Log.trace("Event: BuildingRushed");
            if (BuildingRushed == null)
                return;
            Util.invokeEvent("RushOrders.Api.BuildingRushed", BuildingRushed.GetInvocationList(), null);
        }
}
}
