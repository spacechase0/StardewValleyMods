using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ItemRenderData<TData, TItem> : RenderData<ItemRenderer<TData, TItem>>
    where TData : ModelData
    where TItem : Item
{
    public ItemRenderData(RenderContext ctx, ItemRenderer<TData, TItem> parent)
        : base(ctx, parent)
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            ctx.WorldSpriteBatch.Begin(Vector2.Zero, ctx.WorldTransform);
            Parent.Object.drawInMenu(ctx.WorldSpriteBatch, Vector2.Zero, 4);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}
