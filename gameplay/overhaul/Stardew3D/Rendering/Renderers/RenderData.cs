using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Models;
using StardewValley.Mods;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Rendering.Renderers;


public abstract class RenderDataBase
{
    protected RenderBatcher Batch { get; }

    protected RenderDataBase(RenderContext ctx)
    {
        Batch = ctx.WorldBatch;
    }

    public abstract void Update(RenderContext ctx);
}

public class RenderData<TRenderer>
    : RenderDataBase
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
