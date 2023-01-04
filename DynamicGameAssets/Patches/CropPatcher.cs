using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Crop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CropPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.ResetPhaseDays)),
                prefix: this.GetHarmonyMethod(nameof(Before_ResetPhaseDays))
            );
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.newDay)),
                prefix: this.GetHarmonyMethod(nameof(Before_NewDay))
            );
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.harvest)),
                prefix: this.GetHarmonyMethod(nameof(Before_Harvest))
            );
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.draw)),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw))
            );
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.drawInMenu)),
                prefix: this.GetHarmonyMethod(nameof(Before_DrawInMenu))
            );
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.drawWithOffset)),
                prefix: this.GetHarmonyMethod(nameof(Before_DrawWithOffset))
            );
            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.readyForHarvest)),
                postfix: this.GetHarmonyMethod(nameof(After_ReadyForHarvest))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Crop.ResetPhaseDays"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_ResetPhaseDays(Crop __instance)
        {
            if (__instance is CustomCrop crop)
            {
                crop.ResetPhaseDays();
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Crop.newDay"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_NewDay(Crop __instance, int state, int fertilizer, int xTile, int yTile, GameLocation environment)
        {
            if (__instance is CustomCrop crop)
            {
                crop.NewDay(state, fertilizer, xTile, yTile, environment);
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Crop.harvest"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_Harvest(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester, ref bool __result)
        {
            if (__instance is CustomCrop crop)
            {
                __result = crop.Harvest(xTile, yTile, soil, junimoHarvester);
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Crop.draw"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_Draw(Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
        {
            if (__instance is CustomCrop crop)
            {
                crop.Draw(b, tileLocation, toTint, rotation);
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Crop.drawInMenu"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_DrawInMenu(Crop __instance, SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
        {
            if (__instance is CustomCrop crop)
            {
                crop.DrawInMenu(b, screenPosition, toTint, rotation, scale, layerDepth);
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Crop.drawWithOffset"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_DrawWithOffset(Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
        {
            if (__instance is CustomCrop crop)
            {
                crop.DrawWithOffset(b, tileLocation, toTint, rotation, offset);
                return false;
            }

            return true;
        }

        /// <summary>The method to call after <see cref="HoeDirt.readyForHarvest"/>.</summary>
        private static void After_ReadyForHarvest(HoeDirt __instance, ref bool __result)
        {
            if (__instance.crop != null && __instance.crop is CustomCrop custCrop) {
                var currPhase = custCrop.GetCurrentPhase();
                if (currPhase.HarvestedDrops.Count > 0)
                {
                    __result = true;
                }
            }
        }
    }
}
