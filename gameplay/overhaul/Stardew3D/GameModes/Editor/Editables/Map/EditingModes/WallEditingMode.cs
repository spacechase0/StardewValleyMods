using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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
public class WallEditingMode : BaseEditingMode
{
    public struct WallSelection
    {
        public Point Tile;
        public TileSpot Direction;

        public WallSelection() { }
        public WallSelection(Point tile, TileSpot dir) { Tile = tile; Direction = dir; }
        public WallSelection(int x, int y, TileSpot dir) { Tile = new Point( x, y ); Direction = dir; }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is WallSelection other && other.Tile == Tile && other.Direction == Direction;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tile, Direction);
        }

        public static bool operator == (WallSelection a, WallSelection b)
        {
            return a.Tile == b.Tile && a.Direction == b.Direction;
        }

        public static bool operator != (WallSelection a, WallSelection b)
        {
            return a.Tile != b.Tile || a.Direction != b.Direction;
        }
    }

    [Flags]
    public enum WallSide
    {
        Left = 1 << 0,
        Right = 1 << 1,
        All = Left | Right,
    }

    public override string Id => "Walls";

    public override LocationHandler.TerrainType ShowMissingInLocation => LocationHandler.TerrainType.Walls;

    private bool leftMouse, rightMouse;
    private float mouseHoldTimer = 0;
    private WallSelection? lastHoverTile = null;
    private SelectMode pendingSelectMode = SelectMode.Replace;
    private HashSet<WallSelection> selectedTiles = new();
    private List<Vector3> selBounds = new List<Vector3>();
    private bool selDirty = false;
    private WallSide tileEditType
    {
        get => field;
        set
        {
            field = value;
            selDirty = true;
        }
    } = WallSide.All;

    public WallEditingMode(MapEditable editable)
        : base(editable)
    {
    }

    private void MapModified()
    {
        Editable.MapModified();

        selDirty = true;
    }

    public override ICollection<Element> PopulatePanelContents()
    {
        // TODO
        return [];
    }


    public override void Update()
    {
        UpdateHover();
        UpdateSelection();
        UpdateModifications();
    }

    private void UpdateHover()
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;

        WallSelection? hover = null;
        if (editor.Ui.Controls.Input.IsModifierKeyDown(ModifierKey.Control))
        {
            // Allow selecting tiles which *don't* have walls, so we can add manual overrides
            if (InputUtils.TryHover(Editable.Location, LocationHandler.TerrainType.Floor, Game1.graphics.GraphicsDevice.Viewport, editor.ProjectionMatrix, editor.Camera.ViewMatrix, editor.Ui.Controls.Input.MousePosition, out var hoverTile, out _))
            {
                Vector3 near = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 0), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
                Vector3 far = Game1.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(editor.Ui.Controls.Input.MousePosition.ToVector2(), 1), editor.ProjectionMatrix, editor.Camera.ViewMatrix, Matrix.Identity);
                Ray cursor = new(near, (far - near).Normalized());

                bool xGreater = Math.Abs(cursor.Direction.X) > Math.Abs(cursor.Direction.Z);
                TileSpot dir;
                if (xGreater) dir = cursor.Direction.X < 0 ? TileSpot.West : TileSpot.East;
                else          dir = cursor.Direction.Z < 0 ? TileSpot.North : TileSpot.South;

                hover = new()
                {
                    Tile = hoverTile.Value,
                    Direction = dir,
                };
            }
        }
        else
        {
            if (InputUtils.TryHover(Editable.Location, LocationHandler.TerrainType.Walls, Game1.graphics.GraphicsDevice.Viewport, editor.ProjectionMatrix, editor.Camera.ViewMatrix, editor.Ui.Controls.Input.MousePosition, out var hoverTile, out var hoverWallDir))
            {
                hover = new()
                {
                    Tile = hoverTile.Value,
                    Direction = hoverWallDir.Value,
                };
            }
        }

        bool hoverDirty = hover != lastHoverTile;
        if (hoverDirty)
        {
            lastHoverTile = hover;
            selDirty = true;
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

        if (leftMouse)
        {
            if (!editor.Ui.Controls.Input.IsDown(MouseButton.Left))
                leftMouse = false;

            if (lastHoverTile.HasValue)
                DoSelect([lastHoverTile.Value]);

            selDirty = true;
        }

        if (rightMouse && !editor.Ui.Controls.Input.IsDown(MouseButton.Right))
            rightMouse = false;

        if (!leftMouse && !rightMouse)
            mouseHoldTimer = 0;
    }

    private void UpdateModifications()
    {
        // TODO
    }

    private void DoSelect(ICollection<WallSelection> tiles)
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

    private void MakeQuad(List<Vector3> verts, WallSelection wall)
    {
        float adjustL = 1, adjustR = 1;
        switch (tileEditType)
        {
            case WallSide.Left: adjustR = 0.5f; break;
            case WallSide.Right: adjustL = 0.5f; break;
        }

        Vector3 normal = wall.Direction switch
        {
            TileSpot.North => Vector3.Backward,
            TileSpot.South => Vector3.Forward,
            TileSpot.West => Vector3.Right,
            TileSpot.East => Vector3.Left,
        };
        Vector3 left = wall.Direction switch
        {
            TileSpot.North => Vector3.Left,
            TileSpot.South => Vector3.Right,
            TileSpot.West => Vector3.Backward,
            TileSpot.East => Vector3.Forward,
        } * 0.5f;
        Vector3 right = -left;
        left *= adjustL;
        right *= adjustR;
        float sizeL = 3, sizeR = 3;

        Vector3 pos = DimensionUtils.GetPositionForTile(Editable.Location, wall.Tile).Position + normal * -0.5f;
        if (wall.Tile.X >= 0 && wall.Tile.Y >= 0 && wall.Tile.X < Editable.Location.Map.Layers[0].LayerWidth && wall.Tile.Y < Editable.Location.Map.Layers[0].LayerHeight)
        {
            LocationHandler handler = Mod.State.GetUpdateHandlersFor(Editable.Location)[0] as LocationHandler;
            if (handler.wallData[wall.Tile.X, wall.Tile.Y, (int)wall.Direction] is LocationHandler.WallData wallData)
            {
                left.Y += wallData.LeftOffset;
                right.Y += wallData.RightOffset;
                sizeL = wallData.LeftSize;
                sizeR = wallData.RightSize;

               pos.Y = 0;
            }
        }

        Vector3 adjust00 = left;
        Vector3 adjust10 = right;
        Vector3 adjust01 = left  + Vector3.Up * sizeL;
        Vector3 adjust11 = right + Vector3.Up * sizeR;

        verts.Add(pos + adjust00 + normal * 0.02f);
        verts.Add(pos + adjust01 + normal * 0.02f);
        verts.Add(pos + adjust10 + normal * 0.02f);
        verts.Add(pos + adjust11 + normal * 0.02f);
        verts.Add(pos + adjust10 + normal * 0.02f);
        verts.Add(pos + adjust01 + normal * 0.02f);
    }

    private void UpdateSelectionDisplay()
    {
        if (selDirty)
        {
            selDirty = false;

            selBounds.Clear();
            foreach (var tile in selectedTiles)
            {
                if (pendingSelectMode == SelectMode.Remove && lastHoverTile.HasValue && lastHoverTile.Value == tile)
                    continue;

                MakeQuad(selBounds, tile);
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
            MakeQuad(hover, lastHoverTile.Value);
            SimpleVertex[] v = hover.Select(pos => new SimpleVertex(pos, Vector2.One * 0.5f, Color.Gray)).ToArray();

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
