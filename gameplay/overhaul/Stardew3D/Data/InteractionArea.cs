using Microsoft.Xna.Framework;

namespace Stardew3D.Data;
public abstract class InteractionArea
{
    public abstract string Type { get; }

    public string Purpose { get; set; }

    public Vector3 Rotation { get; set; }
    public Vector3 Translation { get; set; }

    public Color DebugColor
    {
        get
        {
            if (Purpose == $"{Mod.Instance.ModManifest.UniqueID}/ToolAction")
                return Color.Yellow;
            if (Purpose.StartsWith($"{Mod.Instance.ModManifest.UniqueID}/ToolAction/"))
                return Color.Blue;

            return Color.Magenta;
        }
    }

    // Must return vertices of the convex shape, ignoring the position and rotation
    public abstract Vector3[] GetShape();

    // Must return triangles of the convex shape for debug rendering, ignoring the position and rotation, in CCW order
    public abstract Vector3[] GetTriangleVertices();

    public Matrix Transform
    {
        get
        {
            Matrix transform = Matrix.Identity;
            transform *= Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z);
            transform *= Matrix.CreateTranslation(Translation);
            return transform;
        }
    }

    public Vector3[] GetTransformedShape() => GetShape().Transform(Transform);
    public Vector3[] GetTransformedTriangleVertices() => GetTriangleVertices().Transform(Transform);
}
