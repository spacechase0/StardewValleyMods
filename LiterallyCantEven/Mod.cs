using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace LiterallyCantEven
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                if (Game1.player.Money == 0)
                    Game1.player.Money = 1;
                else if (Game1.player.Money % 2 == 0)
                    Game1.player.Money -= 1;
            }
        }
    }
}
