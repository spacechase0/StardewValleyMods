using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using xTile.Layers;
using xTile.Tiles;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class LocationRenderer : RendererFor<ModelData, GameLocation>
{
    internal class AnimationData
    {
        public int AnimIndexStart;
        public int[] AllVertIndices = new int[0];
        public long FrameTime;
    }
    internal class VertexData
    {
        public List<SimpleVertex> Verts = new();
        public List<int> Indices = new();
        public List<AnimationData> Animations = new();
    }
    internal Dictionary<Texture2D, (VertexBuffer Vertices, IndexBuffer Indices, int[] IndexData, List<AnimationData> Animations)> vbos = new();

    internal VertexBuffer waterVbo;
    internal List<SimpleVertex> waterVertices = new();

    private bool dirty = true;
    public bool IsDirty => dirty;

    public PBREnvironment Environment = PBREnvironment.CreateDefault();

    [Flags]
    public enum ShowMissingType
    {
        None = 0,

        Floor = 1 << 0,
        Ceiling = 1 << 1,
        Water = 1 << 2,
        Walls = 1 << 3,
    }
    public ShowMissingType ShowMissing = ShowMissingType.None;

    public LocationRenderer(GameLocation obj)
        : base(obj)
    {
    }

    public void MarkDirty() { dirty = true; }

    public void Build(bool force = false)
    {
        if (dirty || force)
        {
            RefreshVertices();
        }
    }

    public override void Render(RenderContext ctx)
    {
        if (ctx.Reset)
            dirty = true;

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;
        Game1.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        base.Render(ctx);
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new LocationRenderData(ctx, this);
    }

    private void RefreshVertices()
    {
        DimensionUtils.PositionResult[] floorData = new DimensionUtils.PositionResult[Object.Map.Layers[0].LayerWidth * Object.Map.Layers[0].LayerHeight];
        DimensionUtils.PositionResult[] ceilingData = new DimensionUtils.PositionResult[floorData.Length];
        DimensionUtils.PositionResult[] waterData = new DimensionUtils.PositionResult[floorData.Length];
        for (int iy = 0, ind = 0; iy < Object.Map.Layers[0].LayerHeight; iy++)
        {
            for (int ix = 0; ix < Object.Map.Layers[0].LayerWidth; ++ix, ++ind)
            {
                floorData[ind] = DimensionUtils.GetPositionForTile(Object.Map, new(ix, iy), DimensionUtils.TileType.Floor);
                ceilingData[ind] = DimensionUtils.GetPositionForTile(Object.Map, new(ix, iy), DimensionUtils.TileType.Ceiling);
                waterData[ind] = DimensionUtils.GetPositionForTile(Object.Map, new(ix, iy), DimensionUtils.TileType.Water);
            }
        }

        Dictionary<Texture2D, VertexData> vertices = new();
        BuildFloorsAndCeiling(floorData, ceilingData, waterData, vertices);
        BuildWalls(floorData, ceilingData, waterData, vertices);

        foreach (var key in vbos.Keys)
        {
            var existing = vbos[key];
            existing.IndexData = Array.Empty<int>();
            vbos[key] = existing;
        }

        foreach (var entry in vertices)
        {
            if (entry.Value.Verts.Count == 0)
                continue;

            VertexBuffer vbo = null;
            IndexBuffer ibo = null;
            if (vbos.ContainsKey(entry.Key))
            {
                vbo = vbos[entry.Key].Vertices;
                ibo = vbos[entry.Key].Indices;
            }

            if (entry.Value.Verts.Count > 0 && (vbo == null || vbo.VertexCount < entry.Value.Verts.Count))
            {
                int vboSize = (int)Math.Pow(2, Math.Min(4, Math.Ceiling(Math.Log2(entry.Value.Verts.Count))));
                vbo?.Dispose();
                vbo = new(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), entry.Value.Verts.Count, BufferUsage.WriteOnly);
            }
            vbo.SetData(entry.Value.Verts.ToArray());

            if (entry.Value.Indices.Count > 0 && (ibo == null || ibo.IndexCount < entry.Value.Indices.Count))
            {
                int iboSize = (int)Math.Pow(2, Math.Min(4, Math.Ceiling(Math.Log2(entry.Value.Indices.Count))));
                ibo?.Dispose();
                ibo = new(Game1.graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, entry.Value.Indices.Count, BufferUsage.WriteOnly);
            }
            ibo.SetData(entry.Value.Indices.ToArray());

            if (vbos.ContainsKey(entry.Key))
                vbos[entry.Key] = new(vbo, ibo, entry.Value.Indices.ToArray(), entry.Value.Animations);
            else
                vbos.Add(entry.Key, new(vbo, ibo, entry.Value.Indices.ToArray(), entry.Value.Animations));
        }

        dirty = false;
    }

    private void BuildFloorsAndCeiling(DimensionUtils.PositionResult[] floorData, DimensionUtils.PositionResult[] ceilingData, DimensionUtils.PositionResult[] waterData, Dictionary<Texture2D, VertexData> output)
    {
        const float tuck = 0.00001f;

        waterVertices.Clear();

        Dictionary<string, Texture2D> texLookup = new();

        ShowMissingType[,] missing = new ShowMissingType[Object.Map.Layers[0].LayerWidth, Object.Map.Layers[0].LayerHeight];
        for (int iy = 0; iy < missing.GetLength(1); ++iy)
        {
            for (int ix = 0; ix < missing.GetLength(0); ++ix)
                missing[ix, iy] = ShowMissingType.Ceiling | ShowMissingType.Floor;
        }

        List<xTile.Layers.Layer> applicableLayers = new();
        List<xTile.Layers.Layer> ceilingLayers = new();
        List<xTile.Layers.Layer> waterLayers = new();
        applicableLayers.AddRange(Object.backgroundLayers.Select(kvp => kvp.Key));
        applicableLayers.AddRange(Object.buildingLayers.Select(kvp => kvp.Key));
        //applicableLayers.AddRange(location.frontLayers.Select(kvp => kvp.Key));
        //applicableLayers.AddRange(location.alwaysFrontLayers.Select(kvp => kvp.Key));
        ceilingLayers.AddRange(Object.Map.Layers.Where(l => l.Id == "kittycatcasey.Stardew3D/Ceiling" || l.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_")));
        ceilingLayers.Sort((l1, l2) => (l1.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_") ? int.Parse(l1.Id.Substring("kittycatcasey.Stardew3D/Ceiling_".Length)) : 0) -
                                       (l2.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_") ? int.Parse(l2.Id.Substring("kittycatcasey.Stardew3D/Ceiling_".Length)) : 0));
        waterLayers.AddRange(Object.Map.Layers.Where(l => l.Id == "kittycatcasey.Stardew3D/Water" || l.Id.StartsWith("kittycatcasey.Stardew3D/Water_")));
        waterLayers.Sort((l1, l2) => (l1.Id.StartsWith("kittycatcasey.Stardew3D/Water_") ? int.Parse(l1.Id.Substring("kittycatcasey.Stardew3D/Water_".Length)) : 0) -
                                     (l2.Id.StartsWith("kittycatcasey.Stardew3D/Water_") ? int.Parse(l2.Id.Substring("kittycatcasey.Stardew3D/Water_".Length)) : 0));
        ceilingLayers.Add(new("___dummyceilinglayer", Object.Map, Object.map.Layers[0].LayerSize, Object.map.Layers[0].TileSize));
        applicableLayers.AddRange(ceilingLayers);
        ceilingLayers.Add(new("___dummyfloorlayer", Object.Map, Object.map.Layers[0].LayerSize, Object.map.Layers[0].TileSize));
        for (int iy = 0, ind = 0; iy < Object.Map.Layers[0].LayerSize.Height; ++iy)
        {
            for (int ix = 0; ix < Object.Map.Layers[0].LayerSize.Width; ++ix, ++ind)
            {
                foreach (var layer in applicableLayers)
                {
                    ShowMissingType type = ceilingLayers.Contains(layer) ? ShowMissingType.Ceiling : ShowMissingType.Floor;
                    bool showError = false;
                    if (missing[ix, iy].HasFlag(type) && ShowMissing.HasFlag(type))
                        showError = layer.Id is "___dummyfloorlayer" or "___dummyceilinglayer";

                    var tile = layer.Tiles[ix, iy];
                    if (tile == null && !showError)
                        continue;

                    Color col = Color.White;
                    var tilePos = type == ShowMissingType.Ceiling ? ceilingData[ind] : floorData[ind];
                    if (tilePos.ShouldHide)
                    {
                        if (ShowMissing.HasFlag(type))
                        {
                            tilePos.Position.Y = 0;
                            tilePos.QuadFacingNormal = type == ShowMissingType.Ceiling ? Vector3.Down : Vector3.Up;
                            tilePos.QuadVert00.Y = 0;
                            tilePos.QuadVert10.Y = 0;
                            tilePos.QuadVert01.Y = 0;
                            tilePos.QuadVert11.Y = 0;
                            tilePos.HeightBoundingSize = 0;
                            col *= 0.25f;
                        }
                        else
                            continue;
                    }
                    missing[ix, iy] &= ~type;

                    (VertexData Data, int FirstVert) DoTile(StaticTile tile)
                    {
                        Texture2D tex = Game1.mouseCursors;
                        int tileInd = 20 + 31 * 44, tr = 44;
                        if (tile != null)
                        {
                            tileInd = tile.TileIndex;

                            string texKey = PathUtilities.NormalizeAssetName(tile.TileSheet.ImageSource);
                            if (!texLookup.TryGetValue(texKey, out tex))
                                texLookup.Add(texKey, tex = Game1.content.Load<Texture2D>(texKey));

                            tr = tile.TileSheet.SheetWidth;
                        }


                        if (!output.TryGetValue(tex, out var verts))
                            output.Add(tex, verts = new());

                        float tw = tex.ActualWidth;
                        float twIncr = Game1.smallestTileSize / tw;
                        float th = tex.ActualHeight;
                        float thIncr = Game1.smallestTileSize / th;

                        float tx = tileInd % tr * twIncr + tuck;
                        float ty = tileInd / tr * thIncr + tuck;
                        float twidth = twIncr - tuck * 2;
                        float theight = thIncr - tuck * 2;

                        int layerNum = applicableLayers.IndexOf(layer);
                        SimpleVertex v00 = new(tilePos.Position + tilePos.QuadVert00, new Vector2(tx, ty), col);
                        SimpleVertex v10 = new(tilePos.Position + tilePos.QuadVert10, new Vector2(tx + twidth, ty), col);
                        SimpleVertex v01 = new(tilePos.Position + tilePos.QuadVert01, new Vector2(tx, ty + theight), col);
                        SimpleVertex v11 = new(tilePos.Position + tilePos.QuadVert11, new Vector2(tx + twidth, ty + theight), col);
                        int startInd = verts.Verts.Count;
                        if (type == ShowMissingType.Ceiling)
                        {
                            verts.Verts.Add(v00);
                            verts.Verts.Add(v10);
                            verts.Verts.Add(v01);
                            verts.Verts.Add(v11);
                        }
                        else
                        {
                            verts.Verts.Add(v00);
                            verts.Verts.Add(v01);
                            verts.Verts.Add(v10);
                            verts.Verts.Add(v11);
                        }
                        return new(verts, startInd);
                    }

                    void AddIndices(VertexData data, int firstVert)
                    {
                        data.Indices.Add(firstVert + 0);
                        data.Indices.Add(firstVert + 1);
                        data.Indices.Add(firstVert + 2);
                        data.Indices.Add(firstVert + 3);
                        data.Indices.Add(firstVert + 2);
                        data.Indices.Add(firstVert + 1);
                    }

                    switch (tile)
                    {
                        case null:
                        case StaticTile:
                            var data = DoTile(tile as StaticTile);
                            AddIndices(data.Data, data.FirstVert);
                            break;
                        case AnimatedTile animTile:
                            List<int> allVerts = new();
                            bool first = true;
                            int animSpot = 0;
                            List<AnimationData> anim = null;
                            foreach (var staticTile in animTile.TileFrames)
                            {
                                var thisData = DoTile(staticTile);
                                allVerts.Add(thisData.FirstVert);
                                if (first)
                                {
                                    animSpot = thisData.Data.Indices.Count; // Vanilla can't use animated tiles from multiple tilesheets anyways
                                    AddIndices(thisData.Data, thisData.FirstVert);
                                    anim = thisData.Data.Animations;
                                    first = false;
                                }
                            }
                            anim.Add(new() { AnimIndexStart = animSpot, AllVertIndices = allVerts.ToArray(), FrameTime = animTile.FrameInterval });
                            break;
                    }
                }

                {
                    bool hasWater = Object.isWaterTile(ix, iy);
                    Texture2D tex = Game1.mouseCursors;
                    Rectangle texRect = new Rectangle(320, 496, 16, 16);
                    Color color = Object.waterColor.Value;
                    if (!hasWater)
                    {
                        texRect = new(320, 496, 16, 16);
                        color = Color.White;
                    }

                    // TODO: do this properly like the others
                    if (waterLayers.FirstOrDefault()?.Tiles[ix, iy] is StaticTile tile && tile.TileIndex != -1)
                    {
                        string texKey = PathUtilities.NormalizeAssetName(tile.TileSheet.ImageSource);
                        if (!texLookup.TryGetValue(texKey, out tex))
                            texLookup.Add(texKey, tex = Game1.content.Load<Texture2D>(texKey));

                        texRect = Game1.getSourceRectForStandardTileSheet(tex, tile.TileIndex, tex.Width / 16, tex.Height / 16);
                        color = Color.White;

                        hasWater = true;
                    }

                    if (hasWater || ShowMissing.HasFlag(ShowMissingType.Water))
                    {
                        var water = waterData[ind];
                        waterVertices.Add(new SimpleVertex(water.Position + water.QuadVert00, new Vector2(texRect.X, texRect.Y) / tex.Bounds.Size.ToVector2(), color));
                        waterVertices.Add(new SimpleVertex(water.Position + water.QuadVert01, new Vector2(texRect.X, texRect.Y + texRect.Height) / tex.Bounds.Size.ToVector2(), color));
                        waterVertices.Add(new SimpleVertex(water.Position + water.QuadVert10, new Vector2(texRect.X + texRect.Width, texRect.Y) / tex.Bounds.Size.ToVector2(), color));
                        waterVertices.Add(new SimpleVertex(water.Position + water.QuadVert11, new Vector2(texRect.X + texRect.Width, texRect.Y + texRect.Height) / tex.Bounds.Size.ToVector2(), color));
                        waterVertices.Add(new SimpleVertex(water.Position + water.QuadVert10, new Vector2(texRect.X + texRect.Width, texRect.Y) / tex.Bounds.Size.ToVector2(), color));
                        waterVertices.Add(new SimpleVertex(water.Position + water.QuadVert01, new Vector2(texRect.X, texRect.Y + texRect.Height) / tex.Bounds.Size.ToVector2(), color));
                    }
                }
            }
        }
    }

    private void BuildWalls(DimensionUtils.PositionResult[] floorData, DimensionUtils.PositionResult[] ceilingData, DimensionUtils.PositionResult[] waterData, Dictionary<Texture2D, VertexData> output)
    {
        int mapWidth = Object.Map.Layers[0].LayerWidth, mapHeight = Object.Map.Layers[0].LayerHeight;
        DimensionUtils.PositionResult LookupPosition(bool ceiling, int x, int y)
        {
            if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
                return new DimensionUtils.PositionResult(new Point(x, y), ceiling ? DimensionUtils.TileType.Ceiling : DimensionUtils.TileType.Floor);

            var data = ceiling ? ceilingData : floorData;
            return data[ x + y * mapWidth ];
        }

        const float tuck = 0.00001f;

        for (int ix = -1; ix <= Object.Map.Layers[0].LayerSize.Width; ++ix)
        {
            for (int iy = -1; iy <= Object.Map.Layers[0].LayerSize.Height; ++iy)
            {
                var assocData = FloorWallAssociationData.Get($"{PathUtilities.NormalizeAssetName(Object.Map.GetTileSheet(Object.getTileSheetIDAt(ix, iy, "Buildings"))?.ImageSource)}:{Object.getTileIndexAt(new Point(ix, iy), "Buildings")}");
                assocData ??= FloorWallAssociationData.Get($"{PathUtilities.NormalizeAssetName(Object.Map.GetTileSheet(Object.getTileSheetIDAt(ix, iy, "Back"))?.ImageSource)}:{Object.getTileIndexAt(new Point(ix, iy), "Back")}");
                assocData ??= FloorWallAssociationData.Get(Object.doesTileHaveProperty(ix, iy, "Type", "Back") ?? "Default");

                WallDefinitionData wallDef_ = WallDefinitionData.Get(assocData?.WallDefinitionId ?? "");
                //if (assocData != null)
                {
                    var tileFloor = LookupPosition(ceiling: false, ix, iy);
                    var tileOtherFloorWest = LookupPosition(ceiling: false, ix - 1, iy);
                    var tileOtherFloorNorth = LookupPosition(ceiling: false, ix, iy - 1);
                    var tileOtherFloorEast = LookupPosition(ceiling: false, ix + 1, iy);
                    var tileOtherFloorSouth = LookupPosition(ceiling: false, ix, iy + 1);

                    var tileCeiling = LookupPosition(ceiling: true, ix, iy);
                    var tileOtherCeilingWest = LookupPosition(ceiling: true, ix - 1, iy);
                    var tileOtherCeilingNorth = LookupPosition(ceiling: true, ix, iy - 1);
                    var tileOtherCeilingEast = LookupPosition(ceiling: true, ix + 1, iy);
                    var tileOtherCeilingSouth = LookupPosition(ceiling: true, ix, iy + 1);

                    Vector3 floorWest = tileFloor.Position + new Vector3(-0.5f, (tileFloor.QuadVert00.Y + tileFloor.QuadVert01.Y) / 2, 0);
                    Vector3 floorNorth = tileFloor.Position + new Vector3(0, (tileFloor.QuadVert00.Y + tileFloor.QuadVert10.Y) / 2, -0.5f);
                    Vector3 floorEast = tileFloor.Position + new Vector3(0.5f, (tileFloor.QuadVert10.Y + tileFloor.QuadVert11.Y) / 2, 0);
                    Vector3 floorSouth = tileFloor.Position + new Vector3(0, (tileFloor.QuadVert01.Y + tileFloor.QuadVert11.Y) / 2, 0.5f);
                    Vector3 otherFloorWest = tileOtherFloorWest.Position + new Vector3(0.5f, (tileFloor.QuadVert10.Y + tileFloor.QuadVert11.Y) / 2, 0);
                    Vector3 otherFloorNorth = tileOtherFloorNorth.Position + new Vector3(0, (tileFloor.QuadVert01.Y + tileFloor.QuadVert11.Y) / 2, -0.5f);
                    Vector3 otherFloorEast = tileOtherFloorEast.Position + new Vector3(-0.5f, (tileFloor.QuadVert00.Y + tileFloor.QuadVert01.Y) / 2, 0);
                    Vector3 otherFloorSouth = tileOtherFloorSouth.Position + new Vector3(0, (tileFloor.QuadVert00.Y + tileFloor.QuadVert10.Y) / 2, 0.5f);

                    Vector3 ceilingWest = tileCeiling.Position + new Vector3(-0.5f, (tileCeiling.QuadVert00.Y + tileCeiling.QuadVert01.Y) / 2, 0);
                    Vector3 ceilingNorth = tileCeiling.Position + new Vector3(0, (tileCeiling.QuadVert00.Y + tileCeiling.QuadVert10.Y) / 2, -0.5f);
                    Vector3 ceilingEast = tileCeiling.Position + new Vector3(0.5f, (tileCeiling.QuadVert10.Y + tileCeiling.QuadVert11.Y) / 2, 0);
                    Vector3 ceilingSouth = tileCeiling.Position + new Vector3(0, (tileCeiling.QuadVert01.Y + tileCeiling.QuadVert11.Y) / 2, 0.5f);
                    Vector3 otherCeilingWest = tileOtherCeilingWest.Position + new Vector3(0.5f, (tileCeiling.QuadVert10.Y + tileCeiling.QuadVert11.Y) / 2, 0);
                    Vector3 otherCeilingNorth = tileOtherCeilingNorth.Position + new Vector3(0, (tileCeiling.QuadVert01.Y + tileCeiling.QuadVert11.Y) / 2, -0.5f);
                    Vector3 otherCeilingEast = tileOtherCeilingEast.Position + new Vector3(-0.5f, (tileCeiling.QuadVert00.Y + tileCeiling.QuadVert01.Y) / 2, 0);
                    Vector3 otherCeilingSouth = tileOtherCeilingSouth.Position + new Vector3(0, (tileCeiling.QuadVert00.Y + tileCeiling.QuadVert10.Y) / 2, 0.5f);

                    var customWallSize = new float?[4];
                    var customWallOffset = new float?[4];
                    WallDefinitionData[] customWallDefs = [wallDef_, wallDef_, wallDef_, wallDef_];
                    string[] dirNames = ["West", "North", "East", "South"];
                    for (int i = 0; i < customWallSize.Count(); ++i)
                    {
                        string dataSizeLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallData_{dirNames[i]}_Size";
                        string dataOffsetLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallData_{dirNames[i]}_Offset";

                        var dataSize = Object.Map.GetLayer(dataSizeLayer);
                        var dataOffset = Object.Map.GetLayer(dataOffsetLayer);

                        int sizeInd = dataSize?.GetTileIndexAt(ix, iy) ?? -1;
                        int offsetInd = dataOffset?.GetTileIndexAt(ix, iy) ?? -1;

                        customWallSize[i] = sizeInd != -1 ? DimensionUtils.GetValueForDataTileIndex(sizeInd) : null;
                        customWallOffset[i] = offsetInd != -1 ? DimensionUtils.GetValueForDataTileIndex(offsetInd) : null;

                        if ((dataSize?.Tiles[ix, iy]?.Properties?.TryGetValue("kittycatcasey.Stardew3D/WallDefinitionOverride", out var wallDefId) ?? false) &&
                            WallDefinitionData.Get(wallDefId) is WallDefinitionData wallDef)
                        {
                            customWallDefs[i] = wallDef;
                        }

                        if (ix == 4 && iy == 4) ix = ix;

                        if (customWallDefs[i] == null)
                        {
                            Point check = new(ix, iy);
                            switch (i)
                            {
                                case 0: check.X -= 1; break;
                                case 1: check.Y -= 1; break;
                                case 2: check.X += 1; break;
                                case 3: check.Y += 1; break;
                            }

                            var tmpAssoc = FloorWallAssociationData.Get($"{PathUtilities.NormalizeAssetName(Object.Map.GetTileSheet(Object.getTileSheetIDAt(check.X, check.Y, "Buildings"))?.ImageSource)}:{Object.getTileIndexAt(new Point(check.X, check.Y), "Buildings")}");
                            //tmpAssoc ??= FloorWallAssociationData.Get($"{PathUtilities.NormalizeAssetName(Object.Map.GetTileSheet(Object.getTileSheetIDAt(check.X, check.Y, "Back"))?.ImageSource)}:{Object.getTileIndexAt(new Point(check.X, check.Y), "Back")}");
                            //tmpAssoc ??= FloorWallAssociationData.Get(Object.doesTileHaveProperty(check.X, check.Y, "Type", "Back") ?? "Default");
                            if (WallDefinitionData.Get(tmpAssoc?.WallDefinitionId ?? "") is WallDefinitionData validWallDef)
                                customWallDefs[i] = validWallDef;
                            //if (customWallDefs[i] == null)
                            {
                                if ((dataSize?.Tiles[check.X, check.Y]?.Properties?.TryGetValue("kittycatcasey.Stardew3D/WallDefinitionOverride", out var tmpWallDefId) ?? false) &&
                                    WallDefinitionData.Get(tmpWallDefId) is WallDefinitionData tmpWallDef)
                                {
                                    customWallDefs[i] = tmpWallDef;
                                }
                            }
                        }
                    }
                    var customWallSizeMods = new float[4];
                    var customWallOffsetMods = new float[4];
                    foreach (var spot in Enum.GetValues<TileSpot>())
                    {
                        string dataSizeModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallSizeData_{spot}";
                        string dataOffsetModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallOffsetData_{spot}";

                        if (Object.Map.Layers.FirstOrDefault(l => l.Id == dataSizeModifierLayer) is Layer sizeLayer)
                            DimensionUtils.ModifyValueForDataTileIndex(spot, sizeLayer.GetTileIndexAt(ix, iy), ref customWallSizeMods[0], ref customWallSizeMods[1], ref customWallSizeMods[3], ref customWallSizeMods[2]);

                        if (Object.Map.Layers.FirstOrDefault(l => l.Id == dataOffsetModifierLayer) is Layer offsetLayer)
                            DimensionUtils.ModifyValueForDataTileIndex(spot, offsetLayer.GetTileIndexAt(ix, iy), ref customWallSizeMods[0], ref customWallSizeMods[1], ref customWallSizeMods[3], ref customWallSizeMods[2]);
                    }

                    TileSpot[] walls = [TileSpot.West, TileSpot.North, TileSpot.East, TileSpot.South];
                    Vector3[] wallsFacing = [Vector3.Right, Vector3.Backward, Vector3.Left, Vector3.Forward];
                    Vector3[] wallOffsets = [Vector3.Left * 0.5f, Vector3.Forward * 0.5f, Vector3.Right * 0.5f, Vector3.Backward * 0.5f];
                    bool[,] valid = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2]
                    {
                        {
                            floorWest.Y < ceilingWest.Y && tileOtherFloorWest.ShouldHide || customWallSize[0].HasValue,
                            floorWest.Y < otherFloorWest.Y && Math.Abs(floorWest.Y - otherFloorWest.Y) >= 0.1,
                            ceilingWest.Y != otherCeilingWest.Y,
                        },
                        {
                            floorNorth.Y < ceilingNorth.Y && tileOtherFloorNorth.ShouldHide || customWallSize[1].HasValue,
                            floorNorth.Y < otherFloorNorth.Y && Math.Abs(floorNorth.Y - otherFloorNorth.Y) >= 0.1,
                            ceilingNorth.Y != otherCeilingNorth.Y,
                        },
                        {
                            floorEast.Y < ceilingEast.Y && tileOtherFloorEast.ShouldHide || customWallSize[2].HasValue,
                            floorEast.Y < otherFloorEast.Y && Math.Abs(floorEast.Y - otherFloorEast.Y) >= 0.1,
                            ceilingEast.Y != otherCeilingEast.Y,
                        },
                        {
                            floorSouth.Y < ceilingSouth.Y && tileOtherFloorSouth.ShouldHide || customWallSize[3].HasValue,
                            floorSouth.Y < otherFloorSouth.Y && Math.Abs(floorSouth.Y - otherFloorSouth.Y) >= 0.1,
                            ceilingSouth.Y != otherCeilingSouth.Y,
                        },
                    };
                    float[,] edges = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, custom=3]
                    {
                        { floorWest.Y, floorWest.Y, ceilingWest.Y },
                        { floorNorth.Y, floorNorth.Y, ceilingNorth.Y },
                        { floorEast.Y, floorEast.Y, ceilingEast.Y },
                        { floorSouth.Y, floorSouth.Y, ceilingSouth.Y },
                    };
                    for (int ioffset = 0; ioffset < customWallOffset.Length; ++ioffset)
                    {
                        for (int iedge = 0; iedge < edges.GetLength(1); ++iedge)
                        {
                            if (customWallOffset[ioffset].HasValue)
                                edges[ioffset, iedge] += customWallOffset[ioffset].Value;
                        }
                    }
                    float[,,] yForWalls = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, custom=3][leftWallBase=0, rightWallBase=0, leftWallTarget=2, rightWallTarget=3]
                    {
                        {
                            { tileFloor.Position.Y + tileFloor.QuadVert01.Y, tileFloor.Position.Y + tileFloor.QuadVert00.Y, tileCeiling.Position.Y + tileCeiling.QuadVert01.Y, tileCeiling.Position.Y + tileCeiling.QuadVert00.Y },
                            { tileFloor.Position.Y + tileFloor.QuadVert01.Y, tileFloor.Position.Y + tileFloor.QuadVert00.Y, tileOtherFloorWest.Position.Y + tileOtherFloorWest.QuadVert11.Y, tileOtherFloorWest.Position.Y + tileOtherFloorWest.QuadVert10.Y },
                            { tileCeiling.Position.Y + tileCeiling.QuadVert01.Y, tileCeiling.Position.Y + tileCeiling.QuadVert00.Y, tileOtherCeilingWest.Position.Y + tileOtherCeilingWest.QuadVert10.Y, tileOtherCeilingWest.Position.Y + tileOtherCeilingWest.QuadVert11.Y },
                        },
                        {
                            { tileFloor.Position.Y + tileFloor.QuadVert00.Y, tileFloor.Position.Y + tileFloor.QuadVert10.Y, tileCeiling.Position.Y + tileCeiling.QuadVert00.Y, tileCeiling.Position.Y + tileCeiling.QuadVert10.Y },
                            { tileFloor.Position.Y + tileFloor.QuadVert00.Y, tileFloor.Position.Y + tileFloor.QuadVert10.Y, tileOtherFloorNorth.Position.Y + tileOtherFloorNorth.QuadVert01.Y, tileOtherFloorNorth.Position.Y + tileOtherFloorNorth.QuadVert11.Y },
                            { tileCeiling.Position.Y + tileCeiling.QuadVert00.Y, tileCeiling.Position.Y + tileCeiling.QuadVert10.Y, tileOtherCeilingNorth.Position.Y + tileOtherCeilingNorth.QuadVert01.Y, tileOtherCeilingNorth.Position.Y + tileOtherCeilingNorth.QuadVert11.Y },
                        },
                        {
                            { tileFloor.Position.Y + tileFloor.QuadVert10.Y, tileFloor.Position.Y + tileFloor.QuadVert11.Y, tileCeiling.Position.Y + tileCeiling.QuadVert10.Y, tileCeiling.Position.Y + tileCeiling.QuadVert11.Y },
                            { tileFloor.Position.Y + tileFloor.QuadVert10.Y, tileFloor.Position.Y + tileFloor.QuadVert11.Y, tileOtherFloorEast.Position.Y + tileOtherFloorEast.QuadVert00.Y, tileOtherFloorEast.Position.Y + tileOtherFloorEast.QuadVert01.Y },
                            { tileCeiling.Position.Y + tileCeiling.QuadVert10.Y, tileCeiling.Position.Y + tileCeiling.QuadVert11.Y, tileOtherCeilingEast.Position.Y + tileOtherCeilingEast.QuadVert01.Y, tileOtherCeilingEast.Position.Y + tileOtherCeilingEast.QuadVert00.Y },
                        },
                        {
                            { tileFloor.Position.Y + tileFloor.QuadVert11.Y, tileFloor.Position.Y + tileFloor.QuadVert01.Y, tileCeiling.Position.Y + tileCeiling.QuadVert11.Y, tileCeiling.Position.Y + tileCeiling.QuadVert01.Y },
                            { tileFloor.Position.Y + tileFloor.QuadVert11.Y, tileFloor.Position.Y + tileFloor.QuadVert01.Y, tileOtherFloorSouth.Position.Y + tileOtherFloorSouth.QuadVert10.Y, tileOtherFloorSouth.Position.Y + tileOtherFloorSouth.QuadVert00.Y },
                            { tileCeiling.Position.Y + tileCeiling.QuadVert11.Y, tileCeiling.Position.Y + tileCeiling.QuadVert01.Y, tileOtherCeilingSouth.Position.Y + tileOtherCeilingSouth.QuadVert10.Y, tileOtherCeilingSouth.Position.Y + tileOtherCeilingSouth.QuadVert00.Y },
                        },
                    };
                    for (int ioffset = 0; ioffset < customWallOffset.Length; ++ioffset)
                    {
                        for (int iedge = 0; iedge < yForWalls.GetLength(1); ++iedge)
                        {
                            for (int ival = 0; ival < yForWalls.GetLength(2); ++ival)
                            {
                                if (customWallOffset[ioffset].HasValue)
                                    yForWalls[ioffset, iedge, ival] += customWallOffset[ioffset].Value;
                            }

                            if (yForWalls[ioffset, iedge, 0] > yForWalls[ioffset, iedge, 2])
                                Util.Swap(ref yForWalls[ioffset, iedge, 0], ref yForWalls[ioffset, iedge, 2]);
                            if (yForWalls[ioffset, iedge, 1] > yForWalls[ioffset, iedge, 3])
                                Util.Swap(ref yForWalls[ioffset, iedge, 1], ref yForWalls[ioffset, iedge, 3]);
                        }
                    }

                    float[,,] heightsForWalls = new float[yForWalls.GetLength(0), yForWalls.GetLength(1), 2]; // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, custom=3][left=0, right=1]
                    for (int idir = 0; idir < yForWalls.GetLength(0); ++idir)
                    {
                        for (int itype = 0; itype < yForWalls.GetLength(1); ++itype)
                        {
                            heightsForWalls[idir, itype, 0] = yForWalls[idir, itype, 2] - yForWalls[idir, itype, 0];
                            heightsForWalls[idir, itype, 1] = yForWalls[idir, itype, 3] - yForWalls[idir, itype, 1];

                            for (int ival = 0; ival < heightsForWalls.GetLength(2); ++ival)
                            {
                                if (customWallSize[idir].HasValue)
                                    heightsForWalls[idir, itype, ival] = customWallSize[idir].Value;
                            }

                            if (heightsForWalls[idir, itype, 0] < 0 && heightsForWalls[idir, itype, 1] < 0)
                            {
                                heightsForWalls[idir, itype, 0] = -heightsForWalls[idir, itype, 0];
                                heightsForWalls[idir, itype, 1] = -heightsForWalls[idir, itype, 1];
                                Util.Swap(ref yForWalls[idir, itype, 2], ref yForWalls[idir, itype, 0]);
                                Util.Swap(ref yForWalls[idir, itype, 3], ref yForWalls[idir, itype, 1]);
                            }
                        }
                    }

                    int[,] whichModIndices =
                    {
                        { 2, 0 },
                        { 0, 1 },
                        { 1, 3 },
                        { 3, 2 },
                    };

                    for (int iwall = 0; iwall < walls.Length; ++iwall)
                    {
                        var wallDef = customWallDefs[iwall];
                        if (wallDef == null)
                            continue;

                        bool[] canResizeSegment = wallDef.VerticalSegments.Select(s => s.ContinuationMode != WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded).ToArray();
                        var resizableSegments = wallDef.VerticalSegments.Where(s => s.ContinuationMode != WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded).ToArray();
                        var nonresizableSegments = wallDef.VerticalSegments.Where(s => s.ContinuationMode == WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded).ToArray();
                        int resizableSegmentCount = canResizeSegment.Count(b => b);
                        int nonresizableSegmentCount = wallDef.VerticalSegments.Count - resizableSegmentCount;
                        int largestSegSize = wallDef.VerticalSegments.Max(s => s.TextureRegion.Height);
                        int heightOfAllSegs = wallDef.VerticalSegments.Sum(s => s.TextureRegion.Height);
                        int heightOfAllNonresizable = nonresizableSegments.Sum(s => s.TextureRegion.Height);
                        float[] relativeSegSizes = wallDef.VerticalSegments.Select(s => s.TextureRegion.Height / (float)largestSegSize).ToArray();

                        int whichForWallBase = 0;
                        for (; whichForWallBase < edges.GetLength(1); ++whichForWallBase)
                        {
                            if (valid[iwall, whichForWallBase])
                                break;
                        }
                        if (whichForWallBase == edges.GetLength(1))
                            continue;

                        var leftY = yForWalls[iwall, whichForWallBase, 0];
                        var rightY = yForWalls[iwall, whichForWallBase, 1];
                        leftY += customWallOffsetMods[whichModIndices[iwall, 0]];
                        rightY += customWallOffsetMods[whichModIndices[iwall, 1]];
                        var centerY = (leftY + rightY) / 2;
                        var leftSize = heightsForWalls[iwall, whichForWallBase, 0];
                        var rightSize = heightsForWalls[iwall, whichForWallBase, 1];
                        leftSize += customWallSizeMods[whichModIndices[iwall, 0]];
                        rightSize += customWallSizeMods[whichModIndices[iwall, 1]];
                        var centerSize = (leftSize + rightSize) / 2;

                        if (centerSize == 0)
                            continue;

                        float tilesHigh = Math.Max(leftSize, rightSize);

                        float[] relativeSegSizesFull = new float[canResizeSegment.Length];
                        for (int i = 0; i < relativeSegSizesFull.Length; ++i)
                            relativeSegSizesFull[i] = canResizeSegment[i] || resizableSegmentCount == 0 ? (tilesHigh - heightOfAllNonresizable / 16f) / tilesHigh / resizableSegmentCount : wallDef.VerticalSegments[i].TextureRegion.Height / (tilesHigh * 16);

                        float segStartPerc = 1;
                        for (int iseg = 0; iseg < wallDef.VerticalSegments.Count; ++iseg)
                        {
                            var segment = wallDef.VerticalSegments[iseg];

                            var tex = Game1.content.Load<Texture2D>(PathUtilities.NormalizeAssetName(segment.Tilesheet));
                            if (!output.TryGetValue(tex, out var vertData))
                                output.Add(tex, vertData = new());
                            var verts = vertData.Verts;

                            float thisSegmentPerc = relativeSegSizesFull[iseg];
                            segStartPerc -= thisSegmentPerc;

                            float segLeftY = leftY + leftSize * segStartPerc;
                            float segRightY = rightY + rightSize * segStartPerc;
                            float segCenterY = (segLeftY + segRightY) / 2;
                            float segLeftSize = leftSize * thisSegmentPerc;
                            float segRightSize = rightSize * thisSegmentPerc;
                            float segCenterSize = (segLeftSize + segRightSize) / 2;

                            int vertStart = verts.Count;
                            switch (segment.ContinuationMode)
                            {
                                case WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded:
                                case WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch:
                                    RenderHelper.GenerateQuad(verts, new Vector3(ix + 0.5f, 0, iy + 0.5f) + wallOffsets[iwall] + wallsFacing[iwall] * tuck,
                                                              new(0.5f, segRightY), new(-0.5f, segLeftY), new(0.5f, segRightY + segRightSize), new(-0.5f, segLeftY + segLeftSize),
                                                              segment.TextureRegion.X / (float)tex.Width + tuck, segment.TextureRegion.Y / (float)tex.Height + tuck, segment.TextureRegion.Width / (float)tex.Width - tuck * 2, segment.TextureRegion.Height / (float)tex.Height - tuck * 2,
                                                              -wallsFacing[iwall]);
                                    break;

                                case WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile:
                                    float tileSegLeftSize = segment.TextureRegion.Height / 16;
                                    float tileSegRightSize = segment.TextureRegion.Height / 16;
                                    float tileSegCenterSize = (tileSegLeftSize + tileSegRightSize) / 2;
                                    float tilesToDoRaw = MathF.Max(segLeftSize / tileSegLeftSize, segRightSize / tileSegRightSize);
                                    int tilesToDo = (int)MathF.Ceiling(tilesToDoRaw);
                                    for (int itiled = 0; itiled < tilesToDo; ++itiled)
                                    {
                                        float thisTileSegLeftSize = Math.Min(tileSegLeftSize, segLeftSize - tileSegLeftSize * itiled);
                                        float thisTileSegRightSize = Math.Min(tileSegRightSize, segRightSize - tileSegRightSize * itiled);
                                        float thisTileSegCenterSize = (thisTileSegLeftSize + thisTileSegRightSize) / 2;

                                        float thisTileSegSizePerc = Math.Min(1, tilesToDoRaw - itiled);

                                        float tileSegLeftY = segLeftY + tileSegLeftSize * itiled;
                                        float tileSegRightY = segRightY + tileSegRightSize * itiled;
                                        float tileSegCenterY = (tileSegLeftY + tileSegRightY) / 2;

                                        RenderHelper.GenerateQuad(verts, new Vector3(ix + 0.5f, 0, iy + 0.5f) + wallOffsets[iwall] + wallsFacing[iwall] * tuck,
                                                                  new(0.5f, tileSegRightY), new(-0.5f, tileSegLeftY), new(0.5f, tileSegRightY + thisTileSegRightSize), new(-0.5f, tileSegLeftY + thisTileSegLeftSize),
                                                                  segment.TextureRegion.X / (float)tex.Width + tuck, segment.TextureRegion.Y / (float)tex.Height + tuck, segment.TextureRegion.Width / (float)tex.Width - tuck * 2, segment.TextureRegion.Height * thisTileSegSizePerc / tex.Height - tuck * 2,
                                                                  -wallsFacing[iwall]);
                                    }
                                    break;
                            }
                            vertData.Indices.AddRange(Enumerable.Range(vertStart, verts.Count - vertStart));
                        }
                    }
                }
            }
        }
    }
}
