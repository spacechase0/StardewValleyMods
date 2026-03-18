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

public static class MathUtils
{
    // https://medium.com/data-science/change-of-basis-3909ef4bed43
    public static Matrix ChangeBasis(this Matrix input)
    {
        var basisChange = Matrix.Identity;
        basisChange.M33 = -1;
        return basisChange * input * basisChange;
    }
    public static Matrix NoTranslation(this Matrix m)
    {
        m.Translation = Microsoft.Xna.Framework.Vector3.Zero;
        return m;
    }

    public static Matrix Reverse(this Matrix m)
    {
        m.Right = -m.Right;
        m.Up = -m.Up;
        m.Forward = -m.Forward;
        m.Translation = -m.Translation;
        return m;
    }

    public static Vector2 Normalized(this Vector2 v) => Vector2.Normalize(v);
    public static Vector3 Normalized(this Vector3 v) => Vector3.Normalize(v);
    public static Matrix Inverted(this Matrix m) => Matrix.Invert(m);
    public static Matrix Transposed(this Matrix m) => Matrix.Transpose(m);

    public static Vector3[] Transform(this Vector3[] verts, Matrix transform)
    {
        verts = verts.ToArray();
        for (int i = 0; i < verts.Length; ++i)
        {
            verts[i] = Vector3.Transform(verts[i], transform);
        }
        return verts;
    }
}
