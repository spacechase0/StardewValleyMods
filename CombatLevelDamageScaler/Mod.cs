using CombatLevelDamageScaler.Patches;
using Spacechase.Shared.Harmony;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CombatLevelDamageScaler
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Configuration Config;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            Config = helper.ReadConfig<Configuration>();

            HarmonyPatcher.Apply(this,
                new GameLocationPatcher()
            );

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Config = new Configuration(), () => this.Helper.WriteConfig(Config));
                capi.RegisterSimpleOption(this.ModManifest, "Damage Scale", "The amount of damage to scale up per combat level, in percentage.", () => (int)(Config.DamageScalePerLevel * 100), (int val) => Config.DamageScalePerLevel = val / 100f);
            }
        }
    }
}
