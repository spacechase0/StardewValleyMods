using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using Stardew3D.Models;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers;


public abstract class RenderDataBase
{
    protected RenderBatcher Batch { get; }

    protected RenderDataBase(RenderContext ctx)
    {
        Batch = ctx.WorldBatch;
    }

    public abstract void Update(RenderContext ctx);
}

public class RenderData<TRenderer> : RenderDataBase
    where TRenderer : Renderer
{
    protected TRenderer Parent { get; }
    protected ModelObject Model { get; }
    protected InteractionData Interaction { get; }
    protected Vector3 InteractionSize { get; }
    protected ModelObject.ModelObjectInstance instance;

    private string interactionId;
    private List<int> interactionInstances;

    public RenderData(RenderContext ctx, TRenderer parent, int whichMatch = 0)
        : base(ctx)
    {
        Parent = parent;

        Model = Mod.State.ModelManager.RequestModel(Parent.QualifiedId);
        if (Model.Matches.Count > 0)
        {
            instance = Model.Draw(Batch, Matrix.Identity, whichMatch: whichMatch);
        }

        Interaction = InteractionData.Get(Parent.Object, out Vector3 interactionSize, out interactionId);
        InteractionSize = interactionSize;
        GenerateInteractionDebugView();
    }

    public override void Update(RenderContext ctx)
    {
        if (instance != null)
        {
            Model.Update(Batch, instance, ctx.WorldTransform);
        }

        if (Mod.State.RenderDebugInteractions && interactionInstances != null && Parent.Object != Game1.player)
        {
            foreach (var inst in interactionInstances)
                Batch.UpdateInstanced(inst, Matrix.CreateScale( InteractionSize ) * ctx.WorldTransform * Matrix.CreateTranslation(0, InteractionSize.Y / 2, 0), Color.White * 0.5f);
        }
    }

    private void GenerateInteractionDebugView()
    {
        if (Interaction == null || Interaction.Areas.Count == 0)
            return;

        interactionInstances = new();
        for (int i = 0; i < Interaction.Areas.Count; ++i)
        {
            var area = Interaction.Areas[i];

            string rid = $"Interaction/{interactionId}/{i}";
            if (!Batch.HasInstancedVerticesData(rid))
            {
                var verts = area.GetTransformedTriangleVertices().Select( v3 => new SimpleVertex( v3, Vector2.One * 0.5f, area.DebugColor )).ToList();
                verts.AddRange(new BoxInteractionArea()
                {
                    Size = new(1, 0.05f, 0.05f ),
                    Translation = area.Translation + Vector3.Transform( new Vector3( 0.5f, 0, 0 ), area.Transform.NoTranslation() ),
                    Rotation = area.Rotation,
                }.GetTransformedTriangleVertices().Select(v3 => new SimpleVertex(v3, Vector2.Zero, Color.Red)));
                verts.AddRange(new BoxInteractionArea()
                {
                    Size = new(0.05f, 1, 0.05f ),
                    Translation = area.Translation + Vector3.Transform(new Vector3(0, 0.5f, 0), area.Transform.NoTranslation()),
                    Rotation = area.Rotation,
                }.GetTransformedTriangleVertices().Select(v3 => new SimpleVertex(v3, Vector2.Zero, Color.Green)));
                verts.AddRange(new BoxInteractionArea()
                {
                    Size = new(0.05f, 0.05f, 1 ),
                    Translation = area.Translation + Vector3.Transform(new Vector3(0, 0, 0.5f), area.Transform.NoTranslation()),
                    Rotation = area.Rotation,
                }.GetTransformedTriangleVertices().Select(v3 => new SimpleVertex(v3, Vector2.Zero, Color.Blue)));
                RenderBatcher.VerticesRenderData data = new()
                {
                    Vertices = new(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), verts.Count, BufferUsage.WriteOnly),
                    Indices = new(Game1.graphics.GraphicsDevice, IndexElementSize.SixteenBits, verts.Count, BufferUsage.WriteOnly),
                    Effect = Mod.State.GenericModelEffect.Clone(),
                    Blend = BlendState.AlphaBlend,
                    Rasterizer = RasterizerState.CullNone,
                };
                data.Vertices.SetData(verts.ToArray());
                data.Indices.SetData(Enumerable.Range(0, verts.Count).Select(i => (short)i).ToArray());
                (data.Effect as GenericModelEffect).Texture = Game1.staminaRect;
                (data.Effect as GenericModelEffect).Color = Color.White;
                Batch.AddInstancedVerticesData(rid, [data]);
            }

            int instance = Batch.AddInstancedVertices(rid, Matrix.Identity);
            interactionInstances.Add(instance);
        }
    }
}
