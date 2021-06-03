using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;

namespace CookingSkill
{
    public class MultiplayerSaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "cooking-skill-mp.json");

        public Dictionary<long, int> Experience { get; set; } = new Dictionary<long, int>();
    }
}