using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MultiFertilizer.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace MultiFertilizer.Patches
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
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.isTileOccupiedForPlacement)),
                postfix: this.GetHarmonyMethod(nameof(After_isTileOccupiedForPlacement))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="GameLocation.isTileOccupiedForPlacement"/>.</summary>
        private static void After_isTileOccupiedForPlacement(ref GameLocation __instance, ref bool __result, Vector2 tileLocation, Object toPlace = null)
        {
            if (!__result)
                return;

            // get fertilizer
            if (toPlace?.Category != SObject.fertilizerCategory || !DirtHelper.TryGetFertilizer(toPlace.ItemID, out FertilizerData fertilizer))
                return;

            // check if we can apply it
            if (!__instance.TryGetDirt(tileLocation, out HoeDirt dirt, includePots: false) || dirt.HasFertilizer(fertilizer))
                return;

            // recheck vanilla conditions that would block fertilizer placement
            Rectangle tileRect = new Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);
            bool isBlocked = __instance.isTileOccupiedByFarmer(tileLocation) != null || __instance.characters.Any(p => p.GetBoundingBox().Intersects(tileRect));
            if (isBlocked)
                return;

            // mark tile unoccupied to allow placing fertilizer
            __result = false;
        }
    }
}
