using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatLevelDamageScaler.Overrides;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace CombatLevelDamageScaler
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Configuration Config;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<Configuration>();

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            var target = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster),
                                            new Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer) });
            var patch = AccessTools.Method(typeof(DamageMonsterHook), nameof(DamageMonsterHook.Prefix));
            Monitor.Log($"Patching {target} with {patch}");
            harmony.Patch(target, prefix: new HarmonyMethod(patch));
        }
    }
}
