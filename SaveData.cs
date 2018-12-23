using StardewModdingAPI;
using System.IO;

namespace Magic
{
    public class SaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "magic.json");
        
        public int mana = 0;
        public int manaCap = 0;

        public int magicLevel = 0;
        public int magicExp = 0;
        public int freePoints = 0;

        public SpellBook spellBook = new SpellBook();
    }
}
