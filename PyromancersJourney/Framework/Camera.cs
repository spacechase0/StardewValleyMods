using Microsoft.Xna.Framework;

namespace PyromancersJourney.Framework
{
    internal class Camera
    {
        public Vector3 Pos = new(10, 0, 10);
        public Vector3 Up = Vector3.Up;
        public Vector3 Target = Vector3.Zero;

        public Matrix CreateViewMatrix()
        {
            return Matrix.CreateLookAt(this.Pos, this.Target, this.Up);
        }
    }
}
