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
using StardewValley.Objects;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class TelevisionRenderer : ItemRenderer<ModelData, TV>
{
    public TelevisionRenderer(TV obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new TelevisionRenderData(ctx, this);
    }
}
