using SpaceShared;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceShared.APIs
{
    public interface IApi
    {
        event EventHandler<Tool> ToolRushed;
        event EventHandler BuildingRushed;
    }
}
