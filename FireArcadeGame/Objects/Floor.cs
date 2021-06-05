using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FireArcadeGame.Objects
{
    public class Floor : BaseObject
    {
        private Texture2D texInside;
        private Texture2D texOutside;
        private VertexBuffer buffer;
        private VertexBuffer bufferGlow;
        private int triCount;
        private int triCountGlow;
        private bool outside;

        public Floor( World world, bool theOutside )
        :   base( world )
        {
            outside = theOutside;
            texInside = Game1.content.Load<Texture2D>( "Maps\\Mines\\volcano_dungeon" );
            texOutside = Game1.content.Load<Texture2D>( "Maps\\Mines\\volcano_caldera" );
            float tx = 16f / texInside.Width;
            float ty = 16f / texInside.Height;
            Vector2 t = new Vector2( tx, ty );

            var vertices = new List<VertexEverything>();
            var verticesGlow = new List<VertexEverything>();
            for ( int ix = 0; ix < world.map.Size.X; ++ix )
            {
                for ( int iy = 0; iy < world.map.Size.Y; ++iy )
                {
                    Vector2[][] texCoordMap = new Vector2[][]
                    {
                        new Vector2[]
                        {
                            new Vector2( 1, 0 ) * t,
                            new Vector2( 2, 0 ) * t,
                            new Vector2( 2, 1 ) * t,
                            new Vector2( 1, 1 ) * t
                        },
                        new Vector2[]
                        {
                            new Vector2( 0, 20 ) * t,
                            new Vector2( 1, 20 ) * t,
                            new Vector2( 1, 21 ) * t,
                            new Vector2( 0, 21 ) * t
                        }
                    };
                    if ( outside )
                    {
                        float tx2 = 16f / texOutside.Width;
                        float ty2 = 16f / texOutside.Height;
                        Vector2 t2 = new Vector2( tx2, ty2 );
                        texCoordMap[ 0 ] = new Vector2[]
                        {
                            new Vector2( 1, 3 ) * t2,
                            new Vector2( 2, 3 ) * t2,
                            new Vector2( 2, 4 ) * t2,
                            new Vector2( 1, 4 ) * t2
                        };
                    }
                    int tile = ( int ) world.map.Floor[ ix, iy ];


                    float y = 0;
                    var targetVert = vertices;
                    ref int tri = ref triCount;
                    if ( tile == ( int ) FloorTile.Lava )
                    {
                        y = -0.1f;
                        targetVert = verticesGlow;
                        tri = ref triCountGlow;
                    }
                    else
                    {
                        var n = new Vector3( 0, 0, -1 );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0,  0, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1,  0, iy + 0 ), n, texCoordMap[ tile ][ 1 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1, -1, iy + 0 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++tri;

                        targetVert.Add( new VertexEverything( new Vector3( ix + 0,  0, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1, -1, iy + 0 ), n, texCoordMap[ tile ][ 2 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0, -1, iy + 0 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++tri;

                        n = new Vector3( -1, 0, 0 );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0,  0, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0,  0, iy + 1 ), n, texCoordMap[ tile ][ 1 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0, -1, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++tri;

                        targetVert.Add( new VertexEverything( new Vector3( ix + 0,  0, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0, -1, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0, -1, iy + 0 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++tri;

                        n = new Vector3( 1, 0, 0 );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1,  0, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1,  0, iy + 1 ), n, texCoordMap[ tile ][ 1 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1, -1, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++tri;

                        targetVert.Add( new VertexEverything( new Vector3( ix + 1,  0, iy + 0 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1, -1, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1, -1, iy + 0 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++tri;

                        n = new Vector3( 0, 0, 1 );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0,  0, iy + 1 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1,  0, iy + 1 ), n, texCoordMap[ tile ][ 1 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1, -1, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        ++tri;

                        targetVert.Add( new VertexEverything( new Vector3( ix + 0,  0, iy + 1 ), n, texCoordMap[ tile ][ 0 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 1, -1, iy + 1 ), n, texCoordMap[ tile ][ 2 ] ) );
                        targetVert.Add( new VertexEverything( new Vector3( ix + 0, -1, iy + 1 ), n, texCoordMap[ tile ][ 3 ] ) );
                        ++tri;
                    }

                    targetVert.Add( new VertexEverything( new Vector3( ix + 0, y, iy + 0 ), Vector3.Up, texCoordMap[ tile ][ 0 ] ) );
                    targetVert.Add( new VertexEverything( new Vector3( ix + 1, y, iy + 0 ), Vector3.Up, texCoordMap[ tile ][ 1 ] ) );
                    targetVert.Add( new VertexEverything( new Vector3( ix + 1, y, iy + 1 ), Vector3.Up, texCoordMap[ tile ][ 2 ] ) );
                    ++tri;

                    targetVert.Add( new VertexEverything( new Vector3( ix + 0, y, iy + 0 ), Vector3.Up, texCoordMap[ tile ][ 0 ] ) );
                    targetVert.Add( new VertexEverything( new Vector3( ix + 1, y, iy + 1 ), Vector3.Up, texCoordMap[ tile ][ 2 ] ) );
                    targetVert.Add( new VertexEverything( new Vector3( ix + 0, y, iy + 1 ), Vector3.Up, texCoordMap[ tile ][ 3 ] ) );
                    ++tri;
                }
            }

            buffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexEverything ), vertices.Count, BufferUsage.WriteOnly );
            buffer.SetData( vertices.ToArray() );

            bufferGlow = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexEverything ), verticesGlow.Count, BufferUsage.WriteOnly );
            bufferGlow.SetData( verticesGlow.ToArray() );
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );
            //effect.LightingEnabled = true;
            effect.TextureEnabled = true;
            effect.Texture = outside ? texOutside : texInside;
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();
                device.SetVertexBuffer( buffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, triCount );
            }
            
            effect.LightingEnabled = false;
            effect.Texture = texInside;
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();
                device.SetVertexBuffer( bufferGlow );
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, triCountGlow );
            }
        }
    }
}
