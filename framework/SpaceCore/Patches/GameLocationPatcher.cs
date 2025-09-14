using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="GameLocation"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class GameLocationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) }),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_PerformAction), after: "DaLion.ImmersiveProfessions")
            );

            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                harmony.Patch(
                    original: this.RequireMethod<GameLocation>(nameof(GameLocation.answerDialogueAction)),
                    postfix: this.GetHarmonyMethod(nameof(After_AnswerDialogueAction)),
                    transpiler: this.GetHarmonyMethod(nameof(Transpile_AnswerDialogueAction), after: "DaLion.ImmersiveProfessions")
                );
            }
            else
            {
                harmony.Patch(
                    original: this.RequireMethod<GameLocation>(nameof(GameLocation.answerDialogueAction)),
                    postfix: this.GetHarmonyMethod(nameof(After_AnswerDialogueAction)),
                    transpiler: this.GetHarmonyMethod(nameof(GameLocationPatcher.Transpile_AnswerDialogueActionMobile), after: "DaLion.ImmersiveProfessions")
                );
            }

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.explode)),
                postfix: this.GetHarmonyMethod(nameof(After_Explode))
            );
        }


        /*********
        ** Private methods
        *********/

        private static IEnumerable<CodeInstruction> Transpile_PerformAction(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> ret = new List<CodeInstruction>();
            var codes = new List<CodeInstruction>(insns);
            bool isPatched = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (!isPatched && i < codes.Count - 1 && CodeInstructionExtensions.Is(codes[i + 1], OpCodes.Call, PatchHelper.RequireMethod<GameLocation>(nameof(GameLocation.canRespec))))
                {
                    Label canRespec = gen.DefineLabel();

                    ret.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<Skills>(nameof(Skills.CanRespecAnyCustomSkill))).WithLabels(codes[i].labels));
                    ret.Add(new CodeInstruction(OpCodes.Brtrue, canRespec));
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4_0));

                    // TODO: use non-relative reference to label location
                    codes[i + 15].labels.Add(canRespec);
                    isPatched = true;
                }
                else
                {
                    ret.Add(codes[i]);
                }
            }

            if (!isPatched)
                Log.Warn("Failed to patch GameLocation.performAction!");

            return ret;
        }

        private static IEnumerable<CodeInstruction> Transpile_AnswerDialogueAction(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> ret = new List<CodeInstruction>();
            var codes = new List<CodeInstruction>(insns);
            bool isPatched = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (!isPatched && CodeInstructionExtensions.Is(codes[i + 3], OpCodes.Ldstr, "Strings\\Locations:Sewer_DogStatueCancel"))
                {
                    ret.Add(new CodeInstruction(OpCodes.Ldloc_S, SpaceCore.GetLocalIndexForMethod(original, "skill_responses").Single()).WithLabels(codes[i].labels));
                    ret.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<Skills>(nameof(Skills.GetRespecCustomResponses))));
                    ret.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<List<Response>>(nameof(List<Response>.AddRange))));
                    codes[i].labels.Clear();

                    isPatched = true;
                }
                ret.Add(codes[i]);
            }

            if (!isPatched)
                Log.Warn("Failed to patch GameLocation.answerDialogueAction!");

            return ret;
        }


        private static IEnumerable<CodeInstruction> Transpile_AnswerDialogueActionMobile(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> ret = new List<CodeInstruction>();
            var codes = new List<CodeInstruction>(insns);
            LocalBuilder listLocal = null;
            bool isPatched = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (listLocal == null && codes[i].opcode == OpCodes.Ldloc_S && (codes[i].operand as LocalBuilder).LocalType == typeof(List<Response>))
                {
                    listLocal = codes[i].operand as LocalBuilder;
                }
                if (!isPatched && CodeInstructionExtensions.Is(codes[i + 3], OpCodes.Ldstr, "Strings\\Locations:Sewer_DogStatueCancel"))
                {
                    ret.Add(new CodeInstruction(OpCodes.Ldloc_S, listLocal).WithLabels(codes[i].labels));
                    ret.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<Skills>(nameof(Skills.GetRespecCustomResponses))));
                    ret.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<List<Response>>(nameof(List<Response>.AddRange))));
                    codes[i].labels.Clear();

                    isPatched = true;
                }
                ret.Add(codes[i]);
            }

            return ret;
        }

        private static void After_AnswerDialogueAction(GameLocation __instance, string questionAndAnswer, string[] questionParams)
        {
            if (questionAndAnswer.StartsWith("professionForget_"))
            {
                Skills.Skill skill = Skills.GetSkill(questionAndAnswer.Split('_', 2)[1]);
                if (skill is null)
                {
                    return;
                }

                if (Skills.NewLevels.Contains(new KeyValuePair<string, int>(skill.Id, 5)) || Skills.NewLevels.Contains(new KeyValuePair<string, int>(skill.Id, 10)))
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
                    return;
                }

                Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);

                Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
                for (int i = 0; i < skill.Professions.Count; i++)
                {
                    skill.Professions[i].UndoImmediateProfessionPerk();
                    Game1.player.professions.Remove(skill.Professions[i].GetVanillaId());
                }
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
                int level = Skills.GetSkillLevel(Game1.player, skill.Id);
                if (level >= 5)
                {
                    Skills.NewLevels.Add(new KeyValuePair<string, int>(skill.Id, 5));
                }
                if (level >= 10)
                {
                    Skills.NewLevels.Add(new KeyValuePair<string, int>(skill.Id, 10));
                }
                DelayedAction.playSoundAfterDelay("dog_bark", 300);
                DelayedAction.playSoundAfterDelay("dog_bark", 900);
                return;
            }
        }

        /// <summary>The method to call after <see cref="GameLocation.explode"/>.</summary>
        private static void After_Explode(GameLocation __instance, Vector2 tileLocation, int radius, Farmer who)
        {
            SpaceEvents.InvokeBombExploded(who, tileLocation, radius);
        }
    }
}
