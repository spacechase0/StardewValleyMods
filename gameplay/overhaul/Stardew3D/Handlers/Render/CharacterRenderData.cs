using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.Utilities;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class CharacterRenderData : RenderData<CharacterRenderer>
{
    public CharacterRenderData(RenderContext ctx, CharacterRenderer parent)
        : base( ctx, parent )
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            int oldFrame = Parent.Object.Sprite.CurrentFrame;
            if (Parent.Object is not Farmer && Parent.Object.Sprite.CurrentFrame < 16)
            {
                Vector3 pos = Vector3.Transform( ctx.WorldCamera.Position, Matrix.Identity ) * Game1.tileSize;
                int dir = Parent.Object.getGeneralDirectionTowards(new Vector2(pos.X, pos.Z));
                int targetDir = ((dir - Parent.Object.FacingDirection) + 4) % 4;
                if (targetDir is 0 or 2)
                    targetDir = (targetDir + 2) % 4;
                Parent.Object.Sprite.faceDirection(targetDir);
            }

            ctx.WorldSpriteBatch.Begin(Parent.Object.StandingPixel.ToVector2(), ctx.WorldTransform, scale: Parent.Object is NPC ? 1.5f : 1);
            Parent.Object.draw(ctx.WorldSpriteBatch);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);

            Parent.Object.Sprite.CurrentFrame = oldFrame;
        }
    }
}
