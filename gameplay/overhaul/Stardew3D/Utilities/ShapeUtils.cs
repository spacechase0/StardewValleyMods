using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Valve.VR;

namespace Stardew3D.Utilities;

public static class ShapeUtils
{
    extension(Rectangle rect)
    {
        // I'm kinda tired when writing this, so who knows if it is correct.
        // Probably is much sloer than normal solutions, at least.
        public bool LineSegmentIntersects(Vector2 start, Vector2 end, out Vector2 intersection)
        {
            Vector2 minBounds = new Vector2(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
            Vector2 maxBounds = new Vector2(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));

            // The extremes of the segment don't approach the rect
            if (maxBounds.X < rect.Left || maxBounds.Y < rect.Top || minBounds.X >= rect.Right || minBounds.Y >= rect.Bottom)
            {
                intersection = Vector2.Zero;
                return false;
            }

            Vector2 left = start.X < end.X ? start : end;
            Vector2 right = start.X < end.X ? end : start;
            Vector2 up = start.Y < end.Y ? start : end;
            Vector2 down = start.Y < end.Y ? end : start;

            Vector2 segmentDiff = end - start;
            Vector2 norm = segmentDiff.Normalized();
            float len = segmentDiff.Length();

            if (norm.X != 0)
            {
                if (left.X < rect.Left)
                {
                    Vector2 diff = new Vector2(rect.Left - left.X, 0);
                    diff.Y = (diff.X / norm.X) * norm.Y;
                    if (left.Y + diff.Y >= rect.Top && left.Y + diff.Y <= rect.Bottom)
                    {
                        intersection = left + diff;
                        return true;
                    }
                }
                if (right.X > rect.Right)
                {
                    Vector2 diff = new Vector2(rect.Right - right.X, 0);
                    diff.Y = (diff.X / norm.X) * norm.Y;
                    if (right.Y + diff.Y >= rect.Top && right.Y + diff.Y <= rect.Bottom)
                    {
                        intersection = right + diff;
                        return true;
                    }
                }
            }
            if (norm.Y != 0)
            {
                if (up.Y < rect.Top)
                {
                    Vector2 diff = new Vector2(0, rect.Top - up.Y);
                    diff.X = (diff.Y / norm.Y) * norm.X;
                    if (up.X + diff.X >= rect.Left && up.X + diff.X <= rect.Right)
                    {
                        intersection = up + diff;
                        return true;
                    }
                }
                if (down.Y > rect.Bottom)
                {
                    Vector2 diff = new Vector2(0, rect.Bottom - down.Y);
                    diff.X = (diff.Y / norm.Y) * norm.X;
                    if (down.X + diff.X >= rect.Left && down.X + diff.X <= rect.Right)
                    {
                        intersection = down + diff;
                        return true;
                    }
                }
            }

            intersection = Vector2.Zero;
            return false;
        }
    }

    /// <summary>Assumes the points are counter-clockwise order with Y=up. Should also work with clockwise order and Y=down.</summary>
    /// <remarks>If your input matches neither criteria, either the order can be reversed or the Y coordinate can be negated. </remarks>
    public static Vector2[] ConcaveToConvex(this Vector2[] origPoints)
    {
        // I don't know a proper algorithm this, so I kinda just came up with a naive algorithm
        // off the top of my head and tweaked it to work correctly.
        // Probably inefficient, but won't be done very often so should be fine.
        List<Vector2> points = [.. origPoints];
        for (int startPointIndex = 0; startPointIndex < points.Count; startPointIndex++)
        {
            Vector2 startPoint = points[(points.Count + startPointIndex) % points.Count];

            int prevPointIndex = (points.Count + startPointIndex - 1) % points.Count;
            Vector2 prevPoint = points[prevPointIndex];

            float prevAngle = MathF.Atan2( startPoint.Y - prevPoint.Y, startPoint.X - prevPoint.X );

            int nextPointIndex = (points.Count + startPointIndex + 1) % points.Count;
            Vector2 nextPoint = points[nextPointIndex];

            float nextAngle = MathF.Atan2(nextPoint.Y - startPoint.Y, nextPoint.X - startPoint.X);
            if (nextAngle < prevAngle - MathF.PI) nextAngle += MathF.PI * 2;
            if (nextAngle > prevAngle + MathF.PI) nextAngle -= MathF.PI * 2;

            if (nextAngle < prevAngle)
            {
                // Went the wrong direction.
                float testAngle = MathF.Atan2(nextPoint.Y - prevPoint.Y, nextPoint.X - prevPoint.X);
                if (testAngle < prevAngle - MathF.PI) testAngle += MathF.PI * 2;
                if (testAngle > prevAngle + MathF.PI) testAngle -= MathF.PI * 2;

                if (testAngle > prevAngle)
                {
                    // The current point is further out, so the next point is part of the concavity
                    points.RemoveAt(nextPointIndex);
                    startPointIndex -= 1;
                }
                else
                {
                    // The next point is further out, so the current point is part of the concavity
                    points.RemoveAt((points.Count + startPointIndex) % points.Count);
                    startPointIndex -= 2;
                }
            }
        }

        return points.ToArray();
    }
}
