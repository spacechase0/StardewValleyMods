using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.TerrainFeatures;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class HoeDirtRenderData : RenderData<HoeDirtRenderer>
{
    public HoeDirtRenderData(RenderContext ctx, HoeDirtRenderer parent)
        : base( ctx, parent,
            parent.Object.Location == null ? 0 : parent.Object.Tile.ToPoint().X + parent.Object.Tile.ToPoint().Y * parent.Object.Location.Map.Layers[0].LayerWidth)
    {
    }

    public static ConditionalWeakTable<Crop, HoeDirt> cropDirts = new();

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            var oldCrop = Parent.Object.netCrop.Value;
            Parent.Object.netCrop.value = null;
            try
            {
                ctx.WorldSpriteBatch.Begin(Parent.Object.getBoundingBox().Center.ToVector2(), ctx.WorldTransform, orientationOverride: Matrix.CreateLookAt(Vector3.Zero, Vector3.Up, Vector3.Forward) * Matrix.CreateTranslation(Vector3.Up * 0.01f), sameY3d: false);
                Parent.Object.draw(ctx.WorldSpriteBatch);
                ctx.WorldSpriteBatch.End(ctx.WorldBatch);
            }
            finally
            {
                Parent.Object.netCrop.value = oldCrop;
            }
        }

        if (Parent.Object.crop != null)
        {
            cropDirts.AddOrUpdate(Parent.Object.crop, Parent.Object);
            foreach (var renderer in Mod.State.GetRenderHandlersFor(Parent.Object.crop))
                renderer?.Render(ctx);
        }
    }
}
