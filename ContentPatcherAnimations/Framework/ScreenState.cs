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
        public IEnumerable CpPatches { get; set; }

        /// <summary>The patch and animation data for loaded patches.</summary>
        public Dictionary<Patch, PatchData> AnimatedPatches { get; } = new();

        /// <summary>The global animation tick counter.</summary>
        public uint FrameCounter { get; set; }

        /// <summary>The number of ticks until all patch target textures should be reloaded.</summary>
        public int FindTargetsCounter { get; set; }

        /// <summary>A queue of patches whose target textures to reload.</summary>
        public Queue<Patch> FindTargetsQueue { get; } = new();
    }
}
