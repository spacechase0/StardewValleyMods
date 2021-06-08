using System.IO;
using StardewModdingAPI;

namespace CookingSkill
{
    public class SaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "cooking-skill.json");

        public int experience = 0;
    }
}
