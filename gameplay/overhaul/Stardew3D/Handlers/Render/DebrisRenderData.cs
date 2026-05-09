using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Utilities;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class DebrisRenderData : RenderData<DebrisRenderer>
{
    private static ConditionalWeakTable<Debris, Item> tmpItems = new();

    public DebrisRenderData(RenderContext ctx, DebrisRenderer parent)
        : base( ctx, parent)
    {
    }

    public override void Update(RenderContext ctx)
    {
        var debris = Parent.Object;

        base.Update(ctx);

        // All the chunkFinalYLevel checks end up with a Math.Abs() so that it doesn't go *under* the terrain
        // In 3D it would no longer be visible if that happened, which wouldn't happen in 2D
        
        if (debris.item != null)
        {
            Vector2 visualPosition = debris.Chunks[0].GetVisualPosition();
            Matrix worldTransform = Matrix.CreateTranslation(0, Math.Abs(debris.chunkFinalYLevel - visualPosition.Y) / 64, 0) * Matrix.CreateTranslation(visualPosition.To3D(Parent.ParentLocation)) * ctx.WorldTransform;

            foreach (var renderer in Mod.State.GetRenderHandlersFor(debris.item))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = worldTransform;
                renderer?.Render(subCtx);
            }
            return;
        }

        if (instance != null)
            return;

        if (debris.debrisType.Value == Debris.DebrisType.LETTERS)
        {
            Chunk chunk = debris.Chunks[0];
            Vector2 visualPosition = chunk.GetVisualPosition();
            Matrix worldTransform = Matrix.CreateTranslation(visualPosition.To3D(Parent.ParentLocation)) * ctx.WorldTransform;

            ctx.WorldSpriteBatch.Begin(visualPosition, worldTransform);
            var oldSb = Game1.spriteBatch;
            try
            {
                Game1.spriteBatch = ctx.WorldSpriteBatch;
                Game1.drawWithBorder(debris.debrisMessage.Value, Color.Black, debris.nonSpriteChunkColor.Value, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, visualPosition)), chunk.rotation, chunk.scale, (visualPosition.Y + 64f) / 10000f);
            }
            finally
            {
                Game1.spriteBatch = oldSb;
            }
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
            return;
        }
        else if (debris.debrisType.Value == Debris.DebrisType.NUMBERS)
        {
            Chunk chunk = debris.Chunks[0];
            Vector2 visualPosition = chunk.GetVisualPosition();
            Matrix worldTransform = Matrix.CreateTranslation(visualPosition.To3D(Parent.ParentLocation)) * ctx.WorldTransform;
            float num = 0;

            ctx.WorldSpriteBatch.Begin(visualPosition, worldTransform);
            NumberSprite.draw(debris.chunkType.Value, ctx.WorldSpriteBatch, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(new Vector2(visualPosition.X, (float)debris.chunkFinalYLevel - ((float)debris.chunkFinalYLevel - visualPosition.Y)))), debris.nonSpriteChunkColor.Value, chunk.scale * 0.75f, 0.98f + 0.0001f * (float)num, chunk.alpha, -1 * (int)((float)debris.chunkFinalYLevel - visualPosition.Y) / 2);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
            return;
        }
        else if (debris.debrisType.Value == Debris.DebrisType.SPRITECHUNKS)
        {
            foreach (var chunk in debris.Chunks)
            {
                Vector2 visualPosition = chunk.GetVisualPosition();
                Matrix worldTransform = Matrix.CreateTranslation(visualPosition.To3D(Parent.ParentLocation)) * ctx.WorldTransform;
                float num = 0;

                ctx.WorldSpriteBatch.Begin(visualPosition, worldTransform);
                ctx.WorldSpriteBatch.Draw(debris.spriteChunkSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, visualPosition)), new Microsoft.Xna.Framework.Rectangle(chunk.xSpriteSheet.Value, chunk.ySpriteSheet.Value, Math.Min(debris.sizeOfSourceRectSquares.Value, debris.spriteChunkSheet.Bounds.Width), Math.Min(debris.sizeOfSourceRectSquares.Value, debris.spriteChunkSheet.Bounds.Height)), debris.nonSpriteChunkColor.Value * chunk.alpha, chunk.rotation, new Vector2(debris.sizeOfSourceRectSquares.Value / 2, debris.sizeOfSourceRectSquares.Value / 2), chunk.scale, SpriteEffects.None, ((float)(debris.chunkFinalYLevel + 16) + visualPosition.X / 10000f) / 10000f);
                ctx.WorldSpriteBatch.End(ctx.WorldBatch);
            }
            return;
        }
        else if (debris.itemId.Value != null)
        {
            var item = tmpItems.GetValue(debris, deb => ItemRegistry.Create(deb.itemId.Value));

            foreach (var chunk in debris.Chunks)
            {
                Vector2 visualPosition = chunk.GetVisualPosition();
                Matrix worldTransform = Matrix.CreateTranslation(visualPosition.To3D(Parent.ParentLocation)) * ctx.WorldTransform;

                foreach (var renderer in Mod.State.GetRenderHandlersFor(item))
                {
                    RenderContext subCtx = ctx;
                    subCtx.ParentWorldTransform = ctx.WorldTransform;
                    subCtx.WorldTransform = worldTransform;
                    renderer?.Render(subCtx);
                }
            }
        }
        else
        {
            foreach (var chunk in debris.Chunks)
            {
                Vector2 position = Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk.position.Value));
                Microsoft.Xna.Framework.Rectangle sourceRectForStandardTileSheet = Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, debris.chunkType.Value + chunk.randomOffset, 16, 16);
                float layerDepth = (chunk.position.Y + 128f + chunk.position.X / 10000f) / 10000f;

                Matrix worldTransform = Matrix.CreateTranslation(position.To3D(Parent.ParentLocation)) * ctx.WorldTransform;

                ctx.WorldSpriteBatch.Begin(position, worldTransform);
                ctx.WorldSpriteBatch.Draw(Game1.debrisSpriteSheet, position, sourceRectForStandardTileSheet, debris.chunksColor.Value, 0f, Vector2.Zero, 4f * debris.scale.Value, SpriteEffects.None, layerDepth);
                ctx.WorldSpriteBatch.End(ctx.WorldBatch);
            }
        }
    }
}
