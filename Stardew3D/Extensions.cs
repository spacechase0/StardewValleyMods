using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Models;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace Stardew3D
{
    public static class Extensions
    {
        public static Matrix ToMonogame(this System.Numerics.Matrix4x4 mat)
        {
            return new(mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44);
        }

        public static Vector3 GetPositionAtTile(xTile.Map map, Point tile, Vector2 subTile, bool forCeiling = false)
        {
            var data = GetPositionForTile(map, tile, forCeiling);
            if (float.IsNaN(data.Position.Y))
                return data.Position;

            Plane plane = new(data.Position, data.QuadFacingNormal);

            float dist = 100000;
            Ray test = new(new(data.Position.X + subTile.X - 0.5f, dist, data.Position.Y + subTile.Y - 0.5f), Vector3.Down);
            float ret = dist - test.Intersects(plane).Value;

            // TODO: Map resulting X/Z for "region" thing

            return new(tile.X + subTile.X, ret, tile.Y + subTile.Y);
        }

        public static float GetValueForDataTileIndex(int index)
        {
            if (index == -1)
                return float.NaN;

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

        public static (Vector3 Position, Vector3 QuadFacingNormal, Vector3 QuadUpNormal, Vector2 QuadSize) GetPositionForTile(xTile.Map map, Point tile, bool forCeiling = false)
        {
            if (tile.X < 0 || tile.Y < 0 || tile.X >= map.Layers[0].LayerWidth || tile.Y >= map.Layers[0].TileHeight)
                return new(new Vector3(tile.X, float.NaN, tile.Y), forCeiling ? Vector3.Down : Vector3.Up, Vector3.Forward, Vector2.One);

            string dataLayer = $"{Mod.Instance.ModManifest.UniqueID}/{(forCeiling ? "Ceiling" : "Floor")}Data";
            string dataModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/{(forCeiling ? "Ceiling" : "Floor")}ModifierData";

            var data = map.GetLayer(dataLayer);
            var modifiers = map.GetLayer(dataModifierLayer);

            if (data == null)
                return new(new Vector3(tile.X, float.NaN, tile.Y), forCeiling ? Vector3.Down : Vector3.Up, Vector3.Forward, Vector2.One);

            float baseHeight = GetValueForDataTileIndex(data.GetTileIndexAt(tile.X, tile.Y));
            float topLeft = baseHeight;
            float topRight = baseHeight;
            float bottomRight = baseHeight;
            float bottomLeft = baseHeight;

            ModifyValueForDataTileIndex(modifiers?.GetTileIndexAt(tile.X, tile.Y) ?? -1, ref topLeft, ref topRight, ref bottomRight, ref bottomLeft);

            // Probably incorrect implementation, just cobbled something together myself
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

            return new(new Vector3(tile.X + 0.5f, center, tile.Y + 0.5f),
                       Vector3.TransformNormal(forCeiling ? Vector3.Down : Vector3.Up, Matrix.CreateRotationX(rotX) * Matrix.CreateRotationZ(rotZ)),
                       Vector3.TransformNormal(Vector3.Forward, Matrix.CreateRotationX(rotX) * Matrix.CreateRotationZ(rotZ)),
                       new(xLen, zLen));
        }

        public static Vector3 GetPositionAtTile(xTile.Map map, Point pt, TileSpot spot = TileSpot.Center, bool forCeiling = false)
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
            if (float.IsNaN(ret.Y))
                ret.Y = 0;
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

        public static Vector3 To3D(this Vector2 vec, xTile.Map map, bool forCeiling = false)
        {
            Point tile = new((int)(vec.X / Game1.tileSize), (int)(vec.Y / Game1.tileSize));
            Vector2 subTile = new Vector2(vec.X / Game1.tileSize, vec.Y / Game1.tileSize) - tile.ToVector2();
            return GetPositionAtTile(map, tile, subTile, forCeiling);
        }

        public static Vector3 To3D(this Point pt, xTile.Map map, bool forCeiling = false)
        {
            Vector2 subTile = new Vector2(0.5f, 0.5f);
            return GetPositionAtTile(map, pt, subTile, forCeiling);
        }

        public static Matrix Reverse(this Matrix m)
        {
            m.Right = -m.Right;
            m.Up = -m.Up;
            m.Forward = -m.Forward;
            m.Translation = -m.Translation;
            return m;
        }

        public static Vector3 Normalized(this Vector3 v) => Vector3.Normalize(v);
        public static Matrix Invert(this Matrix m) => Matrix.Invert(m);
        public static Matrix Transpose(this Matrix m) => Matrix.Transpose(m);

        // https://medium.com/data-science/change-of-basis-3909ef4bed43
        public static Matrix ChangeBasis(this Matrix input)
        {
            var basisChange = Matrix.Identity;
            basisChange[2, 2] = -1;
            var basisChangeInverse = basisChange;
            return basisChange * input * basisChangeInverse;
        }
    }
}
