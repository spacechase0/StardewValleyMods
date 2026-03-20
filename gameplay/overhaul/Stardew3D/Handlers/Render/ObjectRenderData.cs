using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Objects;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ObjectRenderData : RenderData<ObjectRenderer>
{
    public ObjectRenderData(RenderContext ctx, ObjectRenderer parent)
        : base( ctx, parent,
            parent.Object.Location == null ? 0 : parent.Object.TileLocation.ToPoint().X + parent.Object.TileLocation.ToPoint().Y * parent.Object.Location.Map.Layers[0].LayerWidth)
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            if (Parent.Object is Furniture f && f.furniture_type.Value == Furniture.rug )
                ctx.WorldSpriteBatch.Begin(Parent.Object.GetBoundingBox().Center.ToVector2(), ctx.WorldTransform, orientationOverride: Matrix.CreateLookAt(Vector3.Zero, Vector3.Up, Vector3.Forward) * Matrix.CreateTranslation(Vector3.Up*0.01f), sameY3d: false);
            else
                ctx.WorldSpriteBatch.Begin(Parent.Object.GetBoundingBox().Center.ToVector2(), ctx.WorldTransform);

            if (Parent.Object.Location != null)
                Parent.Object.draw(ctx.WorldSpriteBatch, (int)Parent.Object.TileLocation.X, (int) Parent.Object.TileLocation.Y);
            else
                Parent.Object.draw(ctx.WorldSpriteBatch, 0, 0, 0);

            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}
