using Microsoft.Xna.Framework;

namespace Stardew3D.Rendering;

public interface ICamera
{
    public Vector3 Up { get; }
    public Vector3 Forward { get; }
    public Vector3 Position { get; }
    public Matrix ViewMatrix { get; }
}
