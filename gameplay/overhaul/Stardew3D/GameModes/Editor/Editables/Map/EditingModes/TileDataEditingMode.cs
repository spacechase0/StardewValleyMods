using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using MLEM.Ui.Elements;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;

namespace Stardew3D.GameModes.Editor.Editables.Map.EditingModes;

public class TileDataEditingMode : BaseEditingMode
{
    public readonly DimensionUtils.TileType TileType;

    public override string Id => TileType.ToString();

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

    public TileDataEditingMode(MapEditable editable, DimensionUtils.TileType tileType)
        : base(editable)
    {
        TileType = tileType;
    }

    private void MapModified()
    {
        Editable.MapModified();

        selDirty = true;
        pendingDirty = true;
    }

    public override ICollection<Element> PopulatePanelContents()
    {
        return [];
    }

    private void UpdateHover()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;
        Vector3 near = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 0), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
        Vector3 far = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 1), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
        Ray cursor = new(near, (far - near).Normalized());

        pendingSelectMode = SelectMode.Replace;
        if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Shift))
            pendingSelectMode = SelectMode.Add;
        else if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Alt))
            pendingSelectMode = SelectMode.Remove;

        Vector2 cursorPos2d = new(cursor.Position.X, cursor.Position.Z);
        Vector2 cursorDir2d = new(cursor.Direction.X, cursor.Direction.Z);

        Point? hoverTile = null;
        for (int i = 0; i < 1000; i += 1)
        {
            Point cursorPosTile2d = new Vector2(MathF.Floor(cursorPos2d.X), MathF.Floor(cursorPos2d.Y)).ToPoint();
            Rectangle tileRect = new(cursorPosTile2d.X, cursorPosTile2d.Y, 1, 1);

            Vector2 tile = cursorPosTile2d.ToVector2();
            var quad = DimensionUtils.GetPositionForTile(Editable.Location.Map, cursorPosTile2d, TileType);
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
    }

    private void UpdateSelection()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;
        bool justPressedLeft = false, justPressedRight = false;
        if (editor.Ui.Controls.Input.TryConsumePressed(MouseButton.Left))
            leftMouse = justPressedLeft = true;
        if (editor.Ui.Controls.Input.TryConsumePressed(MouseButton.Right))
            rightMouse = justPressedRight = true;

        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.A) && editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control))
        {
            selectedTiles.Clear();
            for (int ix = 0; ix < Editable.Location.Map.Layers[0].LayerWidth; ++ix)
            {
                for (int iy = 0; iy < Editable.Location.Map.Layers[0].LayerHeight; ++iy)
                {
                    selectedTiles.Add(new(ix, iy));
                }
            }
            selDirty = true;
        }

        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.F) && editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control) &&
            lastHoverTile.HasValue)
        {
            var baseData = DimensionUtils.GetPositionForTile(Editable.Location.Map, lastHoverTile.Value, TileType);
            float min = baseData.Position.Y - baseData.HeightBoundingSize / 2;
            float max = baseData.Position.Y + baseData.HeightBoundingSize / 2;

            HashSet<Point> matching = new();
            HashSet<Point> visited = new();
            Queue<Point> toVisit = new();
            toVisit.Enqueue(lastHoverTile.Value);
            while (toVisit.TryDequeue(out Point check))
            {
                if (visited.Contains(check))
                    continue;
                visited.Add(check);

                var data = DimensionUtils.GetPositionForTile(Editable.Location.Map, check, TileType);
                if (data.Position.Y + data.HeightBoundingSize / 2 < min ||
                    data.Position.Y - data.HeightBoundingSize / 2 > max)
                    continue;
                matching.Add(check);

                if (check.Y == 3) check = check;

                void TryCheck(Point pt)
                {
                    if (visited.Contains(pt))
                        return;
                    if (pt.X < 0 || pt.Y < 0 || pt.X >= Editable.Location.Map.Layers[0].LayerWidth || pt.Y >= Editable.Location.Map.Layers[0].LayerHeight)
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
    }

    private void UpdateModifications()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;

        int scrollAmt = (editor.Ui.Controls.Input.ScrollWheel - editor.Ui.Controls.Input.LastScrollWheel) / 120;
        if (scrollAmt != 0)
        {
            float incr = 1f;
            if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Shift))
                incr = 0.5f;
            else if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control))
                incr = 0.1f;

            foreach (var tile in selectedTiles)
                Editable.Location.ModifyBaseData(TileType, tile, incr * scrollAmt);

            MapModified();
        }

        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.Delete))
        {
            foreach (var tile in selectedTiles)
                Editable.Location.SetBaseData(TileType, tile, null);

            MapModified();
        }
    }

    public override void Update()
    {
        UpdateHover();
        UpdateSelection();
        UpdateModifications();
    }

    private void UpdateSelectionDisplay()
    {
        if (pendingDirty)
        {
            pendingDirty = false;

            pendingBounds.Clear();
            if (pendingSelectMode == SelectMode.Replace || pendingSelectMode == SelectMode.Add)
            {
                foreach (var tile in pendingTiles)
                {
                    var quad = DimensionUtils.GetPositionForTile(Editable.Location.Map, tile, TileType);
                    pendingBounds.Add(quad.Position + quad.QuadVert00 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert11 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert10 + quad.QuadFacingNormal * 0.02f);
                    pendingBounds.Add(quad.Position + quad.QuadVert01 + quad.QuadFacingNormal * 0.02f);
                }
            }

            if (lastHoverTile.HasValue && pendingSelectMode != SelectMode.Remove)
            {
                var quad = DimensionUtils.GetPositionForTile(Editable.Location.Map, lastHoverTile.Value, TileType);
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

                    var quad = DimensionUtils.GetPositionForTile(Editable.Location.Map, tile, TileType);
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

    public override void Render()
    {
        UpdateSelectionDisplay();

        if (pendingBounds.Count > 0)
        {
            SimpleVertex[] v = pendingBounds.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.LightGray * 0.75f)).ToArray();

            Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
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

            Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
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
}
