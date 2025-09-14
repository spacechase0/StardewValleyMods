using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D.Data;
using StardewValley.Characters;

namespace Stardew3D.Models;
public class ModelObject
{
    private ModelManager Manager { get; }
    public string Id { get; }

    private ModelData cachedData = null;
    private ModelRoot cachedModel = null;

    private List<Node> matches = new();
    private List<List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)>> bakedMatches = new();
    public IReadOnlyList<List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)>> Matches => bakedMatches;

    internal ModelObject( ModelManager manager, string id )
    {
        Manager = manager;
        Id = id;
        Load();
    }

    internal void Invalidate()
    {
        cachedData = null;
        cachedModel = null;
        matches.Clear();
        bakedMatches.Clear();
    }

    public void Load()
    {
        try
        {
            if (cachedData != null)
                return;

            Mod.Instance.ModelData.TryGetValue(Id, out cachedData);
            if (cachedData == null)
            {
                Log.Error($"No model data found for {Id}");
                return;
            }

            Manager.modelBeingLoaded = cachedData.ModelFilePath;
            Manager.mapperForModelBeingLoaded = cachedData;
            cachedModel = ModelRoot.Load(cachedData.ModelFilePath, Manager.modelReadContext);

            matches.Clear();
            if (string.IsNullOrEmpty(cachedData.SubModelPath))
            {
                var scene = cachedModel.DefaultScene;
                foreach (var child in scene.VisualChildren)
                {
                    matches.Add(child);
                }
            }
            else
            {
                matches.AddRange(cachedModel.LogicalScenes.Where(scene => scene.Name == cachedData.SubModelPath).SelectMany(scene => scene.VisualChildren));

                if (matches.Count == 0)
                {
                    var nodes = cachedModel.LogicalNodes;
                    foreach (var node in nodes)
                    {
                        string name = "";
                        for (var nodeCheck = node; nodeCheck != null && nodeCheck != nodeCheck.VisualRoot; nodeCheck = nodeCheck.VisualParent)
                        {
                            name = $"/{nodeCheck.Name ?? "null"}" + name;
                        }

                        if (name == cachedData.SubModelPath)
                        {
                            matches.Add(node);
                        }
                    }
                }
            }

            bakedMatches.Clear();

            Matrix matrixTransform = Matrix.CreateScale(cachedData.Scale);
            matrixTransform *= Matrix.CreateRotationX(cachedData.Rotation.X) * Matrix.CreateRotationY(cachedData.Rotation.Y) * Matrix.CreateRotationZ(cachedData.Rotation.Z);
            matrixTransform *= Matrix.CreateTranslation(cachedData.Translation);
            foreach (var entry in matches)
            {
                var inverseEntry = entry.WorldMatrix.ToMonogame().Invert();
                List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)> results = new();

                void GetAllMeshes(IEnumerable<Node> nodes)
                {
                    foreach (var node in nodes)
                    {
                        if (node.Mesh != null)
                        {
                            var meshes = Manager.gltfFactory.ReadMeshContent([node.Mesh]);
                            var actual = Manager.deviceMeshFactory.CreateMeshCollection(meshes.Materials, meshes.Meshes);
                            for (int im = 0; im < actual.Count; ++im)
                            {
                                var mesh = actual[im];
                                for (int ip = 0; ip < mesh.Count; ++ip)
                                {
                                    mesh[ip].Blending = BlendState.AlphaBlend;
                                }
                                results.Add(new(mesh, inverseEntry * node.WorldMatrix.ToMonogame() * matrixTransform));
                            }
                        }
                        GetAllMeshes(node.VisualChildren);
                    }
                }

                GetAllMeshes([entry]);
                bakedMatches.Add(results);
            }
        }
        catch
        {
            Invalidate();
            throw;
        }
        finally
        {
            Manager.modelBeingLoaded = null;
            Manager.mapperForModelBeingLoaded = null;
        }
    }

    public void Draw(PBREnvironment env, Matrix transform, int whichMatch = 0)
    {
        whichMatch %= Matches.Count;

        foreach (var mesh in Matches[whichMatch])
        {
            Manager.DrawContext.DrawMesh(env, mesh.Mesh, mesh.Transform * transform);
        }
    }
}
