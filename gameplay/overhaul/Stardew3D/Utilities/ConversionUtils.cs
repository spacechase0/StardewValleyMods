using Microsoft.Xna.Framework;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Valve.VR;

namespace Stardew3D.Utilities;

public static class ConversionUtils
{
    public static Vector3 ToMonogame(this HmdVector3_t vec)
    {
        return new Vector3(vec.v0, vec.v1, vec.v2);
    }

    public static Matrix ToMonogame(this HmdMatrix34_t mat)
    {
        var m = new Matrix(
            mat.m0, mat.m1, mat.m2, mat.m3,
            mat.m4, mat.m5, mat.m6, mat.m7,
            mat.m8, mat.m9, mat.m10, mat.m11,
            0, 0, 0, 1.0f);
        return m.Transposed();
    }

    public static Matrix ToMonogame(this HmdMatrix44_t mat)
    {
        var m = new Matrix(
            mat.m0, mat.m1, mat.m2, mat.m3,
            mat.m4, mat.m5, mat.m6, mat.m7,
            mat.m8, mat.m9, mat.m10, mat.m11,
            mat.m12, mat.m13, mat.m14, mat.m15);
        return m.Transposed();
    }

    public static Matrix ToMonogame(this System.Numerics.Matrix4x4 mat)
    {
        return new(mat.M11, mat.M12, mat.M13, mat.M14,
            mat.M21, mat.M22, mat.M23, mat.M24,
            mat.M31, mat.M32, mat.M33, mat.M34,
            mat.M41, mat.M42, mat.M43, mat.M44);
    }
}
