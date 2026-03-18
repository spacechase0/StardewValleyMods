using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.FloorWallAssociations;

[DictionaryAssetData<FloorWallAssociationData>("FloorWallAssociations", "Maps/walls_and_floors:&", OwnedAsset = true)]
internal partial class Tilesheet_WallsAndFloors : SpaceShared.Content.BaseDictionaryAssetData
{
    public FloorWallAssociationData _352 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _353 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _336 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _337 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
}
