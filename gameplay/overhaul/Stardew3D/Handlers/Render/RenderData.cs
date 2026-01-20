using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Data;
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;


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
    protected InteractionData Interaction { get; private set; }
    protected ModelObject.ModelObjectInstance instance;

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

        CheckForInteractions(Parent.QualifiedId);
    }

    public override void Update(RenderContext ctx)
    {
        if (instance != null)
        {
            Model.Update(Batch, instance, ctx.WorldTransform);
        }

        if (interactionInstances != null)
        {
            foreach (var inst in interactionInstances)
                Batch.UpdateInstanced(inst, ctx.WorldTransform, Color.Cyan * 0.5f);
        }
    }

    protected void CheckForInteractions(string id)
    {
        if (Interaction != null)
            return;

        Interaction = InteractionData.Get(id);
        if (Interaction == null || Interaction.Areas.Count == 0)
            return;

        interactionInstances = new();
        for (int i = 0; i < Interaction.Areas.Count; ++i)
        {
            var area = Interaction.Areas[i];

            string rid = $"Interaction/{id}/{i}";
            if (!Batch.HasGenericData(rid))
            {
                var verts = area.GetTriangleVertices();
                RenderBatcher.GenericRenderData data = new()
                {
                    Vertices = new(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), verts.Length, BufferUsage.WriteOnly),
                    Indices = new(Game1.graphics.GraphicsDevice, IndexElementSize.SixteenBits, verts.Length, BufferUsage.WriteOnly),
                    Effect = Mod.State.GenericModelEffect.Clone(),
                    Blend = BlendState.AlphaBlend,
                    Rasterizer = RasterizerState.CullNone,
                };
                data.Vertices.SetData(verts);
                data.Indices.SetData(Enumerable.Range(0, verts.Length).Select(i => (short)i).ToArray());
                (data.Effect as GenericModelEffect).Texture = Game1.staminaRect;
                Batch.AddGenericData(rid, [data]);
            }

            int instance = Batch.AddInstanced(rid, Matrix.Identity, Color.Cyan * 0.5f);
            interactionInstances.Add(instance);
        }
    }
}


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
                if (!Batch.HasGenericData(id))
                {
                    List<SimpleVertex> vertices = new();
                    RenderHelper.GenerateQuad(vertices, placeholder.Texture, placeholder.Offset + Vector3.Forward * 0.0005f * ip, placeholder.DisplaySize, placeholder.TextureRegion, Vector3.Forward, texCoordEffect: placeholder.Effects);
                    RenderBatcher.GenericRenderData data = new()
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
                    Batch.AddGenericData(id, [data]);
                }

                int instance = Batch.AddInstanced(id, Matrix.Identity, placeholder.Color);
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

                Matrix billboard = Matrix.Identity;
                if (ctx.CanBillboard)
                {
                    billboard *= Matrix.CreateConstrainedBillboard(Vector3.Zero, lastCamera.Position - ctx.WorldTransform.Translation, Vector3.Up, lastCamera.Forward, Vector3.Forward);
                }
                Batch.UpdateInstanced(placeholderInstances[ip], billboard * ctx.WorldTransform, Parent.Placeholders[ip].Color);
            }
        }
    }
}
