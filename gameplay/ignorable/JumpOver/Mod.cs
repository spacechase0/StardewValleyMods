using JumpOver.Framework;
using SpaceShared;
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
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.AddKeybind(
                    mod: this.ModManifest,
                    name: I18n.Config_JumpKey_Name,
                    tooltip: I18n.Config_JumpKey_Tooltip,
                    getValue: () => Mod.Config.KeyJump,
                    setValue: value => Mod.Config.KeyJump = value
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_RockCrabCrushing_Name,
                    tooltip: I18n.Config_RockCrabCrushing_Tooltip,
                    getValue: () => Mod.Config.RockCrabCrushing,
                    setValue: value => Mod.Config.RockCrabCrushing = value
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
