using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
            MethodInfo[] methods = typeof(SpriteBatch)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.Name == nameof(SpriteBatch.Draw))
                .ToArray();

            foreach (var method in methods)
            {
                harmony.Patch(
                    original: method,
                    postfix: method.GetParameters().Any(p => p.Name == "sourceRectangle")
                        ? this.GetHarmonyMethod(nameof(After_Draw))
                        : this.GetHarmonyMethod(nameof(After_Draw_WithoutSourceRectangle))
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
                tracker.Track(Mod.instance.Helper.GameContent.ParseAssetName(texture.Name), sourceRectangle);
            }
        }

        /// <summary>The method to call after any of the <see cref="SpriteBatch"/> <c>Draw</c> methods that doesn't have a <c>sourceRectangle</c> parameter.</summary>
        /// <param name="texture">The texture that was drawn.</param>
        private static void After_Draw_WithoutSourceRectangle(ref Texture2D texture)
        {
            Rectangle? sourceRectangle = null;
            SpriteBatchPatcher.After_Draw(ref texture, ref sourceRectangle);
        }
    }
}
