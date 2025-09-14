using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace DynamicGameAssets.Patches
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
                prefix: this.GetHarmonyMethod(nameof(Before_IsTileOccupiedForPlacement))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="GameLocation.isTileOccupiedForPlacement"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_IsTileOccupiedForPlacement(GameLocation __instance, Vector2 tileLocation, StardewValley.Object toPlace, ref bool __result)
        {
            if (toPlace is CustomObject obj && !string.IsNullOrEmpty(obj.Data.Plants))
            {
                __result = GameLocationPatcher.Impl(__instance, tileLocation, obj);
                return false;
            }

            return true;
        }

        private static bool Impl(GameLocation this_, Vector2 tileLocation, CustomObject toPlace)
        {
            foreach (ResourceClump resourceClump in this_.resourceClumps)
            {
                if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
                {
                    return true;
                }
            }
            this_.objects.TryGetValue(tileLocation, out StardewValley.Object o);
            Microsoft.Xna.Framework.Rectangle tileLocationRect = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
            for (int i = 0; i < this_.characters.Count; i++)
            {
                if (this_.characters[i] != null && this_.characters[i].GetBoundingBox().Intersects(tileLocationRect))
                {
                    return true;
                }
            }
            if (this_.isTileOccupiedByFarmer(tileLocation) != null && (toPlace == null || !toPlace.isPassable()))
            {
                return true;
            }
            if (this_.largeTerrainFeatures != null)
            {
                foreach (LargeTerrainFeature largeTerrainFeature in this_.largeTerrainFeatures)
                {
                    if (largeTerrainFeature.getBoundingBox().Intersects(tileLocationRect))
                    {
                        return true;
                    }
                }
            }
            if (toPlace?.Category == -19)
            {
                if (toPlace.Category == -19 && this_.terrainFeatures.ContainsKey(tileLocation) && this_.terrainFeatures[tileLocation] is HoeDirt)
                {
                    HoeDirt hoe_dirt = this_.terrainFeatures[tileLocation] as HoeDirt;
                    if ((int)(this_.terrainFeatures[tileLocation] as HoeDirt).fertilizer != 0)
                    {
                        return true;
                    }
                    if (((int)toPlace.parentSheetIndex == 368 || (int)toPlace.parentSheetIndex == 368) && hoe_dirt.crop != null && (int)hoe_dirt.crop.currentPhase != 0)
                    {
                        return true;
                    }
                }
            }
            else if (this_.terrainFeatures.ContainsKey(tileLocation) && tileLocationRect.Intersects(this_.terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && (!this_.terrainFeatures[tileLocation].isPassable() || (this_.terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)this_.terrainFeatures[tileLocation]).crop != null) || (toPlace != null && toPlace.isSapling())))
            {
                return true;
            }
            if ((toPlace == null || !(toPlace is BedFurniture) || this_.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) || !this_.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y + 1), Game1.viewport)) && !this_.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) && (toPlace == null || !(toPlace is Wallpaper)))
            {
                return true;
            }
            if (toPlace?.Category is -74 or -19 && o is IndoorPot)
            {
                if ((int)toPlace.parentSheetIndex == 251)
                {
                    if ((o as IndoorPot).bush.Value == null && (o as IndoorPot).hoeDirt.Value.crop == null)
                    {
                        return false;
                    }
                }
                else if (toPlace.CanPlantThisSeedHere((o as IndoorPot).hoeDirt.Value, (int)tileLocation.X, (int)tileLocation.Y, toPlace.Category == -19) && (o as IndoorPot).bush.Value == null)
                {
                    return false;
                }
            }
            return o != null;
        }
    }
}
