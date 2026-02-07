using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared.Attributes;
using Stardew3D.Data;

namespace Stardew3D.Content.FloorWallAssociations;

[DictionaryAssetData<FloorWallAssociationData>("FloorWallAssociations", "Maps/walls_and_floors:&", OwnedAsset = true)]
internal partial class Tilesheet_WallsAndFloors : SpaceShared.Content.BaseDictionaryAssetData
{
    public FloorWallAssociationData _352 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _353 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _336 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData _337 => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
}
