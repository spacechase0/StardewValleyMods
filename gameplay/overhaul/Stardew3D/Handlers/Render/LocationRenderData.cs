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
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using xTile.Tiles;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class LocationRenderData : RenderData<LocationRenderer>
{
    private int terrainInstance = -1;
    private int waterInstance = -1;

    public LocationRenderData(RenderContext ctx, LocationRenderer parent)
        : base(ctx, parent)
    {
        terrainInstance = Batch.AddDirect((env, color, world, view, proj) =>
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

        waterInstance = Batch.AddDirect((env, color, world, view, proj) =>
        {
            if (Parent.waterVertices.Count == 0)
                return;

            Texture2D tex = Game1.mouseCursors;
            var waterLayer = Parent.Object.Map.GetLayer("kittycatcasey.Stardew3D/Water");
            for (int iy = 0, ind = 0; iy < Parent.Object.Map.Layers[0].LayerSize.Height; ++iy)
            {
                for (int ix = 0; ix < Parent.Object.Map.Layers[0].LayerSize.Width; ++ix)
                {
                    bool hasWater = Parent.Object.isWaterTile(ix, iy);

                    Rectangle srcRect = new Rectangle(Parent.Object.waterAnimationIndex * 64, 2064 + (((ix + iy) % 2 != 0) ? ((!Parent.Object.waterTileFlip) ? 128 : 0) : (Parent.Object.waterTileFlip ? 128 : 0)) + (false ? ((int)Parent.Object.waterPosition) : 0), 64, 64 + (false ? ((int)(0f - Parent.Object.waterPosition)) : 0));
                    if (!hasWater)
                        srcRect = new(320, 496, 16, 16);

                    // TODO: do this properly like the others
                    if (waterLayer?.Tiles[ix, iy] is StaticTile tile && tile.TileIndex != -1)
                    {
                        if (tex == Game1.mouseCursors)
                        {
                            string texKey = PathUtilities.NormalizeAssetName(tile.TileSheet.ImageSource);
                            tex = Game1.content.Load<Texture2D>(texKey);
                        }
                        srcRect = Game1.getSourceRectForStandardTileSheet(tex, tile.TileIndex, 16, 16);
                        hasWater = true;
                    }

                    if (!hasWater && !Parent.ShowMissing.HasFlag(LocationRenderer.ShowMissingType.Water))
                        continue;

                    Parent.waterVertices[ind * 6 + 0] = new(Parent.waterVertices[ind * 6 + 0].Position, (srcRect.Location.ToVector2() + new Vector2(0, 0)) / tex.Bounds.Size.ToVector2(), Parent.waterVertices[ind * 6 + 0].Color);
                    Parent.waterVertices[ind * 6 + 1] = new(Parent.waterVertices[ind * 6 + 1].Position, (srcRect.Location.ToVector2() + new Vector2(0, srcRect.Height)) / tex.Bounds.Size.ToVector2(), Parent.waterVertices[ind * 6 + 1].Color);
                    Parent.waterVertices[ind * 6 + 2] = new(Parent.waterVertices[ind * 6 + 2].Position, (srcRect.Location.ToVector2() + new Vector2(srcRect.Width, 0)) / tex.Bounds.Size.ToVector2(), Parent.waterVertices[ind * 6 + 2].Color);
                    Parent.waterVertices[ind * 6 + 3] = new(Parent.waterVertices[ind * 6 + 3].Position, (srcRect.Location.ToVector2() + srcRect.Size.ToVector2()) / tex.Bounds.Size.ToVector2(), Parent.waterVertices[ind * 6 + 3].Color);
                    Parent.waterVertices[ind * 6 + 4] = new(Parent.waterVertices[ind * 6 + 4].Position, (srcRect.Location.ToVector2() + new Vector2(srcRect.Width, 0)) / tex.Bounds.Size.ToVector2(), Parent.waterVertices[ind * 6 + 4].Color);
                    Parent.waterVertices[ind * 6 + 5] = new(Parent.waterVertices[ind * 6 + 5].Position, (srcRect.Location.ToVector2() + new Vector2(0, srcRect.Height)) / tex.Bounds.Size.ToVector2(), Parent.waterVertices[ind * 6 + 5].Color);

                    ++ind;
                }
            }
            if (Parent.waterVbo == null || Parent.waterVbo.VertexCount < Parent.waterVertices.Count)
            {
                Parent.waterVbo?.Dispose();
                Parent.waterVbo = new VertexBuffer(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), Parent.waterVertices.Count, BufferUsage.WriteOnly);
            }
            Parent.waterVbo.SetData(Parent.waterVertices.ToArray(), 0, Parent.waterVertices.Count);

            Game1.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
            Mod.State.GenericModelEffect.Projection = proj;
            Mod.State.GenericModelEffect.View = view;
            Mod.State.GenericModelEffect.World = world;
            Mod.State.GenericModelEffect.Color = color;
            Mod.State.GenericModelEffect.Texture = tex;
            Game1.graphics.GraphicsDevice.SetVertexBuffer(Parent.waterVbo);

            Mod.State.GenericModelEffect.CurrentTechnique = Mod.State.GenericModelEffect.Techniques["SingleDrawing_Transparent_1"];
            foreach (var pass in Mod.State.GenericModelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Parent.waterVertices.Count / 3);
            }
            Mod.State.GenericModelEffect.CurrentTechnique = Mod.State.GenericModelEffect.Techniques["SingleDrawing_Transparent_2"];
            foreach (var pass in Mod.State.GenericModelEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Parent.waterVertices.Count / 3);
            }

            Mod.State.GenericModelEffect.Color = Color.White;
        }, ctx.WorldTransform, hasTransparency: true);
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        Batch.UpdateDirect(terrainInstance, ctx.WorldTransform);
        Batch.UpdateDirect(waterInstance, ctx.WorldTransform);

        foreach (var obj in Parent.Object.Objects.Pairs)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj.Value))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.Key.ToPoint().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.terrainFeatures.Pairs)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj.Value))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.Value.getBoundingBox().Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.largeTerrainFeatures)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.getBoundingBox().Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.resourceClumps)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.getBoundingBox().Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.furniture)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.boundingBox.Center.ToVector2().To3D(Parent.Object.Map)) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.farmers)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.characters)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.animals.Values)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
                subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                renderer?.Render(subCtx);
            }
        }

        foreach (var obj in Parent.Object.buildings)
        {
            foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
            {
                RenderContext subCtx = ctx;
                subCtx.ParentWorldTransform = ctx.WorldTransform;
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

        if (Parent.Object.currentEvent != null)
        {
            var ev = Parent.Object.currentEvent;
            foreach (var obj in ev.actors)
            {
                foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
                {
                    RenderContext subCtx = ctx;
                    subCtx.ParentWorldTransform = ctx.WorldTransform;
                    subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                    renderer?.Render(subCtx);
                }
            }
            foreach (var obj in ev.farmerActors)
            {
                foreach (var renderer in Mod.State.GetRenderHandlersFor(obj))
                {
                    RenderContext subCtx = ctx;
                    subCtx.ParentWorldTransform = ctx.WorldTransform;
                    subCtx.WorldTransform = Matrix.CreateTranslation(obj.StandingPixel3D) * ctx.WorldTransform;
                    renderer?.Render(subCtx);
                }
            }
        }
    }
}
