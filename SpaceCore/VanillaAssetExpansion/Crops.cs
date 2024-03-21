using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.Internal;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace SpaceCore.VanillaAssetExpansion
{
    public class CropExtensionData
    {
        public class YieldData
        {
            public int ExperienceGained { get; set; } = 5;
            public int NewPhase { get; set; } = -1;
            public List<StardewValley.GameData.GenericSpawnItemData> Drops { get; set; } = new();
        };

        public Dictionary<int, YieldData> YieldOverrides { get; set; } = new();
    }

    [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
    public static class CropHarvestOverridePatch
    {
        public static bool Prefix(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, CropExtensionData>>("spacechase0.SpaceCore/CropExtensionData");
            if (!dict.TryGetValue(__instance.netSeedIndex.Value, out var extData))
                return true;
            if (!extData.YieldOverrides.TryGetValue(__instance.currentPhase.Value, out var yields))
                return true;

            var data = __instance.GetData();

            Random r2 = Utility.CreateRandom((double)xTile * 7.0, (double)yTile * 11.0, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame);
            int fertilizerQualityLevel = soil.GetFertilizerQualityBoostLevel();
            double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
            double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);

            bool success = false;
            List<Item> drops = new();
            foreach (var drop in yields.Drops)
            {
                if (!GameStateQuery.CheckConditions(drop.PerItemCondition, new GameStateQueryContext(__instance.currentLocation, Game1.player, null, null, r2, null, new() { { "Tile", new Vector2(xTile, yTile) } } )))
                    continue;

                var harvestedItems = ItemQueryResolver.TryResolve(drop, new ItemQueryContext(__instance.currentLocation, Game1.player, r2) );
                foreach (var iqr in harvestedItems)
                {
                    Item harvestedItem = iqr.Item as Item;
                    int numToHarvest = harvestedItem.Stack;

                    bool localSuccess = false;

                    HarvestMethod harvestMethod = data?.HarvestMethod ?? HarvestMethod.Grab;
                    if (harvestMethod == HarvestMethod.Scythe)
                    {
                        if (junimoHarvester != null)
                        {
                            DelayedAction.playSoundAfterDelay("daggerswipe", 150, junimoHarvester.currentLocation);
                            if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
                            {
                                junimoHarvester.currentLocation.playSound("harvest");
                                DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
                            }
                            junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
                        }
                        else
                        {
                            Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                        }
                        success = localSuccess = true;
                    }
                    else if (junimoHarvester != null || harvestedItem != null)
                    {
                        if (junimoHarvester == null)
                        {
                            drops.Add(harvestedItem.getOne());
                        }
                        Vector2 initialTile2 = new Vector2(xTile, yTile);
                        if (junimoHarvester == null)
                        {
                            if (!success)
                            {
                                Game1.player.animateOnce(279 + Game1.player.FacingDirection);
                                Game1.player.canMove = false;
                            }
                        }
                        else
                        {
                            junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
                        }
                        if (r2.NextDouble() < Game1.player.team.AverageLuckLevel() / 1500.0 + Game1.player.team.AverageDailyLuck() / 1200.0 + 9.9999997473787516E-05)
                        {
                            numToHarvest *= 2;
                            if (junimoHarvester == null)
                            {
                                if (!success)
                                    Game1.player.currentLocation.playSound("dwoop");
                            }
                            else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
                            {
                                if (!success)
                                    junimoHarvester.currentLocation.playSound("dwoop");
                            }
                        }
                        else if (harvestMethod == HarvestMethod.Grab)
                        {
                            if (junimoHarvester == null)
                            {
                                if (!success)
                                    Game1.player.currentLocation.playSound("harvest");
                            }
                            else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
                            {
                                if (!success)
                                    junimoHarvester.currentLocation.playSound("harvest");
                            }
                            if (junimoHarvester == null)
                            {
                                if (!success)
                                    DelayedAction.playSoundAfterDelay("coin", 260, Game1.player.currentLocation);
                            }
                            else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
                            {
                                if (!success)
                                    DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
                            }
                            if (!__instance.RegrowsAfterHarvest() && (junimoHarvester == null || junimoHarvester.currentLocation.Equals(Game1.currentLocation)))
                            {
                                if (!success)
                                {
                                    Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, Game1.random.NextBool(), 125f));
                                    Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, Game1.random.NextBool(), 50f));
                                }
                            }
                        }
                        success = localSuccess = true;
                    }
                    if (localSuccess)
                    {
                        harvestedItem = (__instance.programColored ? new ColoredObject(__instance.indexOfHarvest, 1, __instance.tintColor.Value) : ItemRegistry.Create(__instance.indexOfHarvest));
                        int price = 0;
                        StardewValley.Object obj = harvestedItem as StardewValley.Object;
                        if (obj != null)
                        {
                            price = obj.Price;
                        }
                        for (int i = 0; i < numToHarvest - 1; i++)
                        {
                            if (junimoHarvester == null)
                            {
                                drops.Add(harvestedItem.getOne());
                            }
                            else
                            {
                                junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
                            }
                        }
                    }
                }
            }
            if (success)
            {
                if (junimoHarvester == null)
                {
                    Game1.player.gainExperience(0, (int)yields.ExperienceGained);
                }
                foreach (var drop in drops)
                {
                    Game1.createItemDebris(drop, new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                }


                if (yields.NewPhase == -1)
                {
                    __result = true;
                    return false;
                }
                __instance.currentPhase.Value = yields.NewPhase;
                __instance.dayOfCurrentPhase.Value = 0;
                __instance.updateDrawMath(__instance.tilePosition);
            }
            __result = false;

            return false;
        }
    }
}
