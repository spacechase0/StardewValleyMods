using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcherAnimations.Framework
{
    /// <summary>Tracks assets that were recently drawn to the screen.</summary>
    internal class AssetDrawTracker
    {
        /*********
        ** Fields
        *********/
        /// <summary>The textures that were drawn recently, indexed by normalized asset name.</summary>
        private readonly Dictionary<IAssetName, AssetDrawData> LastDrawn = new();

        /// <summary>The language suffix for the current language code in asset names, if any.</summary>
        private string CurrentLanguageSuffix;


        /*********
        ** Public methods
        *********/
        /// <summary>Track that an asset was drawn to the screen.</summary>
        /// <param name="assetName">The asset name that was drawn.</param>
        /// <param name="area">The pixel area that was drawn, or <c>null</c> for the entire texture.</param>
        public void Track(IAssetName assetName, Rectangle? area)
        {
            /*
            if (string.IsNullOrWhiteSpace(assetName))
                return;

            assetName = PathUtilities.NormalizeAssetName(assetName);
            if (this.CurrentLanguageSuffix != null && assetName.EndsWith(this.CurrentLanguageSuffix, StringComparison.OrdinalIgnoreCase))
                assetName = assetName[..^this.CurrentLanguageSuffix.Length]; // normalize `assetName.fr-FR` to `assetName`
            */

            if (!this.LastDrawn.TryGetValue(assetName, out AssetDrawData data))
                this.LastDrawn[assetName] = data = new AssetDrawData(assetName);

            data.Track(area);
        }

        /// <summary>Get whether the given pixel area was drawn within the last <paramref name="maxTicksAgo"/> ticks.</summary>
        /// <param name="normalizedAssetName">The normalized asset name to check.</param>
        /// <param name="area">The area to check.</param>
        /// <param name="maxTicksAgo">The maximum ticks since the area was drawn to consider.</param>
        public bool WasDrawnWithin(IAssetName normalizedAssetName, Rectangle area, int maxTicksAgo)
        {
            return
                this.LastDrawn.TryGetValue(normalizedAssetName, out AssetDrawData data)
                && data.WasDrawnWithin(area, maxTicksAgo);
        }

        /// <summary>Forget all textures that haven't been drawn within the given number of ticks.</summary>
        /// <param name="maxTicksAgo">The maximum ticks since the area was drawn to consider.</param>
        public void ForgetExpired(int maxTicksAgo)
        {
            IAssetName[] expiredKeys = this.LastDrawn
                .Where(data => data.Value.ForgetExpired(maxTicksAgo))
                .Select(p => p.Key)
                .ToArray();

            foreach (IAssetName key in expiredKeys)
                this.LastDrawn.Remove(key);
        }

        /// <summary>Raised after the game's selected language changes.</summary>
        /// <param name="code">The new language code.</param>
        public void OnLocaleChanged(LocalizedContentManager.LanguageCode code)
        {
            this.CurrentLanguageSuffix = (code == LocalizedContentManager.LanguageCode.en)
                ? null
                : $".{Game1.content.LanguageCodeString(Game1.content.GetCurrentLanguage())}";
        }
    }
}
