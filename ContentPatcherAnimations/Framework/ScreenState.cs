using System.Collections;
using System.Collections.Generic;

namespace ContentPatcherAnimations.Framework
{
    internal class ScreenState
    {
        public IEnumerable CpPatches;

        public Dictionary<Patch, PatchData> AnimatedPatches = new();

        public uint FrameCounter;
        public int FindTargetsCounter;
        public Queue<Patch> FindTargetsQueue = new();
    }
}
