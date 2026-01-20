using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Stardew3D.Data;
public class BoxInteractionArea : InteractionArea
{
    public override string Type => "Box";

    public Vector3 Size { get; set; }

    public override Vector3[] GetShapeWithoutTransform()
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

    public override Vector3[] GetTriangleVerticesWithoutTransform()
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
