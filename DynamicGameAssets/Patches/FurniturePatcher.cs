using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley.Objects;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Furniture"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FurniturePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Furniture>(nameof(Furniture.rotate)),
                prefix: this.GetHarmonyMethod(nameof(Before_Rotate))
            );
            harmony.Patch(
                original: this.RequireMethod<Furniture>(nameof(Furniture.updateRotation)),
                prefix: this.GetHarmonyMethod(nameof(Before_UpdateRotation))
            );
            harmony.Patch(
                original: this.RequireMethod<Furniture>(nameof(Furniture.placementAction)),
                postfix: this.GetHarmonyMethod(nameof(After_PlacementAction))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Furniture.rotate"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_Rotate(Furniture __instance)
        {
            if (__instance is CustomBasicFurniture furniture)
            {
                if (__instance.rotations.Value > 1)
                {
                    __instance.currentRotation.Value = (__instance.currentRotation.Value + 1) % __instance.rotations.Value;
                    furniture.UpdateRotation();
                }
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Furniture.updateRotation"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_UpdateRotation(Furniture __instance)
        {
            if (__instance is CustomBasicFurniture furniture)
            {
                furniture.UpdateRotation();
                return false;
            }

            return true;
        }
        
        // <summary>The method to call after placing down lamps or sconces to reset the light to be in the right place.</summary>
        // <returns>Nothing, it's void.</returns>
        private static void After_PlacementAction(Furniture __instance, GameLocation location)
        {
            if (__instance is CustomBasicFurniture furniture)
            {
                if (__instance.furniture_type == 7 || __instance.furniture_type == 17)
                {
                    __instance.resetOnPlayerEntry(location, false);
                }
            }
        }
    }
}
