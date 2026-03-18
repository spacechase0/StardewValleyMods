using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.FloorWallAssociations;

[DictionaryAssetData<FloorWallAssociationData>("FloorWallAssociations", "Maps/spring_outdoorsTileSheet2:&", OwnedAsset = true)]
internal partial class Tilesheet_SpringOutdoors2 : SpaceShared.Content.BaseDictionaryAssetData
{
    public FloorWallAssociationData _752 => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData _753 => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData _736 => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData _737 => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData _741 => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };

}
