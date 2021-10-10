using JumpOver.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JumpOver
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                configMenu.RegisterModConfig(
                    mod: this.ModManifest,
                    revertToDefault: () => Mod.Config = new Configuration(),
                    saveToFile: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.RegisterSimpleOption(
                    mod: this.ModManifest,
                    optionName: "Jump Key",
                    optionDesc: "The key to jump",
                    optionGet: () => Mod.Config.KeyJump,
                    optionSet: value => Mod.Config.KeyJump = value
                );
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;

            if (e.Button == Mod.Config.KeyJump && Game1.player.yJumpVelocity == 0)
            {
                // This is terrible for this case, redo it
                new Jump(Game1.player, this.Helper.Events);
            }
        }
    }
}
