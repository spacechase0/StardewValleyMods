using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "(F)&", OwnedAsset = true)]
internal partial class Items_Furniture : SpaceShared.Content.BaseDictionaryAssetData
{
    public ModelData _288 => new()
    {
        ModelFilePath = $"{ModId}:assets/furniture/Armchair.gltf",
        SubModelPath = "/BasicArmchair",
        Rotation = new Vector3(0, MathHelper.ToRadians(180), 0), // TODO: temporary
    };

    public ModelData _1466 => new()
    {
        ModelFilePath = $"{ModId}:assets/furniture/TV.gltf",
        SubModelPath = "/BudgetTV",
    };
}
