using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PyromancersJourney.Framework
{
    internal struct VertexEverything : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public Vector2 Texture;

        VertexDeclaration IVertexType.VertexDeclaration => VertexEverything.VertexDeclaration;

        public static readonly VertexDeclaration VertexDeclaration = new(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 3 + 4 + sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexEverything(Vector3 pos, Vector3 n, Vector2 tex)
        {
            this.Position = pos;
            this.Color = Color.White;
            this.Normal = n;
            this.Texture = tex;
        }
    }
}
