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
            new() { Tilesheet = "Maps/spring_outdoorsTileSheet", TextureRegion = new( 96, 1072, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BathHousePool_Main => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 128, 0, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 128, 16, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 128, 32, 16, 16 ) },
        ],
    };

    public WallDefinitionData BathHousePool_ArchL => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 16, 128, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 16, 144, 16, 15 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 16, 159, 16, 1 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BathHousePool_ArchM => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 32, 128, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 32, 144, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 32, 159, 16, 1 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BathHousePool_ArchR => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 48, 128, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 48, 144, 16, 15 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 48, 159, 16, 1 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BathHousePool_PanelL => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 160, 0, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 160, 16, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 160, 32, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BathHousePool_PanelM => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 96, 0, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 96, 16, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 96, 32, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BathHousePool_PanelR => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 112, 0, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 112, 16, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 112, 32, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData BathHousePool_Pillar => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 32, 0, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 32, 16, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 32, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData BathHousePool_Pool => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 48, 64, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 48, 112, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
        ],
    };
}
