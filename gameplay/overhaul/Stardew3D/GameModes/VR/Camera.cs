using Microsoft.Xna.Framework;
using Stardew3D.Rendering;

namespace Stardew3D.GameModes.VR;
public class Camera : ICamera
{
    public Vector3 Position { get; set; }

    public Matrix HeadsetRotation { get; set; } = Matrix.Identity;
    public float AdditionalRotationY { get; set; } = 0;
    public Matrix AdditionalTransform { get; set; } = Matrix.Identity;

    public Vector3 Up => Vector3.TransformNormal(Vector3.TransformNormal(Vector3.Up, HeadsetRotation), Matrix.CreateRotationY(AdditionalRotationY));
    public Vector3 Forward => Vector3.TransformNormal(Vector3.TransformNormal(Vector3.Forward, HeadsetRotation), Matrix.CreateRotationY(AdditionalRotationY));
    public Matrix ViewMatrix
    {
        get
        {
            var ret = Matrix.Identity;
            ret *= Matrix.CreateLookAt(Position, Position + Forward, Up);
            ret *= AdditionalTransform;
            return ret;
        }
    }
}
