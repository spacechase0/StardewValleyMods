using StardewModdingAPI;
using System.IO;

namespace CookingSkill
{
    public class SaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "cooking-skill.json");
        
        public int experience = 0;
    }
}