using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace SkillPrestige
{
    /// <summary>
    /// A holder to contain replacement routines for the Stardew Valley Crop class.
    /// </summary>
    public class CropReplacement : Crop
    {
        /// <summary>
        /// Pulled from the decompiled Stardew Valley code, *slightly* reworked for readability. 
        /// Added quality adjustment, and regrowth chances.
        /// </summary>
        /// <param name="xTile"></param>
        /// <param name="yTile"></param>
        /// <param name="soil"></param>
        /// <param name="junimoHarvester"></param>
        /// <returns></returns>
        public bool HarvestReplacement(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null)
        {
            var seedIndex = 0;
            if (CropRegrowthFactor.RegrowthChance > 0)
            {
                seedIndex = GetSeedOfCrop();
            }
            if (dead)
            {
                if (!CropRegrowthFactor.GetDeadCropRegrowthSuccess()) return junimoHarvester != null;
                if (junimoHarvester != null) junimoHarvester.tryToAddItemToHut(new Object(seedIndex, 1));
                else Game1.createObjectDebris(seedIndex, xTile, yTile);
                return junimoHarvester != null;
            }
            if (forageCrop)
            {
                Object forageCropObject = null;
                const int foragingExperience = 3;
                if (whichForageCrop == 1) forageCropObject = new Object(399, 1);
                if (Game1.player.professions.Contains(16))
                {
                    if (forageCropObject != null) forageCropObject.Quality = 4;
                }
                else if (Game1.random.NextDouble() < Game1.player.ForagingLevel / 30.0)
                {
                    if (forageCropObject != null) forageCropObject.Quality = 2;
                }
                else if (Game1.random.NextDouble() < Game1.player.ForagingLevel / 15.0)
                    if (forageCropObject != null) forageCropObject.Quality = 1;
                if (junimoHarvester != null)
                {
                    junimoHarvester.tryToAddItemToHut(forageCropObject);
                    return true;
                }
                if (Game1.player.addItemToInventoryBool(forageCropObject))
                {
                    var vector2 = new Vector2(xTile, yTile);
                    Game1.player.animateOnce(279 + Game1.player.facingDirection);
                    Game1.player.canMove = false;
                    Game1.playSound("harvest");
                    DelayedAction.playSoundAfterDelay("coin", 260);
                    if (regrowAfterHarvest == -1)
                    {
                        Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(17, new Vector2(vector2.X * Game1.tileSize, vector2.Y * Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 125f));
                        Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(vector2.X * Game1.tileSize, vector2.Y * Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 50f));
                    }
                    Game1.player.gainExperience(2, foragingExperience);
                    return true;
                }
                Game1.showRedMessage("Inventory Full");
            }
            else if (currentPhase >= phaseDays.Count - 1 && (!fullyGrown || dayOfCurrentPhase <= 0))
            {
                var numberOfCropsProduced = 1;
                var quality = 0;
                var fertilizerQualityBonus = 0;
                if (indexOfHarvest == 0) return true;
                var random = new Random(xTile * 7 + yTile * 11 + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
                switch (soil.fertilizer)
                {
                    case 368:
                        fertilizerQualityBonus = 1;
                        break;
                    case 369:
                        fertilizerQualityBonus = 2;
                        break;
                }
                var qualityFactor = 0.2 * (Game1.player.FarmingLevel / 10f) + 0.2 * fertilizerQualityBonus * ((Game1.player.FarmingLevel + 2) / 12.0) + 0.01;
                var minumumQualityFactor = Math.Min(0.75, qualityFactor * 2.0);
                if (random.NextDouble() < qualityFactor) quality = 2;
                else if (random.NextDouble() < minumumQualityFactor) quality = 1;
                if (minHarvest > 1 || maxHarvest > 1) numberOfCropsProduced = random.Next(minHarvest, Math.Min(minHarvest + 1, maxHarvest + 1 + Game1.player.FarmingLevel / maxHarvestIncreasePerFarmingLevel));
                if (chanceForExtraCrops > 0.0)
                {
                    while (random.NextDouble() < Math.Min(0.9, chanceForExtraCrops)) ++numberOfCropsProduced;
                }
                quality += CropQualityFactor.GetCropQualityIncrease();
                if (harvestMethod == 1)
                {
                    if (junimoHarvester == null) DelayedAction.playSoundAfterDelay("daggerswipe", 150);
                    if (junimoHarvester != null && Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), Game1.tileSize, junimoHarvester.currentLocation))
                    {
                        Game1.playSound("harvest");
                        DelayedAction.playSoundAfterDelay("coin", 260);
                    }
                    
                    for (var index = 0; index < numberOfCropsProduced; ++index)
                    {
                        if (junimoHarvester != null) junimoHarvester.tryToAddItemToHut(new Object(indexOfHarvest, 1, false, -1, quality));
                        else Game1.createObjectDebris(indexOfHarvest, xTile, yTile, -1, quality);
                    }
                    if (CropRegrowthFactor.GetCropRegrowthSuccess())
                    {
                        if (junimoHarvester != null) junimoHarvester.tryToAddItemToHut(new Object(seedIndex, 1));
                        else Game1.createObjectDebris(seedIndex, xTile, yTile);
                    }
                    if (regrowAfterHarvest == -1) return true;
                    dayOfCurrentPhase.Value = regrowAfterHarvest;
                    fullyGrown.Value = true;
                }
                else
                {
                    if (junimoHarvester == null)
                    {
                        var player = Game1.player;
                        var crop = !programColored ? new Object(indexOfHarvest, 1, false, -1, quality) : new ColoredObject(indexOfHarvest, 1, tintColor) {Quality = quality};
                        if (!player.addItemToInventoryBool(crop))
                        {
                            Game1.showRedMessage("Inventory Full");
                            return false;
                        }
                    }
                    var vector2 = new Vector2(xTile, yTile);
                    if (junimoHarvester == null)
                    {
                        Game1.player.animateOnce(279 + Game1.player.facingDirection);
                        Game1.player.canMove = false;
                    }
                    else
                    {
                        var junimoHarvester1 = junimoHarvester;
                        var crop = !programColored ? new Object(indexOfHarvest, 1, false, -1, quality) : new ColoredObject(indexOfHarvest, 1, tintColor) {Quality = quality};
                        junimoHarvester1.tryToAddItemToHut(crop);
                    }
                    if (random.NextDouble() < Game1.player.LuckLevel / 1500.0 + Game1.dailyLuck / 1200.0 + 10f)
                    {
                        numberOfCropsProduced *= 2;
                        if (junimoHarvester == null || Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), Game1.tileSize, junimoHarvester.currentLocation)) Game1.playSound("dwoop");
                    }
                    else if (harvestMethod == 0)
                    {
                        if (junimoHarvester == null || Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), Game1.tileSize, junimoHarvester.currentLocation))
                        {
                            Game1.playSound("harvest");
                            DelayedAction.playSoundAfterDelay("coin", 260);
                        }
                        if (regrowAfterHarvest == -1 && (junimoHarvester == null || junimoHarvester.currentLocation.Equals(Game1.currentLocation)))
                        {
                            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(17, new Vector2(vector2.X * Game1.tileSize, vector2.Y * Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 125f));
                            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(vector2.X * Game1.tileSize, vector2.Y * Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 50f));
                        }
                    }
                    if (indexOfHarvest == 421)
                    {
                        indexOfHarvest.Value = 431;
                        numberOfCropsProduced = random.Next(1, 4);
                    }
                    for (var index = 0; index < numberOfCropsProduced - 1; ++index)
                    {
                        if (junimoHarvester == null)
                            Game1.createObjectDebris(indexOfHarvest, xTile, yTile);
                        else
                            junimoHarvester.tryToAddItemToHut(new Object(indexOfHarvest, 1));
                    }
                    var farmingExperienceGain = (float)(16.0 * Math.Log(0.018 * Convert.ToInt32(Game1.objectInformation[indexOfHarvest].Split('/')[1]) + 1.0, Math.E));
                    if (junimoHarvester == null) Game1.player.gainExperience(0, (int)Math.Round(farmingExperienceGain));
                    if (regrowAfterHarvest == -1) return true;
                    dayOfCurrentPhase.Value = regrowAfterHarvest;
                    fullyGrown.Value = true;
                }
            }
            return false;
        }

        private int GetSeedOfCrop()
        {
            return Game1.content.Load<Dictionary<int, string>>("Data\\Crops").First(x => x.Value.Split('/')[3] == indexOfHarvest.ToString()).Key;
        }
    }
}