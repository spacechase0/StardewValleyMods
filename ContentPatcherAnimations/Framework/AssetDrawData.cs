using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcherAnimations.Framework
{
    /// <summary>Data about when a given asset name was last drawn to the screen.</summary>
    internal class AssetDrawData
    {
        /*********
        ** Fields
        *********/
        /// <summary>The asset name.</summary>
        /// <remarks>This field is only tracked to simplify troubleshooting.</remarks>
        private readonly IAssetName AssetName;

        /// <summary>When the texture was last fully drawn without a source rectangle.</summary>
        private int LastFullyDrawnTick;

        /// <summary>When each source rectangle within the texture were last drawn.</summary>
        private readonly Dictionary<Rectangle, int> DrawnAreas = new();


        /*********
        ** Accessors
        *********/
        /// <summary>When any part of the texture was last drawn.</summary>
        public int LastDrawnTick { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="assetName">The asset name.</param>
        public AssetDrawData(IAssetName assetName)
        {
            this.AssetName = assetName;
        }

        /// <summary>Update for a texture area being drawn to the screen.</summary>
        /// <param name="area">The pixel area that was drawn, or <c>null</c> for the entire texture.</param>
        public void Track(Rectangle? area)
        {
            int ticks = Game1.ticks;

            this.LastDrawnTick = ticks;
            if (area.HasValue)
                this.DrawnAreas[area.Value] = ticks;
            else
                this.LastFullyDrawnTick = ticks;
        }

        /// <summary>Get whether the given pixel area was drawn within the last <paramref name="maxTicksAgo"/> ticks.</summary>
        /// <param name="area">The area to check.</param>
        /// <param name="maxTicksAgo">The maximum ticks since the area was drawn to consider.</param>
        public bool WasDrawnWithin(Rectangle area, int maxTicksAgo)
        {
            int tick = Game1.ticks;

            // not drawn at all in that time
            if ((tick - this.LastDrawnTick) > maxTicksAgo)
                return false;

            // fully drawn
            if ((tick - this.LastFullyDrawnTick) <= maxTicksAgo)
                return true;

            // overlapping area drawn
            foreach ((Rectangle drawnArea, int drawnTick) in this.DrawnAreas)
            {
                if (drawnArea.Intersects(area) && (tick - drawnTick) <= maxTicksAgo)
                    return true;
            }

            return false;
        }

        /// <summary>Forget all areas that haven't been drawn within the given number of ticks.</summary>
        /// <param name="maxTicksAgo">The maximum ticks since the area was drawn to consider.</param>
        /// <returns>Returns whether the entire tracker should be forgotten.</returns>
        public bool ForgetExpired(int maxTicksAgo)
        {
            int tick = Game1.ticks;

            // entire tracker expired
            if ((tick - this.LastDrawnTick) > maxTicksAgo)
                return true;

            // forget expired areas
            Rectangle[] expiredAreas = this.DrawnAreas
                .Where(p => (tick - p.Value) > maxTicksAgo)
                .Select(p => p.Key)
                .ToArray();
            foreach (Rectangle area in expiredAreas)
                this.DrawnAreas.Remove(area);

            return false;
        }
    }
}
