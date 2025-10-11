using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics;

namespace Stardew3D.Rendering;

public class RenderBatcher : IDisposable
{
    public delegate void RenderNonInstanced(PBREnvironment env, Color color, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix);

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
                                                                new VertexElement(64, VertexElementFormat.Color, VertexElementUsage.Color, 0));
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
    public class GenericRenderData : IDisposable
    {
        public VertexBuffer Vertices { get; set; }
        public IndexBuffer Indices { get; set; }
        public Effect Effect { get; set; }
        public BlendState Blend { get; set; } = BlendState.Opaque;

        public void Dispose()
        {
            Vertices?.Dispose();
            Indices?.Dispose();
            Vertices = null;
            Indices = null;
        }
    }
    private class GenericBatchData : BatchData
    {
        public List<GenericRenderData> opaqueVertices = new();
        public List<GenericRenderData> transparentVertices = new();

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
    private ConditionalWeakTable<Mesh, ModelBatchData> modelBatchData = new();
    private Dictionary<string, GenericBatchData> genericBatchData = new();
    private List<(BatchData Batch, int Instance)> instances = new();
    private List<(RenderNonInstanced Action, Matrix Transform, Color color, byte StaysVisibleAfterFrame)> nonInstancedOpaque = new();
    private List<(RenderNonInstanced Action, Matrix Transform, Color color, byte StaysVisibleAfterFrame)> nonInstancedTransparent = new();
    private List<(bool HasTransparency, int Instance)> nonInstanced = new();

    public RenderBatcher(GraphicsDevice graphics)
    {
        this.graphics = graphics;
    }

    public int AddInstanced(Mesh mesh, Matrix transform, Color col, bool staysVisibleAfterFrame = false)
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

    public bool HasGenericData(string genericId)
    {
        return genericBatchData.ContainsKey(genericId);
    }

    public void AddGenericData(string genericId, List<GenericRenderData> data )
    {
        genericBatchData.Add(genericId, new()
        {
            opaqueVertices = data.Where(d => d.Blend == BlendState.Opaque).ToList(),
            transparentVertices = data.Where(d => d.Blend != BlendState.Opaque).ToList()
        } );
    }

    public int AddInstanced(string genericId, Matrix transform, Color? color = null, bool staysVisibleAfterFrame = false )
    {
        color ??= Color.White;
        genericBatchData[genericId].instances.Add(new() { Transform = transform, Color = color.Value, StaysVisibleAfterFrame = staysVisibleAfterFrame ? (byte)1 : (byte)0 } );
        instances.Add(new(genericBatchData[genericId], genericBatchData[genericId].instances.Count - 1));
        return instances.Count - 1;
    }

    public int AddNonInstanced(RenderNonInstanced custom, Matrix transform, Color? color = null, bool staysVisibleAfterFrame = false, bool hasTransparency = false)
    {
        color ??= Color.White;
        int instance;
        if (hasTransparency)
        {
            instance = nonInstancedTransparent.Count;
            nonInstancedTransparent.Add(new(custom, transform, color.Value, staysVisibleAfterFrame ? (byte)1 : (byte)0));
        }
        else
        {
            instance = nonInstancedOpaque.Count;
            nonInstancedOpaque.Add(new(custom, transform, color.Value, staysVisibleAfterFrame ? (byte)1 : (byte)0));
        }
        nonInstanced.Add(new(hasTransparency, instance));
        return nonInstanced.Count - 1;
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

    public void UpdateNonInstanced(int instanceId, Matrix transform, Color? color = null)
    {
        color ??= Color.White;
        if (instanceId < 0) return;
        var container = (nonInstanced[instanceId].HasTransparency ? nonInstancedTransparent : nonInstancedOpaque);
        int inst = nonInstanced[instanceId].Instance;
        container[inst] = new(container[inst].Action, transform, color.Value, container[inst].StaysVisibleAfterFrame);
    }

    public void DrawBatched(PBREnvironment env, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
    {
        bool isMirrorTransform = worldMatrix.Determinant() < 0;

        var oldDepth = graphics.DepthStencilState;

        void DoGenericBatch( List<GenericRenderData> data, VertexBuffer instanceVbo, int instanceCount)
        {
            foreach (var entry in data)
            {
                var effect = entry.Effect;
                if (effect is GenericModelEffect)
                {
                    effect.CurrentTechnique = effect.Techniques["InstancedDrawing"];
                }

                ModelInstance.UpdateProjViewTransforms(effect, projectionMatrix, viewMatrix);
                ModelInstance.UpdateWorldTransforms(effect, worldMatrix);
                env.ApplyTo(effect);

                graphics.BlendState = entry.Blend;
                graphics.SetVertexBuffers(new(entry.Vertices), new(instanceVbo, 0, 1));
                graphics.Indices = entry.Indices;
                graphics.RasterizerState = RenderHelper.RasterizerState;
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, entry.Vertices.VertexCount / 3, instanceCount);
                }
            }
        }

        void DoModelBatch(List<Effect> effects, List<MeshPart> parts, VertexBuffer instanceVbo, int instanceCount)
        {
            foreach (var effect in effects)
            {
                if (effect is GenericModelEffect)
                {
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

        graphics.DepthStencilState = DepthStencilState.Default;
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
        foreach (var entry in genericBatchData)
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

                DoGenericBatch(entry.Value.opaqueVertices, entry.Value.instanceVbo, entry.Value.instances.Count);
            }
        }
        foreach (var entry in nonInstancedOpaque)
        {
            entry.Action( env, entry.color, entry.Transform * worldMatrix, viewMatrix, projectionMatrix );
        }

        // TODO: Sort transparent stuff by position?
        //graphics.DepthStencilState = DepthStencilState.DepthRead;
        foreach (var entry in modelBatchData)
        {
            DoModelBatch(entry.Value.transparentEffects, entry.Value.transparentParts, entry.Value.instanceVbo, entry.Value.instances.Count);
        }
        foreach (var entry in genericBatchData)
        {
            DoGenericBatch(entry.Value.transparentVertices, entry.Value.instanceVbo, entry.Value.instances.Count);
        }
        foreach (var entry in nonInstancedTransparent)
        {
            entry.Action( env, entry.color, entry.Transform * worldMatrix, viewMatrix, projectionMatrix );
        }

        graphics.DepthStencilState = oldDepth;
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
        foreach (var entry in nonInstanced)
        {
            var container = (entry.HasTransparency ? nonInstancedTransparent : nonInstancedOpaque);
            if (container[entry.Instance].StaysVisibleAfterFrame == 0)
            {
                container[entry.Instance] = new(container[entry.Instance].Action,
                                                container[entry.Instance].Transform,
                                                Color.Transparent,
                                                container[entry.Instance].StaysVisibleAfterFrame);
            }
        }
    }

    public void ClearData()
    {
        foreach (var entry in modelBatchData)
        {
            entry.Value.instances.Clear();
        }
        foreach (var entry in genericBatchData)
        {
            entry.Value.instances.Clear();
        }
        instances.Clear();
        nonInstancedOpaque.Clear();
        nonInstancedTransparent.Clear();
        nonInstanced.Clear();
    }

    public void Dispose()
    {
        foreach (var entry in modelBatchData)
            entry.Value.Dispose();
        foreach (var entry in genericBatchData)
            entry.Value.Dispose();

        modelBatchData.Clear();
        genericBatchData.Clear();
        instances.Clear();
        nonInstancedOpaque.Clear();
        nonInstancedTransparent.Clear();
        nonInstanced.Clear();
    }
}
