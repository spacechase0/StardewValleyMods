using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Stardew3D;
using Stardew3D.Rendering;

namespace StardewVR.Handlers.Game;
public class Camera : ICamera
{
    public Vector3 Position { get; set; }

    public Vector3 HeadsetRelativePosition { get; set; }
    public Matrix HeadsetRotation { get; set; } = Matrix.Identity;
    public Matrix AdditionalTransform { get; set; } = Matrix.Identity;

    public Vector3 Up => Vector3.Transform(Vector3.Up, HeadsetRotation);
    public Vector3 Forward => Vector3.Transform(Vector3.Forward, HeadsetRotation);
    public Matrix ViewMatrix
    {
        get
        {
            var ret = Matrix.Identity;
#if false
            ret *= Matrix.CreateTranslation(-Position);
            ret *= HeadsetRotation.Invert();
#else
            ret = Matrix.CreateLookAt(Position, Position + Forward, Up);
#endif
#if true
            ret *= AdditionalTransform;
#endif
            return ret;
        }
    }
}
