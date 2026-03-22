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

        foreach (var entry in Parent.Object.GetExtendedQualifiedIds())
        {
            Interaction = InteractionData.Get(entry);
            if (Interaction != null)
            {
                interactionId = entry;
                break;
            }
        }
        GenerateInteractionDebugView();
    }

    public override void Update(RenderContext ctx)
    {
        if (instance != null)
        {
            Model.Update(Batch, instance, ctx.WorldTransform);
        }

        if (Mod.State.RenderDebugInteractions && interactionInstances != null)
        {
            foreach (var inst in interactionInstances)
                Batch.UpdateInstanced(inst, ctx.WorldTransform, Color.White * 0.5f);
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

#if true
public class RenderDataWithPlaceholder<TData, TObject> : RenderData<RendererWithPlaceholder< TData, TObject >>
    where TData : ModelData
{
    protected ICamera lastCamera;

    private List<int> placeholderInstances;

    public RenderDataWithPlaceholder(RenderContext ctx, RendererWithPlaceholder<TData, TObject> parent, int whichMatch = 0)
        : base(ctx, parent, whichMatch)
    {
        if (Model.Matches.Count == 0)
        {
            placeholderInstances = new();
            for (int ip = 0; ip < Parent.Placeholders.Length; ++ip)
            {
                var placeholder = Parent.Placeholders[ip];

                string id = $"{Parent.QualifiedId}/{ip}";
                if (!Batch.HasInstancedVerticesData(id))
                {
                    List<SimpleVertex> vertices = new();
                    RenderHelper.GenerateQuad(vertices, placeholder.Texture, placeholder.Offset + Vector3.Forward * 0.0005f * ip, placeholder.DisplaySize, placeholder.TextureRegion, Vector3.Forward, texCoordEffect: placeholder.Effects);
                    RenderBatcher.VerticesRenderData data = new()
                    {
                        Vertices = new(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), vertices.Count, BufferUsage.WriteOnly),
                        Indices = new(Game1.graphics.GraphicsDevice, IndexElementSize.SixteenBits, vertices.Count, BufferUsage.WriteOnly),
                        Effect = Mod.State.GenericModelEffect.Clone(),
                        Blend = BlendState.AlphaBlend,
                        Rasterizer = RasterizerState.CullNone,
                    };
                    data.Vertices.SetData(vertices.ToArray());
                    data.Indices.SetData(Enumerable.Range(0, vertices.Count).Select(i => (short)i).ToArray());
                    (data.Effect as GenericModelEffect).Texture = placeholder.Texture;
                    Batch.AddInstancedVerticesData(id, [data]);
                }

                int instance = Batch.AddInstancedVertices(id, Matrix.Identity, placeholder.Color);
                placeholderInstances.Add(instance);
            }
        }
    }

    public override void Update(RenderContext ctx)
    {
        lastCamera = ctx.WorldCamera;

        base.Update(ctx);
        if (placeholderInstances != null)
        {
            for (int ip = 0; ip < Parent.Placeholders.Length; ++ip)
            {
                if (!Parent.Placeholders[ip].DisplayCondition())
                    continue;

                Matrix transform = Parent.Placeholders[ip].OrientationIfNotBillboard;
                if (Parent.Placeholders[ip].Billboard && ctx.CanBillboard)
                {
                    transform *= Matrix.CreateConstrainedBillboard(Vector3.Zero, lastCamera.Position - ctx.WorldTransform.Translation, Vector3.Up, lastCamera.Forward, Vector3.Forward);
                }
                Batch.UpdateInstanced(placeholderInstances[ip], transform * ctx.WorldTransform, Parent.Placeholders[ip].Color);
            }
        }
    }
}
#endif
