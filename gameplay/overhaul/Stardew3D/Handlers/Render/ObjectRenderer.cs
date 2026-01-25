using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Data;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class ObjectRenderer : ItemRenderer<ModelData, StardewValley.Object>
{
    public ObjectRenderer(StardewValley.Object obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new ObjectRenderData(ctx, this);
    }
}
