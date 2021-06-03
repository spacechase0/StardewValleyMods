using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FireArcadeGame.Objects
{
    public class BatEnemy : Enemy
    {
        public static Texture2D tex = Game1.content.Load< Texture2D >( "Characters\\Monsters\\Lava Bat" );

        public override bool Floats => true;

        private int frame = Game1.random.Next( 4 );
        private float frameAccum = 0;

        private static VertexBuffer mainBuffer;

        public BatEnemy( World world )
        :   base( world )
        {
            Health = 2;

            if ( mainBuffer == null )
            {
                float s = 0.75f;

                var vertices = new List<VertexPositionColorTexture>();
                for ( int i = 0; i < 4; ++i )
                {
                    float xa = ( 16f / tex.Width ) * i;
                    float xb = ( 16f / tex.Width ) * ( i + 1 );
                    float ya = ( 24f / tex.Height ) * ( 0 + 1 );
                    float yb = ( 24f / tex.Height ) * 0;
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( s, 0, 0 ), Color.White, new Vector2( xb, ya ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( s, s, 0 ), Color.White, new Vector2( xb, yb ) ) );

                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, s, 0 ), Color.White, new Vector2( xa, yb ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( s, s, 0 ), Color.White, new Vector2( xb, yb ) ) );
                }

                mainBuffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColorTexture ), vertices.Count(), BufferUsage.WriteOnly );
                mainBuffer.SetData( vertices.ToArray() );
            }
        }

        public override void Hurt( int amt )
        {
            base.Hurt( amt );
            if ( Dead )
            {
                Game1.playSound( "batScreech" );
            }
            else
            {
                Game1.playSound( "hitEnemy" );
            }
        }

        public override void DoMovement()
        {
            var player = World.player;
            var diff = player.Position - Position;
            diff.Y = 0;
            diff.Normalize();

            Position += diff / 50 * 1.5f;
        }

        public override void Update()
        {
            base.Update();

            frameAccum += (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if ( frameAccum >= 0.15f )
            {
                frameAccum = 0;
                if ( ++frame >= 4 )
                    frame = 0;
            }
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );

            int frame = this.frame;

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

            device.DepthStencilState = oldStencil;
        }
    }
}
