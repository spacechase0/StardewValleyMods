using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "($/ResourceClump)Maps/springobjects:&", OwnedAsset = true)]
internal partial class ResourceClumps_SpringObjects : SpaceShared.Content.BaseDictionaryAssetData
{
    [DictionaryAssetDataKey("ResourceClump/Stump")]
    public ModelData stump => new()
    {
        ModelFilePath = $"{ModId}:assets/Debris.gltf",
        SubModelPath = "/stump/stump1",
    };

    [DictionaryAssetDataKey("ResourceClump/Log")]
    public ModelData log => new()
    {
        ModelFilePath = $"{ModId}:assets/Debris.gltf",
        SubModelPath = "/log/log1",
        Rotation = new(0, MathHelper.ToRadians(45), 0),
    };

    [DictionaryAssetDataKey("ResourceClump/Boulder")]
    public ModelData boulder => new()
    {
        ModelFilePath = $"{ModId}:assets/Debris.gltf",
        SubModelPath = "/boulder/boulder1",
    };

    public ModelData _600 => new()
    {
        OtherModels =
        [
            new()
            {
                ModelId = "ResourceClump/Stump",
                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Stump",
                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Stump",
                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Stump",
                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
            },
        ],
    };

    public ModelData _602 => new()
    {
        OtherModels =
        [
            new()
            {
                ModelId = "ResourceClump/Log",
                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Log",
                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Log",
                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Log",
                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
            },
        ],
    };

    public ModelData _672 => new()
    {
        OtherModels =
        [
            new()
            {
                ModelId = "ResourceClump/Boulder",
                Rotation = new( 0, MathHelper.ToRadians( 0 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Boulder",
                Rotation = new( 0, MathHelper.ToRadians( 90 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Boulder",
                Rotation = new( 0, MathHelper.ToRadians( 180 ), 0 ),
            },
            new()
            {
                ModelId = "ResourceClump/Boulder",
                Rotation = new( 0, MathHelper.ToRadians( 270 ), 0 ),
            },
        ],
    };
}
