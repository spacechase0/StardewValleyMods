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
    public FloorWallAssociationData _412 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _413 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _404 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _405 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _420 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _421 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _428 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _429 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
}
