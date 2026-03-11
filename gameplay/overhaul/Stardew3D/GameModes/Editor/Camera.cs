using Microsoft.Xna.Framework;
using Stardew3D.Rendering;

namespace Stardew3D.GameModes.Editor;

internal class Camera : ICamera
{
    public Vector3 Up { get; set; } = Vector3.Up;

    public Vector3 Forward { get; set; } = Vector3.Forward;

    public Vector3 Position { get; set; } = Vector3.Zero;

    public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position + Forward, Up);
}
