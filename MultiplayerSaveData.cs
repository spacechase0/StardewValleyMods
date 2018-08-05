using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace CookingSkill
{
    public class MultiplayerSaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "cooking-skill-mp.json");

        public Dictionary<long, int> Experience { get; set; } = new Dictionary<long, int>();
    }
}