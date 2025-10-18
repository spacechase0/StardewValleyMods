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
    protected int WhichMatch { get; }

    protected List<int> Instances { get; } = new();

    public RenderData(RenderContext ctx, TRenderer parent, int whichMatch = 0)
        : base(ctx)
    {
        Parent = parent;
        WhichMatch = whichMatch;

        Model = Mod.State.ModelManager.RequestModel(Parent.QualifiedId);
        if (Model.Matches.Count > 0)
        {
            Instances.AddRange(Model.Draw(Batch, Matrix.Identity, whichMatch: WhichMatch));
        }
    }

    public override void Update(RenderContext ctx)
    {
        if (Model.Matches.Count > 0)
        {
            Model.Update(Batch, Instances.ToArray(), ctx.WorldTransform, whichMatch: WhichMatch);
        }
    }
}


public class RenderDataWithPlaceholder<TData, TObject> : RenderData<RendererWithPlaceholder< TData, TObject >>
    where TData : ModelData
{
    protected ICamera lastCamera;

    public RenderDataWithPlaceholder(RenderContext ctx, RendererWithPlaceholder<TData, TObject> parent, int whichMatch = 0)
        : base(ctx, parent, whichMatch)
    {
        if (Model.Matches.Count == 0)
        {
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
                Instances.Add(instance);
            }
        }
    }

    public override void Update(RenderContext ctx)
    {
        lastCamera = ctx.WorldCamera;

        base.Update(ctx);
        if (Model.Matches.Count == 0)
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
                Batch.UpdateInstanced(Instances[ip], billboard * ctx.WorldTransform, Parent.Placeholders[ip].Color);
            }
        }
    }
}
