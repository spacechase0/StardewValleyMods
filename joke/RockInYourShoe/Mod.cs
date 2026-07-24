using HarmonyLib;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.Attributes;
using StardewValley;
using StardewValley.Objects;

namespace RockInYourShoe;

[HasHarmony]
public partial class Mod : BaseMod<Mod>
{
    protected override void ModEntry()
    {
    }
}

[HarmonyPatch(typeof(FarmerSprite), nameof(FarmerSprite.checkForFootstep))]
public static class FootstepPainPatch
{
    private static int counter = 0;
    public static void Postfix(FarmerSprite __instance)
    {
        Farmer farmer = __instance.owner ?? Game1.player;
        if (farmer != Game1.player || farmer.boots.Value is not Boots boots)
            return;

        if ((__instance.currentSingleAnimation >= 32 /*0x20*/ && __instance.currentSingleAnimation <= 56 || __instance.currentSingleAnimation >= 128 /*0x80*/ && __instance.currentSingleAnimation <= 152) && __instance.currentAnimationIndex % 4 == 0)
        {
            int score = boots.defenseBonus.Value + boots.immunityBonus.Value;
            if (score < 1) score = 1;

            int chance = 1 + (int)Math.Ceiling(score / 2f);
            Log.Debug($"meow: {score} {chance}");
            if (++counter % 2 == 0 && Game1.random.Next(chance) != 0)
            {
                farmer.takeDamage(1, true, null);
                farmer.temporarilyInvincible = false;
            }
        }
    }
}
