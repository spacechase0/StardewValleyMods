using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;

namespace MoonMisadventures.Game
{
    public class LaunchJourney : IMinigame
    {
        private enum UfoStage
        {
            Launch,
            Zoom,
            Finished,
        }

        private const int MaxMoonFrame = 6;
        private readonly Vector2 UfoLaunchPos = new Vector2( 310, 260 );
        private readonly Vector2 MoonPos = new Vector2( 460, 50 );
        private readonly Vector2 MoonOrigin = new Vector2( 26, 39 );
        private readonly Vector2 LocalLandingPos = new Vector2( 16, 33 );

        private float zoom;
        private Vector2 offset;
        private Texture2D bg;
        private Texture2D ufo;
        private Texture2D moon;

        private float delay = 1.5f;

        private UfoStage ufoStage = UfoStage.Launch;
        private float ufoTimer = 1f;
        private Vector2 ufoPos;
        private float ufoScale = 1;

        private bool doingMoonStuff = false;
        private float moonShake = 0;
        private int moonFrame = 0;
        private float moonFrameTime = 0;

        private float finishTime = 0;

        public LaunchJourney()
        {
            Game1.globalFadeToClear();
            // music


            bg = Assets.LaunchBackground;
            ufo = Assets.LaunchUfo;
            moon = Assets.LaunchMoon;

            changeScreenSize();

            ufoPos = UfoLaunchPos;
        }

        public string minigameId()
        {
            return null;
        }

        public bool overrideFreeMouseMovement()
        {
            return Game1.options.SnappyMenus;
        }

        public void receiveEventPoke( int data )
        {
        }

        public void receiveKeyPress( Keys k )
        {
        }

        public void receiveKeyRelease( Keys k )
        {
        }

        public void receiveLeftClick( int x, int y, bool playSound = true )
        {
        }

        public void receiveRightClick( int x, int y, bool playSound = true )
        {
        }

        public void leftClickHeld( int x, int y )
        {
        }

        public void releaseLeftClick( int x, int y )
        {
        }

        public void releaseRightClick( int x, int y )
        {
        }

        public void changeScreenSize()
        {
            zoom = Game1.viewport.Height / bg.Height;
            offset.X = (Game1.viewport.Width - bg.Width * zoom) / 2;
            offset.Y = 0;
        }

        public bool doMainGameUpdates()
        {
            return false;
        }

        public bool tick( GameTime time )
        {
            if ( delay > 0 )
            {
                delay -= ( float ) time.ElapsedGameTime.TotalSeconds;
                return false;
            }

            if ( ufoStage == UfoStage.Finished && moonShake <= 0 && moonFrame == MaxMoonFrame && finishTime <= 0 )
            {
                Game1.warpFarmer( "Custom_MM_MoonLandingArea", 9, 31, 0 );
                return true;
            }

            switch ( ufoStage )
            {
                case UfoStage.Launch:
                    ufoPos += new Vector2( 0, -15 ) *  ( float ) time.ElapsedGameTime.TotalSeconds;
                    ufoTimer -= ( float ) time.ElapsedGameTime.TotalSeconds;
                    if ( ufoTimer <= 0 )
                    {
                        ufoStage = UfoStage.Zoom;
                        Game1.playSound( "wand" );
                    }
                    break;
                case UfoStage.Zoom:
                    Vector2 target = MoonPos - MoonOrigin + LocalLandingPos;
                    float dist = Vector2.Distance( ufoPos, target );
                    float startDist = Vector2.Distance( UfoLaunchPos, target );
                    float speed = 150 + dist / startDist * 600;

                    Vector2 dir = ( target - ufoPos );
                    dir.Normalize();

                    if ( dir.Y >= 0 )
                    {
                        doingMoonStuff = true;
                        ufoStage = UfoStage.Finished;
                        moonShake = 0.3f;
                        Game1.playSound( "explosion" );
                    }
                    else
                    {
                        if ( dist < 25 )
                            doingMoonStuff = true;

                        ufoPos += dir * speed * ( float ) time.ElapsedGameTime.TotalSeconds;
                        ufoScale = 0.25f + ( dist / startDist ) * 0.75f;
                    }

                    break;
                case UfoStage.Finished:
                    break;
            }

            if ( doingMoonStuff )
            {
                moonFrameTime += ( float ) time.ElapsedGameTime.TotalSeconds;
                moonShake -= ( float ) time.ElapsedGameTime.TotalSeconds;
                if ( moonFrameTime > 0.1f && moonFrame < MaxMoonFrame )
                {
                    ++moonFrame;
                    moonFrameTime = 0;
                    if ( moonFrame == MaxMoonFrame )
                        finishTime = 2f;
                }
                else if ( moonFrame >= MaxMoonFrame )
                {
                    finishTime -= ( float ) time.ElapsedGameTime.TotalSeconds;
                }
            }

            return false;
        }

        public void draw( SpriteBatch b )
        {
            b.Begin( SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp );

            b.Draw( bg, offset, null, Color.White, 0, Vector2.Zero, zoom, SpriteEffects.None, 0 );

            Vector2 shake = Vector2.Zero;
            if ( moonShake > 0 )
            {
                shake = new Vector2( Game1.random.Next( -1, 2 ), Game1.random.Next( -1, 2 ) );
            }
            b.Draw( moon, offset + MoonPos * zoom + shake, new Rectangle( moonFrame * 48, 0, 48, 48 ), Color.White, 0, MoonOrigin, zoom, SpriteEffects.None, 0.5f );

            b.Draw( ufo, offset + ufoPos * zoom, null, Color.White, 0, new Vector2( ufo.Width / 2, ufo.Height / 2 ), ufoScale * zoom, SpriteEffects.None, 1 );

            b.End();
        }

        public bool forceQuit()
        {
            return false;
        }

        public void unload()
        {
        }
    }
}
