using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Overrides
{
    public class SpriteBatchTileSheetAdjustments
    {
        public static void Prefix1(SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }
        
        public static void Prefix2(SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }

        public static void Prefix3(SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }
        public static void Prefix4(SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }
        public static void Prefix5(SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }

        public static void FixTilesheetReference(ref Texture2D tex, ref Rectangle sourceRect)
        {
            if (sourceRect.Y + sourceRect.Height < 4096)
                return;

            var target = TileSheetExtensions.GetAdjustedTileSheetTarget(tex, sourceRect);
            tex = TileSheetExtensions.GetTileSheet(tex, target.TileSheet);
            sourceRect.Y = target.Y;
        }
    }
}
