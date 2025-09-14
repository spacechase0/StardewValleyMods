using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.VanillaAssetExpansion
{
    // Note that food buff stuff is in Patches/SkillBuffPatcher.cs and Skills.SkillBuff
    // I really need to refactor that

    [HarmonyPatch(typeof(Buff), nameof(Buff.OnAdded))]
    public static class BuffOnAddedCustomPatch
    {
        public static void Postfix(Buff __instance)
        {
            if (__instance is Skills.SkillBuff)
                return;

            if (__instance.customFields == null)
                return;

            if (__instance.customFields.Any(b => b.Key.StartsWith("spacechase.SpaceCore.SkillBuff.") ||
                                                 b.Key.StartsWith("spacechase0.SpaceCore.SkillBuff.") ||
                                                 b.Key.StartsWith("spacechase0.SpaceCore/HealthRegeneration") ||
                                                 b.Key.StartsWith("spacechase0.SpaceCore/StaminaRegeneration")))
            {
                Game1.player.applyBuff(new Skills.SkillBuff(__instance, __instance.id, __instance.customFields));
            }
        }
    }

    // Not needed anymore since skillbuff handles all the custom buff stuff
    /*
    [HarmonyPatch(typeof(Buff), nameof(Buff.OnRemoved))]
    public static class BuffOnRemovedCustomPatch
    {
        public static void Postfix(Buff __instance)
        {
            if (__instance is Skills.SkillBuff)
                return;

            if (!DataLoader.Buffs(Game1.content).TryGetValue(__instance.id, out var buff))
                return;
            if (buff.CustomFields == null)
                return;

            if (buff.CustomFields.TryGetValue("spacechase0.SpaceCore/HealthRegeneration", out string valStr))
            {
                if (float.TryParse(valStr, out float val))
                    Game1.player.GetExtData().HealthRegen -= val;
            }
            if (buff.CustomFields.TryGetValue("spacechase0.SpaceCore/StaminaRegeneration", out valStr))
            {
                if (float.TryParse(valStr, out float val))
                    Game1.player.GetExtData().StaminaRegen -= val;
            }
        }
    }
    */

    [HarmonyPatch(typeof(BuffsDisplay), "getDescription", [typeof(Buff)])]
    public static class BuffsDisplayDescriptionExtrasPatch
    {
        public static void Prefix(Buff buff, ref object __state)
        {
            __state = buff.description;

            if (buff is Skills.SkillBuff sb)
            {
                sb.description += sb.DescriptionHook();
                return;
            }

            if (!DataLoader.Buffs(Game1.content).TryGetValue(buff.id, out var buffData))
                return;
            if (buffData.CustomFields == null)
                return;

            if (buffData.CustomFields.TryGetValue("spacechase0.SpaceCore/HealthRegeneration", out string valStr))
            {
                if (float.TryParse(valStr, out float val))
                {
                    buff.description += (val >= 0 ? "+" : "") + val + " " + I18n.HealthRegen();
                }
            }
            if (buffData.CustomFields.TryGetValue("spacechase0.SpaceCore/StaminaRegeneration", out valStr))
            {
                if (float.TryParse(valStr, out float val))
                {
                    buff.description += (val >= 0 ? "+" : "") + val + " " + I18n.StaminaRegen();
                }
            }
        }

        public static void Postfix(Buff buff, ref object __state)
        {
            buff.description = (string)__state;
        }
    }
}
