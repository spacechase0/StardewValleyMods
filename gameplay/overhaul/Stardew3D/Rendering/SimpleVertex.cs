using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Stardew3D.Rendering;

public struct SimpleVertex : IVertexType
{
    public Vector3 Position = new();
    public Vector2 TexCoord = new();
    public Color Color = new();
    public Vector3 Normal = new();

    public static readonly VertexDeclaration vDecl;
    VertexDeclaration IVertexType.VertexDeclaration => vDecl;

    public SimpleVertex() { }
    public SimpleVertex(Vector3 pos, Vector2 texCoords)
    {
        Position = pos;
        TexCoord = texCoords;
        Color = Color.White;
    }
    public SimpleVertex(Vector3 pos, Vector2 texCoords, Color col)
    {
        Position = pos;
        TexCoord = texCoords;
        Color = col;
    }

    public static SimpleVertex From2D(VertexPositionColorTexture orig, Vector2 pos2d, Vector3 basePos3d, float scale = 1)
    {
        Vector3 pos = orig.Position;
        pos.X -= pos2d.X;
        pos.Y -= pos2d.Y;
        pos /= Game1.tileSize;
        pos.Y = -pos.Y;

        return new SimpleVertex(pos * scale + basePos3d, orig.TextureCoordinate, orig.Color);
    }

    static SimpleVertex()
    {
        VertexElement[] velems = new[]
        {
            new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
            new VertexElement( 12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
            new VertexElement( 20, VertexElementFormat.Color, VertexElementUsage.Color, 0 ),
            new VertexElement( 24, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
        };
        vDecl = new VertexDeclaration(velems);
    }
}
