using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;

namespace CookingSkill.Framework
{
    internal class MultiplayerSaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "cooking-skill-mp.json");

        public Dictionary<long, int> Experience { get; set; } = new();
    }
}
