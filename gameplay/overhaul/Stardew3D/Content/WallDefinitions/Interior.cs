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

    public WallDefinitionData BathHouseEntry_Wall => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 208, 80, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 208, 96, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 208, 112, 16, 16 ) },
        ],
    };

    public WallDefinitionData BathHouseEntry_DoorW => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 208, 80, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 144, 208, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 144, 224, 16, 16 ) },
        ],
    };

    public WallDefinitionData BathHouseEntry_DoorM => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 208, 80, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 160, 208, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 160, 224, 16, 16 ) },
        ],
    };

    public WallDefinitionData BathHouseEntry_WallCrack => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 208, 80, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 208, 96, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 192, 112, 16, 16 ) },
        ],
    };

    public WallDefinitionData BathHouse_ShowerWall => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 64, 208, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 64, 224, 16, 16 ) },
        ],
    };

    public WallDefinitionData BathHouse_ShowerHead => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 80, 208, 16, 16 ) },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 64, 208, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/bathhouse_tiles", TextureRegion = new( 80, 224, 16, 16 ) },
        ],
    };

    public WallDefinitionData Trailer_MainWallpaper => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 448, 288, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 288, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData Trailer_PennyWallpaper => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 160, 448, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 160, 464, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData TrailerBig_EntryWallpaper => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 976, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 1008, 16, 12 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 1020, 16, 4 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData WizardHouse_Walls => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 288, 576, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 272, 576, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 448, 10, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 448, 16, 16, 16 ) },
        ],
    };

    public WallDefinitionData WizardHouseBasement_Wall1 => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 688, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 704, 16, 10 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 714, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 432, 720, 16, 16 ) },
        ],
    };

    public WallDefinitionData WizardHouseBasement_Wall2 => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 448, 688, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 448, 704, 16, 10 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 448, 714, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 448, 720, 16, 16 ) },
        ],
    };

    public WallDefinitionData WizardHouseBasement_Ladder => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 144, 640, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 144, 656, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 144, 672, 16, 16 ) },
        ],
    };

    public WallDefinitionData SpouseRoom_Penny => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 112, 16, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 112, 32, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 112, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_MainWallpaper => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 1056, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 144, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_AltarRoom => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 384, 288, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 384, 304, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_AltarRoom_Curtain => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 384, 272, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
        ],
    };

    public WallDefinitionData SeedShop_Kitchen => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 112, 16, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 112, 32, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 112, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_ParentBedroom => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 336, 288, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 336, 304, 16, 16 ), ContinuationMode = WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_AbigailBedroom_Empty => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 480, 112, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 480, 128, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 480, 144, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_AbigailBedroom_Shell => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 480, 112, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 480, 128, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 208, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_AbigailBedroom_Fish => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 208, 16, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 208, 32, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 480, 144, 16, 16 ) },
        ],
    };

    public WallDefinitionData SeedShop_AbigailBedroom_FishShell => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 208, 16, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 208, 32, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 208, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData SebastianRoom => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 352, 416, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 352, 432, 16, 10 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 448, 10, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 448, 16, 16, 16 ) },
        ],
    };

    public WallDefinitionData Saloon_Main => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 352, 368, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 160, 160, 16, 10 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData Saloon_Bedroom => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 448, 0, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 448, 16, 16, 10 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 48, 16, 16 ) },
        ],
    };

    public WallDefinitionData Saloon_Arcade => new()
    {
        VerticalSegments =
        [
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 160, 176, 16, 16 ) },
            new() { Tilesheet = "Maps/townInterior_2", TextureRegion = new( 160, 192, 16, 10 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 42, 16, 6 ) },
            new() { Tilesheet = "Maps/townInterior", TextureRegion = new( 256, 48, 16, 16 ) },
        ],
    };
}
