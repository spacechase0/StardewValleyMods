using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ToolRenderData : RenderData<ToolRenderer>
{
    public ToolRenderData(RenderContext ctx, ToolRenderer parent)
        : base(ctx, parent)
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            // Tools are really weird...
            ctx.WorldSpriteBatch.Begin(new Vector2(32,0), ctx.WorldTransform);
            ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(Parent.Object.QualifiedItemId);
            ctx.WorldSpriteBatch.Draw(dataOrErrorItem.GetTexture(), new Vector2(0, -64), dataOrErrorItem.GetSourceRect(), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}
