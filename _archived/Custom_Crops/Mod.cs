using System;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace Custom_Crops
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.ConsoleCommands.Add("customcrop", "...", OnCustomCropCommand);
        }

        private void OnCustomCropCommand(string cmd, string[] args)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new CustomCropMenu();
        }
    }
}
