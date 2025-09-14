using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Stardew3D
{
    public struct MyVertex : IVertexType
    {
        public Vector3 Position = new();
        public Vector2 TexCoord = new();
        public Color Color = new();
        public Vector3 Normal = new();
        public Byte4 BlendIndices = new();
        public Vector4 BlendWeight = new();

        public static readonly VertexDeclaration vDecl;
        VertexDeclaration IVertexType.VertexDeclaration => vDecl;

        public MyVertex() { }
        public MyVertex(Vector3 pos, Vector2 texCoords)
        {
            Position = pos;
            TexCoord = texCoords;
            Color = Color.White;
        }
        public MyVertex(Vector3 pos, Vector2 texCoords, Color col)
        {
            Position = pos;
            TexCoord = texCoords;
            Color = col;
        }

        static MyVertex()
        {
            VertexElement[] velems = new[]
            {
                new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
                new VertexElement( 12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
                new VertexElement( 20, VertexElementFormat.Color, VertexElementUsage.Color, 0 ),
                new VertexElement( 24, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
                new VertexElement( 36, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0 ),
                new VertexElement( 40, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0 ),
            };
            vDecl = new VertexDeclaration( velems );
        }
    }
}
