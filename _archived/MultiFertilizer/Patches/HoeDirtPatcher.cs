using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiFertilizer.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MultiFertilizer.Patches
{
    /// <summary>Applies Harmony patches to <see cref="HoeDirt"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class HoeDirtPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.plant)),
                prefix: this.GetHarmonyMethod(nameof(Before_Plant))
            );

            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.DrawOptimized)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_DrawOptimized))
            );

            harmony.Patch(
                original: this.RequireMethod<HoeDirt>("applySpeedIncreases"),
                prefix: this.GetHarmonyMethod(nameof(Before_ApplySpeedIncreases)),
                postfix: this.GetHarmonyMethod(nameof(After_ApplySpeedIncreases))
            );

            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.canPlantThisSeedHere)),
                prefix: this.GetHarmonyMethod(nameof(Before_CanPlantThisSeedHere))
            );

            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.dayUpdate)),
                prefix: this.GetHarmonyMethod(nameof(Before_DayUpdate)),
                postfix: this.GetHarmonyMethod(nameof(After_DayUpdate))
            );

            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.seasonUpdate)),
                prefix: this.GetHarmonyMethod(nameof(Before_SeasonUpdate))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="HoeDirt.plant"/>.</summary>
        private static bool Before_Plant(HoeDirt __instance, string item_id, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
        {
            if (isFertilizer && DirtHelper.TryGetFertilizer(item_id, out FertilizerData fertilizer))
            {
                // vanilla logic: basic/quality fertilizer must be applied before seed sprouts
                if (item_id is "368" or "369" && __instance.crop?.currentPhase.Value > 0)
                    return false;

                // custom logic: allow placing fertilizer unless already present
                if (__instance.HasFertilizer(fertilizer))
                    return false;

                __instance.modData[fertilizer.Key] = fertilizer.Level.ToString();
                if (fertilizer.Key == Mod.KeySpeed)
                    Mod.Instance.Helper.Reflection.GetMethod(__instance, "applySpeedIncreases").Invoke(who);
                location.playSound("dirtyHit");
                return true;
            }
            return true;
        }

        /// <summary>The method which transpiles <see cref="HoeDirt.DrawOptimized"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_DrawOptimized(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            bool foundFert = false;
            bool stopCaring = false;
            bool replaceNextLdLoc = false;

            // When we find the the fertilizer reference, replace the next draw with our call
            // Add the HoeDirt instance at the end of the argument list

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (stopCaring)
                {
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldfld && (insn.operand as FieldInfo).Name == "fertilizer")
                {
                    newInsns.Add(insn);
                    foundFert = true;
                    replaceNextLdLoc = true;
                }
                else if (replaceNextLdLoc && insn.IsLdloc())
                {   // Instead of loading the fertilizer and looking at it
                    // we simply stick a 1 on the stack.
                    newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                    replaceNextLdLoc = false;
                }
                else if (foundFert && insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == "Draw")
                {
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_0));

                    insn.opcode = OpCodes.Call;
                    insn.operand = PatchHelper.RequireMethod<HoeDirtPatcher>(nameof(DrawMultiFertilizer));
                    newInsns.Add(insn);

                    stopCaring = true;
                }
                else
                    newInsns.Add(insn);
            }

            return newInsns;
        }

        /// <summary>The method to call before <see cref="HoeDirt.applySpeedIncreases"/>.</summary>
        private static void Before_ApplySpeedIncreases(HoeDirt __instance, Farmer who, out int __state)
        {
            __state = __instance.fertilizer.Value;
            if (__instance.TryGetFertilizer(Mod.KeySpeed, out FertilizerData fertilizer))
                __instance.fertilizer.Value = fertilizer.Id;
        }

        /// <summary>The method to call after <see cref="HoeDirt.applySpeedIncreases"/>.</summary>
        private static void After_ApplySpeedIncreases(HoeDirt __instance, Farmer who, int __state)
        {
            __instance.fertilizer.Value = __state;
        }

        /// <summary>The method to call before <see cref="HoeDirt.canPlantThisSeedHere"/>.</summary>
        private static bool Before_CanPlantThisSeedHere(HoeDirt __instance, string item_id, int tileX, int tileY, bool isFertilizer, ref bool __result)
        {
            if (isFertilizer && DirtHelper.TryGetFertilizer(item_id, out FertilizerData fertilizer))
            {
                __result = !__instance.HasFertilizer(fertilizer);
                return false;
            }
            return true;
        }

        /// <summary>The method to call before <see cref="HoeDirt.dayUpdate"/>.</summary>
        private static void Before_DayUpdate(HoeDirt __instance, GameLocation environment, Vector2 tileLocation, out int __state)
        {
            __state = __instance.fertilizer.Value;
            if (__instance.TryGetFertilizer(Mod.KeyRetain, out FertilizerData fertilizer))
                __instance.fertilizer.Value = fertilizer.Id;
        }

        /// <summary>The method to call after <see cref="HoeDirt.dayUpdate"/>.</summary>
        private static void After_DayUpdate(HoeDirt __instance, GameLocation environment, Vector2 tileLocation, int __state)
        {
<<<<<<< HEAD
            __instance.fertilizer.Value = "0";
=======
            __instance.fertilizer.Value = __state;
>>>>>>> origin/develop
        }

        /// <summary>The method to call before <see cref="HoeDirt.seasonUpdate"/>.</summary>
        private static void Before_SeasonUpdate(HoeDirt __instance, bool onLoad)
        {
            if (!onLoad && !__instance.currentLocation.SeedsIgnoreSeasonsHere() && (__instance.crop == null || __instance.crop.dead.Value || !__instance.crop.seasonsToGrowIn.Contains(Game1.currentLocation.GetSeasonForLocation())))
            {
                foreach (string key in DirtHelper.GetFertilizerTypes())
                    __instance.modData.Remove(key);
            }
        }

        private static void DrawMultiFertilizer(SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, Rectangle? sourceRect, Color col, float rot, Vector2 origin, float scale, SpriteEffects fx, float depth, HoeDirt __instance)
        {
            List<FertilizerData> fertilizers = new List<FertilizerData>();

            foreach (string type in DirtHelper.GetFertilizerTypes())
            {
                if (__instance.TryGetFertilizer(type, out FertilizerData fertilizer))
                    fertilizers.Add(fertilizer);
            }

            foreach (FertilizerData fertilizer in fertilizers)
                spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(173 + fertilizer.SpriteIndex / 3 * 16, 462 + fertilizer.SpriteIndex % 3 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.89E-08f);

            // Draw custom fertilizer, if needed.
            if (__instance.fertilizer.Value > 0)
            {
                spriteBatch.Draw(tex, pos, sourceRect, col, rot, origin, scale, fx, depth);
            }
        }
    }
}
