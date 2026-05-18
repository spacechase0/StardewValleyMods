using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.FloorWallAssociations;

[DictionaryAssetData<FloorWallAssociationData>("FloorWallAssociations", "Maps/farmhouse_tiles:&", OwnedAsset = true)]
internal partial class Tilesheet_FarmhouseTiles : SpaceShared.Content.BaseDictionaryAssetData
{
    public FloorWallAssociationData _181 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
}
