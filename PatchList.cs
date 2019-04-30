using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPatcherAnimations
{
    public class Patch
    {
        public string LogName; // To identify, to check if it is active
        public string Action; // To make sure this is an EditImage
        public string Target; // So we know what asset

        public string FromFile;
        public Rectangle FromArea;
        public Rectangle ToArea;

        // MINE
        public int AnimationFrameTime = -1;
        public int AnimationFrameCount = -1;
    }
    public class PatchList
    {
        public ISemanticVersion Format;
        public List<Patch> Changes;
    }
}
