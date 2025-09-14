using System;
using SpaceShared;

// TODO: event command

namespace FactionsFramework
{
    public class Mod : StardewModdingAPI.Mod
    {
        internal static Mod instance;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
        }
    }
}
