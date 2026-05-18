using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.FloorWallAssociations;

[DictionaryAssetData<FloorWallAssociationData>("FloorWallAssociations", "Maps/townInterior:&", OwnedAsset = true)]
internal partial class Tilesheet_TownInterior : SpaceShared.Content.BaseDictionaryAssetData
{
    public FloorWallAssociationData _574 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _575 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _606 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _607 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _638 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _639 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _1043 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
}
