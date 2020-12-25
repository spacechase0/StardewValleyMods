using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

namespace FireArcadeGame.Objects
{
    public class Walls : BaseObject
    {
        private Texture2D tex;
        private VertexBuffer buffer;
        private int triCount;

        public Walls( World world )
        :   base( world )
        {
            tex = Game1.content.Load<Texture2D>( "Maps\\Mines\\volcano_dungeon" );
            float tx = 16f / tex.Width;
            float ty = 16f / tex.Height;
            Vector2 t = new Vector2( tx, ty );

            var vertices = new List<VertexEverything>();
            for ( int ix = 0; ix < world.map.Size.X; ++ix )
            {
                for ( int iy = 0; iy < world.map.Size.Y; ++iy )
                {
                    Vector2[][] texCoordMap = new Vector2[][]
                    {
                        new Vector2[ 0 ],
                        new Vector2[]
                        {
                            new Vector2( 1, 0 ) * t,
                            new Vector2( 2, 0 ) * t,
                            new Vector2( 2, 1 ) * t,
                            new Vector2( 1, 1 ) * t
                        }
                    };
                    int tile = ( int ) world.map.Walls[ ix, iy ];
                
                    if ( tile != ( int ) WallTile.Empty )
                    {
                        var n = new Vector3( 0, 0, -1 );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 1, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 1, iy + 0 ), n, texCoordMap[ tile ][ 1 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 0, iy + 0 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++triCount;

                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 1, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 0, iy + 0 ), n, texCoordMap[ tile ][ 2 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 0, iy + 0 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++triCount;

                        n = new Vector3( -1, 0, 0 );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 1, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 1, iy + 1 ), n, texCoordMap[ tile ][ 1 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 0, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++triCount;

                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 1, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 0, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 0, iy + 0 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++triCount;

                        n = new Vector3( 1, 0, 0 );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 1, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 1, iy + 1 ), n, texCoordMap[ tile ][ 1 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 0, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++triCount;

                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 1, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 0, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 0, iy + 0 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++triCount;

                        n = new Vector3( 0, 0, 1 );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 1, iy + 1 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 1, iy + 1 ), n, texCoordMap[ tile ][ 1 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 0, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++triCount;

                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 1, iy + 1 ), n, texCoordMap[ tile ][ 0 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 1, 0, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        vertices.Add( new VertexEverything( new Vector3( ix + 0, 0, iy + 1 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++triCount;
                    }
                }
            }

            buffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexEverything ), vertices.Count, BufferUsage.WriteOnly );
            buffer.SetData( vertices.ToArray() );
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );
            //effect.LightingEnabled = true;
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
