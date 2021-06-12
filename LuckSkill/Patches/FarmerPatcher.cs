using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace LuckSkill.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Farmer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FarmerPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.gainExperience)),
                prefix: this.GetHarmonyMethod(nameof(Before_GainExperience)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_GainExperience))
            );

            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.getProfessionForSkill)),
                postfix: this.GetHarmonyMethod(nameof(After_GetProfessionForSkill))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="Farmer.gainExperience"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_GainExperience(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // This fixes experience gain.
            // TODO: Learn how to use ILGenerator

            int skipCounter = 3; // Skip the first three instructions, which just skip things if it is the luck skill
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (skipCounter > 0)
                {
                    --skipCounter;
                    continue;
                }

                newInsns.Add(insn);
            }

            return newInsns;
        }

        /// <summary>The method to call before <see cref="Farmer.gainExperience"/>.</summary>
        private static void Before_GainExperience(Farmer __instance, int which, ref int howMuch)
        {
            // This fixes overpowered geodes.

            if (which == Farmer.luckSkill && Game1.currentLocation is MineShaft ms)
            {
                bool foundGeode = false;
                var st = new StackTrace();
                foreach (var frame in st.GetFrames())
                {
                    if (frame.GetMethod().Name.Contains(nameof(MineShaft.checkStoneForItems)))
                    {
                        foundGeode = true;
                        break;
                    }
                }

                if (foundGeode)
                {
                    int msa = ms.getMineArea();
                    if (msa != 0)
                    {
                        howMuch /= msa;
                    }
                }
            }
        }

        /// <summary>The method to call after <see cref="Farmer.getProfessionForSkill"/>.</summary>
        public static void After_GetProfessionForSkill(Farmer __instance, int skillType, int skillLevel, ref int __result)
        {
            // Get profession hook

            if (skillType != Farmer.luckSkill)
                return;

            if (skillLevel == 5)
            {
                if (__instance.professions.Contains(Mod.ProfessionDailyLuck))
                    __result = Mod.ProfessionDailyLuck;
                else if (__instance.professions.Contains(Mod.ProfessionMoreQuests))
                    __result = Mod.ProfessionMoreQuests;
            }
            else if (skillLevel == 10)
            {
                if (__instance.professions.Contains(Mod.ProfessionDailyLuck))
                {
                    if (__instance.professions.Contains(Mod.ProfessionChanceMaxLuck))
                        __result = Mod.ProfessionChanceMaxLuck;
                    else if (__instance.professions.Contains(Mod.ProfessionNoBadLuck))
                        __result = Mod.ProfessionNoBadLuck;
                }
                else if (__instance.professions.Contains(Mod.ProfessionMoreQuests))
                {
                    if (__instance.professions.Contains(Mod.ProfessionNightlyEvents))
                        __result = Mod.ProfessionNightlyEvents;
                    else if (__instance.professions.Contains(Mod.ProfessionJunimoHelp))
                        __result = Mod.ProfessionJunimoHelp;
                }
            }
        }
    }
}
