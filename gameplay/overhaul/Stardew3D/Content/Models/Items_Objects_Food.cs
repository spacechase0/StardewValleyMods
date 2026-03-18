using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "(O)&", OwnedAsset = true)]
internal partial class Items_Objects_Food : SpaceShared.Content.BaseDictionaryAssetData
{
    public ModelData _167 => new()
    {
        ModelFilePath = $"{ModId}:assets/objects/JojaCola.gltf",
    };
}
