using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ObjectRenderData : RenderDataWithPlaceholder<ModelData, StardewValley.Object>
{
    private int nonInstanced = -1;

    public ObjectRenderData(RenderContext ctx, ObjectRenderer parent)
        : base( ctx, parent,
            parent.Object.Location == null ? 0 : parent.Object.TileLocation.ToPoint().X + parent.Object.TileLocation.ToPoint().Y * parent.Object.Location.Map.Layers[0].LayerWidth)
    {
        if (Parent.Object.Location != null)
        {
            nonInstanced = Batch.AddNonInstanced((env, color, world, view, proj) =>
            {
                if (Parent.Object.readyForHarvest.Value)
                {
                    var pos = new Vector3(0, 2.5f, 0);
                    var held = Parent.Object.heldObject.Value;
                    var draw = ItemRegistry.GetDataOrErrorItem(held.QualifiedItemId);
                    RenderHelper.DrawBillboard(lastCamera, Game1.mouseCursors, pos, new Vector2(1, 1), new Rectangle(141, 465, 20, 24), col: color, additionalTransform: world);
                    RenderHelper.DrawBillboard(lastCamera, draw.GetTexture(), pos + new Vector3(0, 0.1f, 0), new Vector2(0.9f, 0.9f), draw.GetSourceRect(0), col: color, additionalTransform: world);
                }
            }, Matrix.Identity, hasTransparency: true);
        }
    }

    public override void Update(RenderContext ctx)
    {
        // Why was this added?
        /*
        if (Parent.Object.Location == null)
        {
            if (Model.Matches.Count == 0)
            {
                ctx.WorldTransform = Matrix.CreateScale(1f / 8) * ctx.WorldTransform;
            }
            else
            {
                ctx.WorldTransform = Matrix.CreateTranslation(0.10f, 0.40f, 0.0f) * ctx.WorldTransform;
            }
        }
        //*/

        base.Update(ctx);

        if (nonInstanced != -1)
        {
            Batch.UpdateNonInstanced(nonInstanced, ctx.WorldTransform);
        }
    }
}
