using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "($/Building)Farmhouse/&", OwnedAsset = true)]
internal partial class Buildings_FarmHouse : SpaceShared.Content.BaseDictionaryAssetData
{
    public ModelData _0 => new()
    {
        ModelFilePath = $"{ModId}:assets/buildings/farmhouse.gltf",
        SubModelPath = "/0",
        UseExistingTransformHierarchy = 1,
    };
}
