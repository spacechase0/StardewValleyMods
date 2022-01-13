using System.Collections;
using System.Collections.Generic;
using StardewValley;

namespace ContentPatcherAnimations.Framework
{
    /// <summary>The animation state for a screen.</summary>
    internal class ScreenState
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw patches loaded by Content Patcher for all installed content packs.</summary>
        public IEnumerable RawPatches { get; set; }

        /// <summary>The patch and animation data for loaded patches.</summary>
        public Dictionary<Patch, PatchData> AnimatedPatches { get; } = new();

        /// <summary>The assets that were recently drawn to the screen.</summary>
        public AssetDrawTracker AssetDrawTracker { get; } = new();

        /// <summary>The global animation tick counter.</summary>
        public uint FrameCounter { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Raised after the game's selected language changes.</summary>
        /// <param name="code">The new language code.</param>
        public void OnLocaleChanged(LocalizedContentManager.LanguageCode code)
        {
            this.AssetDrawTracker.OnLocaleChanged(code);
        }
    }
}
