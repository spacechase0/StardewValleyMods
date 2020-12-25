using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireArcadeGame.Objects
{
    public class TestTriangle : BaseObject
    {
        private VertexBuffer buffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColor ), 3, BufferUsage.WriteOnly );

        public TestTriangle( World world )
        : base( world )
        {
            VertexPositionColor[] test = new VertexPositionColor[3]
            {
                new VertexPositionColor( new Vector3( 0, 0, 0 ), Color.Red ),
                new VertexPositionColor( new Vector3( 1, 2, 0 ), Color.Green ),
                new VertexPositionColor( new Vector3( 2, 0, 0 ), Color.Blue ),
            };
            buffer.SetData( test );
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );
            effect.TextureEnabled = false;
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();
                device.SetVertexBuffer( buffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, 0, 3 );
            }
        }
    }
}
