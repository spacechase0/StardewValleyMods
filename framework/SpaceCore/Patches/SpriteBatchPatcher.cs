using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
        internal static Dictionary<(string, Rectangle), TextureOverridePackData> packOverrides = new();
        private static bool InDrawRedirection = false;
        private delegate bool TryGetTextureOverrideDelegate(Texture2D tex, Rectangle? sourceRect, out TextureOverridePackData packData);
        private static TryGetTextureOverrideDelegate TryGetTextureOverride = TryGetTextureOverride_Standard;
        private static readonly HashSet<(string Texture, Rectangle? SourceRect)> RecordedDraws = [];

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
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_4))
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_5))
            );
            harmony.Patch(
                original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Color) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw_6))
            );
        }

        public static void ToggleSpriteBatchPatcherRecordingMode(IModHelper helper)
        {
            if (TryGetTextureOverride == TryGetTextureOverride_Standard)
            {
                TryGetTextureOverride = TryGetTextureOverride_Recording;
                Log.Info($"Begun recording distinct draws.");
            }
            else
            {
                TryGetTextureOverride = TryGetTextureOverride_Standard;
                helper.Data.WriteJsonFile("drawn_texture_and_sourcerect.json", RecordedDraws);
                Log.Info($"Recorded {RecordedDraws.Count} distinct draws, wrote '{Path.Join(helper.DirectoryPath, "drawn_texture_and_sourcerect.json")}");
                RecordedDraws.Clear();
            }
        }

        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color,float,Vector2,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw_1(SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color, float rotation, ref Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            if (TryGetTextureOverride(texture, sourceRectangle, out TextureOverridePackData packData))
            {
                texture = packData.sourceTex;
                Rectangle newRect = packData.FullSheetMode ? packData.GetDrawOverrideSourceRect(sourceRectangle.Value) : packData.sourceRectCache;
                if (sourceRectangle != newRect)
                {
                    if (origin != Vector2.Zero)
                    {
                        origin = new(origin.X / sourceRectangle.Value.Width * newRect.Width, origin.Y / sourceRectangle.Value.Height * newRect.Height);
                    }
                    sourceRectangle = newRect;
                }
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color)"/>.</summary>
        private static void Before_Draw_2(SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, ref Rectangle? sourceRectangle, Color color)
        {
            if (TryGetTextureOverride(texture, sourceRectangle, out TextureOverridePackData packData))
            {
                texture = packData.sourceTex;
                sourceRectangle = packData.FullSheetMode ? packData.GetDrawOverrideSourceRect(sourceRectangle.Value) : packData.sourceRectCache;
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color,float,Vector2,Vector2,SpriteEffects,float)"/>.</summary>
        private static void Before_Draw_3(SpriteBatch __instance, ref Texture2D texture, Vector2 position, ref Rectangle? sourceRectangle, Color color, float rotation, ref Vector2 origin, ref Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (TryGetTextureOverride(texture, sourceRectangle, out TextureOverridePackData packData))
            {
                Rectangle newRect;
                if (packData.FullSheetMode)
                {
                    newRect = packData.GetDrawOverrideSourceRect(sourceRectangle.Value);
                    if (packData.SourceSizeModifer != 1)
                        scale = new(scale.X / packData.SourceSizeModifer, scale.Y / packData.SourceSizeModifer);
                }
                else
                {
                    if (sourceRectangle.Value.Width != packData.sourceRectCache.Width || sourceRectangle.Value.Height != packData.sourceRectCache.Height)
                    {
                        scale = new(scale.X * (sourceRectangle.Value.Width / (float)packData.sourceRectCache.Width), scale.Y * (sourceRectangle.Value.Height / (float)packData.sourceRectCache.Height));
                    }
                    newRect = packData.sourceRectCache;
                }
                if (sourceRectangle != newRect)
                {
                    if (origin != Vector2.Zero)
                    {
                        origin = new(origin.X / sourceRectangle.Value.Width * newRect.Width, origin.Y / sourceRectangle.Value.Height * newRect.Height);
                    }
                    sourceRectangle = newRect;
                }

                texture = packData.sourceTex;
            }
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Color)"/>.</summary>
        private static bool Before_Draw_4(SpriteBatch __instance, ref Texture2D texture, Vector2 position, Color color)
        {
            if (TryGetTextureOverride(texture, Rectangle.Empty, out TextureOverridePackData packData))
            {
                bool needDrawRedirect;
                Rectangle overrideSourceRect;

                if (packData.FullSheetMode)
                {
                    needDrawRedirect = packData.SourceSizeModifer != 1 || texture.Bounds != packData.sourceTex.Bounds;
                    overrideSourceRect = packData.GetDrawOverrideSourceRect(texture.Bounds);
                }
                else
                {
                    needDrawRedirect = texture.Bounds.Width != packData.sourceRectCache.Width || texture.Bounds.Height != packData.sourceRectCache.Height;
                    overrideSourceRect = packData.sourceRectCache;
                }

                if (needDrawRedirect)
                {
                    InDrawRedirection = true;
                    __instance.Draw(packData.sourceTex, new Rectangle((int)position.X, (int)position.Y, texture.Bounds.Width, texture.Bounds.Height), overrideSourceRect, color);
                    InDrawRedirection = false;
                    return false;
                }

                texture = packData.sourceTex;
            }
            return true;
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color)"/>.</summary>
        private static bool Before_Draw_5(SpriteBatch __instance, ref Texture2D texture, ref Vector2 position, ref Rectangle? sourceRectangle, Color color)
        {
            if (TryGetTextureOverride(texture, sourceRectangle, out TextureOverridePackData packData))
            {
                bool needDrawRedirect;
                Rectangle overrideSourceRect;

                if (packData.FullSheetMode)
                {
                    needDrawRedirect = packData.SourceSizeModifer != 1;
                    overrideSourceRect = packData.GetDrawOverrideSourceRect(sourceRectangle.Value);
                }
                else
                {
                    needDrawRedirect = sourceRectangle.Value.Width != packData.sourceRectCache.Width || sourceRectangle.Value.Height != packData.sourceRectCache.Height;
                    overrideSourceRect = packData.sourceRectCache;
                }

                if (needDrawRedirect)
                {
                    InDrawRedirection = true;
                    __instance.Draw(packData.sourceTex, new Rectangle((int)position.X, (int)position.Y, sourceRectangle.Value.Width, sourceRectangle.Value.Height), overrideSourceRect, color);
                    InDrawRedirection = false;
                    return false;
                }

                sourceRectangle = overrideSourceRect;
                texture = packData.sourceTex;
            }
            return true;
        }

        /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Color)"/>.</summary>
        private static bool Before_Draw_6(SpriteBatch __instance, ref Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            if (TryGetTextureOverride(texture, Rectangle.Empty, out TextureOverridePackData packData))
            {
                bool needDrawRedirect;
                Rectangle overrideSourceRect;

                if (packData.FullSheetMode)
                {
                    needDrawRedirect = packData.SourceSizeModifer != 1 || texture.Bounds != packData.sourceTex.Bounds;
                    overrideSourceRect = packData.GetDrawOverrideSourceRect(texture.Bounds);
                }
                else
                {
                    needDrawRedirect = texture.Bounds.Width != packData.sourceRectCache.Width || texture.Bounds.Height != packData.sourceRectCache.Height;
                    overrideSourceRect = packData.sourceRectCache;
                }

                if (needDrawRedirect)
                {
                    InDrawRedirection = true;
                    __instance.Draw(packData.sourceTex, destinationRectangle, overrideSourceRect, color);
                    InDrawRedirection = false;
                    return false;
                }

                texture = packData.sourceTex;
            }
            else
            {
                if (texture.Name == "Animals/Error")
                {
                    Console.WriteLine($"{destinationRectangle}: {texture.Bounds}");
                    Console.WriteLine(string.Join(' ', packOverrides.Select(value => value.ToString())));
                }
            }
            return true;
        }

        /// <summary>Obtain pack data from cached pack overrides</summary>
        /// <param name="tex"></param>
        /// <param name="sourceRect"></param>
        /// <param name="packData"></param>
        /// <returns></returns>
        private static bool TryGetTextureOverride_Standard(Texture2D tex, Rectangle? sourceRect, out TextureOverridePackData packData)
        {
            packData = null;
            if (InDrawRedirection || tex == null || tex.Name == null || sourceRect is null)
                return false;
            // override by name and rect
            if (packOverrides.TryGetValue((tex.Name, sourceRect.Value), out packData))
            {
                return true;
            }
            // no specific override, fallback to empty
            if (!sourceRect.Value.IsEmpty && packOverrides.TryGetValue((tex.Name, Rectangle.Empty), out packData))
            {
                return true;
            }
            return false;
        }

        private static bool TryGetTextureOverride_Recording(Texture2D tex, Rectangle? sourceRect, out TextureOverridePackData packData)
        {
            RecordedDraws.Add((tex.Name, sourceRect));
            return TryGetTextureOverride_Standard(tex, sourceRect, out packData);
        }
    }
}
