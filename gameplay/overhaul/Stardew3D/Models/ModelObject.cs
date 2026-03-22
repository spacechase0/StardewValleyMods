using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;

namespace Stardew3D.Models;
public class ModelObject
{
    private ModelManager Manager { get; }
    public string Id { get; }

    private ModelData cachedData = null;
    private ModelRoot cachedModel = null;

    private List<Node> matches = new();
    private List<Dictionary<string, List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)>>> bakedMatches = new();
    public IReadOnlyList<Dictionary<string, List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)>>> Matches => bakedMatches;

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
        bool pushed = false;
        try
        {
            if (cachedData != null)
                return;

            cachedData = ModelData.Get(Id);
            if (cachedData == null)
            {
                Log.Error($"No model data found for {Id}");
                return;
            }

            matches.Clear();
            bakedMatches.Clear();
            if (!string.IsNullOrEmpty(cachedData.ModelFilePath))
            {
                Manager.modelBeingLoaded = cachedData.ModelFilePath;
                Manager.mapperForModelBeingLoaded.Push( cachedData );
                pushed = true;
                cachedModel = ModelRoot.Load(cachedData.ModelFilePath, Manager.modelReadContext);

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
                            for (var nodeCheck = node; nodeCheck != null && nodeCheck != null; nodeCheck = nodeCheck.VisualParent)
                            {
                                if (nodeCheck.Name == null && nodeCheck.VisualParent == null)
                                    break;

                                name = $"/{nodeCheck.Name ?? "null"}" + name;
                            }

                            if (name == cachedData.SubModelPath)
                            {
                                matches.Add(node);
                            }
                        }
                    }
                }

                Matrix additionalTransform = Matrix.CreateScale(cachedData.Scale);
                additionalTransform *= Matrix.CreateRotationX(cachedData.Rotation.X) * Matrix.CreateRotationY(cachedData.Rotation.Y) * Matrix.CreateRotationZ(cachedData.Rotation.Z);
                additionalTransform *= Matrix.CreateTranslation(cachedData.Translation);
                foreach (var entry in matches)
                {
                    var baseTransform = entry.WorldMatrix.ToMonogame().Inverted();
                    {
                        Node toApply = entry;
                        for (int i = cachedData.UseExistingTransformHierarchy; i != 0 && toApply != null; --i, toApply = entry.VisualParent)
                        {
                            baseTransform *= toApply.WorldMatrix.ToMonogame();
                        }
                    }

                    Dictionary<string, List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)>> allResults = new();

                    void GetAllMeshes(IEnumerable<Node> nodes)
                    {
                        foreach (var node in nodes)
                        {
                            string name = "";
                            for (var nodeCheck = node; nodeCheck != null && nodeCheck != null; nodeCheck = nodeCheck.VisualParent)
                            {
                                if (nodeCheck.Name == null && nodeCheck.VisualParent == null)
                                    break;

                                name = $"/{nodeCheck.Name ?? "null"}" + name;
                            }

                            List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)> results = new();
                            if (node.Mesh != null)
                            {
                                bool forceTransparent = cachedData.ForceTransparency.Any(s => name.StartsWith(s));

                                var meshes = Manager.gltfFactory.ReadMeshContent([node.Mesh]);
                                if (meshes.Meshes.Meshes.SelectMany(m => m.Parts).Count() == 0) continue; // No clue why I get an empty mesh as a child of a mesh which has no children pre-export
                                var actual = Manager.deviceMeshFactory.CreateMeshCollection(meshes.Materials, meshes.Meshes);
                                for (int im = 0; im < actual.Count; ++im)
                                {
                                    var mesh = actual[im];
                                    if (forceTransparent)
                                    {
                                        for (int ip = 0; ip < mesh.Count; ++ip)
                                        {
                                            mesh[ip].Blending = BlendState.AlphaBlend;
                                        }
                                    }
                                    results.Add(new(mesh, baseTransform * node.WorldMatrix.ToMonogame() * additionalTransform));
                                }
                            }

                            if (!allResults.TryAdd(name, results))
                            {
                                for (int i = 1; i < 100; ++i)
                                {
                                    if (allResults.TryAdd($"{name}{i}", results))
                                        break;
                                }
                            }

                            GetAllMeshes(node.VisualChildren);
                        }
                    }

                    GetAllMeshes([entry]);
                    bakedMatches.Add(allResults);
                }
            }

            foreach (var entry in cachedData.OtherModels)
            {
                Matrix mat = Matrix.Identity;
                mat *= Matrix.CreateScale(entry.Scale);
                mat *= Matrix.CreateRotationX(entry.Rotation.X) * Matrix.CreateRotationY(entry.Rotation.Y) * Matrix.CreateRotationZ(entry.Rotation.Z);
                mat *= Matrix.CreateTranslation(entry.Translation);
                try
                {
                    Manager.mapperForModelBeingLoaded.Push(entry);
                    var model = Manager.RequestModel(entry.ModelId);
                    foreach (var match in model.Matches)
                    {
                        Dictionary<string, List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)>> bakedMatch = new();
                        foreach (var matchEntry in match)
                        {
                            List<(MonoScene.Graphics.Mesh Mesh, Matrix Transform)> results = new();
                            foreach (var submodel in matchEntry.Value)
                            {
                                results.Add(new(submodel.Mesh, submodel.Transform * mat));
                            }
                            bakedMatch.Add($"{entry.ModelId}/{matchEntry.Key}", results);
                        }
                        bakedMatches.Add(bakedMatch);
                    }
                }
                finally
                {
                    Manager.mapperForModelBeingLoaded.Pop();
                }
            }

            foreach (var entry in bakedMatches)
            {
                foreach (var matchEntry in entry)
                {
                    foreach (var mesh in matchEntry.Value)
                    {
                        // These aren't up to date if we manually changed stuff earlier.
                        mesh.Mesh._OpaquePrimitives = null;
                        mesh.Mesh._TranslucidPrimitives = null;

                        for (int i = 0; i < mesh.Mesh.Count; ++i)
                        {
                            var part = mesh.Mesh[i];

                            var newEffect = Mod.State.GenericModelEffect.Clone() as GenericModelEffect;
                            newEffect.CurrentTechnique = newEffect.Techniques["SingleDrawing"];
                            switch (part.Effect)
                            {
                                case GenericModelEffect effect:
                                    newEffect.Texture = effect.Texture;
                                    break;
                                case BasicEffect effect:
                                    newEffect.Texture = effect.Texture;
                                    break;
                                case AlphaTestEffect effect:
                                    newEffect.Texture = effect.Texture;
                                    break;
                                case SkinnedEffect effect:
                                    newEffect.Texture = effect.Texture;
                                    break;
                            }

                            part.Effect = newEffect;
                        }
                    }
                }
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
            if ( pushed )
                Manager.mapperForModelBeingLoaded.Pop();
        }
    }

    private RenderBatcher immediateBatch;
    public void Draw(PBREnvironment env, Matrix transform, Color? color = null, int whichMatch = 0)
    {
        if (immediateBatch == null)
            immediateBatch = new(Game1.graphics.GraphicsDevice);
        else
            immediateBatch.ClearData();

        Draw(immediateBatch, Matrix.Identity, color, whichMatch);
        immediateBatch.DrawBatched(env, transform, Manager.DrawContext._View, Manager.DrawContext.GetProjectionMatrix());
    }

    public class ModelObjectInstance
    {
        public int[] BatcherInstances { get; internal init; }
        public int WhichMatch { get; internal init; }
    }

    public ModelObjectInstance Draw(RenderBatcher batch, Matrix transform, Color? color = null, int whichMatch = 0)
    {
        color ??= Color.White;
        whichMatch %= Matches.Count;

        List<int> forThis = new();
        foreach (var entry in Matches[whichMatch])
        {
            foreach (var matchEntry in entry.Value)
            {
                forThis.Add(batch.AddInstancedModel(matchEntry.Mesh, matchEntry.Transform * transform, color.Value));
            }
        }
        return new()
        {
            BatcherInstances = forThis.ToArray(),
            WhichMatch = whichMatch,
        };
    }

    public void Update(RenderBatcher batch, ModelObjectInstance instance, Matrix transform, Color? color = null)
    {
        color ??= Color.White;

        int i = 0;
        foreach (var entry in Matches[instance.WhichMatch])
        {
            foreach (var matchEntry in entry.Value)
            {
                batch.UpdateInstanced(instance.BatcherInstances[i], matchEntry.Transform * transform, color.Value);
                ++i;
            }
        }
    }
}
