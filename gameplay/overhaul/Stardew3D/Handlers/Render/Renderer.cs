using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Data;
using Stardew3D.Handlers;
using Stardew3D.Rendering;
using StardewValley.Mods;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public abstract class Renderer : IRenderHandler
{
    public string QualifiedId { get; }
    public ModelData BaseModelData { get; }

    public Renderer(string qualifiedId)
    {
        QualifiedId = qualifiedId;
        BaseModelData = ModelData.Get(QualifiedId);
    }

    public abstract void Render(RenderContext ctx);
}

public abstract class RendererFor<TData, TObject> : Renderer
    where TData : ModelData
{
    public TData ModelData => BaseModelData as TData;
    public TObject Object { get; }

    protected ConditionalWeakTable<RenderBatcher, RenderDataBase> renderData = new();

    public RendererFor(string qualifiedId, TObject obj)
        : base(qualifiedId)
    {
        Object = obj;
    }

    public override void Render(RenderContext ctx)
    {
        RenderDataBase data = renderData.GetValue(ctx.WorldBatch, key => CreateInitialRenderData(ctx));
        data.Update(ctx);
    }

    protected abstract RenderDataBase CreateInitialRenderData(RenderContext ctx);
}

public class GenericRenderer<TData, TObject> : RendererFor<TData, TObject>
    where TData : ModelData
{
    public GenericRenderer(string qualifiedId, TObject obj)
        : base(qualifiedId, obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new RenderData<GenericRenderer<TData, TObject>>(ctx, this);
    }
}
