using FireArcadeGame.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireArcadeGame
{
    public class World
    {
        public static readonly int SCALE = 1;

        public Camera cam = new Camera();
        private Matrix proj = Matrix.CreatePerspectiveFieldOfView( MathHelper.ToRadians( 70 ), Game1.game1.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 100 );
        
        private RenderTarget2D target;

        public Map map = new Map( new Vector2( 9, 9 ) );
        public List<BaseObject> objects = new List<BaseObject>();

        public World()
        {
            target = new RenderTarget2D( Game1.game1.GraphicsDevice, 500 / SCALE, 500 / SCALE, false, Game1.game1.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents );
            
            map.Floor[ 4, 3 ] = FloorTile.Lava;
            map.Floor[ 3, 4 ] = FloorTile.Lava;
            map.Floor[ 4, 4 ] = FloorTile.Lava;
            map.Floor[ 5, 4 ] = FloorTile.Lava;
            map.Floor[ 4, 5 ] = FloorTile.Lava;
            for ( int ix = 0; ix < map.Size.X; ++ix )
            {
                for ( int iy = 0; iy < map.Size.Y; ++iy )
                {
                    if ( ( ix == 0 || ix == map.Size.X - 1 ) ||
                         ( iy == 0 || iy == map.Size.Y - 1 ) )
                        map.Walls[ ix, iy ] = WallTile.Stone;
                }
            }

            objects.Add( new Floor( this ) );
            objects.Add( new Walls( this ) );
            objects.Add( new TestTriangle( this ) { Position = Vector3.Zero } );
            objects.Add( new TestTriangle( this ) { Position = new Vector3( 5, 0, 0 ) } );
            objects.Add( new TestTriangle( this ) { Position = new Vector3( -5, 0, 0 ) } );
            objects.Add( new Player( this ) { Position = new Vector3( 4.5f, 0.5f, 2 ) } );
        }

        Vector3 baseCamPos = new Vector3( 4.5f, 2, 4.5f );
        float camAngle;
        public void Update()
        {
            foreach ( var obj in objects )
            {
                obj.Update();
            }

            /*
            camAngle = ( camAngle + 0.025f ) % 360;
            cam.pos = baseCamPos + new Vector3( ( float ) Math.Cos( camAngle ) * map.Size.X, 0, (float) Math.Sin( camAngle ) * map.Size.Y );
            cam.target = baseCamPos;
            */
        }

        public void Render()
        {
            Game1.spriteBatch.End();

            var device = Game1.game1.GraphicsDevice;
            var oldTargets = device.GetRenderTargets();
            device.SetRenderTarget( target );
            device.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            device.Clear( Color.Black );

            RasterizerState rast = new RasterizerState();
            rast.CullMode = CullMode.None;
            var oldRast = device.RasterizerState;
            device.RasterizerState = rast;
            {
                foreach ( var obj in objects )
                {
                    obj.Render( device, proj, cam );
                }
            }
            device.RasterizerState = oldRast;
            device.SetVertexBuffer( null );
            device.SetRenderTargets( oldTargets );
            Game1.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null );
            var oldTarget = oldTargets[0].RenderTarget as RenderTarget2D;
            Game1.spriteBatch.Draw( target, new Vector2( ( oldTarget.Width - 500 ) / 2, ( oldTarget.Height - 500 ) / 2 ), null, Color.White, 0, Vector2.Zero, SCALE, SpriteEffects.None, 1 );
        }
    }
}
