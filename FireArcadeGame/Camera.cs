using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireArcadeGame
{
    public class Camera
    {
        public Vector3 pos = new Vector3( 10, 0, 10 );
        public Vector3 up = Vector3.Up;
        public Vector3 target = Vector3.Zero;

        public Matrix CreateViewMatrix()
        {
            return Matrix.CreateLookAt( pos, target, up );
        }
    }
}
