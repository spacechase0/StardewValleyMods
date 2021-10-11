using CombatLevelDamageScaler.Framework;
using CombatLevelDamageScaler.Patches;
using Spacechase.Shared.Patching;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CombatLevelDamageScaler
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Configuration Config;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            HarmonyPatcher.Apply(this,
                new GameLocationPatcher()
            );

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.AddOption(
                    mod: this.ModManifest,
                    name: () => "Damage Scale",
                    tooltip: () => "The amount of damage to scale up per combat level, in percentage.",
                    getValue: () => (int)(Mod.Config.DamageScalePerLevel * 100),
                    setValue: value => Mod.Config.DamageScalePerLevel = value / 100f
                );
            }
        }
    }
}
