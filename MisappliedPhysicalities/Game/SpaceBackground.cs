using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MisappliedPhysicalities.Game
{
    public class SpaceBackground : Background
    {
        private Rectangle starTexRect = new Rectangle(0, 1453, 639, 195);

        public SpaceBackground()
        :   base(new Color( 0, 0, 25 ), false)
        {
        }

        public void Update( xTile.Dimensions.Rectangle viewport )
        {
            // ...
        }

        public void Draw( SpriteBatch b )
        {
            try
            {
                Color[] tints = new[]
                {
                    new Color( 255, 220, 220 ),
                    new Color( 170, 255, 170 ),
                    new Color( 230, 230, 255 )
                };
                Vector2[] posMods = new[]
                {
                    new Vector2( 0, 0 ),
                    new Vector2( starTexRect.Width / 3 * 1, starTexRect.Height / 3 * 1 ),
                    new Vector2( starTexRect.Width / 3 * 2, starTexRect.Height / 3 * 2 ),
                };
                float[] posMult = new[] { 0.1f, 0.3f, 0.5f };


                float incrx = Game1.viewport.Width / starTexRect.Width;
                float incry = Game1.viewport.Height / starTexRect.Height;

                for ( int i = 0; i < 3; ++i )
                {
                    float sx = -( ( Game1.viewport.X * posMult[ i ] + posMods[ i ].X ) % ( starTexRect.Width * Game1.pixelZoom ) );
                    float sy = -( ( Game1.viewport.Y * posMult[ i ] + posMods[ i ].Y ) % ( starTexRect.Height * Game1.pixelZoom ));
                    for ( int ix = -1; ix <= incrx + 1; ++ix )
                    {
                        for ( int iy = -1; iy <= incry + 1; ++iy )
                        {
                            float rx = sx + ix * starTexRect.Width * Game1.pixelZoom;
                            float ry = sy + iy * starTexRect.Height * Game1.pixelZoom;

                            b.Draw( Game1.mouseCursors, new Vector2( rx, ry ), starTexRect, tints[ i ], 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.001f * i );
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                SpaceShared.Log.Error( "Exception: " + e );
            }
        }
    }
}
