using StardewValley;

namespace CombatLevelDamageScaler.Overrides
{
    internal class DamageMonsterHook
    {
        internal static void Prefix(ref int minDamage, ref int maxDamage, Farmer who)
        {
            float scale = 1.0f + who.CombatLevel * Mod.Config.DamageScalePerLevel;
            minDamage = (int)(minDamage * scale);
            maxDamage = (int)(maxDamage * scale);
        }
    }
}
