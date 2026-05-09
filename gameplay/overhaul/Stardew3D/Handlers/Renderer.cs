using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers;
public abstract class Renderer : IRenderHandler
{
    public object Object { get; }
    public string QualifiedId { get; }
    public ModelData BaseModelData { get; }

    public Renderer(object obj)
    {
        Object = obj;
        QualifiedId = obj.GetExtendedQualifiedId();

        foreach (var entry in obj.GetExtendedQualifiedIds())
        {
            BaseModelData = ModelData.Get(entry);
            if (BaseModelData != null)
            {
                QualifiedId = entry;
                break;
            }
        }
    }

    public abstract void Render(RenderContext ctx);
}

public class RendererFor<TData, TObject> : Renderer
    where TData : ModelData
{
    public TData ModelData => BaseModelData as TData;
    public new TObject Object { get; }

    protected ConditionalWeakTable<RenderBatcher, RenderDataBase> renderData = new();

    public RendererFor(TObject obj)
        : base(obj)
    {
        Object = obj;
    }

    public override void Render(RenderContext ctx)
    {
        if (ctx.Reset)
            renderData.Clear();

        RenderDataBase data = renderData.GetValue(ctx.WorldBatch, key => CreateInitialRenderData(ctx));
        data.Update(ctx);
    }

    protected virtual RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new RenderData<RendererFor<TData, TObject>>(ctx, this);
    }
}
