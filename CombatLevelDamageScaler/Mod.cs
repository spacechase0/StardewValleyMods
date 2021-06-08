using CombatLevelDamageScaler.Overrides;
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
            Log.Monitor = Monitor;
            Config = helper.ReadConfig<Configuration>();

            HarmonyPatcher.Apply(this,
                new GameLocationPatcher()
            );

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config));
                capi.RegisterSimpleOption(ModManifest, "Damage Scale", "The amount of damage to scale up per combat level, in percentage.", () => (int)(Config.DamageScalePerLevel * 100), (int val) => Config.DamageScalePerLevel = val / 100f);
            }
        }
    }
}
