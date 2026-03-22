using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

using PlaceholderData = Stardew3D.Handlers.RendererWithPlaceholder<Stardew3D.DataModels.ModelData, StardewValley.Debris>.PlaceholderData;

namespace Stardew3D.Handlers.Render;

public class DebrisRenderer : RendererFor<ModelData, Debris>
{
    private Item[] Items;
    private PlaceholderData[] Particles;

    // TODO: Find a better way than setting this manually from the outside
    // Debris has no concept of who owns it, which makes this difficult
    public GameLocation ParentLocation { get; set; }

    public DebrisRenderer(Debris obj)
        : base(obj)
    {
        if (Object.item != null)
        {
            Items = [ Object.item ];
        }
        else if (Object.debrisType.Value == Debris.DebrisType.LETTERS)
        {
            Color borderCol = Color.Black;
            Color textCol = Object.nonSpriteChunkColor.Value;

            string str = Object.debrisMessage.Value;

            // TODO
        }
        else if (Object.debrisType.Value == Debris.DebrisType.NUMBERS)
        {
            int secondOffset = -1 * (int)((float)Object.chunkFinalYLevel - Object.Chunks[0].position.Y) / 2;
            if (secondOffset < 0)
                secondOffset = 0;

            string str = Object.chunkType.Value.ToString();
            float x = -str.Length / 2f * 0.25f;
            List<PlaceholderData> particles = new();
            for ( int i = 0; i < str.Length; ++i )
            {
                int currentDigit = str[i];
                int textX = 512 + currentDigit * 8 % 48;
                int textY = 128 + currentDigit * 8 / 48 * 8;

                particles.Add(new()
                {
                    Texture = Game1.mouseCursors,
                    TextureRegion = new Rectangle(textX, textY, 8, 8),
                    DefaultDisplaySizeScale = 0.75f * 4,
                    Color = Object.nonSpriteChunkColor.Value,
                    OffsetOverride = new Vector3(x, (i == str.Length - 2) ? secondOffset : 0, 0)
                });
                x += 4f / 64;
            }
            Particles = particles.ToArray();
        }
        else if (Object.debrisType.Value == Debris.DebrisType.SPRITECHUNKS)
        {
            List<PlaceholderData> particles = new();
            foreach (var chunk in Object.chunks)
            {
                particles.Add(new()
                {
                    Texture = Object.spriteChunkSheet,
                    TextureRegion = new Rectangle(chunk.xSpriteSheet.Value, chunk.ySpriteSheet.Value, Math.Min(Object.sizeOfSourceRectSquares.Value, Object.spriteChunkSheet.Width), Math.Min(Object.sizeOfSourceRectSquares.Value, Object.spriteChunkSheet.Height)),
                    DefaultDisplaySizeScale = 1,
                    Color = Object.nonSpriteChunkColor.Value,
                    // TODO: rotation
                });
            }
            Particles = particles.ToArray();
        }
        else if (Object.itemId.Value != null)
        {
            List<Item> items = new();
            for (int i = 0; i < Object.chunks.Count; ++i)
            {
                items.Add(ItemRegistry.Create(Object.itemId.Value));
            }
            Items = items.ToArray();
        }
        else
        {
            List<PlaceholderData> particles = new();
            foreach (var chunk in Object.chunks)
            {
                particles.Add(new()
                {
                    Texture = Game1.debrisSpriteSheet,
                    TextureRegion = Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, Object.chunkType.Value + chunk.randomOffset, 16, 16),
                    DefaultDisplaySizeScale = 1,
                    Color = Object.chunksColor.Value,
                });
            }
            Particles = particles.ToArray();
        }
    }

    public override void Render(RenderContext ctx)
    {
        base.Render(ctx);

        if (Items != null)
        {
            for (int ii = 0; ii < Items.Length; ++ii)
            {
                var item = Items[ii];
                if (item == null || ii >= Object.Chunks.Count)
                    continue;
                var chunk = Object.Chunks[ii];

                int finalY = Object.chunkFinalYLevel;
                if (Object.movingFinalYLevel)
                    finalY = Object.chunkFinalYTarget;

                Vector2 pos = chunk.GetVisualPosition() + new Vector2(32, 32);
                if (!Object.chunksMoveTowardPlayer)
                    pos.Y += -chunk.position.Y + finalY;

                Vector3 pos3d = pos.To3D(ParentLocation?.Map);
                if (!Object.chunksMoveTowardPlayer)
                    pos3d.Y += (finalY - chunk.position.Y) / 64f;

                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.Identity;
                subCtx.WorldTransform *= Matrix.CreateScale(0.5f);
                subCtx.WorldTransform *= Matrix.CreateTranslation(pos3d);
                subCtx.WorldTransform *= ctx.WorldTransform;

                var handlers = Mod.State.GetRenderHandlersFor(item);
                foreach (var sub in handlers)
                    sub.Render(subCtx);
            }
        }
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new DebrisRenderData(ctx, this);
    }

    private class DebrisRenderData : RenderDataBase
    {
        private DebrisRenderer Parent;

        private int id;

        private ICamera lastCamera;

        public DebrisRenderData(RenderContext ctx, DebrisRenderer parent)
        :   base(ctx)
        {
            Parent = parent;

            if (Parent.Particles?.Length > 0)
            {
                id = ctx.WorldBatch.AddDirect((PBREnvironment env, Color color, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix) =>
                {
                    if (color.A == 0)
                        return;

                    for (int ip = 0; ip < Parent.Particles.Length; ++ip )
                    {
                        var particle = Parent.Particles[ip];
                        Chunk chunk = ip < Parent.Object.Chunks.Count ? Parent.Object.Chunks[ip] : null;

                        int finalY = Parent.Object.chunkFinalYLevel;
                        if (Parent.Object.movingFinalYLevel)
                            finalY = Parent.Object.chunkFinalYTarget;

                        Vector2 pos = (chunk?.GetVisualPosition() ?? Vector2.Zero);
                        if (!Parent.Object.chunksMoveTowardPlayer)
                            pos.Y += -chunk.position.Y + finalY;

                        Vector3 pos3d = pos.To3D(Parent.ParentLocation?.Map);
                        if (!Parent.Object.chunksMoveTowardPlayer)
                            pos3d.Y += (finalY - chunk.position.Y) / 64f;

                        RenderHelper.DrawBillboard(lastCamera, particle.Texture, pos3d, particle.DisplaySize, particle.TextureRegion, particle.Color * (color.A / 255f), worldMatrix);
                    }
                }, Matrix.Identity, hasTransparency: true);
            }
        }

        public override void Update(RenderContext ctx)
        {
            lastCamera = ctx.WorldCamera;
            ctx.WorldBatch.UpdateDirect(id, Matrix.Identity);
        }
    }
}
