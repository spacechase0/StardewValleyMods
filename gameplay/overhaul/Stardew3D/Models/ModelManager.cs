using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics.Pipeline;
using SharpGLTF.Schema2;
using SpaceShared;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Stardew3D.Models;
public class ModelManager
{
    internal GltfModelFactory gltfFactory = null;
    internal ReadContext modelReadContext = null;
    internal DeviceMeshFactory deviceMeshFactory = null;
    public ModelDrawingContext DrawContext { get; private set; } = null;

    internal Dictionary<string, ModelObject> models = new();

    internal string modelBeingLoaded = null;
    internal Stack<IModelMapping> mapperForModelBeingLoaded = new();

    internal ModelManager()
    {
        gltfFactory = new(Game1.graphics.GraphicsDevice);

        modelReadContext = ReadContext.Create(ReadFileForModel);
        modelReadContext.ImageDecoder = ReadImageForModel;
        modelReadContext.Validation = SharpGLTF.Validation.ValidationMode.Skip; // Otherwise it complains about no actual images for stuff that should go through the game content pipeline

        deviceMeshFactory = new ClassicMeshFactory(Game1.graphics.GraphicsDevice);
        AccessTools.Field(deviceMeshFactory.GetType(), "_TextureFactory").SetValue(deviceMeshFactory, new ContentPipelineTextureFactory(Game1.graphics.GraphicsDevice));
        DrawContext = new(Game1.graphics.GraphicsDevice);
    }

    public ModelObject RequestModel(string id)
    {
        id = id.Replace('\\', '/');
        if (!models.TryGetValue(id, out var model))
        {
            models.Add(id, model = new ModelObject(this, id));
        }
        return model;
    }

    // TODO: Use model associations instead

    private ArraySegment<byte> ReadFileForModel(string assetName)
    {
        foreach (var entry in mapperForModelBeingLoaded)
        {
            if (entry.TextureMap.TryGetValue(PathUtilities.NormalizePath(assetName), out string newAssetName))
            {
                assetName = newAssetName;
            }
        }

        int colon = modelBeingLoaded.IndexOf(':');
        string assetCtx = modelBeingLoaded.Substring(0, colon).ToLower();
        string assetDir = Path.GetDirectoryName(modelBeingLoaded.Substring(colon + 1));
        string fullAssetPath = $"SMAPI/{assetCtx}/{assetDir}";

        colon = assetName.IndexOf(':');
        if (colon == -1)
        {
            string resolved = null;
            if (Game1.content.DoesAssetExist<Texture2D>(assetName)) resolved = assetName;
            else if (Game1.content.DoesAssetExist<Texture2D>(Path.Combine(fullAssetPath, assetName))) resolved = Path.Combine(fullAssetPath, assetName);

            if (resolved == null)
                throw new ContentLoadException($"While loading {modelBeingLoaded}, texture \"{assetName}\" does not exist.");

            return Encoding.ASCII.GetBytes($"MGTEX:{resolved}");
        }
        else
        {
            string modId = assetName.Substring(0, colon), file = assetName.Substring(colon + 1);
            if (!Mod.Instance.Helper.ModRegistry.IsLoaded(modId))
            {
                throw new ContentLoadException($"Mod \"{modId}\" not present.");
            }

            if (file.EndsWith(".gltf"))
            {
                var filePath = Util.FetchFullPath(Mod.Instance.Helper.ModRegistry, assetName, ':');
                if (!File.Exists(filePath))
                {
                    throw new ContentLoadException($"Model \"{assetName}\" does not exist.");
                }

                return File.ReadAllBytes(filePath);
            }
            else
            {
                // Assume texture until I add more stuff
                if (!Game1.content.DoesAssetExist<Texture2D>($"{fullAssetPath}/{file}"))
                {
                    throw new ContentLoadException($"While loading {modelBeingLoaded}, texture \"{assetName}\" does not exist.");
                }
                return Encoding.ASCII.GetBytes($"MGTEX:{fullAssetPath}/{file}");
            }
        }
    }

    private bool ReadImageForModel(Image image)
    {
        // It'll be loaded in the normal content pipeline later
        return true;
    }
}
