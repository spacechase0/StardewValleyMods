using System.Collections.Generic;
using StardewModdingAPI;

namespace ContentPatcherAnimations.Framework
{
    internal class Patch
    {
        public string LogName; // To identify, to check if it is active
        public string Action; // To make sure this is an EditImage

        // Target and FromFile are taken from CP since it handles tokens
        // Same for FromARea and ToArea

        // MINE
        public int AnimationFrameTime = -1;
        public int AnimationFrameCount = -1;
    }

    internal class PatchList
    {
        public ISemanticVersion Format;
        public List<Patch> Changes;
    }
}
