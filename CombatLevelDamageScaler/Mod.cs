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
                configMenu.RegisterModConfig(
                    mod: this.ModManifest,
                    revertToDefault: () => Mod.Config = new Configuration(),
                    saveToFile: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.RegisterSimpleOption(
                    mod: this.ModManifest,
                    optionName: "Damage Scale",
                    optionDesc: "The amount of damage to scale up per combat level, in percentage.",
                    optionGet: () => (int)(Mod.Config.DamageScalePerLevel * 100),
                    optionSet: value => Mod.Config.DamageScalePerLevel = value / 100f
                );
            }
        }
    }
}
