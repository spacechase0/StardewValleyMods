using Microsoft.Xna.Framework;
using Stardew3D.Rendering;

namespace Stardew3D.GameModes.FirstPerson;

public class Camera : ICamera
{
    public Vector3 Position { get; set; }
    public float RotationForHorizontal { get; set; } = 0;
    public float RotationForVertical { get; set; } = 0;

    public virtual Vector3 Up => Vector3.Cross(Vector3.Transform(Vector3.Right, Matrix.CreateRotationY(RotationForHorizontal)), Forward);
    public virtual Vector3 Forward => Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(RotationForVertical) * Matrix.CreateRotationY(RotationForHorizontal));

    public virtual Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position + Forward, Up);
}