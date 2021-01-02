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
    public class TigerSlimeEnemy : Enemy
    {
        public static Texture2D tex = Game1.content.Load< Texture2D >( "Characters\\Monsters\\Tiger Slime" );

        private int frame = Game1.random.Next( 4 );
        private float frameAccum = 0;
        public int eyeType = Game1.random.Next( 4 );

        private static VertexBuffer mainBuffer;
        private static VertexBuffer eyesBuffer;

        public TigerSlimeEnemy( World world )
        :   base( world )
        {
            Health = eyeType + 1;

            if ( mainBuffer == null )
            {
                float s = 0.75f;

                var vertices = new List<VertexPositionColorTexture>();
                for ( int f = 0; f < 4; ++f )
                {
                    for ( int i = 0; i < 4; ++i )
                    {
                        float xa = ( 16f / tex.Width ) * i;
                        float xb = ( 16f / tex.Width ) * ( i + 1 );
                        float ya = ( 24f / tex.Height ) * ( f + 1 );
                        float yb = ( 24f / tex.Height ) * f;
                        vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                        vertices.Add( new VertexPositionColorTexture( new Vector3( s, 0, 0 ), Color.White, new Vector2( xb, ya ) ) );
                        vertices.Add( new VertexPositionColorTexture( new Vector3( s, s, 0 ), Color.White, new Vector2( xb, yb ) ) );

                        vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                        vertices.Add( new VertexPositionColorTexture( new Vector3( 0, s, 0 ), Color.White, new Vector2( xa, yb ) ) );
                        vertices.Add( new VertexPositionColorTexture( new Vector3( s, s, 0 ), Color.White, new Vector2( xb, yb ) ) );
                    }
                }

                mainBuffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColorTexture ), vertices.Count(), BufferUsage.WriteOnly );
                mainBuffer.SetData( vertices.ToArray() );

                vertices.Clear();
                for ( int i = 0; i < 4; ++i )
                {
                    int x = 32 + i % 2 * 16;
                    int y = 120 + i / 2 * 24;
                    float xa = ( x / (float) tex.Width );
                    float xb = ( ( x + 16 ) / (float) tex.Width );
                    float ya = ( ( y + 24 ) / (float) tex.Height );
                    float yb = ( y / (float) tex.Height );

                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( s, 0, 0 ), Color.White, new Vector2( xb, ya ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( s, s, 0 ), Color.White, new Vector2( xb, yb ) ) );

                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, s, 0 ), Color.White, new Vector2( xa, yb ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( s, s, 0 ), Color.White, new Vector2( xb, yb ) ) );
                }

                eyesBuffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColorTexture ), vertices.Count(), BufferUsage.WriteOnly );
                eyesBuffer.SetData( vertices.ToArray() );
            }
        }

        public override void Hurt( int amt )
        {
            base.Hurt( amt );
            if ( Dead )
            {
                Game1.playSound( "slimedead" );
            }
            else
            {
                Game1.playSound( "slimeHit" );
            }
        }

        public override void DoMovement()
        {
            var player = World.player;
            var diff = player.Position - Position;
            diff.Y = 0;
            diff.Normalize();

            Position += diff / 50;
        }

        public override void Update()
        {
            base.Update();

            frameAccum += (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if ( frameAccum >= 0.2f )
            {
                frameAccum = 0;
                if ( ++frame >= 4 )
                    frame = 0;
            }
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );

            int facing = 0;
            int frame = this.frame;
            frame += facing * 4;


            var oldStencil = device.DepthStencilState;
            var newStencil = new DepthStencilState();
            newStencil.DepthBufferWriteEnable = false;
            device.DepthStencilState = newStencil;

            var camForward = ( cam.pos - cam.target );
            camForward.Normalize();
            effect.World = Matrix.CreateConstrainedBillboard( Position, cam.pos, cam.up, null, null );
            effect.TextureEnabled = true;
            effect.Texture = tex;
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();

                device.SetVertexBuffer( mainBuffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, frame * 6, 2 );
            }

            if ( facing != 3 )
            {
                var eyePos = new Vector3( 0, 0.1f, 0 );
                //if ( facing == 1 ) eyePos.X = 4 / 16f;
                //if ( facing == 2 ) eyePos.X = -4 / 16f;
                switch ( this.frame )
                {
                    case 0: break;
                    case 1: eyePos.Y += 0.05f; break;
                    case 2: eyePos.Y += 0.1f; break;
                    case 3: eyePos.Y += 0.05f; break;
                }
                eyePos.Y *= 0.75f;

                effect.World = Matrix.CreateConstrainedBillboard( Position + eyePos, cam.pos, cam.up, null, null );
                for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
                {
                    var pass = effect.CurrentTechnique.Passes[ e ];
                    pass.Apply();

                    device.SetVertexBuffer( eyesBuffer );
                    device.DrawPrimitives( PrimitiveType.TriangleList, eyeType * 6, 2 );
                }
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
