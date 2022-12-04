using System;
using SpaceShared;
using StardewModdingAPI;

namespace SocialNuance
{
    public class Mod : StardewModdingAPI.Mod
    {
        public Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            //Config = Helper.ReadConfig<Configuration>();
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
