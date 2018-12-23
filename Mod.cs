using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Spenny
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;

            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(8))
            {
                NPC penny = Game1.getCharacterFromName("Penny");
                if (penny == null)
                    return;

                penny.faceDirection((penny.FacingDirection + 1) % 4);
                if (penny.yJumpOffset == 0)
                    penny.jump();
            }
        }
    }
}
