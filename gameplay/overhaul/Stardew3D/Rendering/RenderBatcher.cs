using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics;
using SpaceShared;

namespace Stardew3D.Rendering;

public class RenderBatcher : IDisposable
{
    public delegate void RenderDirect(PBREnvironment env, Color color, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix);

    private GraphicsDevice graphics;

    private class BatchData : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct InstanceData : IVertexType
        {
            internal static VertexDeclaration _vertexDecl = new(Marshal.SizeOf<InstanceData>(),
                                                                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                                                                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                                                                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                                                                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
                                                                new VertexElement(64, VertexElementFormat.Color, VertexElementUsage.Color, 1));
            public VertexDeclaration VertexDeclaration => _vertexDecl;

            public Matrix Transform;
            public Color Color;

            // This causes problems as a bool
            // C# bool might be 4 bytes anyways, so...
            public byte StaysVisibleAfterFrame;
        }

        public List<InstanceData> instances = new();
        public VertexBuffer instanceVbo;

        public virtual void Dispose()
        {
            instanceVbo?.Dispose();
            instanceVbo = null;
        }
    }
    public class VerticesRenderData : IDisposable
    {
        public VertexBuffer Vertices { get; set; }
        public IndexBuffer Indices { get; set; }
        public Effect Effect { get; set; }
        public BlendState Blend { get; set; } = BlendState.Opaque;
        public RasterizerState Rasterizer { get; set; } = RasterizerState.CullClockwise;

        public void Dispose()
        {
            Vertices?.Dispose();
            Indices?.Dispose();
            Vertices = null;
            Indices = null;
        }
    }
    private class VerticesBatchData : BatchData
    {
        public List<VerticesRenderData> opaqueVertices = new();
        public List<VerticesRenderData> transparentVertices = new();

        public override void Dispose()
        {
            foreach (var entry in opaqueVertices)
                entry.Dispose();
            foreach (var entry in transparentVertices)
                entry.Dispose();
            base.Dispose();
        }
    }
    private class ModelBatchData : BatchData
    {
        public List<Effect> opaqueEffects = new();
        public List<Effect> transparentEffects = new();
        public List<MeshPart> opaqueParts = new();
        public List<MeshPart> transparentParts = new();

        /*
        public override void Dispose()
        {
            foreach (var entry in opaqueEffects)
                entry.Dispose();
            foreach (var entry in transparentEffects)
                entry.Dispose();
            base.Dispose();
        }
        //*/
    }

    private class SpriteData : IDisposable
    {
        public struct SpriteInstance
        {
            public Vector3 Position;
            public int Layer;
            public Matrix? Orientation;
        }
        public List<SpriteInstance> Instances { get; set; } = new();
        public List<SimpleVertex> Vertices { get; set; } = new();

        public Effect Effect { get; set; } = RenderHelper.GenericEffect.Clone();

        public void Dispose()
        {
            Effect?.Dispose();
            Effect = null;
        }
    }

    private ConditionalWeakTable<Mesh, ModelBatchData> modelBatchData = new();
    private Dictionary<string, VerticesBatchData> verticesBatchData = new();
    private List<(BatchData Batch, int Instance)> instances = new();
    private List<(RenderDirect Action, Matrix Transform, Color color, byte StaysVisibleAfterFrame)> directOpaque = new();
    private List<(RenderDirect Action, Matrix Transform, Color color, byte StaysVisibleAfterFrame)> directTransparent = new();
    private List<(bool HasTransparency, int Instance)> direct = new();
    private ConditionalWeakTable<Texture2D, SpriteData> sprites = new();

    public RenderBatcher(GraphicsDevice graphics)
    {
        this.graphics = graphics;
    }

    public int AddInstancedModel(Mesh mesh, Matrix transform, Color col, bool staysVisibleAfterFrame = false)
    {
        if (!modelBatchData.TryGetValue(mesh, out ModelBatchData data))
        {
            data = new();

            data.opaqueEffects = mesh.OpaqueEffects.ToList();
            data.transparentEffects = mesh.TranslucidEffects.ToList();

            for (int i = 0; i < mesh.Count; ++i)
            {
                var part = mesh[i];
                if (part.Blending == BlendState.Opaque)
                    data.opaqueParts.Add(part);
                else
                    data.transparentParts.Add(part);
            }

            modelBatchData.Add(mesh, data);
        }

        data.instances.Add(new() { Transform = transform, Color = col, StaysVisibleAfterFrame = staysVisibleAfterFrame ? (byte)1 : (byte)0 } );
        instances.Add(new(data, data.instances.Count - 1));
        return instances.Count - 1;
    }

    public bool HasInstancedVerticesData(string verticesId)
    {
        return verticesBatchData.ContainsKey(verticesId);
    }

    public void AddInstancedVerticesData(string verticesId, List<VerticesRenderData> data )
    {
        verticesBatchData.Add(verticesId, new()
        {
            opaqueVertices = data.Where(d => d.Blend == BlendState.Opaque).ToList(),
            transparentVertices = data.Where(d => d.Blend != BlendState.Opaque).ToList()
        } );
    }

    public int AddInstancedVertices(string genericId, Matrix transform, Color? color = null, bool staysVisibleAfterFrame = false )
    {
        color ??= Color.White;
        verticesBatchData[genericId].instances.Add(new() { Transform = transform, Color = color.Value, StaysVisibleAfterFrame = staysVisibleAfterFrame ? (byte)1 : (byte)0 } );
        instances.Add(new(verticesBatchData[genericId], verticesBatchData[genericId].instances.Count - 1));
        return instances.Count - 1;
    }

    public int AddDirect(RenderDirect custom, Matrix transform, Color? color = null, bool staysVisibleAfterFrame = false, bool hasTransparency = false)
    {
        color ??= Color.White;
        int instance;
        if (hasTransparency)
        {
            instance = directTransparent.Count;
            directTransparent.Add(new(custom, transform, color.Value, staysVisibleAfterFrame ? (byte)1 : (byte)0));
        }
        else
        {
            instance = directOpaque.Count;
            directOpaque.Add(new(custom, transform, color.Value, staysVisibleAfterFrame ? (byte)1 : (byte)0));
        }
        direct.Add(new(hasTransparency, instance));
        return direct.Count - 1;
    }

    internal void AddBillboardSprite(Vector2 pos2d, Vector3 pos, int layer, SpriteBatchItem item, float scale = 1)
    {
        var data = sprites.GetOrCreateValue(item.Texture);
        data.Instances.Add(new()
        {
            Position = pos,
            Layer = layer,
        });

        SimpleVertex tl = SimpleVertex.From2D(item.vertexTL, pos2d, Vector3.Zero, scale);
        SimpleVertex tr = SimpleVertex.From2D(item.vertexTR, pos2d, Vector3.Zero, scale);
        SimpleVertex bl = SimpleVertex.From2D(item.vertexBL, pos2d, Vector3.Zero, scale);
        SimpleVertex br = SimpleVertex.From2D(item.vertexBR, pos2d, Vector3.Zero, scale);
        Util.Swap(ref tl.TexCoord, ref tr.TexCoord);
        Util.Swap(ref bl.TexCoord, ref br.TexCoord);
        data.Vertices.AddRange([tl, tr, bl, br, bl, tr]);
    }

    internal void AddSprite(Vector2 pos2d, Vector3 pos, Matrix orientation, int layer, SpriteBatchItem item, float scale = 1)
    {
        var data = sprites.GetOrCreateValue(item.Texture);
        data.Instances.Add(new()
        {
            Position = pos,
            Layer = layer,
            Orientation = orientation,
        });

        SimpleVertex tl = SimpleVertex.From2D(item.vertexTL, pos2d, Vector3.Zero, scale);
        SimpleVertex tr = SimpleVertex.From2D(item.vertexTR, pos2d, Vector3.Zero, scale);
        SimpleVertex bl = SimpleVertex.From2D(item.vertexBL, pos2d, Vector3.Zero, scale);
        SimpleVertex br = SimpleVertex.From2D(item.vertexBR, pos2d, Vector3.Zero, scale);
        Util.Swap(ref tl.TexCoord, ref tr.TexCoord);
        Util.Swap(ref bl.TexCoord, ref br.TexCoord);
        data.Vertices.AddRange([tl, tr, bl, br, bl, tr]);
    }

    public void UpdateInstanced(int instanceId, Matrix transform, Color? color = null)
    {
        color ??= Color.White;
        if (instanceId < 0) return;
        instances[instanceId].Batch.instances[instances[instanceId].Instance] = new()
        {
            Transform = transform,
            Color = color.Value,
            StaysVisibleAfterFrame = instances[instanceId].Batch.instances[instances[instanceId].Instance].StaysVisibleAfterFrame,
        };
    }

    public void UpdateDirect(int instanceId, Matrix transform, Color? color = null)
    {
        color ??= Color.White;
        if (instanceId < 0) return;
        var container = (direct[instanceId].HasTransparency ? directTransparent : directOpaque);
        int inst = direct[instanceId].Instance;
        container[inst] = new(container[inst].Action, transform, color.Value, container[inst].StaysVisibleAfterFrame);
    }

    public void PrepareSprites(Matrix worldMatrix, ICamera cam)
    {
        foreach (var sprite in sprites)
        {
            for (int i = 0; i < sprite.Value.Instances.Count; ++i)
            {
                var inst = sprite.Value.Instances[i];
                var pos = inst.Position;

                Matrix transform;
                if (inst.Orientation.HasValue)
                    transform = inst.Orientation.Value * Matrix.CreateTranslation(pos);
                else
                    transform = Matrix.CreateConstrainedBillboard(pos, cam.Position - worldMatrix.Translation, Vector3.Up, cam.Forward, Vector3.Backward);

                for (int iv = 0; iv < 6; ++iv)
                {
                    var v = sprite.Value.Vertices[i * 6 + iv];
                    v.Position = Vector3.Transform(v.Position, transform);
                    v.Position += transform.Forward * (inst.Layer * 0.001f);
                    sprite.Value.Vertices[i * 6 + iv] = v;
                }
            }
        }
    }

    public void DrawBatched(PBREnvironment env, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
    {
        bool isMirrorTransform = worldMatrix.Determinant() < 0;

        var oldDepth = graphics.DepthStencilState;
        var oldRaster = graphics.RasterizerState;

        void DoVerticesBatch( List<VerticesRenderData> data, VertexBuffer instanceVbo, int instanceCount, int? transparentTechnique = null)
        {
            foreach (var entry in data)
            {
                var effect = entry.Effect;
                if (effect is GenericModelEffect generic)
                {
                    if (transparentTechnique.HasValue)
                        effect.CurrentTechnique = effect.Techniques[$"InstancedDrawing_Transparent_{transparentTechnique.Value}"];
                    else
                        effect.CurrentTechnique = effect.Techniques["InstancedDrawing"];
                }

                ModelInstance.UpdateProjViewTransforms(effect, projectionMatrix, viewMatrix);
                ModelInstance.UpdateWorldTransforms(effect, worldMatrix);
                env.ApplyTo(effect);

                graphics.BlendState = entry.Blend;
                graphics.SetVertexBuffers(new(entry.Vertices), new(instanceVbo, 0, 1));
                graphics.Indices = entry.Indices;
                graphics.RasterizerState = entry.Rasterizer;
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, entry.Vertices.VertexCount / 3, instanceCount);
                }
            }
        }

        void DoModelBatch(List<Effect> effects, List<MeshPart> parts, VertexBuffer instanceVbo, int instanceCount, int? transparentTechnique = null)
        {
            foreach (var effect in effects)
            {
                if (effect is GenericModelEffect)
                {
                    if (transparentTechnique.HasValue)
                        effect.CurrentTechnique = effect.Techniques[$"InstancedDrawing_Transparent_{transparentTechnique.Value}"];
                    else
                        effect.CurrentTechnique = effect.Techniques["InstancedDrawing"];
                }

                ModelInstance.UpdateProjViewTransforms(effect, projectionMatrix, viewMatrix);
                ModelInstance.UpdateWorldTransforms(effect, worldMatrix);
                env.ApplyTo(effect);
            }
            foreach (var part in parts)
            {
                var geom = part.Geometry as MeshTriangles;
                graphics.BlendState = part.Blending;
                graphics.SetVertexBuffers(new(geom._SharedVertexBuffer), new(instanceVbo, 0, 1));
                graphics.Indices = geom._SharedIndexBuffer;
                graphics.RasterizerState = isMirrorTransform ? geom._BackRasterizer : geom._FrontRasterizer;
                foreach (var pass in part.Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, geom._VertexOffset, geom._IndexOffset, geom._PrimitiveCount, instanceCount);
                }
            }
        }

        void DoSpritesBatch(Texture2D tex, SpriteData sprite, int? transparentTechnique = null)
        {
            var effect = sprite.Effect;
            if (effect is GenericModelEffect generic)
            {
                if (transparentTechnique.HasValue)
                    effect.CurrentTechnique = effect.Techniques[$"SingleDrawing_Transparent_{transparentTechnique.Value}"];
                else
                    effect.CurrentTechnique = effect.Techniques["SingleDrawing"];

                generic.Texture = tex;
            }

            ModelInstance.UpdateProjViewTransforms(effect, projectionMatrix, viewMatrix);
            ModelInstance.UpdateWorldTransforms(effect, worldMatrix);
            env.ApplyTo(effect);

            graphics.BlendState = BlendState.AlphaBlend;
            graphics.RasterizerState = RasterizerState.CullClockwise;
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserPrimitives(PrimitiveType.TriangleList, sprite.Vertices.ToArray(), 0, sprite.Vertices.Count / 3);
            }
        }

        graphics.DepthStencilState = DepthStencilState.Default;
        graphics.RasterizerState = RasterizerState.CullClockwise;
        foreach (var entry in modelBatchData)
        {
            if (entry.Value.instances.Count > 0)
            {
                if (entry.Value.instanceVbo == null || entry.Value.instanceVbo.VertexCount < entry.Value.instances.Count)
                {
                    int vboSize = (int)Math.Pow(2, Math.Max(4, Math.Ceiling(Math.Log2(entry.Value.instances.Count))));

                    entry.Value.instanceVbo?.Dispose();
                    entry.Value.instanceVbo = new VertexBuffer(graphics, typeof(BatchData.InstanceData), vboSize, BufferUsage.WriteOnly);
                }
                entry.Value.instanceVbo.SetData(entry.Value.instances.ToArray());

                DoModelBatch(entry.Value.opaqueEffects, entry.Value.opaqueParts, entry.Value.instanceVbo, entry.Value.instances.Count);
            }
        }
        foreach (var entry in verticesBatchData)
        {
            if (entry.Value.instances.Count > 0)
            {
                if (entry.Value.instanceVbo == null || entry.Value.instanceVbo.VertexCount < entry.Value.instances.Count)
                {
                    int vboSize = (int)Math.Pow(2, Math.Max(4, Math.Ceiling(Math.Log2(entry.Value.instances.Count))));

                    entry.Value.instanceVbo?.Dispose();
                    entry.Value.instanceVbo = new VertexBuffer(graphics, typeof(BatchData.InstanceData), vboSize, BufferUsage.WriteOnly);
                }
                entry.Value.instanceVbo.SetData(entry.Value.instances.ToArray());

                DoVerticesBatch(entry.Value.opaqueVertices, entry.Value.instanceVbo, entry.Value.instances.Count);
            }
        }
        foreach (var entry in directOpaque)
        {
            entry.Action( env, entry.color, entry.Transform * worldMatrix, viewMatrix, projectionMatrix );
        }

        // TODO: Sort transparent stuff by position?
        foreach (var entry in modelBatchData)
        {
            DoModelBatch(entry.Value.transparentEffects, entry.Value.transparentParts, entry.Value.instanceVbo, entry.Value.instances.Count, transparentTechnique: 1);
        }
        foreach (var entry in verticesBatchData)
        {
            DoVerticesBatch(entry.Value.transparentVertices, entry.Value.instanceVbo, entry.Value.instances.Count, transparentTechnique: 1);
        }

        foreach (var entry in sprites)
        {
            if (entry.Value.Instances.Count == 0)
                continue;
            DoSpritesBatch(entry.Key, entry.Value, transparentTechnique: 1);
        }
        graphics.DepthStencilState = DepthStencilState.DepthRead;
        foreach (var entry in modelBatchData)
        {
            DoModelBatch(entry.Value.transparentEffects, entry.Value.transparentParts, entry.Value.instanceVbo, entry.Value.instances.Count, transparentTechnique: 2);
        }
        foreach (var entry in verticesBatchData)
        {
            DoVerticesBatch(entry.Value.transparentVertices, entry.Value.instanceVbo, entry.Value.instances.Count, transparentTechnique: 2);
        }
        foreach (var entry in directTransparent)
        {
            entry.Action( env, entry.color, entry.Transform * worldMatrix, viewMatrix, projectionMatrix );
        }
        foreach (var entry in sprites)
        {
            if (entry.Value.Instances.Count == 0)
                continue;
            DoSpritesBatch(entry.Key, entry.Value, transparentTechnique: 2);
        }
        graphics.DepthStencilState = oldDepth;
        graphics.RasterizerState = oldRaster;
    }

    public void HideInstancesAfterFrame()
    {
        foreach (var entry in instances)
        {
            if (entry.Batch.instances[entry.Instance].StaysVisibleAfterFrame == 0)
            {
                entry.Batch.instances[entry.Instance] = new()
                {
                    Transform = entry.Batch.instances[entry.Instance].Transform,
                    Color = Color.Transparent,
                    StaysVisibleAfterFrame = entry.Batch.instances[entry.Instance].StaysVisibleAfterFrame,
                };
            }
        }
        foreach (var entry in direct)
        {
            var container = (entry.HasTransparency ? directTransparent : directOpaque);
            if (container[entry.Instance].StaysVisibleAfterFrame == 0)
            {
                container[entry.Instance] = new(container[entry.Instance].Action,
                                                container[entry.Instance].Transform,
                                                Color.Transparent,
                                                container[entry.Instance].StaysVisibleAfterFrame);
            }
        }
        foreach (var entry in sprites)
        {
            entry.Value.Instances.Clear();
            entry.Value.Vertices.Clear();
        }
    }

    public void ClearData()
    {
        foreach (var entry in modelBatchData)
        {
            entry.Value.instances.Clear();
        }
        foreach (var entry in verticesBatchData)
        {
            entry.Value.instances.Clear();
        }
        foreach (var entry in sprites)
        {
            entry.Value.Instances.Clear();
            entry.Value.Vertices.Clear();
        }
        instances.Clear();
        directOpaque.Clear();
        directTransparent.Clear();
        direct.Clear();
    }

    public void Dispose()
    {
        foreach (var entry in modelBatchData)
            entry.Value.Dispose();
        foreach (var entry in verticesBatchData)
            entry.Value.Dispose();
        foreach (var entry in sprites)
            entry.Value.Dispose();

        modelBatchData.Clear();
        verticesBatchData.Clear();
        instances.Clear();
        directOpaque.Clear();
        directTransparent.Clear();
        direct.Clear();
        sprites.Clear();
    }
}
