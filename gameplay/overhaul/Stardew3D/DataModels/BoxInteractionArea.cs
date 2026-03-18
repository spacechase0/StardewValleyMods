using Microsoft.Xna.Framework;

namespace Stardew3D.DataModels;
public class BoxInteractionArea : InteractionArea
{
    public override string Type => "Box";

    public Vector3 Size { get; set; }

    public override BoundingBox GetBoundingBox()
    {
        return new BoundingBox(-Size / 2, Size / 2);
    }

    public override Vector3[] GetShape()
    {
        return
        [
            new Vector3( -Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),
        ];
    }

    public override Vector3[] GetTriangleVertices()
    {
        return
        [
            new Vector3( -Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),

            new Vector3( -Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),

            new Vector3( -Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),

            new Vector3( -Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),

            new Vector3( -Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3( -Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3( -Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),

            new Vector3(  Size.X / 2, -Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2, -Size.Z / 2 ),
            new Vector3(  Size.X / 2, -Size.Y / 2,  Size.Z / 2 ),
            new Vector3(  Size.X / 2,  Size.Y / 2,  Size.Z / 2 ),
        ];
    }
}
