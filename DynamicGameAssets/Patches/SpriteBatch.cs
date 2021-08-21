using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.Patches
{
    public class SpriteBatchTileSheetAdjustments
    {
        internal static Dictionary< Rectangle, TexturedRect > objectOverrides = new Dictionary<Rectangle, TexturedRect>();
        internal static Dictionary< Rectangle, TexturedRect > weaponOverrides = new Dictionary<Rectangle, TexturedRect>();

        public static void Prefix1( SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth )
        {
            if ( sourceRectangle.HasValue )
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference( ref texture, ref rect );
                sourceRectangle = rect;
            }
        }

        public static void Prefix2( SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color )
        {
            if ( sourceRectangle.HasValue )
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference( ref texture, ref rect );
                sourceRectangle = rect;
            }
        }

        public static void Prefix3( SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth )
        {
            if ( sourceRectangle.HasValue )
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference( ref texture, ref rect );
                sourceRectangle = rect;
            }
        }
        public static void Prefix4( SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth )
        {
            if ( sourceRectangle.HasValue )
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference( ref texture, ref rect );
                sourceRectangle = rect;
            }
        }
        public static void Prefix5( SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color )
        {
            if ( sourceRectangle.HasValue )
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference( ref texture, ref rect );
                sourceRectangle = rect;
            }
        }

        public static void FixTilesheetReference( ref Texture2D tex, ref Rectangle sourceRect )
        {
            if ( tex == Game1.objectSpriteSheet && objectOverrides.ContainsKey( sourceRect ) )
            {
                var texRect = objectOverrides[ sourceRect ];
                tex = texRect.Texture;
                sourceRect = texRect.Rect.HasValue ? texRect.Rect.Value : new Rectangle( 0, 0, tex.Width, tex.Height );
            }
            else if ( tex == Tool.weaponsTexture && weaponOverrides.ContainsKey( sourceRect ) )
            {
                var texRect = weaponOverrides[ sourceRect ];
                tex = texRect.Texture;
                sourceRect = texRect.Rect.HasValue ? texRect.Rect.Value : new Rectangle( 0, 0, tex.Width, tex.Height );
            }
        }
    }
}
