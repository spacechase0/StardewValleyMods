using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FireArcadeGame.Objects
{
    public class LevelWarp : BaseObject
    {
        private Texture2D tex;
        private VertexBuffer buffer;
        private int triCount;

        public LevelWarp( World world )
        :   base( world )
        {
            Position = new Vector3( world.map.Size.X / 2f, 0, world.map.Size.Y / 2f );
            tex = Game1.content.Load<Texture2D>( "Maps\\Mines\\mine_dark" );

            Vector2 tb = new Vector2( 224f / tex.Width, 224f / tex.Height );
            float tx = 16f / tex.Width, ty = 16f / tex.Height;

            var vertices = new List<VertexPositionColorTexture>();
            vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0.01f, 0 ), Color.White, tb + new Vector2(  0,  0 ) ) );
            vertices.Add( new VertexPositionColorTexture( new Vector3( 1, 0.01f, 0 ), Color.White, tb + new Vector2( tx,  0 ) ) );
            vertices.Add( new VertexPositionColorTexture( new Vector3( 1, 0.01f, 1 ), Color.White, tb + new Vector2( tx, ty ) ) );
            ++triCount;

            vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0.01f, 0 ), Color.White, tb + new Vector2(  0,  0 ) ) );
            vertices.Add( new VertexPositionColorTexture( new Vector3( 1, 0.01f, 1 ), Color.White, tb + new Vector2( tx, ty ) ) );
            vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0.01f, 1 ), Color.White, tb + new Vector2(  0, ty ) ) );
            ++triCount;

            buffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColorTexture ), vertices.Count, BufferUsage.WriteOnly );
            buffer.SetData( vertices.ToArray() );
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );
            effect.TextureEnabled = true;
            effect.Texture = tex;
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();
                device.SetVertexBuffer( buffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, triCount );
            }
        }
    }
}
