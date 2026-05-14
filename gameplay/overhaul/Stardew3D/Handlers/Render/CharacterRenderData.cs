using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Monsters;
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

        if (Parent.Object == Game1.player && Mod.State.ActiveMode.Tags.Contains(IGameMode.CategoryFirstPerson))
            return;

        if (instance == null)
        {
            int oldFrame = Parent.Object.Sprite.CurrentFrame;
            if (Parent.Object.GetType() == typeof(NPC) && Parent.Object.Sprite.CurrentFrame < 16)
            {
                Vector3 pos = Vector3.Transform( ctx.WorldCamera.Position, Matrix.Identity ) * Game1.tileSize;
                int dir = Parent.Object.getGeneralDirectionTowards(new Vector2(pos.X, pos.Z));
                int targetDir = ((dir - Parent.Object.FacingDirection) + 4) % 4;
                if (targetDir is 0 or 2)
                    targetDir = (targetDir + 2) % 4;
                Parent.Object.Sprite.faceDirection(targetDir);
            }

            if (Parent.Object is Monster monster && monster.isGlider.Value)
            {
                ctx.WorldTransform *= Matrix.CreateTranslation(0, -0.25f, 0);
            }

            ctx.WorldSpriteBatch.Begin(Parent.Object.StandingPixel.ToVector2(), ctx.WorldTransform, scale: Parent.Object?.GetType() == typeof(NPC) ? 1.5f : 1);
            Parent.Object.draw(ctx.WorldSpriteBatch);
            Parent.Object.drawAboveAlwaysFrontLayer(ctx.WorldSpriteBatch);
            if (Parent.Object is Monster monster2)
                monster2.drawAboveAllLayers(ctx.WorldSpriteBatch);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);

            Parent.Object.Sprite.CurrentFrame = oldFrame;
        }
    }
}
