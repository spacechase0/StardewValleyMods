using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared.Attributes;
using SpaceShared.Content;
using Stardew3D.Data;

namespace Stardew3D.Content.FloorWallAssociations;

[DictionaryAssetData<FloorWallAssociationData>("FloorWallAssociations", OwnedAsset = true)]
internal partial class TileMaterials : SpaceShared.Content.BaseDictionaryAssetData
{
    public FloorWallAssociationData Wood => new() { WallDefinitionId = $"{ModId}/GenericHouseWall" };
    public FloorWallAssociationData Dirt => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData Grass => new() { WallDefinitionId = $"{ModId}/GenericCliffWall" };
    public FloorWallAssociationData Stone => new() { WallDefinitionId = $"{ModId}/GenericCaveWall" };
}
