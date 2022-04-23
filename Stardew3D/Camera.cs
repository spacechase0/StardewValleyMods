using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Stardew3D
{
    public class Camera
    {
        public Vector3 Target { get; set; }
        public float RotationX { get; set; } = MathHelper.ToRadians(30);
        public float RotationY { get; set; } = MathHelper.ToRadians(90);
        public float Distance { get; set; } = 5;

        public Vector3 GetUp()
        {
            return Vector3.Up;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(MathF.Cos(RotationY), MathF.Cos(RotationX), MathF.Sin(RotationY)) * Distance + Target;
        }

        public Matrix CreateViewMatrix()
        {
            return Matrix.CreateLookAt(GetPosition(), Target, GetUp());
        }
    }
}
