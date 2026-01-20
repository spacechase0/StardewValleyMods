using Microsoft.Xna.Framework;

namespace Stardew3D.Data;
public abstract class InteractionArea
{
    public abstract string Type { get; }

    //public string Purpose { get; set; } // "Interaction", ...

    public Vector3 Rotation { get; set; }
    public Vector3 Translation { get; set; }

    // Must return vertices of the convex shape, ignoring the position and rotation
    public abstract Vector3[] GetShapeWithoutTransform();

    // Must return triangles of the convex shape for debug rendering, ignoring the position and rotation, in CCW order
    public abstract Vector3[] GetTriangleVerticesWithoutTransform();

    public Vector3[] GetTransformedShape()
    {
        Matrix transform = Matrix.Identity;
        transform *= Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z);
        transform *= Matrix.CreateTranslation(Translation);

        Vector3[] shape = GetShapeWithoutTransform();
        for (int i = 0; i < shape.Length; ++i)
        {
            shape[i] = Vector3.Transform(shape[i], transform);
        }
        return shape;
    }

    public Vector3[] GetTriangleVertices()
    {
        Matrix transform = Matrix.Identity;
        transform *= Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z);
        transform *= Matrix.CreateTranslation(Translation);

        Vector3[] verts = GetTriangleVerticesWithoutTransform();
        for (int i = 0; i < verts.Length; ++i)
        {
            verts[i] = Vector3.Transform(verts[i], transform);
        }
        return verts;
    }
}
