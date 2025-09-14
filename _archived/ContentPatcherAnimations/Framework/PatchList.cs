using System.Collections.Generic;
using StardewModdingAPI;

namespace ContentPatcherAnimations.Framework
{
    /// <summary>A list of patches for a content pack.</summary>
    internal class PatchList
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The Content Patcher format version.</summary>
        public ISemanticVersion Format { get; set; }

        /// <summary>The loaded Content Patcher patches.</summary>
        public List<Patch> Changes { get; set; }
    }
}
