using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Network.NetEvents;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ToolRenderData : RenderDataWithPlaceholder<ModelData, StardewValley.Tool>
{
    private int nonInstanced = -1;

    public ToolRenderData(RenderContext ctx, ToolRenderer parent)
        : base( ctx, parent)
    {
    }
}
