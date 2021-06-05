using System;
using StardewValley;

namespace SpaceShared.APIs
{
    public interface IApi
    {
        event EventHandler<Tool> ToolRushed;
        event EventHandler BuildingRushed;
    }
}
