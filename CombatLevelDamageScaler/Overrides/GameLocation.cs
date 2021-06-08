using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;

namespace CombatLevelDamageScaler.Overrides
{
    /// <summary>Applies Harmony patches to <see cref="TODO"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class GameLocationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.damageMonster), new[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer) }),
                prefix: this.GetHarmonyMethod(nameof(Before_DamageMonster))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="GameLocation.damageMonster(Rectangle,int,int,bool,Farmer)"/>.</summary>
        private static void Before_DamageMonster(ref int minDamage, ref int maxDamage, Farmer who)
        {
            float scale = 1.0f + who.CombatLevel * Mod.Config.DamageScalePerLevel;
            minDamage = (int)(minDamage * scale);
            maxDamage = (int)(maxDamage * scale);
        }
    }
}
