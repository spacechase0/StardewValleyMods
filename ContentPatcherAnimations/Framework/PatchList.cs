using System.Collections.Generic;
using StardewModdingAPI;

namespace ContentPatcherAnimations.Framework
{
    internal class PatchList
    {
        public ISemanticVersion Format;
        public List<Patch> Changes;
    }
}
