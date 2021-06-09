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
            Mod.Config = helper.ReadConfig<Configuration>();

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
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Damage Scale", "The amount of damage to scale up per combat level, in percentage.", () => (int)(Mod.Config.DamageScalePerLevel * 100), (int val) => Mod.Config.DamageScalePerLevel = val / 100f);
            }
        }
    }
}
