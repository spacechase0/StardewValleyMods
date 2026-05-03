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
using SpaceShared;
using Stardew3D.Handlers;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;

namespace Stardew3D.GameModes.Editor.Editables.Map.EditingModes;

public class TileDataEditingMode : BaseEditingMode
{
    public readonly DimensionUtils.TileType TileType;

    public override string Id => TileType.ToString();
    public override LocationHandler.TerrainType ShowMissingInLocation => TileType switch
    {
        DimensionUtils.TileType.Floor => LocationHandler.TerrainType.Floor,
        DimensionUtils.TileType.Ceiling => LocationHandler.TerrainType.Ceiling,
        DimensionUtils.TileType.Water => LocationHandler.TerrainType.Water,
        _ => throw new InvalidOperationException(),
    };

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
    private TileSpot tileEditType
    {
        get => field;
        set
        {
            field = value;
            selDirty = true;
            pendingDirty = true;
        }
    } = TileSpot.Center;
    public bool smartSlopeEdit = false;

    public TileDataEditingMode(MapEditable editable, DimensionUtils.TileType tileType)
        : base(editable)
    {
        TileType = tileType;
        if (editable.EditingMode is TileDataEditingMode other)
        {
            selectedTiles = other.selectedTiles;
            selBounds = other.selBounds;
        }
    }

    private void MapModified()
    {
        Editable.MapModified();

        selDirty = true;
        pendingDirty = true;
    }

    public override ICollection<Element> PopulatePanelContents()
    {
        List<Element> elems = new();
        elems.Add(new Paragraph(MLEM.Ui.Anchor.AutoLeft, 1, _ => $"Mode: {tileEditType}{(smartSlopeEdit ? " (Smart)" : "")}"));

        TileSpot[] vals =
        [
            TileSpot.NorthWest,
            TileSpot.North,
            TileSpot.NorthEast,
            TileSpot.West,
            TileSpot.Center,
            TileSpot.East,
            TileSpot.SouthWest,
            TileSpot.South,
            TileSpot.SouthEast,
        ];
        for ( int i = 0; i < vals.Length; ++i )
        {
            var val = vals[i];
            string str = val switch
            {
                TileSpot.Center => "*",
                TileSpot.North => "N",
                TileSpot.South => "S",
                TileSpot.East => "E",
                TileSpot.West => "W",
                TileSpot.NorthWest => "NW",
                TileSpot.NorthEast => "NE",
                TileSpot.SouthEast => "SE",
                TileSpot.SouthWest => "SW",
                _ => throw new InvalidOperationException(),
            };
            var button = new Button(MLEM.Ui.Anchor.AutoInline, new Vector2(32, 32), $"<f Default 0.5>{str}")
            {
                OnPressed = _ =>
                {
                    tileEditType = val;
                    smartSlopeEdit = false;
                }
            };

            if (i % 3 == 0)
                button.Anchor = MLEM.Ui.Anchor.AutoLeft;
            elems.Add(button);
        }

        elems.Add(new VerticalSpace(24));

        elems.Add(new Paragraph(MLEM.Ui.Anchor.AutoLeft, 1, "Smart Slope"));
        vals =
        [
            TileSpot.North,
            TileSpot.South,
            TileSpot.West,
            TileSpot.East,
        ];
        for (int i = 0; i < vals.Length; ++i)
        {
            var val = vals[i];
            var button = new Button(MLEM.Ui.Anchor.AutoInline, new Vector2(0.35f, 24), $"<f Default 0.5>{val}")
            {
                OnPressed = _ =>
                {
                    tileEditType = val;
                    smartSlopeEdit = true;
                }
            };
            if (i % 2 == 0)
                button.Anchor = MLEM.Ui.Anchor.AutoLeft;

            elems.Add(button);
        }

        return elems;
    }

    private void UpdateHover()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;

        Point? hoverTile;
        if (!InputUtils.TryHover(Editable.Location, ShowMissingInLocation, Game1.graphics.GraphicsDevice.Viewport, editor.ProjectionMatrix, editor.Camera.ViewMatrix, editor.Ui.Controls.Input.MousePosition, out hoverTile, out _))
            hoverTile = null;

        bool hoverDirty = hoverTile != lastHoverTile;
        if (hoverDirty)
        {
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

        // Selection mode
        var oldSel = pendingSelectMode;
        pendingSelectMode = SelectMode.Replace;
        if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Shift))
            pendingSelectMode = SelectMode.Add;
        else if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Alt))
            pendingSelectMode = SelectMode.Remove;
        if (oldSel != pendingSelectMode)
            selDirty = true;

        // Select all
        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.A) && editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control))
        {
            HashSet<Point> pending = new();
            for (int ix = 0; ix < Editable.Location.Map.Layers[0].LayerWidth; ++ix)
            {
                for (int iy = 0; iy < Editable.Location.Map.Layers[0].LayerHeight; ++iy)
                {
                    pending.Add(new(ix, iy));
                }
            }
            DoSelect(pending);
        }

        // Flood fill
        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.F) && lastHoverTile.HasValue)
        {
            var baseData = DimensionUtils.GetPositionForTile(Editable.Location, lastHoverTile.Value, TileType);
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

                var data = DimensionUtils.GetPositionForTile(Editable.Location, check, TileType);
                if (data.ShouldHide != baseData.ShouldHide ||
                    data.Position.Y + data.HeightBoundingSize / 2 < min ||
                    data.Position.Y - data.HeightBoundingSize / 2 > max)
                    continue;
                matching.Add(check);

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

            DoSelect(matching);
        }

        // Drag select has finished
        if (leftMouse && !editor.Ui.Controls.Input.IsDown(MouseButton.Left))
        {
            leftMouse = false;

            HashSet<Point> pending = new();
            if (pendingStartTile.HasValue && lastHoverTile.HasValue)
            {
                for (int ix = Math.Min(pendingStartTile.Value.X, lastHoverTile.Value.X); ix <= Math.Max(pendingStartTile.Value.X, lastHoverTile.Value.X); ++ix)
                {
                    for (int iy = Math.Min(pendingStartTile.Value.Y, lastHoverTile.Value.Y); iy <= Math.Max(pendingStartTile.Value.Y, lastHoverTile.Value.Y); ++iy)
                    {
                        pending.Add(new(ix, iy));
                    }
                }
            }
            DoSelect(pending);

            pendingStartTile = null;
            pendingDirty = true;
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

    private void DoSelect(ICollection<Point> tiles)
    {
        if (pendingSelectMode == SelectMode.Replace)
            selectedTiles.Clear();
        foreach (var sel in tiles)
        {
            if (pendingSelectMode == SelectMode.Remove)
                selectedTiles.Remove(sel);
            else
                selectedTiles.Add(sel);
        }
        selDirty = true;
    }

    private void UpdateModifications()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;

        int scrollAmt = (editor.Ui.Controls.Input.ScrollWheel - editor.Ui.Controls.Input.LastScrollWheel) / 120;
        if (scrollAmt != 0)
        {
            float incr = 1f;
            if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Shift))
                incr = 1f / 4;
            else if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control))
                incr = 1f / 16;

            incr *= scrollAmt;

            if (smartSlopeEdit && selectedTiles.Count > 0)
            {
                List<Point> tiles = selectedTiles.ToList();

                TileSpot opposite = tileEditType switch
                {
                    TileSpot.East => TileSpot.West,
                    TileSpot.West => TileSpot.East,
                    TileSpot.South => TileSpot.North,
                    TileSpot.North => TileSpot.South,
                    _ => throw new InvalidOperationException(),
                };
                Point min = Point.Zero, max = Point.Zero;
                int steps = 0;
                switch (tileEditType)
                {
                    case TileSpot.West:
                    case TileSpot.East:
                        tiles.Sort((a, b) => Comparer<int>.Default.Compare(a.X, b.X));
                        min = tiles.First();
                        max = tiles.Last();
                        if (tileEditType == TileSpot.West)
                            Util.Swap(ref min, ref max);
                        steps = Math.Abs(min.X - max.X);
                        break;
                    case TileSpot.North:
                    case TileSpot.South:
                        tiles.Sort((a, b) => Comparer<int>.Default.Compare(a.Y, b.Y));
                        min = tiles.First();
                        max= tiles.Last();
                        if (tileEditType == TileSpot.North)
                            Util.Swap(ref min, ref max);
                        steps = Math.Abs(min.Y - max.Y);
                        break;
                }

                float baseHeight = Editable.Location.GetDimensionData(TileType, min, TileSpot.Center);
                float endHeight = Editable.Location.GetDimensionData(TileType, max, TileSpot.Center);
                endHeight += Editable.Location.GetDimensionData(TileType, max, tileEditType);
                endHeight += incr * (steps + 1);

                incr = (endHeight - baseHeight) / (steps + 1);
                foreach (var tile in tiles)
                {
                    int amt = tileEditType switch
                    {
                        TileSpot.West => Math.Abs(min.X - tile.X),
                        TileSpot.East => Math.Abs(min.X - tile.X),
                        TileSpot.North => Math.Abs(min.Y - tile.Y),
                        TileSpot.South => Math.Abs(min.Y - tile.Y),
                        _ => throw new InvalidOperationException(),
                    };
                    Editable.Location.SetDimensionData(TileType, tile, baseHeight + incr * amt, TileSpot.Center);
                    Editable.Location.SetDimensionData(TileType, tile, incr, tileEditType);
                }
            }
            else
            {
                foreach (var tile in selectedTiles)
                    Editable.Location.ModifyDimensionData(TileType, tile, incr, tileEditType);
            }

            MapModified();
        }

        if (editor.Ui.Controls.Input.TryConsumePressed(Keys.Delete))
        {
            foreach (var tile in selectedTiles)
            {
                foreach ( var type in Enum.GetValues<TileSpot>() )
                    Editable.Location.SetDimensionData(TileType, tile, null, type);
            }

            MapModified();
        }
    }

    public override void Update()
    {
        UpdateHover();
        UpdateSelection();
        UpdateModifications();
    }

    private void MakeQuad(List<Vector3> verts, DimensionUtils.PositionResult quad)
    {
        float adjustL = 0, adjustR = 0;
        float adjustU = 0, adjustD = 0;
        switch (tileEditType)
        {
            case TileSpot.West: adjustR -= 0.5f; break;
            case TileSpot.North: adjustD -= 0.5f; break;
            case TileSpot.East: adjustL += 0.5f; break;
            case TileSpot.South: adjustU += 0.5f; break;
            case TileSpot.NorthWest:
                adjustR -= 0.5f;
                adjustD -= 0.5f;
                break;
            case TileSpot.NorthEast:
                adjustL += 0.5f;
                adjustD -= 0.5f;
                break;
            case TileSpot.SouthWest:
                adjustR -= 0.5f;
                adjustU += 0.5f;
                break;
            case TileSpot.SouthEast:
                adjustL += 0.5f;
                adjustU += 0.5f;
                break;
        }

        Vector3 adjust00 = quad.QuadVert00 + new Vector3(adjustL, 0, adjustU);
        Vector3 adjust10 = quad.QuadVert10 + new Vector3(adjustR, 0, adjustU);
        Vector3 adjust01 = quad.QuadVert01 + new Vector3(adjustL, 0, adjustD);
        Vector3 adjust11 = quad.QuadVert11 + new Vector3(adjustR, 0, adjustD);
        switch (tileEditType)
        {
            case TileSpot.West:
                adjust10.Y = Utility.Lerp(adjust00.Y, adjust10.Y, 0.5f);
                adjust11.Y = Utility.Lerp(adjust01.Y, adjust11.Y, 0.5f);
                break;
            case TileSpot.North:
                adjust01.Y = Utility.Lerp(adjust00.Y, adjust01.Y, 0.5f);
                adjust11.Y = Utility.Lerp(adjust10.Y, adjust11.Y, 0.5f);
                break;
            case TileSpot.East:
                adjust00.Y = Utility.Lerp(adjust00.Y, adjust10.Y, 0.5f);
                adjust01.Y = Utility.Lerp(adjust01.Y, adjust11.Y, 0.5f);
                break;
            case TileSpot.South:
                adjust00.Y = Utility.Lerp(adjust00.Y, adjust01.Y, 0.5f);
                adjust10.Y = Utility.Lerp(adjust10.Y, adjust11.Y, 0.5f);
                break;
            case TileSpot.NorthWest:
                adjust10.Y = Utility.Lerp(adjust00.Y, adjust10.Y, 0.5f);
                adjust01.Y = Utility.Lerp(adjust00.Y, adjust01.Y, 0.5f);
                adjust11.Y = Utility.Lerp(adjust00.Y, adjust11.Y, 0.5f);
                break;
            case TileSpot.NorthEast:
                adjust00.Y = Utility.Lerp(adjust10.Y, adjust00.Y, 0.5f);
                adjust01.Y = Utility.Lerp(adjust10.Y, adjust01.Y, 0.5f);
                adjust11.Y = Utility.Lerp(adjust10.Y, adjust11.Y, 0.5f);
                break;
            case TileSpot.SouthWest:
                adjust00.Y = Utility.Lerp(adjust01.Y, adjust00.Y, 0.5f);
                adjust10.Y = Utility.Lerp(adjust01.Y, adjust10.Y, 0.5f);
                adjust11.Y = Utility.Lerp(adjust01.Y, adjust11.Y, 0.5f);
                break;
            case TileSpot.SouthEast:
                adjust00.Y = Utility.Lerp(adjust11.Y, adjust00.Y, 0.5f);
                adjust10.Y = Utility.Lerp(adjust11.Y, adjust10.Y, 0.5f);
                adjust01.Y = Utility.Lerp(adjust11.Y, adjust01.Y, 0.5f);
                break;
        }

        verts.Add(quad.Position + adjust00 + quad.QuadFacingNormal * 0.02f);
        verts.Add(quad.Position + adjust01 + quad.QuadFacingNormal * 0.02f);
        verts.Add(quad.Position + adjust10 + quad.QuadFacingNormal * 0.02f);
        verts.Add(quad.Position + adjust11 + quad.QuadFacingNormal * 0.02f);
        verts.Add(quad.Position + adjust10 + quad.QuadFacingNormal * 0.02f);
        verts.Add(quad.Position + adjust01 + quad.QuadFacingNormal * 0.02f);
    }

    private void UpdateSelectionDisplay()
    {
        if (pendingDirty)
        {
            pendingDirty = false;

            pendingTiles.Clear();
            if (pendingStartTile.HasValue && lastHoverTile.HasValue)
            {
                for (int ix = Math.Min(pendingStartTile.Value.X, lastHoverTile.Value.X); ix <= Math.Max(pendingStartTile.Value.X, lastHoverTile.Value.X); ++ix)
                {
                    for (int iy = Math.Min(pendingStartTile.Value.Y, lastHoverTile.Value.Y); iy <= Math.Max(pendingStartTile.Value.Y, lastHoverTile.Value.Y); ++iy)
                    {
                        pendingTiles.Add(new(ix, iy));
                    }
                }
            }

            pendingBounds.Clear();
            if (pendingSelectMode == SelectMode.Replace || pendingSelectMode == SelectMode.Add)
            {
                foreach (var tile in pendingTiles)
                    MakeQuad(pendingBounds, DimensionUtils.GetPositionForTile(Editable.Location, tile, TileType));
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

                    MakeQuad(selBounds, DimensionUtils.GetPositionForTile(Editable.Location, tile, TileType));
                }
            }
        }
    }

    public override void Render()
    {
        UpdateSelectionDisplay();

        Game1.graphics.GraphicsDevice.BlendState = new BlendState()
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,

            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,

            ColorBlendFunction = BlendFunction.Subtract,
            AlphaBlendFunction = BlendFunction.Add,

            //BlendFactor = Color.White * 0.5f,
        };
        Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        //Game1.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
        RenderHelper.GenericEffect.Texture = Game1.staminaRect;
        RenderHelper.GenericEffect.World = Matrix.Identity;

        if (lastHoverTile.HasValue && pendingSelectMode != SelectMode.Remove)
        {
            List<Vector3> hover = new();
            MakeQuad(hover, DimensionUtils.GetPositionForTile(Editable.Location, lastHoverTile.Value, TileType));
            SimpleVertex[] v = hover.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.Gray)).ToArray();

            RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing"];
            foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
            }
        }
        if (pendingBounds.Count > 0)
        {
            SimpleVertex[] v = pendingBounds.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.LightGray)).ToArray();

            RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing"];
            foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
            }
        }
        if (selBounds.Count > 0)
        {
            SimpleVertex[] v = selBounds.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.DarkGray)).ToArray();

            RenderHelper.GenericEffect.CurrentTechnique = RenderHelper.GenericEffect.Techniques["SingleDrawing"];
            foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
            }
        }
    }
}
