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

public static class DimensionUtils
{
    extension(Character character)
    {
        public Vector3 StandingPixel3D
        {
            get
            {
                Vector2 pos = character.StandingPixel.ToVector2();
                pos += (character.Position - character.Position.ToPoint().ToVector2());
                pos.Y += -character.yJumpOffset;

                Vector3 ret = pos.To3D(character.currentLocation?.Map);
                if (character is Monster monster && monster.isGlider.Value)
                {
                    ret.Y += 1.25f;
                }

                return ret;
            }
        }
    }

    public static Vector3 GetPositionAtTile(xTile.Map map, Point tile, Vector2 subTile, bool? forCeiling = null)
    {
        var data = GetPositionForTile(map, tile, forCeiling);
        if (data.ShouldHide)
            return data.Position;

        Plane plane = new(data.Position, data.QuadFacingNormal);

        float dist = 100000;
        Ray test = new(new(data.Position.X + subTile.X - 0.5f, dist, data.Position.Z + subTile.Y - 0.5f), Vector3.Down);
        float ret = dist - test.Intersects(plane).Value;

        // TODO: Map resulting X/Z for "region" thing

        return new(tile.X + subTile.X, ret, tile.Y + subTile.Y);
    }

    public static int GetDataTileIndexForValue(float value)
    {
        if (value > 10) value = 10;
        if (value < -10) value = -10;

        int num = (int) Math.Round(Math.Abs(value) * 10);
        int x = num % 10;
        int y = num / 10;
        if (value < 0)
            x += 10;

        return x + y * 20;
    }

    public static float GetValueForDataTileIndex(int index)
    {
        if (index == -1)
            return 0;

        float ret = (index % 10) / 10f + (index / 20);
        if (index % 20 >= 10)
            ret = -ret;
        return ret;
    }

    public static void ModifyValueForDataTileIndex(int index, ref float topLeft, ref float topRight, ref float bottomRight, ref float bottomLeft)
    {
        if (index == -1)
            return;
        TileSpot whichType = (TileSpot)(index / 200);

        float modAmount = GetValueForDataTileIndex(index % 200);
        switch (whichType)
        {
            case TileSpot.West: topLeft += modAmount; bottomLeft += modAmount; break;
            case TileSpot.North: topLeft += modAmount; topRight += modAmount; break;
            case TileSpot.East: topRight += modAmount; bottomRight += modAmount; break;
            case TileSpot.South: bottomLeft += modAmount; bottomRight += modAmount; break;
            case TileSpot.NorthWest: topLeft += modAmount; break;
            case TileSpot.NorthEast: topRight += modAmount; break;
            case TileSpot.SouthEast: bottomRight += modAmount; break;
            case TileSpot.SouthWest: bottomLeft += modAmount; break;
        }
    }

    public struct PositionResult
    {
        public Vector3 Position;
        public Vector3 QuadFacingNormal;
        public Vector3 QuadVert00;
        public Vector3 QuadVert10;
        public Vector3 QuadVert01;
        public Vector3 QuadVert11;
        public float HeightBoundingSize;
        public bool ShouldHide;

        public PositionResult(Point tilePos, bool forCeiling = false)
        : this(new Vector3(tilePos.X + 0.5f, 0, tilePos.Y + 0.5f),
                forCeiling ? Vector3.Down : Vector3.Up,
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f),
                0, true)
        {
        }

        public PositionResult(Vector3 pos, Vector3 quadNormal, Vector3 quadTL, Vector3 quadTR, Vector3 quadBL, Vector3 quadBR, float heightBounding, bool shouldHide = false)
        {
            Position = pos;
            QuadFacingNormal = quadNormal;
            QuadVert00 = quadTL;
            QuadVert10 = quadTR;
            QuadVert01 = quadBL;
            QuadVert11 = quadBR;
            HeightBoundingSize = heightBounding;
            ShouldHide = shouldHide;
        }
    }

    public static PositionResult GetPositionForTile(xTile.Map map, Point tile, bool? forCeiling_ = null)
    {
        bool forCeiling = forCeiling_ ?? false;

        if (map == null)
            return new(tile);

        if (tile.X < 0 || tile.Y < 0 || tile.X >= map.Layers[0].LayerWidth || tile.Y >= map.Layers[0].LayerHeight)
            return new(tile);

        string dataLayer = $"{Mod.Instance.ModManifest.UniqueID}/{(forCeiling ? "Ceiling" : "Floor")}Data";
        string dataModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/{(forCeiling ? "Ceiling" : "Floor")}ModifierData";

        var data = map.GetLayer(dataLayer);
        var modifiers = map.Layers.Where(l => l.Id == dataModifierLayer || l.Id.StartsWith( $"{dataModifierLayer}_" ));

        int tileInd = data?.GetTileIndexAt(tile.X, tile.Y) ?? -1;
        if (data == null || tileInd == -1)
            return new(tile) { ShouldHide = (data != null) };

        float baseHeight = GetValueForDataTileIndex(tileInd);
        float topLeft = baseHeight;
        float topRight = baseHeight;
        float bottomRight = baseHeight;
        float bottomLeft = baseHeight;

        foreach (var modifier in modifiers)
        {
            ModifyValueForDataTileIndex(modifier.GetTileIndexAt(tile.X, tile.Y), ref topLeft, ref topRight, ref bottomRight, ref bottomLeft);
        }

        // Probably incorrect implementation, just cobbled something together myself
        float top = MathF.Max(MathF.Max(topLeft, topRight), MathF.Max(bottomRight, bottomRight));
        float bottom = MathF.Min(MathF.Min(topLeft, topRight), MathF.Min(bottomRight, bottomRight));
        float center = (topLeft + topRight + bottomLeft + bottomRight) / 4;

        float posX = (topRight + bottomRight) / 2;
        float negX = (topLeft + bottomLeft) / 2;
        float xDiff = posX - negX;
        float xLen = MathF.Sqrt(1 + xDiff * xDiff);

        float posZ = (topLeft + topRight) / 2;
        float negZ = (bottomLeft + bottomRight) / 2;
        float zDiff = posZ - negZ;
        float zLen = MathF.Sqrt(1 + zDiff * zDiff);

        float rotX = MathF.Asin(zDiff / zLen);
        float rotZ = MathF.Asin(xDiff / xLen);

        float leftSize = MathF.Sqrt( 1 + MathF.Pow(topLeft - bottomLeft, 2) );
        float topSize = MathF.Sqrt(1 + MathF.Pow(topLeft - topRight, 2));
        float rightSize = MathF.Sqrt(1 + MathF.Pow(topRight - bottomRight, 2));
        float bottomSize = MathF.Sqrt(1 + MathF.Pow(bottomLeft - bottomRight, 2));

        return new(new Vector3(tile.X + 0.5f, center, tile.Y + 0.5f),
            Vector3.TransformNormal(forCeiling ? Vector3.Down : Vector3.Up, Matrix.CreateRotationX(rotX) * Matrix.CreateRotationZ(rotZ)),
            new(-0.5f, topLeft - center, -0.5f),
            new(0.5f, topRight - center, -0.5f),
            new(-0.5f, bottomLeft - center, 0.5f),
            new(0.5f, bottomRight - center, 0.5f),
            top - bottom);
    }

    public static Vector3 GetPositionAtTile(xTile.Map map, Point pt, TileSpot spot = TileSpot.Center, bool? forCeiling = null)
    {
        Vector2[] spotMapping =
        [
            new(0.0f, 0.5f),
            new(0.5f, 0.0f),
            new(1.0f, 0.5f),
            new(0.5f, 1.0f),
            new(0.0f, 0.0f),
            new(1.0f, 0.0f),
            new(1.0f, 1.0f),
            new(0.0f, 1.0f),
            new(0.5f, 0.5f),
        ];
        return GetPositionAtTile(map, pt, spotMapping[(int)spot], forCeiling);
    }

    public static Vector3 GetPosition3D( this Character character )
    {
        var tilePos = character.GetBoundingBox().Center.ToVector2();
        var ret = tilePos.To3D( character.currentLocation.Map ) + new Vector3(0, -character.yJumpOffset, 0);
        return ret;
    }

    public static float GetFacing3D(this Character character)
    {
        switch (character.FacingDirection)
        {
            case Game1.down: return 0;
            case Game1.up: return MathF.PI;
            case Game1.left: return -MathF.PI / 2;
            case Game1.right: return MathF.PI / 2;
        }
        return 0;
    }

    public static Vector3 To3D(this Vector2 vec, xTile.Map map, bool? forCeiling = null)
    {
        Point tile = new((int)(vec.X / Game1.tileSize), (int)(vec.Y / Game1.tileSize));
        Vector2 subTile = new Vector2(vec.X / Game1.tileSize, vec.Y / Game1.tileSize) - tile.ToVector2();
        return GetPositionAtTile(map, tile, subTile, forCeiling);
    }

    public static Vector3 To3D(this Point pt, xTile.Map map, bool? forCeiling = null)
    {
        Vector2 subTile = new Vector2(0.5f, 0.5f);
        return GetPositionAtTile(map, pt, subTile, forCeiling);
    }
}
