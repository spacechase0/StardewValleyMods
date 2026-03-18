using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class LocationRenderData : RenderData<LocationRenderer>
{
    private int terrainInstance = -1;

    public LocationRenderData(RenderContext ctx, LocationRenderer parent)
        : base(ctx, parent)
    {
        terrainInstance = Batch.AddNonInstanced((env, color, world, view, proj) =>
        {
            Game1.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
            Mod.State.GenericModelEffect.CurrentTechnique = Mod.State.GenericModelEffect.Techniques["SingleDrawing"];
            Mod.State.GenericModelEffect.Projection = proj;
            Mod.State.GenericModelEffect.View = view;
            Mod.State.GenericModelEffect.World = world;
            Mod.State.GenericModelEffect.Color = color;
            foreach (var entry in Parent.vbos)
            {
                if (entry.Value.Vertices.VertexCount == 0 || entry.Value.IndexData.Length == 0)
                    continue;

                foreach (var anim in entry.Value.Animations)
                {
                    int frameCount = anim.AllVertIndices.Length;
                    int currentAnimTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % (frameCount * (int)anim.FrameTime);
                    int currFrame = currentAnimTime / (int)anim.FrameTime;

                    int indStart = anim.AllVertIndices[currFrame];
                    Array.Copy((int[])[indStart + 0, indStart + 1, indStart + 2, indStart + 3, indStart + 2, indStart + 1], 0, entry.Value.IndexData, anim.AnimIndexStart, 6);
                }
                entry.Value.Indices.SetData(entry.Value.IndexData);

                Mod.State.GenericModelEffect.Texture = entry.Key;

                Game1.graphics.GraphicsDevice.SetVertexBuffer(entry.Value.Vertices);
                Game1.graphics.GraphicsDevice.Indices = entry.Value.Indices;
                foreach (var pass in Mod.State.GenericModelEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game1.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, entry.Value.IndexData.Length / 3);
                }
            }
            Mod.State.GenericModelEffect.Color = Color.White;
        }, ctx.WorldTransform, hasTransparency: false);
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        Batch.UpdateNonInstanced(terrainInstance, ctx.WorldTransform);

        foreach (var obj in Parent.Object.Objects.Pairs)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj.Value))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.Key.ToPoint().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.terrainFeatures.Pairs)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj.Value))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.Value.getBoundingBox().Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.largeTerrainFeatures)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.getBoundingBox().Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.resourceClumps)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.getBoundingBox().Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.furniture)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.boundingBox.Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.farmers)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.characters)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.animals.Values)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.buildings)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.GetBoundingBox().Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var debris in Parent.Object.debris)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(debris))
            {
                if (renderer is DebrisRenderer debRender)
                    debRender.ParentLocation = Parent.Object;

                renderer?.Render(ctx);
            }
        }
    }
}
