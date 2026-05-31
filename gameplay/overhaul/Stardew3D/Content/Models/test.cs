using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "$/&", OwnedAsset = true)]
internal partial class test : SpaceShared.Content.BaseDictionaryAssetData
{
    /*
    [DictionaryAssetDataKey("($/Tree)&")]
    public ModelData _1 => new()
    {
        ModelFilePath = $"{ModId}:assets/test sprite version/oak grown.glb",
    };
    */

    [DictionaryAssetDataKey("(F)&")]
    public ModelData _800 => new()
    {
        ModelFilePath = $"{ModId}:assets/test sprite version/winter table.glb",
        Scale = new Vector3(0.5f),
        Translation = new(0, 0, -0.25f),
    };

    [DictionaryAssetDataKey("($/Building)&")]
    public ModelData Shed => new()
    {
        ModelFilePath = $"{ModId}:assets/test sprite version/small shed.glb",
        Scale = new Vector3(1, 1, 3f / 7) * 0.35f,
    };
}
