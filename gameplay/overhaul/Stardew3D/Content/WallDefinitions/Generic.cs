using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.WallDefinitions;

[DictionaryAssetData<WallDefinitionData>("WallDefinitions", "$/&", OwnedAsset = true)]
internal partial class Generic : SpaceShared.Content.BaseDictionaryAssetData
{
    public WallDefinitionData GenericHouseWall => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/walls_and_floors", TextureRegion = new( 0, 0, 16, 16 * 3 - 7), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/walls_and_floors", TextureRegion = new( 0, 16 * 3 - 7, 16, 4) },
        ],
    };

    public WallDefinitionData GenericCliffWall => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/spring_outdoorsTileSheet", TextureRegion = new( 17 * 16, 18 * 16, 16, 16) },
            new() { Tilesheet = "Maps/spring_outdoorsTileSheet", TextureRegion = new( 17 * 16, 19 * 16, 16, 16), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/spring_outdoorsTileSheet", TextureRegion = new( 17 * 16, 20 * 16, 16, 16) },
        ],
    };

    public WallDefinitionData GenericCaveWall => new()
    {
        VerticalSegments =
        [
            new () { Tilesheet = "Maps/Mines/mine", TextureRegion = new ( 10 * 16, 4 * 16, 16, 16) },
            new () { Tilesheet = "Maps/Mines/mine", TextureRegion = new ( 10 * 16, 5 * 16, 16, 32), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/Mines/mine", TextureRegion = new(10 * 16, 6 * 16, 16, 16) },
        ],
    };

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
