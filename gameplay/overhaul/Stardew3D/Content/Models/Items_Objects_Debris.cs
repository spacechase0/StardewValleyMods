using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "(O)&", OwnedAsset = true)]
internal partial class Items_Objects_Debris : SpaceShared.Content.BaseDictionaryAssetData
{
    [DictionaryAssetDataKey("Debris/Stone/1")]
    public ModelData stone1 => new()
    {
        ModelFilePath = $"{ModId}:assets/Debris.gltf",
        SubModelPath = "/stone/stone1",
    };

    [DictionaryAssetDataKey("Debris/Stone/2")]
    public ModelData stone2 => new()
    {
        ModelFilePath = $"{ModId}:assets/Debris.gltf",
        SubModelPath = "/stone/stone2",
    };

    [DictionaryAssetDataKey("Debris/Wood/1")]
    public ModelData wood1 => new()
    {
        ModelFilePath = $"{ModId}:assets/Debris.gltf",
        SubModelPath = "/wood/wood1",
        Rotation = new(0, MathHelper.ToRadians(-45), 0),
    };

    [DictionaryAssetDataKey("Debris/Wood/2")]
    public ModelData wood2 => new()
    {
        ModelFilePath = $"{ModId}:assets/Debris.gltf",
        SubModelPath = "/wood/wood2",
        Rotation = new(0, MathHelper.ToRadians(45), 0),
    };

    public ModelData _450 => new()
    {
        OtherModels =
        [
            new()
            {
                ModelId = "Debris/Stone/1",
                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Stone/1",
                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Stone/1",
                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Stone/1",
                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Stone/2",
                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Stone/2",
                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Stone/2",
                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Stone/2",
                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
            },
        ],
    };

    public ModelData _343 => new()
    {
        OtherModels =
        [
            new()
            {
                ModelId = "(O)450",
            },
        ],
    };

    public ModelData _294 => new()
    {
        OtherModels =
        [
            new()
            {
                ModelId = "Debris/Wood/1",
                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Wood/1",
                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Wood/1",
                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Wood/1",
                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Wood/2",
                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Wood/2",
                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Wood/2",
                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
            },
            new()
            {
                ModelId = "Debris/Wood/2",
                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
            },
        ]
    };

    public ModelData _295 => new()
    {
        OtherModels =
        [
            new()
            {
                ModelId = "(O)294",
            },
        ],
    };
}
