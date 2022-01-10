using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcherAnimations.Framework;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;

namespace ContentPatcherAnimations.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SpriteBatch"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class SpriteBatchPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the manager which tracks assets that were recently drawn to the screen.</summary>
        private static Func<AssetDrawTracker> GetDrawTracker;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getDrawTracker">Get the manager which tracks assets that were recently drawn to the screen.</param>
        public SpriteBatchPatcher(Func<AssetDrawTracker> getDrawTracker)
        {
            SpriteBatchPatcher.GetDrawTracker = getDrawTracker;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            var methods = new[]
            {
                this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color) }),
                this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
                this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }),
                this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color) }),
                this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) })
            };

            foreach (var method in methods)
            {
                harmony.Patch(
                    original: method,
                    postfix: this.GetHarmonyMethod(nameof(After_Draw))
                );
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after any of the <see cref="SpriteBatch"/> <c>Draw</c> methods.</summary>
        /// <param name="texture">The texture that was drawn.</param>
        /// <param name="sourceRectangle">The pixel area within the texture that was drawn, or <c>null</c> if the entire texture was drawn.</param>
        private static void After_Draw(ref Texture2D texture, ref Rectangle? sourceRectangle)
        {
            if (!string.IsNullOrWhiteSpace(texture?.Name))
            {
                AssetDrawTracker tracker = SpriteBatchPatcher.GetDrawTracker();
                tracker.Track(texture.Name, sourceRectangle);
            }
        }
    }
}
