using System;
using StardewValley.BellsAndWhistles;

namespace BugNet.Framework
{
    internal class CritterData
    {
        public TextureTarget Texture { get; set; }
        public Func<string> Name { get; set; }
        public Func<int, int, Critter> MakeFunction { get; set; }
    }
}
