using FireArcadeGame.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireArcadeGame.Projectiles
{
    public class PlayerFireball : BaseProjectile
    {
        public static Texture2D tex = Mod.instance.Helper.Content.Load< Texture2D >( "assets/fireball.png" );

        public int Level = 0;
        public Vector2 Speed;

        private static VertexBuffer buffer;

        public override bool HurtsPlayer => false;
        public override int Damage => Level + 1;

        public PlayerFireball( World world )
        :   base( world )
        {
            if ( buffer == null )
            {
                var vertices = new List<VertexPositionColorTexture>();
                for ( int i = 0; i < 4; ++i )
                {
                    float a = 0.25f * i;
                    float b = 0.25f * ( i + 1 );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( a, 0 ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 1, 0, 0 ), Color.White, new Vector2( b, 0 ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 1, 1, 0 ), Color.White, new Vector2( b, 1 ) ) );

                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 0, 0 ), Color.White, new Vector2( a, 0 ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 0, 1, 0 ), Color.White, new Vector2( a, 1 ) ) );
                    vertices.Add( new VertexPositionColorTexture( new Vector3( 1, 1, 0 ), Color.White, new Vector2( b, 1 ) ) );
                }

                buffer = new VertexBuffer( Game1.game1.GraphicsDevice, typeof( VertexPositionColorTexture ), vertices.Count(), BufferUsage.WriteOnly );
                buffer.SetData( vertices.ToArray() );
            }
        }

        public override void Trigger( BaseObject target )
        {
            if ( target is Enemy enemy )
            {
                enemy.Hurt( Damage );
                if ( Level == 3 )
                {
                    // explode
                }
                Dead = true;
            }
        }

        public override void Update()
        {
            base.Update();
            Position += new Vector3( Speed.X, 0, Speed.Y );

            if ( World.map.IsAirSolid( Position.X, Position.Z ) )
            {
                Dead = true;
            }
        }

        public override void Render( GraphicsDevice device, Matrix projection, Camera cam )
        {
            base.Render( device, projection, cam );
            var camForward = ( cam.pos - cam.target );
            camForward.Normalize();
            effect.World = Matrix.CreateConstrainedBillboard( Position, cam.pos, cam.up, null, null );
            effect.TextureEnabled = true;
            effect.Texture = tex;
            for ( int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e )
            {
                var pass = effect.CurrentTechnique.Passes[ e ];
                pass.Apply();
                device.SetVertexBuffer( buffer );
                device.DrawPrimitives( PrimitiveType.TriangleList, 6 * Level, 2 );
            }
        }
    }
}
