using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public void ModifyBaseData(string layerName, Point tile, float amount)
        {
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

            locRenderer.Build(force: true);
        }
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

    private bool leftMouse, rightMouse;
    private float mouseHoldTimer = 0;
    private List<Vector3> selBounds = new List<Vector3>();
    public void Update()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;
        Vector3 near = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 0), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
        Vector3 far = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 1), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
        Ray cursor = new(near, (far - near).Normalized());

        Vector2 cursorPos2d = new(cursor.Position.X, cursor.Position.Z);
        Vector2 cursorDir2d = new(cursor.Direction.X, cursor.Direction.Z);

        if (editor.Ui.Controls.Input.TryConsumePressed(MouseButton.Left))
            leftMouse = true;
        if (editor.Ui.Controls.Input.TryConsumePressed(MouseButton.Right))
            rightMouse = true;

        if (leftMouse && !editor.Ui.Controls.Input.IsDown(MouseButton.Left))
            leftMouse = false;
        if (rightMouse && !editor.Ui.Controls.Input.IsDown(MouseButton.Right))
            rightMouse = false;

        if (!leftMouse && !rightMouse)
            mouseHoldTimer = 0;

        selBounds.Clear();
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
                selBounds.Add(quad.Position + quad.QuadVert00 + quad.QuadFacingNormal * 0.02f);
                selBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                selBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                selBounds.Add(quad.Position + quad.QuadVert11 + quad.QuadFacingNormal * 0.02f);
                selBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                selBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);

                if (leftMouse)
                {
                    if (mouseHoldTimer > 0)
                        mouseHoldTimer -= (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;

                    if (mouseHoldTimer <= 0)
                    {
                        mouseHoldTimer = 0.025f;
                        Location.ModifyBaseData($"{Mod.Instance.ModManifest.UniqueID}/FloorData", cursorPosTile2d, 0.1f);
                        MapModified();
                    }
                }
                else if (rightMouse)
                {
                    if (mouseHoldTimer > 0)
                        mouseHoldTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;

                    if (mouseHoldTimer <= 0)
                    {
                        mouseHoldTimer = 0.025f;
                        Location.ModifyBaseData($"{Mod.Instance.ModManifest.UniqueID}/FloorData", cursorPosTile2d, -0.1f);
                        MapModified();
                    }
                }
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
