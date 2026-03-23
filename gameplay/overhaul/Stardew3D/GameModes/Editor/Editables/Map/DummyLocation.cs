using Microsoft.Xna.Framework;
using Stardew3D.Utilities;
using StardewValley;
using xTile.Tiles;

namespace Stardew3D.GameModes.Editor.Editables.Map;

public class DummyLocation : GameLocation
{
    public LocalizedContentManager MapLoader;

    // modifierType = 0/1/2/3/4 none/left/down/right/up
    // [modifierType][tilePos] = dataIndex
    public float[][] floorHeightmap;
    public float[][] ceilingHeightmap;

    // side = left up right down
    // modifierType = 0/1/2 base/first/second
    // [side][modifierType][tilePos] = dataIndex
    public float[][][] wallSizes;
    public float[][][] wallOffsets;

    public DummyLocation(LocalizedContentManager mapLoader, string locName, string mapAsset)
    {
        name.Value = locName;
        mapPath.Value = mapAsset;
        MapLoader = mapLoader;

        _mapPathDirty = false;
        reloadMap();
    }

    protected override LocalizedContentManager getMapLoader()
    {
        return MapLoader.CreateTemporary();
    }

    public float GetDimensionData(DimensionUtils.TileType tileType, Point tile, TileSpot modType = TileSpot.Center)
    {
        string layerName = $"{Mod.Instance.ModManifest.UniqueID}/{tileType}Data_{modType}";
        var layer = Map.GetLayer(layerName);
        if (layer == null)
            return 0;

        if (tile.X < 0 || tile.Y < 0 || tile.X >= layer.LayerWidth || tile.Y >= layer.LayerHeight)
            return 0;

        int ind = layer.Tiles[tile.X, tile.Y]?.TileIndex ?? -1;
        return DimensionUtils.GetValueForDataTileIndex(ind);
    }

    public void ModifyDimensionData(DimensionUtils.TileType tileType, Point tile, float amount, TileSpot modType = TileSpot.Center)
    {
        string layerName = $"{Mod.Instance.ModManifest.UniqueID}/{tileType}Data_{modType}";
        var layer = Map.GetLayer(layerName);
        if (layer == null)
            Map.AddLayer(layer = new(layerName, Map, Map.Layers[0].LayerSize, Map.Layers[0].TileSize));

        if (tile.X < 0 || tile.Y < 0 || tile.X >= layer.LayerWidth || tile.Y >= layer.LayerHeight)
            return;

        int ind = layer.Tiles[tile.X, tile.Y]?.TileIndex ?? -1;
        float val = DimensionUtils.GetValueForDataTileIndex(ind);
        val += amount;
        ind = DimensionUtils.GetDataTileIndexForValue(val);

        TileSheet ts = Map.TileSheets.FirstOrDefault(ts => ts.Id.EndsWith("dataValues3d"));
        if (ts == null)
            Map.AddTileSheet(ts = new("dataValues3d", Map, "ThirdDimensionData\\data", new(32, 16), new(16, 16)));

        layer.Tiles[tile.X, tile.Y] = new StaticTile(layer, ts, BlendMode.Alpha, ind);
    }

    public void SetDimensionData(DimensionUtils.TileType tileType, Point tile, float? value, TileSpot modType = TileSpot.Center)
    {
        string layerName = $"{Mod.Instance.ModManifest.UniqueID}/{tileType}Data_{modType}";
        var layer = Map.GetLayer(layerName);
        if (layer == null)
            Map.AddLayer(layer = new(layerName, Map, Map.Layers[0].LayerSize, Map.Layers[0].TileSize));

        if (tile.X < 0 || tile.Y < 0 || tile.X >= layer.LayerWidth || tile.Y >= layer.LayerHeight)
            return;

        TileSheet ts = Map.TileSheets.FirstOrDefault(ts => ts.Id.EndsWith("dataValues3d"));
        if (ts == null)
            Map.AddTileSheet(ts = new("dataValues3d", Map, "ThirdDimensionData\\floor", new(32, 16), new(16, 16)));

        if (value.HasValue)
        {
            int ind = DimensionUtils.GetDataTileIndexForValue(value.Value);
            layer.Tiles[tile.X, tile.Y] = new StaticTile(layer, ts, BlendMode.Alpha, ind);
        }
        else
        {
            layer.Tiles[tile.X, tile.Y] = null;
        }
    }

#if false
    public override void OnMapLoad(xTile.Map map)
    {
        base.OnMapLoad(map);
        int arrSize = Map.Layers[0].LayerWidth * Map.Layers[0].LayerHeight;
        floorHeightmap = [new float[arrSize], new float[arrSize], new float[arrSize], new float[arrSize], new float[arrSize]];
        ceilingHeightmap = [new float[arrSize], new float[arrSize], new float[arrSize], new float[arrSize], new float[arrSize]];
        wallSizes = [new float[4][], new float[4][], new float[4][], new float[4][]];
        wallOffsets = [new float[4][], new float[4][], new float[4][], new float[4][]];
        for (int i = 0; i < 4; ++i)
        {
            wallSizes[i] = new float[3][];
            wallOffsets[i] = new float[3][];
            for (int j = 0; j < 3; ++j)
            {
                wallSizes[i][j] = new float[arrSize];
                wallOffsets[i][j] = new float[arrSize];
            }
        }

        Dictionary<string, float[]> dataLayerNames = new()
        {
            [$"{Mod.Instance.ModManifest.UniqueID}/FloorData"] = floorHeightmap[0],
            [$"{Mod.Instance.ModManifest.UniqueID}/CeilingData"] = ceilingHeightmap[0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_West_Size"] = wallSizes[0][0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_West_Offset"] = wallOffsets[0][0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_North_Size"] = wallSizes[1][0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_North_Offset"] = wallOffsets[1][0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_East_Size"] = wallSizes[2][0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_East_Offset"] = wallOffsets[2][0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_South_Size"] = wallSizes[3][0],
            [$"{Mod.Instance.ModManifest.UniqueID}/WallData_South_Offset"] = wallOffsets[3][0],
        };
        string[] modifierLayerNames =
        [
            $"{Mod.Instance.ModManifest.UniqueID}/FloorModifierData",
            $"{Mod.Instance.ModManifest.UniqueID}/CeilingModifierData",
            $"{Mod.Instance.ModManifest.UniqueID}/WallSizeModifierData",
            $"{Mod.Instance.ModManifest.UniqueID}/WallOffsetModifierData",
        ];

        foreach (var layer in Map.Layers)
        {
            if (!layer.Id.StartsWith($"{Mod.Instance.ModManifest.UniqueID}/"))
                continue;

            var baseDataKey = dataLayerNames.Keys.FirstOrDefault(n => layer.Id == n || layer.Id.StartsWith($"{n}_");
            if (baseDataKey != null)
            {
                var data = dataLayerNames[baseDataKey];
                for (int i = 0; i < arrSize; ++i)
                {
                    int ix = i % layer.LayerWidth, iy = i / layer.LayerHeight;
                    if (layer.Tiles[ix, iy] is not StaticTile tile)
                        continue;

                    data[i] = DimensionUtils.GetValueForDataTileIndex(tile.TileIndex);
                }
            }
            int[][] arr;
            switch (layer.Id)
            {
                case :
                default: continue;
            }
        }
    }
#endif
}
