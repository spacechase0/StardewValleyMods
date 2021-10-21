using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SpriteBatch"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class SpriteBatchPatcher : BasePatcher
    {
        /*********
        ** Accessors
        *********/
        internal static Dictionary<Rectangle, TexturedRect> objectOverrides = new();
        internal static Dictionary<Rectangle, TexturedRect> weaponOverrides = new();
        internal static Dictionary<Rectangle, TexturedRect> hatOverrides = new();
        internal static Dictionary<Rectangle, TexturedRect> shirtOverrides = new();
        internal static Dictionary<Rectangle, TexturedRect> pantsOverrides = new();
        internal static Dictionary<string, Dictionary<Rectangle, TextureOverridePackData>> packOverrides = new();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_1), before: "spacechase0.SpaceCore")
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_2), before: "spacechase0.SpaceCore")
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_3), before: "spacechase0.SpaceCore")
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_4), before: "spacechase0.SpaceCore")
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_5), before: "spacechase0.SpaceCore")
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
            if (tex == Game1.objectSpriteSheet && SpriteBatchPatcher.objectOverrides.ContainsKey(sourceRect))
            {
                var texRect = SpriteBatchPatcher.objectOverrides[sourceRect];
                tex = texRect.Texture;
                sourceRect = texRect.Rect ?? new Rectangle(0, 0, tex.Width, tex.Height);
            }
            else if (tex == Tool.weaponsTexture && SpriteBatchPatcher.weaponOverrides.ContainsKey(sourceRect))
            {
                var texRect = SpriteBatchPatcher.weaponOverrides[sourceRect];
                tex = texRect.Texture;
                sourceRect = texRect.Rect ?? new Rectangle(0, 0, tex.Width, tex.Height);
            }
            else if (tex == FarmerRenderer.hatsTexture && SpriteBatchPatcher.hatOverrides.ContainsKey(sourceRect))
            {
                var texRect = SpriteBatchPatcher.hatOverrides[sourceRect];
                tex = texRect.Texture;
                sourceRect = texRect.Rect ?? new Rectangle(0, 0, tex.Width, tex.Height);
            }
            else if (tex == FarmerRenderer.shirtsTexture && SpriteBatchPatcher.shirtOverrides.ContainsKey(sourceRect))
            {
                var texRect = SpriteBatchPatcher.shirtOverrides[sourceRect];
                tex = texRect.Texture;
                sourceRect = texRect.Rect ?? new Rectangle(0, 0, tex.Width, tex.Height);
            }
            else if (tex == FarmerRenderer.pantsTexture)
            {
                foreach (var pants in SpriteBatchPatcher.pantsOverrides)
                {
                    if (pants.Key.Contains(sourceRect))
                    {
                        tex = pants.Value.Texture;
                        var oldSource = sourceRect;
                        sourceRect = pants.Value.Rect ?? new Rectangle(0, 0, tex.Width, tex.Height);
                        int localX = oldSource.X - pants.Key.X;
                        int localY = oldSource.Y - pants.Key.Y;
                        sourceRect = new Rectangle(sourceRect.X + localX, sourceRect.Y + localY, oldSource.Width, oldSource.Height);
                        if (sourceRect.X < 0)
                            sourceRect.X += 192;
                        if (sourceRect.Y < 0)
                            sourceRect.Y += 688;
                        return;
                    }
                }
            }

            if (tex.Name == null)
                return;
            if (SpriteBatchPatcher.packOverrides.ContainsKey(tex.Name))
            {
                if (SpriteBatchPatcher.packOverrides[tex.Name].ContainsKey(sourceRect))
                {
                    var texRect = SpriteBatchPatcher.packOverrides[tex.Name][sourceRect].GetCurrentTexture();
                    tex = texRect.Texture;
                    sourceRect = texRect.Rect ?? new Rectangle(0, 0, tex.Width, tex.Height);
                }
            }
        }
    }
}
