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
using StardewValley;
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

public class RendererFor<TData, TObject> : Renderer
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

    protected virtual RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new RenderData<RendererFor<TData, TObject>>(ctx, this);
    }
}

public abstract class RendererWithPlaceholder<TData, TObject> : RendererFor<TData, TObject>
    where TData : ModelData
{
    public struct PlaceholderData
    {
        public Texture2D Texture { get; set; }
        public Rectangle TextureRegion { get; set; }
        public Vector2 DisplaySize => DisplaySizeOverride ?? DefaultDisplaySize;
        public Vector2 DefaultDisplaySize => new Vector2(1, TextureRegion.Height / (float)TextureRegion.Width) * DefaultDisplaySizeScale;
        public Vector2? DisplaySizeOverride { get; set; }
        public float DefaultDisplaySizeScale { get; set; } = 1;
        public Vector3 Offset => OffsetOverride ?? DefaultOffset;
        public Vector3 DefaultOffset => new(0, (DisplaySize.Y / (float)DisplaySize.X) / 2, 0);
        public Vector3? OffsetOverride { get; set; }
        public Color Color { get; set; } = Color.White;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        public Func<bool> DisplayCondition { get; set; } = static () => true;

        public PlaceholderData() { }
    }

    public abstract PlaceholderData[] Placeholders{ get; }

    public RendererWithPlaceholder(string qualifiedId, TObject obj)
        : base(qualifiedId, obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new RenderDataWithPlaceholder<TData, TObject>(ctx, this);
    }
}
