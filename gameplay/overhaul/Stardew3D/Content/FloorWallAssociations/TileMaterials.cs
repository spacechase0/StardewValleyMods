using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.FloorWallAssociations;

[DictionaryAssetData<FloorWallAssociationData>("FloorWallAssociations", OwnedAsset = true)]
internal partial class TileMaterials : SpaceShared.Content.BaseDictionaryAssetData
{
    public FloorWallAssociationData Wood => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData Dirt => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData Grass => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData Stone => new() { WallDefinitionId = $"{ModId}/GenericCaveWall" };
}
