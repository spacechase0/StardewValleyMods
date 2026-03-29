using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.Utilities;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class BuildingRenderData : RenderData<BuildingRenderer>
{
    public BuildingRenderData(RenderContext ctx, BuildingRenderer parent)
        : base( ctx, parent )
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            if ( Parent.Object.buildingType.Value == "Fish Pond")
                ctx.WorldSpriteBatch.Begin(Parent.Object.GetBoundingBox().Center.ToVector2(), ctx.WorldTransform, orientationOverride: Matrix.CreateLookAt(Vector3.Zero, Vector3.Up, Vector3.Forward) * Matrix.CreateTranslation(Vector3.Up * 0.01f) * Matrix.CreateTranslation(0, 0, Parent.Object.tilesHigh.Value/2f));
            else
                ctx.WorldSpriteBatch.Begin(Parent.Object.GetBoundingBox().Center.ToVector2(), ctx.WorldTransform);
            Parent.Object.draw(ctx.WorldSpriteBatch);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}
