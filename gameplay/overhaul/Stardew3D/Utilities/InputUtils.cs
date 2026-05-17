using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Handlers;
using StardewValley;
using static Stardew3D.Handlers.LocationHandler;
using static Stardew3D.Utilities.DimensionUtils;

namespace Stardew3D.Utilities;

public static class InputUtils
{
    public static bool TryHover(GameLocation loc, TerrainType check, Viewport viewport, Matrix projMatrix, Matrix viewMatrix, Point mousePos, out Point hoverTile, out TileSpot wallDir, int maxRange = 1000)
    {
        Vector3 near = viewport.Unproject(new Vector3(mousePos.ToVector2(), 0), projMatrix, viewMatrix, Matrix.Identity);
        Vector3 far = viewport.Unproject(new Vector3(mousePos.ToVector2(), 1), projMatrix, viewMatrix, Matrix.Identity);
        Ray cursor = new(near, (far - near).Normalized());

        return TryHover(loc, check, cursor, out _, out hoverTile, out _, out wallDir, maxRange);
    }
    public static bool TryHover(GameLocation loc, TerrainType check, Ray cursor, out TerrainType hoverType, out Point hoverTile, out float hoverDist, out TileSpot wallDir, int maxRange = 1000)
    {
        LocationHandler handler = Mod.State.GetUpdateHandlersFor(loc)[0] as LocationHandler;

        Rectangle mapBounds = new Rectangle(0, 0, loc.Map.Layers[0].LayerWidth, loc.Map.Layers[0].LayerHeight);

        Vector2 cursorPos2d = new(cursor.Position.X, cursor.Position.Z);
        Vector2 cursorDir2d = new(cursor.Direction.X, cursor.Direction.Z);

        hoverType = TerrainType.None;
        hoverTile = Point.Zero;
        hoverDist = float.MaxValue;
        wallDir = TileSpot.Center;
        for (int i = 0; i < maxRange; i += 1)
        {
            Point cursorPosTile2d = new Vector2(MathF.Floor(cursorPos2d.X), MathF.Floor(cursorPos2d.Y)).ToPoint();
            Rectangle tileRect = new(cursorPosTile2d.X, cursorPosTile2d.Y, 1, 1);

            Vector2 tile = cursorPosTile2d.ToVector2();

            bool Check(TileType tileType, out Point hoverTile, out float hoverDist)
            {
                var quad = DimensionUtils.GetPositionForTile(loc, cursorPosTile2d, tileType);

                Plane plane = new Plane(quad.Position, quad.QuadFacingNormal);
                cursor.Intersects(ref plane, out var dist);
                Vector3 intersectAt = dist.HasValue ? (cursor.Position + cursor.Direction * dist.Value) : Vector3.Zero;
                Vector2 intersectAt2d = new Vector2(intersectAt.X, intersectAt.Z);
                if (dist.HasValue && tileRect.Contains(intersectAt2d))
                {
                    hoverTile = cursorPosTile2d;
                    hoverDist = dist.Value;
                    return true;
                }

                hoverTile = Point.Zero;
                hoverDist = float.MaxValue;
                return false;
            }

            if (check.HasFlag(TerrainType.Floor) && Check(TileType.Floor, out hoverTile, out hoverDist))
            {
                hoverType = TerrainType.Floor;
                return true;
            }
            if (check.HasFlag(TerrainType.Ceiling) && Check(TileType.Ceiling, out hoverTile, out hoverDist))
            {
                hoverType = TerrainType.Ceiling;
                return true;
            }
            if (check.HasFlag(TerrainType.Water) && Check(TileType.Water, out hoverTile, out hoverDist))
            {
                hoverType = TerrainType.Water;
                return true;
            }

            if (check.HasFlag(TerrainType.Walls) && handler != null && mapBounds.Contains(cursorPosTile2d) && handler.wallData != null)
            {
                for (int idir = 0; idir < 4; ++idir)
                {
                    if (handler.wallData[cursorPosTile2d.X, cursorPosTile2d.Y, idir] is not WallData data)
                        continue;

                    Vector3 normal = idir switch
                    {
                        (int)TileSpot.North => Vector3.Backward,
                        (int)TileSpot.South => Vector3.Forward,
                        (int)TileSpot.West => Vector3.Right,
                        (int)TileSpot.East => Vector3.Left,
                        _ => throw new InvalidOperationException(),
                    };

                    // Wall is facing a different direction
                    if (Vector3.Dot(normal, cursor.Direction) >= 0)
                        continue;

                    Vector3 wallCenter = DimensionUtils.GetPositionAtTile(loc, cursorPosTile2d, idir switch
                    {
                        (int)TileSpot.North => new Vector2(0.5f, 0),
                        (int)TileSpot.South => new Vector2(0.5f, 1),
                        (int)TileSpot.West => new Vector2(0, 0.5f),
                        (int)TileSpot.East => new Vector2(1, 0.5f),
                        _ => throw new InvalidOperationException(),
                    });
                    Plane plane = new Plane(wallCenter, normal);

                    // Wall is out of range
                    if (cursor.Intersects(plane) is not float dist || dist > maxRange)
                        continue;

                    Vector3 spot = cursor.Position + cursor.Direction * dist;
                    if (spot.X >= tile.X && spot.X <= tile.X + 1 && (idir == (int)TileSpot.North || idir == (int)TileSpot.South))
                    {
                        float checkPerc = spot.X - tile.X;
                        if (idir == (int)TileSpot.South) checkPerc = 1 - checkPerc;
                        float bottom = Utility.Lerp(data.LeftOffset, data.RightOffset, checkPerc);
                        float size = Utility.Lerp(data.LeftSize, data.RightSize, checkPerc);
                        if (spot.Y < bottom || spot.Y > bottom + size)
                            continue;
                    }
                    else if (spot.Z >= tile.Y && spot.Z <= tile.Y + 1 && (idir == (int)TileSpot.West || idir == (int)TileSpot.East))
                    {
                        float checkPerc = spot.Z - tile.Y;
                        if (idir == (int)TileSpot.West) checkPerc = 1 - checkPerc;
                        float bottom = Utility.Lerp(data.LeftOffset, data.RightOffset, checkPerc);
                        float size = Utility.Lerp(data.LeftSize, data.RightSize, checkPerc);
                        if (spot.Y < bottom || spot.Y > bottom + size)
                            continue;
                    }
                    else continue;

                    // TODO: Check for top/bottom wall boundaries

                    hoverTile = cursorPosTile2d;
                    hoverType = TerrainType.Walls;
                    hoverDist = dist;
                    wallDir = (TileSpot) idir;
                    return true;
                }
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

        return false;
    }
}
