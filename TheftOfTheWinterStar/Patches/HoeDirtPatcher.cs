using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace TheftOfTheWinterStar.Patches
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
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.canPlantThisSeedHere)),
                prefix: this.GetHarmonyMethod(nameof(Before_CanPlantThisSeedHere))
            );

            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.plant)),
                prefix: this.GetHarmonyMethod(nameof(Before_Plant))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="HoeDirt.canPlantThisSeedHere"/>.</summary>
        /// <remarks>This patch allows planting crops out of season when near a Tempus Globe.</remarks>
        private static bool Before_CanPlantThisSeedHere(HoeDirt __instance, int objectIndex, int tileX, int tileY, bool isFertilizer, ref bool __result)
        {
            if (isFertilizer)
                return true;

            int seasonalDelimiter = Mod.Ja.GetBigCraftableId("Tempus Globe");

            var loc = Game1.currentLocation;
            for (int ix = -2; ix <= 2; ++ix)
            {
                for (int iy = -2; iy <= 2; ++iy)
                {
                    var key = new Vector2(tileX + ix, tileY + iy);
                    if (loc.objects.TryGetValue(key, out SObject obj))
                    {
                        if (obj.bigCraftable.Value && obj.ParentSheetIndex == seasonalDelimiter)
                        {
                            if (__instance.crop == null)
                            {
                                Crop crop = new(objectIndex, tileX, tileY);
                                __result = !crop.raisedSeeds.Value || !Utility.doesRectangleIntersectTile(Game1.player.GetBoundingBox(), tileX, tileY);
                            }
                            else
                                __result = false;
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="HoeDirt.plant"/>.</summary>
        /// <remarks>This patch allows planting crops out of season when near a Tempus Globe.</remarks>
        private static bool Before_Plant(HoeDirt __instance, int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location, ref bool __result)
        {
            if (isFertilizer)
                return true;

            int seasonalDelimiter = Mod.Ja.GetBigCraftableId("Tempus Globe");

            bool foundDelimiter = false;
            for (int ix = -2; ix <= 2; ++ix)
            {
                for (int iy = -2; iy <= 2; ++iy)
                {
                    var key = new Vector2(tileX + ix, tileY + iy);
                    if (location.objects.ContainsKey(key))
                    {
                        var obj = location.objects[key];
                        if (obj.bigCraftable.Value && obj.ParentSheetIndex == seasonalDelimiter)
                        {
                            foundDelimiter = true;
                        }
                    }
                }
            }

            if (!foundDelimiter)
                return true;

            // Now for the original method
            Crop c = new(index, tileX, tileY);
            if (c.seasonsToGrowIn.Count == 0)
            {
                return false;
            }
            if (!who.currentLocation.IsFarm && !who.currentLocation.IsGreenhouse && !who.currentLocation.CanPlantSeedsHere(index, tileX, tileY) && who.currentLocation.IsOutdoors)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13919"));
                return false;
            }
            if (foundDelimiter || !who.currentLocation.IsOutdoors || who.currentLocation.IsGreenhouse || c.seasonsToGrowIn.Contains(location.GetSeasonForLocation()) || who.currentLocation.SeedsIgnoreSeasonsHere())
            {
                __instance.crop = c;
                if (c.raisedSeeds.Value)
                {
                    location.playSound("stoneStep");
                }
                location.playSound("dirtyHit");
                Game1.stats.SeedsSown++;
                PatchHelper.RequireMethod<HoeDirt>("applySpeedIncreases").Invoke(__instance, new object[] { who });
                __instance.nearWaterForPaddy.Value = -1;
                if (__instance.hasPaddyCrop() && __instance.paddyWaterCheck(location, new Vector2(tileX, tileY)))
                {
                    __instance.state.Value = 1;
                    __instance.updateNeighbors(location, new Vector2(tileX, tileY));
                }
                __result = true;
                return false;
            }
            if (c.seasonsToGrowIn.Count > 0 && !c.seasonsToGrowIn.Contains(location.GetSeasonForLocation()))
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13925"));
            }
            return false;
        }
    }
}
