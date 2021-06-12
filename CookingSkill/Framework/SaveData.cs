using System.IO;
using StardewModdingAPI;

namespace CookingSkill.Framework
{
    internal class SaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "cooking-skill.json");

        public int Experience = 0;
    }
}
