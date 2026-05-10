using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.VanillaAssetExpansion;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class FenceRenderData : RenderData<FenceRenderer>
{
    private int[] instances = new int[4] { -1, -1, -1, -1 };
    public FenceRenderData(RenderContext ctx, FenceRenderer parent)
        : base( ctx, parent)
    {
        if (instance != null)
            return;

        if (Parent.Object.Location != null)
        {
            int origDrawSum = Parent.Object.getDrawSum();
            for (int i = 0; i < 4; ++i)
            {
                int leftX = 0, leftY = 0;
                switch (i)
                {
                    case Game1.up: leftX = 1; break;
                    case Game1.down: leftX = -1; break;
                    case Game1.left: leftY = -1; break;
                    case Game1.right: leftY = 1; break;
                }

                bool left = Parent.Object.Location.objects.TryGetValue(Parent.Object.TileLocation + new Vector2(leftX, leftY), out var leftObj) && leftObj is Fence leftFence && leftFence.countsForDrawing(Parent.Object.ItemId);
                bool right = Parent.Object.Location.objects.TryGetValue(Parent.Object.TileLocation + new Vector2(-leftX, -leftY), out var rightObj) && rightObj is Fence rightFence && rightFence.countsForDrawing(Parent.Object.ItemId);

                int drawSum = 0;
                if (left) drawSum += 10;
                if (right) drawSum += 100;

                if (drawSum == 0 && origDrawSum != 0)
                    continue;

                int ind = Fence.fenceDrawGuide[drawSum];
                if (Parent.Object.health.Value <= 1 && !Parent.Object.repairQueued.Value)
                    ind = 1;

                Vector3 facing = i switch
                {
                    Game1.up => Vector3.Forward,
                    Game1.down => Vector3.Backward,
                    Game1.left => Vector3.Left,
                    Game1.right => Vector3.Right,
                };

                Vector2 quadSize = new Vector2(1, 2);
                Rectangle rect = new Rectangle(ind * Fence.fencePieceWidth % Parent.Object.fenceTexture.Value.Bounds.Width, ind * Fence.fencePieceWidth / Parent.Object.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight);
                string genericId = $"{Parent.Object.QualifiedItemId}/Fence/{left}/{right}";
                if (Parent.Object.isGate.Value)
                {
                    if (!(origDrawSum is 10 or 100 or 110 && i is Game1.up or Game1.down) &&
                        !(origDrawSum is 500 or 1000 or 1500 && i is Game1.left or Game1.right))
                    {
                        if (origDrawSum == 0)
                        {
                            genericId += $"/Gate";
                            ind = 17;
                            rect = new Rectangle(ind * Fence.fencePieceWidth % Parent.Object.fenceTexture.Value.Bounds.Width, ind * Fence.fencePieceWidth / Parent.Object.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight);
                        }
                        else continue;
                    }
                    else
                    {
                        genericId += $"/Gate_{Parent.Object.gatePosition.Value == 88}";

                        quadSize = new Vector2(1.5f, 3);
                        if (left && right)
                        {
                            quadSize.Y = 2;
                            rect = new Rectangle(0, 128, 24, 32);
                        }
                        else if (left) rect = new Rectangle(0, 192, 24, 48);
                        else if (right) rect = new Rectangle(0, 240, 24, 48);
                        if (Parent.Object.gatePosition.Value == 88)
                            rect.Offset(24, 0);
                    }
                }
                genericId += "meow3";

                if (!ctx.WorldBatch.HasInstancedVerticesData(genericId))
                {
                    List<SimpleVertex> verts = new();
                    RenderHelper.GenerateQuad(verts, Parent.Object.fenceTexture.Value, new Vector3(0, quadSize.Y / 2, 0), quadSize, rect, Vector3.Backward);
                    RenderBatcher.VerticesRenderData data = new()
                    {
                        Vertices = new(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), verts.Count, BufferUsage.WriteOnly),
                        Indices = new(Game1.graphics.GraphicsDevice, IndexElementSize.SixteenBits, verts.Count, BufferUsage.WriteOnly),
                        Effect = Mod.State.GenericModelEffect.Clone(),
                        Blend = BlendState.AlphaBlend,
                        Rasterizer = RasterizerState.CullClockwise,
                    };
                    data.Vertices.SetData(verts.ToArray());
                    data.Indices.SetData(Enumerable.Range(0, verts.Count).Select(i => (short)i).ToArray());
                    (data.Effect as GenericModelEffect).Texture = Parent.Object.fenceTexture.Value;
                    (data.Effect as GenericModelEffect).Color = Color.White;
                    ctx.WorldBatch.AddInstancedVerticesData(genericId, [data]);
                }

                instances[i] = Batch.AddInstancedVertices(genericId, Matrix.Identity);
            }
        }
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        for ( int i = 0; i < instances.Length; ++i )
        {
            int inst = instances[i];
            if (inst == -1)
                continue;
            Batch.UpdateInstanced(inst, Matrix.CreateRotationY( MathHelper.ToRadians( 180 + -i * 90 ) ) * ctx.WorldTransform);
        }
    }
}
