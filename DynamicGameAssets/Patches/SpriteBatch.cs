using DynamicGameAssets.PackData;
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
        internal static Dictionary< Rectangle, TexturedRect > hatOverrides = new Dictionary<Rectangle, TexturedRect>();
        internal static Dictionary< Rectangle, TexturedRect > shirtOverrides = new Dictionary<Rectangle, TexturedRect>();
        internal static Dictionary< Rectangle, TexturedRect > pantsOverrides = new Dictionary<Rectangle, TexturedRect>();

        internal static Dictionary<string, Dictionary<Rectangle, TextureOverridePackData>> packOverrides = new();

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
            else if ( tex == FarmerRenderer.hatsTexture && hatOverrides.ContainsKey( sourceRect ) )
            {
                var texRect = hatOverrides[ sourceRect ];
                tex = texRect.Texture;
                sourceRect = texRect.Rect.HasValue ? texRect.Rect.Value : new Rectangle( 0, 0, tex.Width, tex.Height );
            }
            else if ( tex == FarmerRenderer.shirtsTexture && shirtOverrides.ContainsKey( sourceRect ) )
            {
                var texRect = shirtOverrides[ sourceRect ];
                tex = texRect.Texture;
                sourceRect = texRect.Rect.HasValue ? texRect.Rect.Value : new Rectangle( 0, 0, tex.Width, tex.Height );
            }
            else if ( tex == FarmerRenderer.pantsTexture )
            {
                foreach ( var pants in pantsOverrides )
                {
                    if ( pants.Key.Contains( sourceRect ) )
                    {
                        tex = pants.Value.Texture;
                        var oldSource = sourceRect;
                        sourceRect = pants.Value.Rect.HasValue ? pants.Value.Rect.Value : new Rectangle( 0, 0, tex.Width, tex.Height );
                        int localX = oldSource.X - pants.Key.X;
                        int localY = oldSource.Y - pants.Key.Y;
                        sourceRect = new Rectangle( sourceRect.X + localX, sourceRect.Y + localY, oldSource.Width, oldSource.Height );
                        if ( sourceRect.X < 0 )
                            sourceRect.X += 192;
                        if ( sourceRect.Y < 0 )
                            sourceRect.Y += 688;
                        return;
                    }
                }
            }

            if ( tex.Name == null )
                return;
            if ( packOverrides.ContainsKey( tex.Name ) )
            {
                if ( packOverrides[ tex.Name ].ContainsKey( sourceRect ) )
                {
                    var texRect = packOverrides[ tex.Name ][ sourceRect ].GetCurrentTexture();
                    tex = texRect.Texture;
                    sourceRect = texRect.Rect.HasValue ? texRect.Rect.Value : new Rectangle( 0, 0, tex.Width, tex.Height );
                }
            }
        }
    }
}
