using System.Collections;
using System.Collections.Generic;

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

        /// <summary>The global animation tick counter.</summary>
        public uint FrameCounter { get; set; }
    }
}
