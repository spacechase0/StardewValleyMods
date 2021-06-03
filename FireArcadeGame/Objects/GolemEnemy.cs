using FireArcadeGame.Projectiles;
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
    public class GolemEnemy : Enemy
    {
        public static Texture2D tex = Mod.instance.Helper.Content.Load< Texture2D >( "assets/golem.png" );

        public override RectangleF BoundingBox { get; } = new RectangleF( -0.5f, -0.5f, 1, 1 );

        private enum AnimState
        {
            Glow,
            Shoot,
            Immune,
            Summon,
        }
        private AnimState state = AnimState.Glow;
        private int frame = 0;
        private float frameAccum = 0;

        private VertexBuffer buffer;

        public GolemEnemy( World world )
        :   base( world )
        {
            Health = 50;

            var vertices = new List<VertexPositionColorTexture>();
            for ( int f = 0; f < 12 * 9; ++f )
            {
                int fx = f % 12;
                int fy = f / 12;

                float xa = ( 80f / tex.Width ) * fx;
                float xb = ( 80f / tex.Width ) * ( fx + 1 );
                float ya = ( 80f / tex.Height ) * ( fy + 1 );
                float yb = ( 80f / tex.Height ) * fy;
                vertices.Add( new VertexPositionColorTexture( new Vector3( -1.5f, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                vertices.Add( new VertexPositionColorTexture( new Vector3( 1.5f, 0, 0 ), Color.White, new Vector2( xb, ya ) ) );
                vertices.Add( new VertexPositionColorTexture( new Vector3( 1.5f, 3, 0 ), Color.White, new Vector2( xb, yb ) ) );

                vertices.Add( new VertexPositionColorTexture( new Vector3( -1.5f, 0, 0 ), Color.White, new Vector2( xa, ya ) ) );
                vertices.Add( new VertexPositionColorTexture( new Vector3( -1.5f, 3, 0 ), Color.White, new Vector2( xa, yb ) ) );
                vertices.Add( new VertexPositionColorTexture( new Vector3( 1.5f, 3, 0 ), Color.White, new Vector2( xb, yb ) ) );
            }

            buffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColorTexture ), vertices.Count(), BufferUsage.WriteOnly );
            buffer.SetData( vertices.ToArray() );
        }

        public override void Hurt( int amt )
        {
            if ( state == AnimState.Immune && frame > 3 && frame < 11 )
            {
                Game1.playSound( "crit" );
                return;
            }

            base.Hurt( amt );
            if ( Dead )
            {
                Game1.playSound( "explosion" );
            }
            else
            {
                Game1.playSound( "stoneCrack" );
            }
        }

        public override void DoMovement()
        {
        }

        public override void Update()
        {
            base.Update();

            frameAccum += (float) Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if ( frameAccum >= 0.2f )
            {
                frameAccum = 0;

                switch ( state )
                {
                    case AnimState.Glow:
                        if ( ++frame > 7 )
                        {
                            GoToNextState();
                        }
                        break;
                    case AnimState.Shoot:
                        if ( ++frame == 8 )
                        {
                            var player = World.player;
                            var speed = player.Position - Position;
                            speed.Y = 0;
                            speed.Normalize();
                            speed /= 10;

                            World.projectiles.Add( new GolemArm( World )
                            {
                                Position = Position + new Vector3( 0, 0.5f, 0 ),
                                Speed = new Vector2( speed.X, speed.Z )
                            } );
                        }
                        else if ( frame > 16 )
                        {
                            GoToNextState();
                        }
                        break;
                    case AnimState.Immune:
                        if ( frame < 7 )
                            ++frame;
                        if ( World.objects.OfType< Enemy >().Count() <= 1 )
                        {
                            if ( ++frame > 14 )
                            {
                                GoToNextState();
                            }
                        }
                        break;
                    case AnimState.Summon:
                        if ( ++frame > 6 )
                        {
                            GoToNextState();
                        }
                        break;
                }
            }
        }

        private void GoToNextState()
        {
            frame = 0;
            if ( state == AnimState.Summon )
            {
                int amt = 2;
                for ( int i = 0; i < amt; ++i )
                {
                    for ( int t = 0; t < 10; ++t )
                    {
                        Vector2 pos = new Vector2( 1 + Game1.random.Next( (int)World.map.Size.X - 2 ), 1 + Game1.random.Next( (int)World.map.Size.Y - 2 ) );
                        if ( World.map.IsAirSolid( pos.X, pos.Y ) )
                        {
                            continue;
                        }

                        World.QueueObject( new BatEnemy( World ) { Position = new Vector3( pos.X + 0.5f, 0.5f, pos.Y + 0.5f ) } );
                        break;
                    }
                }
                Game1.playSound( "debuffHit" );
                state = AnimState.Immune;
            }
            else if ( state != AnimState.Glow )
            {
                state = AnimState.Glow;
            }
            else
            {
                switch ( Game1.random.Next( 4 ) )
                {
                    case 0:
                    case 1:
                        state = AnimState.Glow;
                        break;
                    case 2:
                        state = AnimState.Shoot;
                        break;
                    case 3:
                        state = AnimState.Summon;
                        break;
                }
            }
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );

            int fx = this.frame;
            int fy = 0;
            switch ( state )
            {
                case AnimState.Glow:
                    fy = 1;
                    break;
                case AnimState.Shoot:
                    fy = 2;
                    if ( fx > 8 )
                        fx = 8 - ( fx - 8 );
                    break;
                case AnimState.Immune:
                    fy = 3;
                    if ( fx > 7 )
                        fx = 7 - ( fx - 7 );
                    break;
                case AnimState.Summon:
                    fy = 5;
                    if ( fx > 6 )
                        fx = 6 - ( fx - 6 );
                    break;
            }
            int frame = fy * 12 + fx;

            var oldStencil = device.DepthStencilState;
            var newStencil = new DepthStencilState();
            newStencil.DepthBufferWriteEnable = false;
            device.DepthStencilState = newStencil;

            effect.World = Matrix.CreateConstrainedBillboard( Position, cam.pos, Vector3.Up, null, null );
            effect.TextureEnabled = true;
            effect.Texture = tex;
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();

                device.SetVertexBuffer( buffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, frame * 6, 2 );
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
