using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.WallDefinitions;

[DictionaryAssetData<WallDefinitionData>("WallDefinitions", "$/&", OwnedAsset = true)]
internal partial class Interior : SpaceShared.Content.BaseDictionaryAssetData
{
    public WallDefinitionData BusTunnelWall => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/spring_outdoorsTileSheet", TextureRegion = new( 80, 1088, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BusTunnelEdgeWall => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/spring_outdoorsTileSheet", TextureRegion = new( 96, 1072, 16, 9 ) },
        ],
    };
}
