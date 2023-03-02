using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ConstellationVoyage
{
    [HarmonyPatch(typeof(Background), nameof(Background.update))]
    public static class BackgroundUpdatePatch
    {
        public static void Postfix(Background __instance, xTile.Dimensions.Rectangle viewport)
        {
            if (__instance is SpaceBackground bg)
            {
                bg.Update(viewport);
            }
        }
    }

    [HarmonyPatch(typeof(Background), nameof(Background.draw))]
    public static class BackgroundDrawPatch
    {
        public static void Postfix(Background __instance, SpriteBatch b)
        {
            if (__instance is SpaceBackground bg)
            {
                bg.Draw(b);
            }
        }
    }
    public class SpaceBackground : Background
    {
        internal Vector2 offset = Vector2.Zero;
        private Rectangle starTexRect = new Rectangle(0, 1453, 639, 195);

        public SpaceBackground()
        :   base(new Color( 0, 0, 12 ), false)
        {
        }

        public void Update( xTile.Dimensions.Rectangle viewport )
        {
            //offset += new Vector2( 25, 0 );
        }

        public void Draw( SpriteBatch b )
        {
            try
            {
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp );

                Rectangle display = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
                b.Draw( Game1.staminaRect, display, Game1.staminaRect.Bounds, this.c, 0f, Vector2.Zero, SpriteEffects.None, 0f );

                Color[] tints = new[]
                {
                    new Color( 255, 200, 200 ),
                    new Color( 170, 255, 170 ),
                    new Color( 150, 150, 255 )
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
                    float sx = -( ( ( Game1.viewport.X + offset.X ) * posMult[ i ] + posMods[ i ].X ) % ( starTexRect.Width * Game1.pixelZoom ) );
                    float sy = -( ( ( Game1.viewport.Y + offset.Y ) * posMult[ i ] + posMods[ i ].Y ) % ( starTexRect.Height * Game1.pixelZoom ));
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

                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp );
            }
            catch ( Exception e )
            {
                SpaceShared.Log.Error( "Exception: " + e );
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp );
            }
        }
    }
}
