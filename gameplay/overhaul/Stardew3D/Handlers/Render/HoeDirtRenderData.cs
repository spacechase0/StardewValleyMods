using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Network.NetEvents;
using StardewValley.TerrainFeatures;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class HoeDirtRenderData : RenderDataWithPlaceholder<ModelData, HoeDirt>
{
    public HoeDirtRenderData(RenderContext ctx, HoeDirtRenderer parent)
        : base( ctx, parent,
            parent.Object.Location == null ? 0 : parent.Object.Tile.ToPoint().X + parent.Object.Tile.ToPoint().Y * parent.Object.Location.Map.Layers[0].LayerWidth)
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (Parent.Object.crop != null)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(Parent.Object.crop))
                renderer?.Render(ctx);
        }
    }
}
