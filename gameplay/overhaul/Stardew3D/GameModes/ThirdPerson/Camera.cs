using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Stardew3D.Rendering;
using Stardew3D.Utilities;

namespace Stardew3D.GameModes.ThirdPerson
{
    public class Camera : ICamera
    {
        public Vector3 Target { get; set; }
        public float RotationX { get; set; } = MathHelper.ToRadians(30);
        public float RotationY { get; set; } = MathHelper.ToRadians(90);
        public float Distance { get; set; } = 5;

        public Vector3 Up => Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(RotationX) * Matrix.CreateRotationY(RotationY));
        public Vector3 Forward => (Target - Position).Normalized();
        public Vector3 Position => Target + Vector3.Transform( Vector3.Backward * Distance, Matrix.CreateRotationX( RotationX ) * Matrix.CreateRotationY( RotationY ) );
        public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Target, Up);
    }
}
