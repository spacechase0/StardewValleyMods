using System;
using StardewValley;

namespace SpaceShared.APIs
{
    internal interface IApi
    {
        event EventHandler<Tool> ToolRushed;
        event EventHandler BuildingRushed;
    }
}
