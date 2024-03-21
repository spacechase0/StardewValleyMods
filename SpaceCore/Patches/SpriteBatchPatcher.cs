using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceCore.VanillaAssetExpansion;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SpriteBatch"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class SpriteBatchPatcher : BasePatcher
    {
        /*********
        ** Accessors
        *********/
        internal static Dictionary<string, Dictionary<Rectangle, TextureOverridePackData>> packOverrides = new();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_1))
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_2))
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_3))
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_4))
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_5))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color,float,Vector2,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw_1(SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                SpriteBatchPatcher.FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color)"/>.</summary>
        private static void Before_Draw_2(SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                SpriteBatchPatcher.FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color,float,Vector2,Vector2,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw_3(SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                SpriteBatchPatcher.FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color,float,Vector2,float,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw_4(SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                SpriteBatchPatcher.FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color)"/>.</summary>
        private static void Before_Draw_5(SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color)
        {
            if (sourceRectangle.HasValue)
            {
                Rectangle rect = sourceRectangle.Value;
                SpriteBatchPatcher.FixTilesheetReference(ref texture, ref rect);
                sourceRectangle = rect;
            }
        }

        private static void FixTilesheetReference(ref Texture2D tex, ref Rectangle sourceRect)
        {
            // override by name
            if (tex?.Name != null && SpriteBatchPatcher.packOverrides.TryGetValue(tex.Name, out var overrides) && overrides.TryGetValue(sourceRect, out var packOverride))
            {
                tex = packOverride.sourceTex;
                sourceRect = packOverride.sourceRectCache;
            }
        }

        /// <summary>Override the texture being drawn if it matches a target texture and the source rectangle matches an override for that type.</summary>
        /// <param name="currentTexture">The texture being drawn to the sprite batch.</param>
        /// <param name="currentSourceRect">The source rectangle being drawn to the sprite batch.</param>
        /// <param name="fromTexture">The target texture to detect.</param>
        /// <param name="overrides">The texture overrides to apply.</param>
        /// <returns>Returns whether the texture was overridden.</returns>
        private static bool TryOverride(ref Texture2D currentTexture, ref Rectangle currentSourceRect, Texture2D fromTexture, IDictionary<Rectangle, TexturedRect> overrides)
        {
            if (currentTexture == fromTexture && overrides.TryGetValue(currentSourceRect, out TexturedRect packOverride))
            {
                currentTexture = packOverride.Texture;
                currentSourceRect = packOverride.Rect ?? new Rectangle(0, 0, currentSourceRect.Width, currentSourceRect.Height);
                return true;
            }

            return false;
        }
    }
}
