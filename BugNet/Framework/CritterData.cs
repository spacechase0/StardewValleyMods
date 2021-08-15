using System;
using StardewValley.BellsAndWhistles;

namespace BugNet.Framework
{
    internal class CritterData
    {
        public TextureTarget Texture { get; set; }
        public string DefaultName { get; set; }
        public Func<string> TranslatedName { get; set; }
        public Func<int, int, Critter> MakeFunction { get; set; }
    }
}
