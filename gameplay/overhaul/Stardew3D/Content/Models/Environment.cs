using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "$/&", OwnedAsset = true)]
internal partial class Environment : SpaceShared.Content.BaseDictionaryAssetData
{
    public ModelData Skybox => new()
    {
        ModelFilePath = $"{ModId}:assets/Skybox.gltf",
        //TextureMap = new() { [ "skybox.png" ] = Mod.Instance.Helper.ModContent.GetInternalAssetName("assets/skybox.png").BaseName },
        Scale = new(180),
    };
}
