using System;
using SpaceShared;
using StardewModdingAPI;

namespace CavernMadness
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);
        }
    }
}
