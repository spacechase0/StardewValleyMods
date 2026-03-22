using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using MLEM.Ui.Elements;
using SpaceShared;
using Stardew3D.GameModes.Editor.Editables;
using Stardew3D.Handlers.Render;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Extensions;
using xTile.Tiles;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.GameModes.Editor.Editables.Map;

internal class MapEditable : IEditable
{
    private class DummyLocation : GameLocation
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

        public void ModifyBaseData(DimensionUtils.TileType tileType, Point tile, float amount)
        {
            string layerName = $"{Mod.Instance.ModManifest.UniqueID}/{tileType}Data";
            var layer = Map.GetLayer(layerName);
            if (layer == null)
                Map.AddLayer(layer = new(layerName, Map, Map.Layers[0].LayerSize, Map.Layers[0].TileSize));

            if (tile.X < 0 || tile.Y < 0 || tile.X >= layer.LayerWidth || tile.Y >= layer.LayerHeight)
                return;

            float val = DimensionUtils.GetValueForDataTileIndex(layer.Tiles[tile.X, tile.Y]?.TileIndex ?? -1);
            val += amount;

            TileSheet ts = Map.GetTileSheet("dataValues");
            if (ts == null)
                Map.AddTileSheet(ts = new("dataValues", Map, "ThirdDimensionData\\floor", new(20, 10), new(16, 16)));

            layer.Tiles[tile.X, tile.Y] = new StaticTile(layer, ts, BlendMode.Alpha, DimensionUtils.GetDataTileIndexForValue(val));
        }

        public void SetBaseData(DimensionUtils.TileType tileType, Point tile, float? value)
        {
            string layerName = $"{Mod.Instance.ModManifest.UniqueID}/{tileType}Data";
            var layer = Map.GetLayer(layerName);
            if (layer == null)
                Map.AddLayer(layer = new(layerName, Map, Map.Layers[0].LayerSize, Map.Layers[0].TileSize));

            if (tile.X < 0 || tile.Y < 0 || tile.X >= layer.LayerWidth || tile.Y >= layer.LayerHeight)
                return;

            TileSheet ts = Map.GetTileSheet("dataValues");
            if (ts == null)
                Map.AddTileSheet(ts = new("dataValues", Map, "ThirdDimensionData\\floor", new(20, 10), new(16, 16)));

            layer.Tiles[tile.X, tile.Y] = value.HasValue ? new StaticTile(layer, ts, BlendMode.Alpha, DimensionUtils.GetDataTileIndexForValue(value.Value)) : null;
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

    public string Id { get; init; }
    public string LocationName { get; init; }
    public string AssetName { get; init; }
    public bool HasUnsavedChanges { get; private set; }

    private LocalizedContentManager ToLoadContentFrom;
    private DummyLocation Location;

    public MapEditable(LocalizedContentManager toLoadContentFrom, string locName, string assetName)
    {
        ToLoadContentFrom = toLoadContentFrom;
        Id = assetName;
        LocationName = locName;
        AssetName = assetName;
    }

    public void Dispose()
    {
    }

    public void MapModified()
    {
        HasUnsavedChanges = true;

        foreach (var renderer in Mod.State.GetRenderHandlersFor(Location))
        {
            if (renderer is not LocationRenderer locRenderer)
                continue;

            locRenderer.MarkDirty();
        }
        selDirty = true;
        pendingDirty = true;
    }

    public void Reset()
    {
        Location.reloadMap();
        MapModified();
    }

    public void Clear()
    {
        var layers = Location.Map.Layers.Where(l => l.Id.StartsWith($"{Mod.Instance.ModManifest.UniqueID}/"));
        foreach (var layer in layers.ToArray())
            Location.Map.RemoveLayer(layer);

        MapModified();
    }

    public ICollection<Element> PopulatePanelContents()
    {
        Location = new DummyLocation(ToLoadContentFrom, LocationName, AssetName);

        Point mapSize = new(Location.Map.Layers[0].LayerWidth, Location.Map.Layers[0].LayerHeight);

        float maxHeight = float.MinValue;
        for (int ix = 0; ix < mapSize.X; ++ix)
        {
            for (int iy = 0; iy < mapSize.Y; ++iy)
            {
                float y = new Point(ix, iy).To3D(Location.Map).Y;
                maxHeight = Math.Max(maxHeight, y);
            }
        }
        float camHeight = (mapSize.Y / 2 / 0.707f) * 0.55f;
        var camPos = new Vector3(mapSize.X / 2f, maxHeight + camHeight, mapSize.Y * 1.15f);
        (Mod.State.ActiveMode as EditorGameMode).SetCamera(camPos, new Vector3(0, -0.707f, -0.707f));

        Group dangerButtons = new(MLEM.Ui.Anchor.BottomCenter, new Vector2(1, 0), setHeightBasedOnChildren: true);
        dangerButtons.AddChild(new Button(MLEM.Ui.Anchor.AutoCenter, new Vector2(1, 32), "Reset")
        {
            AutoSizeAddedAbsolute = new Vector2(-32, 0),
            OnPressed = _ =>
            {
                (Mod.State.ActiveMode as EditorGameMode).DoAfterConfirm(Reset);
            },
        });
        dangerButtons.AddChild(new Button(MLEM.Ui.Anchor.AutoCenter, new Vector2(1, 32), "Clear")
        {
            AutoSizeAddedAbsolute = new Vector2(-64, 0),
            NormalColor = Color.Red,
            HoveredColor = Color.DarkRed,
            OnPressed = _ =>
            {
                (Mod.State.ActiveMode as EditorGameMode).DoAfterConfirm(Clear);
            },
        });
        return
        [
            new Paragraph(MLEM.Ui.Anchor.TopLeft, 1, Id),
            dangerButtons,
        ];
    }

    public void BeforeHidePanelContents()
    {
        Location.MapLoader?.Dispose();
        Location = null;
    }

    private enum SelectMode
    {
        Replace,
        Add,
        Remove,
    }

    private bool leftMouse, rightMouse;
    private float mouseHoldTimer = 0;
    private Point? pendingStartTile = null;
    private Point? lastHoverTile = null;
    private SelectMode pendingSelectMode = SelectMode.Replace;
    private HashSet<Point> pendingTiles = new();
    private HashSet<Point> selectedTiles = new();
    private List<Vector3> selBounds = new List<Vector3>();
    private List<Vector3> pendingBounds = new List<Vector3>();
    private bool selDirty = false;
    private bool pendingDirty = false;
    public void Update()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;
        Vector3 near = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 0), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
        Vector3 far = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 1), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
        Ray cursor = new(near, (far - near).Normalized());

        Vector2 cursorPos2d = new(cursor.Position.X, cursor.Position.Z);
        Vector2 cursorDir2d = new(cursor.Direction.X, cursor.Direction.Z);

        pendingSelectMode = SelectMode.Replace;
        if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Shift))
            pendingSelectMode = SelectMode.Add;
        else if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Alt))
            pendingSelectMode = SelectMode.Remove;

        bool justPressedLeft = false, justPressedRight = false;
        if (editor.Ui.Controls.Input.TryConsumePressed(MouseButton.Left))
            leftMouse = justPressedLeft = true;
        if (editor.Ui.Controls.Input.TryConsumePressed(MouseButton.Right))
            rightMouse = justPressedRight = true;

        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.A) && editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control))
        {
            selectedTiles.Clear();
            for (int ix = 0; ix < Location.Map.Layers[0].LayerWidth; ++ix)
            {
                for (int iy = 0; iy < Location.Map.Layers[0].LayerHeight; ++iy)
                {
                    selectedTiles.Add(new(ix, iy));
                }
            }
            selDirty = true;
        }

        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.F) && editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control) &&
            lastHoverTile.HasValue)
        {
            var baseData = DimensionUtils.GetPositionForTile(Location.Map, lastHoverTile.Value);
            float min = baseData.Position.Y - baseData.HeightBoundingSize / 2;
            float max= baseData.Position.Y + baseData.HeightBoundingSize / 2;

            HashSet<Point> matching = new();
            HashSet<Point> visited = new();
            Queue<Point> toVisit = new();
            toVisit.Enqueue(lastHoverTile.Value);
            while (toVisit.TryDequeue(out Point check))
            {
                if (visited.Contains(check))
                    continue;
                visited.Add(check);

                var data = DimensionUtils.GetPositionForTile(Location.Map, check);
                if (data.Position.Y + data.HeightBoundingSize / 2 < min ||
                    data.Position.Y - data.HeightBoundingSize / 2 > max)
                    continue;
                matching.Add(check);

                if (check.Y == 3) check = check;

                void TryCheck(Point pt)
                {
                    if (visited.Contains(pt))
                        return;
                    if (pt.X < 0 || pt.Y < 0 || pt.X >= Location.Map.Layers[0].LayerWidth || pt.Y >= Location.Map.Layers[0].LayerHeight)
                        return;

                    toVisit.Enqueue(pt);
                }

                TryCheck(check + new Point(-1, 0));
                TryCheck(check + new Point(1, 0));
                TryCheck(check + new Point(0, -1));
                TryCheck(check + new Point(0, 1));
            }

            if (pendingSelectMode == SelectMode.Replace)
                selectedTiles.Clear();
            foreach (var sel in matching)
            {
                if (pendingSelectMode == SelectMode.Remove)
                    selectedTiles.Remove(sel);
                else
                    selectedTiles.Add(sel);
            }
            selDirty = true;
        }

        if (leftMouse && !editor.Ui.Controls.Input.IsDown(MouseButton.Left))
        {
            leftMouse = false;

            if (pendingStartTile.HasValue && lastHoverTile.HasValue)
            {
                if (pendingSelectMode == SelectMode.Replace)
                    selectedTiles.Clear();

                for (int ix = Math.Min(pendingStartTile.Value.X, lastHoverTile.Value.X); ix <= Math.Max(pendingStartTile.Value.X, lastHoverTile.Value.X); ++ix)
                {
                    for (int iy = Math.Min(pendingStartTile.Value.Y, lastHoverTile.Value.Y); iy <= Math.Max(pendingStartTile.Value.Y, lastHoverTile.Value.Y); ++iy)
                    {
                        if (pendingSelectMode == SelectMode.Remove)
                            selectedTiles.Remove(new(ix, iy));
                        else
                            selectedTiles.Add(new(ix, iy));
                    }
                }
                selDirty = true;

                pendingStartTile = null;
            }
        }
        if (rightMouse && !editor.Ui.Controls.Input.IsDown(MouseButton.Right))
            rightMouse = false;

        if (!leftMouse && !rightMouse)
            mouseHoldTimer = 0;

        if (justPressedLeft)
        {
            pendingStartTile = lastHoverTile;
        }

        int scrollAmt = (editor.Ui.Controls.Input.ScrollWheel - editor.Ui.Controls.Input.LastScrollWheel) / 120;
        if (scrollAmt != 0)
        {
            float incr = 1f;
            if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Shift))
                incr = 0.5f;
            else if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control))
                incr = 0.1f;

            foreach (var tile in selectedTiles)
                Location.ModifyBaseData(DimensionUtils.TileType.Floor, tile, incr * scrollAmt);

            MapModified();
        }

        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.Delete))
        {
            foreach (var tile in selectedTiles)
                Location.SetBaseData(DimensionUtils.TileType.Floor, tile, null);

            MapModified();

        }

        Point? hoverTile = null;
        for (int i = 0; i < 1000; i += 1)
        {
            Point cursorPosTile2d = new Vector2( MathF.Floor(cursorPos2d.X), MathF.Floor(cursorPos2d.Y)).ToPoint();
            Rectangle tileRect = new(cursorPosTile2d.X, cursorPosTile2d.Y, 1, 1);

            Vector2 tile = cursorPosTile2d.ToVector2();
            var quad = DimensionUtils.GetPositionForTile(Location.Map, cursorPosTile2d);
            /*
            if (float.IsNaN(quad.Position.Y))
            {
                quad.Position.Y = 0;
                quad.QuadFacingNormal = Vector3.Up;
                quad.QuadVert00.Y = 0;
                quad.QuadVert10.Y = 0;
                quad.QuadVert01.Y = 0;
                quad.QuadVert11.Y = 0;
                quad.HeightBoundingSize = 0;
            }
            */

            Plane plane = new Plane(quad.Position, quad.QuadFacingNormal);
            cursor.Intersects(ref plane, out var dist);
            Vector3 intersectAt = dist.HasValue ? (cursor.Position + cursor.Direction * dist.Value) : Vector3.Zero;
            Vector2 intersectAt2d = new Vector2(intersectAt.X, intersectAt.Z);
            if (dist.HasValue && tileRect.Contains(intersectAt2d))
            {
                hoverTile = cursorPosTile2d;
                break;
            }

            if (!tileRect.LineSegmentIntersects(cursorPos2d, cursorPos2d + cursorDir2d * 10, out var intersect))
            {
                tileRect.LineSegmentIntersects(cursorPos2d, cursorPos2d + cursorDir2d * 10, out _);
                break; // ???
            }

            cursorPos2d = intersect;
            if (cursorDir2d.X < 0)
                cursorPos2d.X -= 0.001f;
            else
                cursorPos2d.X += 0.001f;
            if (cursorDir2d.Y < 0)
                cursorPos2d.Y -= 0.001f;
            else
                cursorPos2d.Y += 0.001f;
        }

        bool hoverDirty = hoverTile != lastHoverTile;
        if (hoverDirty)
        {
            pendingTiles.Clear();
            if (pendingStartTile.HasValue)
            {
                for (int ix = Math.Min(pendingStartTile.Value.X, hoverTile.Value.X); ix <= Math.Max(pendingStartTile.Value.X, hoverTile.Value.X); ++ix)
                {
                    for (int iy = Math.Min(pendingStartTile.Value.Y, hoverTile.Value.Y); iy <= Math.Max(pendingStartTile.Value.Y, hoverTile.Value.Y); ++iy)
                    {
                        pendingTiles.Add(new(ix, iy));
                    }
                }
            }
            lastHoverTile = hoverTile;
            pendingDirty = true;
        }

        if (pendingDirty)
        {
            pendingDirty = false;

            pendingBounds.Clear();
            if (pendingSelectMode == SelectMode.Replace || pendingSelectMode == SelectMode.Add)
            {
                foreach (var tile in pendingTiles)
                {
                    var quad = DimensionUtils.GetPositionForTile(Location.Map, tile);
                    pendingBounds.Add(quad.Position + quad.QuadVert00 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert11 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                }
            }

            if (hoverTile.HasValue && pendingSelectMode != SelectMode.Remove)
            {
                var quad = DimensionUtils.GetPositionForTile(Location.Map, hoverTile.Value);
                pendingBounds.Add(quad.Position + quad.QuadVert00 + quad.QuadFacingNormal * 0.02f);
                pendingBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                pendingBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                pendingBounds.Add(quad.Position + quad.QuadVert11 + quad.QuadFacingNormal * 0.02f);
                pendingBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                pendingBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
            }

            selDirty = true;
        }

        if (selDirty)
        {
            selDirty = false;

            selBounds.Clear();
            if (!pendingStartTile.HasValue || pendingSelectMode != SelectMode.Replace)
            {
                foreach (var tile in selectedTiles)
                {
                    if (pendingSelectMode == SelectMode.Remove && pendingTiles.Contains(tile))
                        continue;

                    var quad = DimensionUtils.GetPositionForTile(Location.Map, tile);
                    selBounds.Add(quad.Position + quad.QuadVert00 + quad.QuadFacingNormal * 0.02f);
                    selBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                    selBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                    selBounds.Add(quad.Position + quad.QuadVert11 + quad.QuadFacingNormal * 0.02f);
                    selBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                    selBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                }
            }
        }
    }

    public void RenderWorld(RenderBatcher b)
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;
        RenderContext ctx = new()
        {
            Time = Game1.currentGameTime,
            TargetScreen = null,

            WorldSpriteBatch = new(Location),

            WorldBatch = b,
            WorldEnvironment = editor.EditorEnvironment,
            WorldCamera = editor.Camera,
            WorldTransform = Matrix.Identity,
        };

        foreach (var renderer in Mod.State.GetRenderHandlersFor(Location))
        {
            if (renderer is LocationRenderer locRenderer)
            {
                locRenderer.EvenMissing = true;
                locRenderer.Build();
            }

            renderer?.Render(ctx);
        }
    }

    public void AfterRenderWorld()
    {
        if (pendingBounds.Count > 0)
        {
            SimpleVertex[] v = pendingBounds.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.LightGray * 0.75f)).ToArray();

            RenderHelper.GenericEffect.Texture = Game1.staminaRect;
            RenderHelper.GenericEffect.World = Matrix.Identity;
            {
                RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing_Transparent_1"];
                foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
                }
            }
            {
                RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing_Transparent_2"];
                foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
                }
            }
        }
        if (selBounds.Count > 0)
        {
            SimpleVertex[] v = selBounds.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.LightGray * 0.75f)).ToArray();

            RenderHelper.GenericEffect.Texture = Game1.staminaRect;
            RenderHelper.GenericEffect.World = Matrix.Identity;
            {
                RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing_Transparent_1"];
                foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
                }
            }
            {
                RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing_Transparent_2"];
                foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
                }
            }
        }
    }

    public Dictionary<string, string> Save()
    {
        throw new System.NotImplementedException();
        HasUnsavedChanges = false;
    }
}
