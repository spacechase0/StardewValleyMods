using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SixLabors.ImageSharp.Processing;
using Stardew3D;
using Valve.VR;

namespace StardewVR
{
    public static class Extensions
    {
        // https://medium.com/data-science/change-of-basis-3909ef4bed43
        public static Matrix ChangeBasis(this Matrix input)
        {
            var basisChange = Matrix.Identity;
            basisChange.M33 = -1;
            return basisChange * input * basisChange;
        }

        public static Matrix ToMonogame(this HmdMatrix34_t mat)
        {
            var m = new Matrix(
                mat.m0, mat.m1, mat.m2, mat.m3,
                mat.m4, mat.m5, mat.m6, mat.m7,
                mat.m8, mat.m9, mat.m10, mat.m11,
                0, 0, 0, 1.0f);
            return m.Transpose();
        }

        public static Matrix ToMonogame(this HmdMatrix44_t mat)
        {
            var m = new Matrix(
                mat.m0, mat.m1, mat.m2, mat.m3,
                mat.m4, mat.m5, mat.m6, mat.m7,
                mat.m8, mat.m9, mat.m10, mat.m11,
                mat.m12, mat.m13, mat.m14, mat.m15);
            return m.Transpose();
        }

        public static Matrix NoTranslation(this Matrix m)
        {
            m.Translation = Microsoft.Xna.Framework.Vector3.Zero;
            return m;
        }
    }
}
