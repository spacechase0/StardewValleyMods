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
                break;
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

#if true
public abstract class RendererWithPlaceholder<TData, TObject> : RendererFor<TData, TObject>
    where TData : ModelData
{
    public struct PlaceholderData
    {
        public Texture2D Texture { get; set; }
        public Rectangle TextureRegion { get; set; }
        public Vector2 DisplaySize => DisplaySizeOverride ?? DefaultDisplaySize;
        public Vector2 DefaultDisplaySize => new Vector2(TextureRegion.Width / 16f, TextureRegion.Height / 16f) * DefaultDisplaySizeScale;
        public Vector2? DisplaySizeOverride { get; set; }
        public float DefaultDisplaySizeScale { get; set; } = 1;
        public Vector3 Offset => OffsetOverride ?? DefaultOffset;
        public Vector3 DefaultOffset => new(0, DisplaySize.Y / 2, 0);
        public Vector3? OffsetOverride { get; set; }
        public Color Color { get; set; } = Color.White;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        public bool Billboard { get; set; } = true;
        public Matrix OrientationIfNotBillboard { get; set; } = Matrix.Identity;

        public Func<bool> DisplayCondition { get; set; } = static () => true;

        public PlaceholderData() { }
    }

    public abstract PlaceholderData[] Placeholders{ get; }

    public RendererWithPlaceholder(TObject obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new RenderDataWithPlaceholder<TData, TObject>(ctx, this);
    }
}
#endif
