using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Data;
using Stardew3D.Rendering;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using xTile;
using xTile.Tiles;
using static Stardew3D.IRenderHandler;

namespace Stardew3D.Rendering.Renderers;

public class LocationRenderer : RendererFor<LocationModelData, GameLocation>
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

    private bool dirty = true;
    public bool IsDirty => dirty;

    public PBREnvironment Environment = PBREnvironment.CreateDefault();

    public LocationRenderer(GameLocation obj)
        : base($"({Mod.Instance.ModManifest.UniqueID}/Location){obj.Name}", obj)
    {
    }

    public void Build(bool force = false)
    {
        if (dirty || force)
        {
            RefreshVertices();
        }
    }

    public override void Render(RenderContext ctx)
    {
        if (Game1.GetKeyboardState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
        {
            dirty = true;
        }

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
        Dictionary<Texture2D, VertexData> vertices = new();
        BuildFloorsAndCeiling(vertices);
        BuildWalls(vertices);

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

    private void BuildFloorsAndCeiling(Dictionary<Texture2D, VertexData> output)
    {
        const float tuck = 0.00001f;

        List<xTile.Layers.Layer> applicableLayers = new();
        List<xTile.Layers.Layer> ceilingLayers = new();
        applicableLayers.AddRange(Object.backgroundLayers.Select(kvp => kvp.Key));
        applicableLayers.AddRange(Object.buildingLayers.Select(kvp => kvp.Key));
        //applicableLayers.AddRange(location.frontLayers.Select(kvp => kvp.Key));
        //applicableLayers.AddRange(location.alwaysFrontLayers.Select(kvp => kvp.Key));
        ceilingLayers.AddRange(Object.Map.Layers.Where(l => l.Id == "kittycatcasey.Stardew3D/Ceiling" || l.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_")));
        ceilingLayers.Sort((l1, l2) => (l1.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_") ? int.Parse(l1.Id.Substring("kittycatcasey.Stardew3D/Ceiling_".Length)) : 0) -
                                       (l2.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_") ? int.Parse(l2.Id.Substring("kittycatcasey.Stardew3D/Ceiling_".Length)) : 0));
        applicableLayers.AddRange(ceilingLayers);
        for (int ix = 0; ix < Object.Map.Layers[0].LayerSize.Width; ++ix)
        {
            for (int iy = 0; iy < Object.Map.Layers[0].LayerSize.Height; ++iy)
            {
                foreach (var layer in applicableLayers)
                {
                    bool isCeiling = ceilingLayers.Contains(layer);

                    var tile = layer.Tiles[ix, iy];
                    if (tile == null)
                        continue;

                    var tilePos = Extensions.GetPositionForTile(Object.Map, new(ix, iy), isCeiling);
                    if (float.IsNaN(tilePos.Position.Y))
                        continue;

                    (VertexData Data, int FirstVert) DoTile(StaticTile tile)
                    {
                        var tex = Game1.content.Load<Texture2D>(PathUtilities.NormalizeAssetName(tile.TileSheet.ImageSource));
                        if (!output.TryGetValue(tex, out var verts))
                            output.Add(tex, verts = new());

                        int tr = tile.TileSheet.SheetWidth;
                        float tw = tex.ActualWidth;
                        float twIncr = Game1.smallestTileSize / tw;
                        float th = tex.ActualHeight;
                        float thIncr = Game1.smallestTileSize / th;

                        float tx = tile.TileIndex % tr * twIncr + tuck;
                        float ty = tile.TileIndex / tr * thIncr + tuck;
                        float twidth = twIncr - tuck * 2;
                        float theight = thIncr - tuck * 2;

                        int layerNum = applicableLayers.IndexOf(layer);
                        SimpleVertex v00 = new(tilePos.Position + tilePos.QuadVert00, new Vector2(tx, ty));
                        SimpleVertex v10 = new(tilePos.Position + tilePos.QuadVert10, new Vector2(tx + twidth, ty));
                        SimpleVertex v01 = new(tilePos.Position + tilePos.QuadVert01, new Vector2(tx, ty + theight));
                        SimpleVertex v11 = new(tilePos.Position + tilePos.QuadVert11, new Vector2(tx + twidth, ty + theight));
                        int startInd = verts.Verts.Count;
                        if (isCeiling)
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
                        case StaticTile staticTile:
                            var data = DoTile(staticTile);
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
            }
        }
    }

    private void BuildWalls(Dictionary<Texture2D, VertexData> output)
    {
        const float tuck = 0.00001f;

        var floorWalls = Game1.content.Load<Dictionary<string, FloorWallAssociationData>>($"{Mod.Instance.ModManifest.UniqueID}/FloorWallAssociations");
        var wallDefs = Game1.content.Load<Dictionary<string, WallDefinitionData>>($"{Mod.Instance.ModManifest.UniqueID}/WallDefinitions");

        for (int ix = 0; ix < Object.Map.Layers[0].LayerSize.Width; ++ix)
        {
            for (int iy = 0; iy < Object.Map.Layers[0].LayerSize.Height; ++iy)
            {
                floorWalls.TryGetValue($"{PathUtilities.NormalizeAssetName(Object.Map.GetTileSheet(Object.getTileSheetIDAt(ix, iy, "Back"))?.ImageSource)}:{Object.getTileIndexAt(new Point(ix, iy), "Back")}", out var assocData);
                if (assocData == null)
                {
                    string type = Object.doesTileHaveProperty(ix, iy, "Type", "Back") ?? "Default";
                    floorWalls.TryGetValue(type, out assocData);
                }

                WallDefinitionData wallDef_ = null;
                wallDefs.TryGetValue(assocData?.WallDefinitionId ?? "", out wallDef_);
                //if (assocData != null)
                {
                    Vector3 floorWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.West, forCeiling: false);
                    Vector3 floorNorth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.North, forCeiling: false);
                    Vector3 floorEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.East, forCeiling: false);
                    Vector3 floorSouth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.South, forCeiling: false);
                    Vector3 otherFloorWest = Extensions.GetPositionAtTile(Object.Map, new(ix - 1, iy), TileSpot.East, forCeiling: false);
                    Vector3 otherFloorNorth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy - 1), TileSpot.South, forCeiling: false);
                    Vector3 otherFloorEast = Extensions.GetPositionAtTile(Object.Map, new(ix + 1, iy), TileSpot.West, forCeiling: false);
                    Vector3 otherFloorSouth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy + 1), TileSpot.North, forCeiling: false);
                    float floorNorthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.NorthWest, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorNorthWest = Extensions.GetPositionAtTile(Object.Map, new(ix - 1, iy), TileSpot.NorthEast, forCeiling: false).Y;
                    float otherVerticalSpotForFloorNorthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy - 1), TileSpot.SouthWest, forCeiling: false).Y;
                    float floorNorthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.NorthEast, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorNorthEast = Extensions.GetPositionAtTile(Object.Map, new(ix + 1, iy), TileSpot.NorthWest, forCeiling: false).Y;
                    float otherVerticalSpotForFloorNorthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy - 1), TileSpot.SouthEast, forCeiling: false).Y;
                    float floorSouthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.SouthWest, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorSouthWest = Extensions.GetPositionAtTile(Object.Map, new(ix - 1, iy), TileSpot.SouthEast, forCeiling: false).Y;
                    float otherVerticalSpotForFloorSouthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy + 1), TileSpot.NorthWest, forCeiling: false).Y;
                    float floorSouthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.SouthEast, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorSouthEast = Extensions.GetPositionAtTile(Object.Map, new(ix + 1, iy), TileSpot.SouthWest, forCeiling: false).Y;
                    float otherVerticalSpotForFloorSouthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy + 1), TileSpot.NorthEast, forCeiling: false).Y;
                    Vector3 ceilingWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.West, forCeiling: true);
                    Vector3 ceilingNorth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.North, forCeiling: true);
                    Vector3 ceilingEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.East, forCeiling: true);
                    Vector3 ceilingSouth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.South, forCeiling: true);
                    Vector3 otherCeilingWest = Extensions.GetPositionAtTile(Object.Map, new(ix - 1, iy), TileSpot.East, forCeiling: true);
                    Vector3 otherCeilingNorth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy - 1), TileSpot.South, forCeiling: true);
                    Vector3 otherCeilingEast = Extensions.GetPositionAtTile(Object.Map, new(ix + 1, iy), TileSpot.West, forCeiling: true);
                    Vector3 otherCeilingSouth = Extensions.GetPositionAtTile(Object.Map, new(ix, iy + 1), TileSpot.North, forCeiling: true);
                    float ceilingNorthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.NorthWest, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingNorthWest = Extensions.GetPositionAtTile(Object.Map, new(ix - 1, iy), TileSpot.NorthEast, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingNorthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy - 1), TileSpot.SouthWest, forCeiling: true).Y;
                    float ceilingNorthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.NorthEast, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingNorthEast = Extensions.GetPositionAtTile(Object.Map, new(ix + 1, iy), TileSpot.NorthWest, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingNorthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy - 1), TileSpot.SouthEast, forCeiling: true).Y;
                    float ceilingSouthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.SouthWest, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingSouthWest = Extensions.GetPositionAtTile(Object.Map, new(ix - 1, iy), TileSpot.SouthEast, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingSouthWest = Extensions.GetPositionAtTile(Object.Map, new(ix, iy + 1), TileSpot.NorthWest, forCeiling: true).Y;
                    float ceilingSouthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy), TileSpot.SouthEast, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingSouthEast = Extensions.GetPositionAtTile(Object.Map, new(ix + 1, iy), TileSpot.SouthWest, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingSouthEast = Extensions.GetPositionAtTile(Object.Map, new(ix, iy + 1), TileSpot.NorthEast, forCeiling: true).Y;

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

                        customWallSize[i] = Extensions.GetValueForDataTileIndex(dataSize?.GetTileIndexAt(ix, iy) ?? -1);
                        customWallOffset[i] = Extensions.GetValueForDataTileIndex(dataOffset?.GetTileIndexAt(ix, iy) ?? -1);

                        if ((dataSize?.Tiles[ix, iy]?.Properties?.TryGetValue("kittycatcasey.Stardew3D/WallDefinitionOverride", out var wallDefId) ?? false) &&
                            wallDefs.TryGetValue(wallDefId, out WallDefinitionData wallDef))
                        {
                            customWallDefs[i] = wallDef;
                        }

                        if (float.IsNaN(customWallSize[i].Value)) customWallSize[i] = null;
                        if (float.IsNaN(customWallOffset[i].Value)) customWallOffset[i] = null;
                    }
                    var customWallSizeMods = new float[4];
                    var customWallOffsetMods = new float[4];
                    string dataSizeModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallSizeModifierData";
                    string dataOffsetModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallOffsetModifierData";
                    var dataSizeModifiers = Object.Map.Layers.Where(l => l.Id == dataSizeModifierLayer || l.Id.StartsWith($"{dataSizeModifierLayer}_"));
                    var dataOffsetModifiers = Object.Map.Layers.Where(l => l.Id == dataOffsetModifierLayer || l.Id.StartsWith($"{dataOffsetModifierLayer}_"));
                    foreach (var modifier in dataSizeModifiers)
                    {
                        Extensions.ModifyValueForDataTileIndex(modifier.GetTileIndexAt(ix, iy), ref customWallSizeMods[0], ref customWallSizeMods[1], ref customWallSizeMods[3], ref customWallSizeMods[2]);
                    }
                    foreach (var modifier in dataOffsetModifiers)
                    {
                        Extensions.ModifyValueForDataTileIndex(modifier.GetTileIndexAt(ix, iy), ref customWallOffsetMods[0], ref customWallOffsetMods[1], ref customWallOffsetMods[3], ref customWallOffsetMods[2]);
                    }

                    TileSpot[] walls = [TileSpot.West, TileSpot.North, TileSpot.East, TileSpot.South];
                    Vector3[] wallsFacing = [Vector3.Right, Vector3.Backward, Vector3.Left, Vector3.Forward];
                    Vector3[] wallOffsets = [Vector3.Left * 0.5f, Vector3.Forward * 0.5f, Vector3.Right * 0.5f, Vector3.Backward * 0.5f];
                    bool[,] valid = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3]
                    {
                        {
                            !float.IsNaN( floorWest.Y  ) && !float.IsNaN( ceilingWest.Y  ) && float.IsNaN( otherFloorWest.Y  ) || customWallSize[0].HasValue,
                            !float.IsNaN( floorWest.Y  ) && !float.IsNaN( otherFloorWest.Y  ) && Math.Abs(floorWest.Y - otherFloorWest.Y) >= 0.1,
                            !float.IsNaN( ceilingWest.Y ) && !float.IsNaN( otherCeilingWest.Y ),
                            !float.IsNaN( otherFloorWest.Y  ) && !float.IsNaN( otherCeilingWest.Y  ),
                        },
                        {
                            !float.IsNaN( floorNorth.Y ) && !float.IsNaN( ceilingNorth.Y ) && float.IsNaN( otherFloorNorth.Y ) || customWallSize[1].HasValue,
                            !float.IsNaN( floorNorth.Y ) && !float.IsNaN( otherFloorNorth.Y ) && Math.Abs(floorNorth.Y - otherFloorNorth.Y) >= 0.1,
                            !float.IsNaN( ceilingNorth.Y ) && !float.IsNaN( otherCeilingNorth.Y ),
                            !float.IsNaN( otherFloorNorth.Y ) && !float.IsNaN( otherCeilingNorth.Y ),
                        },
                        {
                            !float.IsNaN( floorEast.Y  ) && !float.IsNaN( ceilingEast.Y  ) && float.IsNaN( otherFloorEast.Y  ) || customWallSize[2].HasValue,
                            !float.IsNaN( floorEast.Y  ) && !float.IsNaN( otherFloorEast.Y  ) && Math.Abs(floorEast.Y - otherFloorEast.Y) >= 0.1,
                            !float.IsNaN( ceilingEast.Y ) && !float.IsNaN( otherCeilingEast.Y ),
                            !float.IsNaN( otherFloorEast.Y  ) && !float.IsNaN( otherCeilingEast.Y  ),
                        },
                        {
                            !float.IsNaN( floorSouth.Y ) && !float.IsNaN( ceilingSouth.Y ) && float.IsNaN( otherFloorSouth.Y ) || customWallSize[3].HasValue,
                            !float.IsNaN( floorSouth.Y ) && !float.IsNaN( otherFloorSouth.Y ) && Math.Abs(floorSouth.Y - otherFloorSouth.Y) >= 0.1,
                            !float.IsNaN( ceilingSouth.Y ) && !float.IsNaN( otherCeilingSouth.Y ),
                            !float.IsNaN( otherFloorSouth.Y ) && !float.IsNaN( otherCeilingSouth.Y ),
                        },
                    };
                    float[,] edges = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3, custom=4]
                    {
                        { floorWest.Y, floorWest.Y, ceilingWest.Y, otherFloorWest.Y },
                        { floorNorth.Y, floorNorth.Y, ceilingNorth.Y, otherFloorNorth.Y },
                        { floorEast.Y, floorEast.Y, ceilingEast.Y, otherFloorEast.Y },
                        { floorSouth.Y, floorSouth.Y, ceilingSouth.Y, otherFloorSouth.Y },
                    };
                    for (int ioffset = 0; ioffset < customWallOffset.Length; ++ioffset)
                    {
                        for (int iedge = 0; iedge < edges.GetLength(1); ++iedge)
                        {
                            if (customWallOffset[ioffset].HasValue)
                                edges[ioffset, iedge] += customWallOffset[ioffset].Value;
                        }
                    }
                    float[,,] yForWalls = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3, custom=4][leftWallBase=0, rightWallBase=0, leftWallTarget=2, rightWallTarget=3]
                    {
                        {
                            { floorSouthWest, floorNorthWest, ceilingSouthWest, ceilingNorthWest },
                            { floorSouthWest, floorNorthWest, otherHorizontalSpotForFloorSouthWest, otherHorizontalSpotForFloorNorthWest },
                            { ceilingSouthWest, ceilingNorthWest, otherHorizontalSpotForCeilingSouthWest, otherHorizontalSpotForCeilingNorthWest },
                            { otherHorizontalSpotForFloorSouthWest, otherHorizontalSpotForFloorNorthWest, otherHorizontalSpotForCeilingSouthWest, otherHorizontalSpotForCeilingNorthWest }
                        },
                        {
                            { floorNorthWest, floorNorthEast, ceilingNorthWest, ceilingNorthEast },
                            { floorNorthWest, floorNorthEast, otherVerticalSpotForFloorNorthWest, otherVerticalSpotForFloorNorthEast },
                            { ceilingNorthWest, ceilingNorthEast, otherVerticalSpotForCeilingNorthWest, otherVerticalSpotForCeilingNorthEast },
                            { otherVerticalSpotForFloorNorthWest, otherVerticalSpotForFloorNorthEast, otherVerticalSpotForCeilingNorthWest, otherVerticalSpotForCeilingNorthEast },
                        },
                        {
                            { floorNorthEast, floorSouthEast, ceilingNorthEast, ceilingSouthEast },
                            { floorNorthEast, floorSouthEast, otherHorizontalSpotForFloorNorthEast, otherHorizontalSpotForFloorSouthEast },
                            { ceilingNorthEast, ceilingSouthEast, otherHorizontalSpotForCeilingNorthEast, otherHorizontalSpotForCeilingSouthEast },
                            { otherHorizontalSpotForFloorNorthEast, otherHorizontalSpotForFloorSouthEast, otherHorizontalSpotForCeilingNorthEast, otherHorizontalSpotForCeilingSouthEast },
                        },
                        {
                            { floorSouthEast, floorSouthWest, ceilingSouthEast, ceilingSouthWest },
                            { floorSouthEast, floorSouthWest, otherVerticalSpotForFloorSouthEast, otherVerticalSpotForFloorSouthWest },
                            { ceilingSouthEast, ceilingSouthWest, otherVerticalSpotForCeilingSouthEast, otherVerticalSpotForCeilingSouthWest },
                            { otherVerticalSpotForFloorSouthEast, otherVerticalSpotForFloorSouthWest, otherVerticalSpotForCeilingSouthEast, otherVerticalSpotForCeilingSouthWest },
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

                    float[,,] heightsForWalls = new float[yForWalls.GetLength(0), yForWalls.GetLength(1), 2]; // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3, custom=4][left=0, right=1]
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
                            if (valid[iwall, whichForWallBase] && !float.IsNaN(edges[iwall, whichForWallBase]))
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
