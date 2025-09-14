using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace SpennyDeluxe
{
    /// <summary>Applies Harmony patches to <see cref="SpriteBatch"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class SpriteBatchPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw1))
            );

            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw2))
            );

            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw3))
            );

            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw4))
            );

            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw5))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color,float,Vector2,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw1(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, ref float rotation, ref Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            if (texture.Name == "Portraits/Penny" || texture.Name == "Portraits/Penny_Beach")
            {
                if (sourceRectangle.HasValue)
                {
                    origin = sourceRectangle.Value.Center.ToVector2() / 2;
                    rotation += (float)(Game1.currentGameTime.TotalGameTime.TotalSeconds * 5 % 360);
                }
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color)"/>.</summary>
        private static bool Before_Draw2(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {
            if (texture.Name == "Portraits/Penny" || texture.Name == "Portraits/Penny_Beach")
            {
                if (sourceRectangle.HasValue)
                {
                    __instance.Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 1);
                    return false;
                }
            }
            return true;
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color,float,Vector2,Vector2,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw3(SpriteBatch __instance, Texture2D texture, ref Vector2 position, Rectangle? sourceRectangle, Color color, ref float rotation, ref Vector2 origin, ref Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (texture.Name == "Portraits/Penny" || texture.Name == "Portraits/Penny_Beach")
            {
                if (sourceRectangle.HasValue)
                {
                    origin = sourceRectangle.Value.Size.ToVector2() / 2;
                    position += origin * scale;
                    rotation += (float)(Game1.currentGameTime.TotalGameTime.TotalSeconds * 5 % 360);
                }
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color,float,Vector2,float,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw4(SpriteBatch __instance, Texture2D texture, ref Vector2 position, Rectangle? sourceRectangle, Color color, ref float rotation, ref Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            /*
            if (texture.Name == "Portraits/Penny" || texture.Name == "Portraits/Penny_Beach")
            {
                if (sourceRectangle.HasValue)
                {
                    origin = sourceRectangle.Value.Size.ToVector2() / 2;
                    //position.X -= origin.X * scale * 2;
                    rotation += (float)(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4 % 360);
                }
            }
            */
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color)"/>.</summary>
        private static bool Before_Draw5(SpriteBatch __instance, ref Texture2D texture, ref Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            if (texture.Name == "Portraits/Penny" || texture.Name == "Portraits/Penny_Beach")
            {
                if (sourceRectangle.HasValue)
                {
                    __instance.Draw(texture, position, sourceRectangle, color, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
                    return false;
                }
            }
            return true;
        }
    }
}
