using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
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
            public int RegrowDays { get; set; } = -1; // Takes priority over newphase
            public int NewPhase { get; set; } = -1;
            public List<StardewValley.GameData.GenericSpawnItemData> Drops { get; set; } = new();
            public bool IgnoreDefaultDrops { get; set; } = true;
        };

        public Dictionary<int, YieldData> YieldOverrides { get; set; } = new();
    }

    [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
    public static class CropHarvestOverridePatch
    {
        public static bool Prefix(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, CropExtensionData>>("spacechase0.SpaceCore/CropExtensionData");
            if (__instance.netSeedIndex.Value == null || !dict.TryGetValue(__instance.netSeedIndex.Value, out var extData))
                return true;
            if (!extData.YieldOverrides.TryGetValue(__instance.currentPhase.Value, out var yields))
                return true;

            if (__instance.fullyGrown.Value && __instance.dayOfCurrentPhase.Value > 0)
                return true;

            var data = __instance.GetData();

            Random r2 = Utility.CreateRandom((double)xTile * 7.0, (double)yTile * 11.0, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame);
            int fertilizerQualityLevel = soil.GetFertilizerQualityBoostLevel();
            double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
            double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);

            List<Item> drops = new();
            bool success = false;

            void DoStuff( Item harvestedItem, int numToHarvest, ref bool localSuccess )
            {
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
                        drops.Add(harvestedItem.getOne());
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
            }

            var yieldsDrops = yields.Drops.DeepClone();
            if (!yields.IgnoreDefaultDrops)
            {
                int cropQuality = 0;
                if (fertilizerQualityLevel >= 3 && r2.NextDouble() < chanceForGoldQuality / 2.0)
                {
                    cropQuality = 4;
                }
                else if (r2.NextDouble() < chanceForGoldQuality)
                {
                    cropQuality = 2;
                }
                else if (r2.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
                {
                    cropQuality = 1;
                }
                cropQuality = MathHelper.Clamp(cropQuality, data?.HarvestMinQuality ?? 0, data?.HarvestMaxQuality ?? cropQuality);
                int numToHarvest = 1;
                if (data != null)
                {
                    int minStack = data.HarvestMinStack;
                    int maxStack = Math.Max(minStack, data.HarvestMaxStack);
                    if (data.HarvestMaxIncreasePerFarmingLevel > 0f)
                    {
                        maxStack += (int)((float)Game1.player.FarmingLevel * data.HarvestMaxIncreasePerFarmingLevel);
                    }
                    if (minStack > 1 || maxStack > 1)
                    {
                        numToHarvest = r2.Next(minStack, maxStack + 1);
                    }
                }
                if (data != null && data.ExtraHarvestChance > 0.0)
                {
                    while (r2.NextDouble() < Math.Min(0.9, data.ExtraHarvestChance))
                    {
                        numToHarvest++;
                    }
                }
                Item harvestedItem = (__instance.programColored.Value ? new ColoredObject(__instance.indexOfHarvest.Value, 1, __instance.tintColor.Value)
                {
                    Quality = cropQuality
                } : ItemRegistry.Create(__instance.indexOfHarvest.Value, 1, cropQuality));

                bool localSuccess = false;
                DoStuff(harvestedItem, numToHarvest, ref localSuccess);
                if (localSuccess)
                {
                    harvestedItem = (__instance.programColored.Value ? new ColoredObject(__instance.indexOfHarvest.Value, 1, __instance.tintColor.Value) : ItemRegistry.Create(__instance.indexOfHarvest.Value));
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

            foreach (var drop in yieldsDrops)
            {
                if (!GameStateQuery.CheckConditions(drop.PerItemCondition, new GameStateQueryContext(__instance.currentLocation, Game1.player, null, null, r2, null, new() { { "Tile", new Vector2(xTile, yTile) } } )))
                    continue;

                var harvestedItems = ItemQueryResolver.TryResolve(drop, new ItemQueryContext(__instance.currentLocation, Game1.player, r2, "SpaceCore Crop Override Yields entry") );
                foreach (var iqr in harvestedItems)
                {
                    Item harvestedItem = iqr.Item as Item;
                    int numToHarvest = harvestedItem.Stack;

                    bool localSuccess = false;

                    DoStuff(harvestedItem, numToHarvest, ref localSuccess);
                    if (localSuccess)
                    {
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

                if (yields.RegrowDays > -1)
                {
                    __instance.fullyGrown.Value = true;
                    __instance.updateDrawMath(__instance.tilePosition);
                    __instance.dayOfCurrentPhase.Value = yields.RegrowDays;

                    __result = false;
                    return false;
                }
                else if (yields.NewPhase == -1)
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
