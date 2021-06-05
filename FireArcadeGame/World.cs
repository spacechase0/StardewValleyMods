using System.Collections.Generic;
using System.IO;
using System.Linq;
using FireArcadeGame.Objects;
using FireArcadeGame.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

namespace FireArcadeGame
{
    public class World
    {
        public static readonly int SCALE = 4;

        public Player player;
        public LevelWarp warp;
        public Camera cam = new Camera();
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView( MathHelper.ToRadians( 70 ), Game1.game1.GraphicsDevice.DisplayMode.AspectRatio, 0.01f, 100 );
        
        private RenderTarget2D target;
        private SpriteBatch spriteBatch;

        private bool nextLevelQueued = false;
        private int currLevel = 0;
        private Vector2 warpPos;
        public Map map;
        public List<BaseObject> objects = new List<BaseObject>();
        public List<BaseProjectile> projectiles = new List<BaseProjectile>();
        private List<BaseObject> queuedObjects = new List<BaseObject>();

        public int ScreenSize => target.Width;

        public bool HasQuit = false;

        public World()
        {
            target = new RenderTarget2D( Game1.game1.GraphicsDevice, 500 / SCALE, 500 / SCALE, false, Game1.game1.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents );
            spriteBatch = new SpriteBatch( Game1.game1.GraphicsDevice );

            InitLevel( "0" );

            Game1.changeMusicTrack( "VolcanoMines", track_interruptable: false, Game1.MusicContext.MiniGame );
        }

        public void Quit()
        {
            HasQuit = true;
            Game1.changeMusicTrack( "none" );
        }

        public void QueueObject( BaseObject obj )
        {
            queuedObjects.Add( obj );
        }

        Vector3 baseCamPos = new Vector3( 4.5f, 2, 4.5f );
        float camAngle;
        public void Update()
        {
            if ( nextLevelQueued )
            {
                nextLevelQueued = false;
                NextLevel();
            }

            foreach ( var obj in objects )
            {
                obj.Update();
            }
            foreach ( var proj in projectiles )
            {
                proj.Update();
            }

            for ( int i = objects.Count - 1; i >= 0; --i )
            {
                if ( objects[ i ].Dead )
                    objects.RemoveAt( i );
            }

            for ( int i = projectiles.Count - 1; i >= 0; --i )
            {
                if ( projectiles[ i ].Dead )
                    projectiles.RemoveAt( i );
            }

            foreach ( var obj in queuedObjects )
            {
                objects.Add( obj );
            }
            queuedObjects.Clear();

            if ( objects.OfType<Enemy>().Count() == 0 )
            {
                if ( warp == null )
                {
                    map.Floor[ (int) warpPos.X, (int) warpPos.Y ] = FloorTile.Stone;
                    objects.Add( warp = new LevelWarp( this ) { Position = new Vector3( warpPos.X, 0, warpPos.Y ) } );
                    Game1.playSound( "detector" );
                }
            }

            /*
            camAngle = ( camAngle + 0.025f ) % 360;
            cam.pos = baseCamPos + new Vector3( ( float ) Math.Cos( camAngle ) * map.Size.X, 0, (float) Math.Sin( camAngle ) * map.Size.Y );
            cam.target = baseCamPos;
            */
        }

        public void Render()
        {
            var device = Game1.game1.GraphicsDevice;
            var oldTargets = device.GetRenderTargets();
            device.SetRenderTarget( target );
            var oldDepth = device.DepthStencilState;
            device.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            device.Clear( map.Sky );

            RasterizerState rast = new RasterizerState();
            rast.CullMode = CullMode.None;
            var oldRast = device.RasterizerState;
            device.RasterizerState = rast;
            var oldSample = device.SamplerStates[0];
            device.SamplerStates[0] = SamplerState.PointClamp;
            {
                /*
                cam.pos = new Vector3( 4.5f, 4.5f, 4.5f );
                cam.target = new Vector3( 4.5f, 0, 4.5f );
                cam.up = new Vector3( 0, 0, 1 );
                //*/

                foreach ( var obj in objects )
                {
                    obj.Render( device, projection, cam );
                }
                foreach ( var proj in projectiles )
                {
                    proj.Render( device, projection, cam );
                }
            }
            {
                DepthStencilState depth2 = new DepthStencilState();
                depth2.DepthBufferFunction = CompareFunction.Always;
                var oldDepth2 = device.DepthStencilState;
                device.DepthStencilState = depth2;
                {
                    foreach ( var obj in objects )
                    {
                        obj.RenderOver( device, projection, cam );
                    }
                }
                device.DepthStencilState = oldDepth2;
            }
            device.SamplerStates[ 0 ] = oldSample;
            device.RasterizerState = oldRast;
            device.SetVertexBuffer( null );
            device.DepthStencilState = oldDepth;

            foreach ( var obj in objects )
            {
                obj.RenderUi( spriteBatch );
            }

            device.SetRenderTargets( oldTargets );
            Game1.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null );
            //var oldTarget = oldTargets[0].RenderTarget as RenderTarget2D;
            Game1.spriteBatch.Draw( target, new Vector2( ( Game1.graphics.PreferredBackBufferWidth - 500 ) / 2, ( Game1.graphics.PreferredBackBufferHeight - 500 ) / 2 ), null, Color.White, 0, Vector2.Zero, SCALE, SpriteEffects.None, 1 );
            Game1.spriteBatch.End();
        }

        public void QueueNextLevel()
        {
            nextLevelQueued = true;
        }

        private void NextLevel()
        {
            switch ( ++currLevel )
            {
                case 1: InitLevel( "1" ); break;
                case 2: InitLevel( "2" ); break;
                case 3: InitLevel( "boss" ); break;
                case 4: InitLevel( "ending" ); break;
                case 5:
                    Quit();
                    Game1.drawObjectDialogue( "You won!" );
                    if ( !Game1.player.hasOrWillReceiveMail( "BeatPyromancersJourney" ) )
                    {
                        Game1.player.mailReceived.Add( "BeatPyromancersJourney" );
                        Game1.player.addItemByMenuIfNecessaryElseHoldUp( new StardewValley.Object( 848, 25 ) );
                    }
                    break;
            }
        }

        private void InitLevel( string path )
        {
            string[] lines = File.ReadAllLines( Path.Combine( Mod.instance.Helper.DirectoryPath, "assets", "levels", path + ".txt" ) );

            string[] toks = lines[ 0 ].Split( ' ' );

            warp = null;
            objects.Clear();
            projectiles.Clear();

            Vector2 playerPos = Vector2.Zero;

            map  = new Map( new Vector2( int.Parse( toks[ 0 ] ), int.Parse( toks[ 1 ] ) ) );
            if ( toks.Length > 2 && toks[ 2 ] == "sky" )
            {
                map.Sky = Color.SkyBlue;
            }
            for ( int i = 1; i <= map.Size.Y; ++i )
            {
                int iy = i - 1;
                for ( int ix = 0; ix < map.Size.X; ++ix )
                {
                    map.Floor[ ix, iy ] = FloorTile.Stone;
                    map.Walls[ ix, iy ] = WallTile.Empty;
                    switch ( lines[ i ][ ix ] )
                    {
                        case ' ': break;
                        case '#':
                        case 'M':
                            map.Walls[ ix, iy ] = WallTile.Stone;
                            if ( lines[ i ][ ix ] == 'M' )
                            {
                                objects.Add( new MuralThing( this ) { Position = new Vector3( ix, 0, iy - 0.01f ) } );
                            }
                            break;
                        case 'L':
                            map.Floor[ ix, iy ] = FloorTile.Lava;
                            break;
                        case 'F':
                            // TODO: Forge
                            break;
                        case 'P': playerPos = new Vector2( ix, iy ); break;
                        case 's': objects.Add( new TigerSlimeEnemy( this ) { Position = new Vector3( ix + 0.5f, 0, iy + 0.5f ) } ); break;
                        case 'b': objects.Add( new BatEnemy( this ) { Position = new Vector3( ix + 0.5f, 0.5f, iy + 0.5f ) } ); break;
                        case 'W':
                        case 'G':
                            warpPos = new Vector2( ix, iy );
                            if ( lines[ i ][ ix ] == 'G' )
                            {
                                map.Floor[ ix, iy ] = FloorTile.Lava;
                                objects.Add( new GolemEnemy( this ) { Position = new Vector3( ix + 0.5f, -0.65f, iy + 0.5f ) } );
                            }
                            break;
                        
                        default:
                            Log.warn( "Unknown tile type " + lines[ i ][ ix ] + "!" );
                            break;
                    }
                }
            }


            objects.Insert( 0, new Floor( this, path == "ending" ) );
            objects.Insert( 1, new Walls( this, path == "ending" ) );
            objects.Add( player = new Player( this ) { Position = new Vector3( playerPos.X, 0.5f, playerPos.Y ) } );

            if ( path == "0" || path == "ending" )
            {
                objects.Add( warp = new LevelWarp( this ) { Position = new Vector3( warpPos.X, 0, warpPos.Y ) } );
            }
        }
    }
}
