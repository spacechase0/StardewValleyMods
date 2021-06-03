using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatLevelDamageScaler.Overrides;
using Harmony;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CombatLevelDamageScaler
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Configuration Config;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = Monitor;
            Config = helper.ReadConfig<Configuration>();

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            var target = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster),
                                            new Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer) });
            var patch = AccessTools.Method(typeof(DamageMonsterHook), nameof(DamageMonsterHook.Prefix));
            Log.trace($"Patching {target} with {patch}");
            harmony.Patch(target, prefix: new HarmonyMethod(patch));

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
