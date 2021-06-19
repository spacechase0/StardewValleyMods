using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Harmony;
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
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
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
        private static bool Before_Plant(HoeDirt __instance, int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
        {
            if (isFertilizer)
            {
                if (__instance.crop != null && __instance.crop.currentPhase.Value != 0)
                    return false;

                int level = 0;
                string key = "";
                switch (index)
                {
                    case 368: level = 1; key = Mod.KeyFert; break;
                    case 369: level = 2; key = Mod.KeyFert; break;
                    case 919: level = 3; key = Mod.KeyFert; break;
                    case 370: level = 1; key = Mod.KeyRetain; break;
                    case 371: level = 2; key = Mod.KeyRetain; break;
                    case 920: level = 3; key = Mod.KeyRetain; break;
                    case 465: level = 1; key = Mod.KeySpeed; break;
                    case 466: level = 2; key = Mod.KeySpeed; break;
                    case 918: level = 3; key = Mod.KeySpeed; break;
                }

                if (__instance.modData.ContainsKey(key))
                    return false;
                else
                {
                    __instance.modData[key] = level.ToString();
                    if (key == Mod.KeySpeed)
                        Mod.Instance.Helper.Reflection.GetMethod(__instance, "applySpeedIncreases").Invoke(who);
                    location.playSound("dirtyHit");
                    return true;
                }
            }
            return true;
        }

        /// <summary>The method which transpiles <see cref="HoeDirt.DrawOptimized"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_DrawOptimized(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            bool foundFert = false;
            bool stopCaring = false;

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
                    foundFert = true;
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
        private static void Before_ApplySpeedIncreases(HoeDirt __instance, Farmer who)
        {
            if (!__instance.modData.TryGetValue(Mod.KeySpeed, out string rawValue))
                return;

            int index = 0;
            switch (int.Parse(rawValue))
            {
                case 1: index = 465; break;
                case 2: index = 466; break;
                case 3: index = 918; break;
            }

            __instance.fertilizer.Value = index;
        }

        /// <summary>The method to call after <see cref="HoeDirt.applySpeedIncreases"/>.</summary>
        private static void After_ApplySpeedIncreases(HoeDirt __instance, Farmer who)
        {
            __instance.fertilizer.Value = 0;
        }

        /// <summary>The method to call before <see cref="HoeDirt.canPlantThisSeedHere"/>.</summary>
        private static bool Before_CanPlantThisSeedHere(HoeDirt __instance, int objectIndex, int tileX, int tileY, bool isFertilizer, ref bool __result)
        {
            if (isFertilizer)
            {
                string key = objectIndex switch
                {
                    368 => Mod.KeyFert,
                    369 => Mod.KeyFert,
                    919 => Mod.KeyFert,
                    370 => Mod.KeyRetain,
                    371 => Mod.KeyRetain,
                    920 => Mod.KeyRetain,
                    465 => Mod.KeySpeed,
                    466 => Mod.KeySpeed,
                    918 => Mod.KeySpeed,
                    _ => ""
                };

                __result = !__instance.modData.ContainsKey(key);
                return false;
            }
            return true;
        }

        /// <summary>The method to call before <see cref="HoeDirt.dayUpdate"/>.</summary>
        private static void Before_DayUpdate(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            if (!__instance.modData.TryGetValue(Mod.KeyRetain, out string rawValue))
                return;

            int index = 0;
            switch (int.Parse(rawValue))
            {
                case 1: index = 370; break;
                case 2: index = 371; break;
                case 3: index = 920; break;
            }

            __instance.fertilizer.Value = index;
        }

        /// <summary>The method to call after <see cref="HoeDirt.dayUpdate"/>.</summary>
        private static void After_DayUpdate(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            __instance.fertilizer.Value = 0;
        }

        /// <summary>The method to call before <see cref="HoeDirt.seasonUpdate"/>.</summary>
        private static void Before_SeasonUpdate(HoeDirt __instance, bool onLoad)
        {
            if (!onLoad && (__instance.crop == null || __instance.crop.dead.Value || !__instance.crop.seasonsToGrowIn.Contains(Game1.currentLocation.GetSeasonForLocation())))
            {
                __instance.modData.Remove(Mod.KeyFert);
                __instance.modData.Remove(Mod.KeyRetain);
                __instance.modData.Remove(Mod.KeySpeed);
            }
        }

        private static void DrawMultiFertilizer(SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, Rectangle? sourceRect, Color col, float rot, Vector2 origin, float scale, SpriteEffects fx, float depth, HoeDirt __instance)
        {
            List<int> fertilizers = new List<int>();
            if (__instance.modData.TryGetValue(Mod.KeyFert, out string rawFertValue))
            {
                int level = int.Parse(rawFertValue);
                int index = 0;
                switch (level)
                {
                    case 1: index = 368; break;
                    case 2: index = 369; break;
                    case 3: index = 919; break;
                }
                if (index != 0)
                    fertilizers.Add(index);
            }
            if (__instance.modData.TryGetValue(Mod.KeyRetain, out string rawRetainerValue))
            {
                int level = int.Parse(rawRetainerValue);
                int index = 0;
                switch (level)
                {
                    case 1: index = 370; break;
                    case 2: index = 371; break;
                    case 3: index = 920; break;
                }
                if (index != 0)
                    fertilizers.Add(index);
            }
            if (__instance.modData.TryGetValue(Mod.KeySpeed, out string rawSpeedValue))
            {
                int level = int.Parse(rawSpeedValue);
                int index = 0;
                switch (level)
                {
                    case 1: index = 465; break;
                    case 2: index = 466; break;
                    case 3: index = 918; break;
                }
                if (index != 0)
                    fertilizers.Add(index);
            }
            foreach (int fertilizer in fertilizers)
            {
                if (fertilizer != 0)
                {
                    int fertilizerIndex = 0;
                    switch (fertilizer)
                    {
                        case 369:
                            fertilizerIndex = 1;
                            break;
                        case 370:
                            fertilizerIndex = 3;
                            break;
                        case 371:
                            fertilizerIndex = 4;
                            break;
                        case 920:
                            fertilizerIndex = 5;
                            break;
                        case 465:
                            fertilizerIndex = 6;
                            break;
                        case 466:
                            fertilizerIndex = 7;
                            break;
                        case 918:
                            fertilizerIndex = 8;
                            break;
                        case 919:
                            fertilizerIndex = 2;
                            break;
                    }
                    spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(173 + fertilizerIndex / 3 * 16, 462 + fertilizerIndex % 3 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.9E-08f);
                }
            }
        }
    }
}
