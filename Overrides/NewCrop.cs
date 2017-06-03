using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SObject = StardewValley.Object;

namespace SpaceCore.Overrides
{
    //[XmlType(TypeName = "Crop")]
    public class NewCrop : Crop
    {
        // Only used for XML serialization
        public NewCrop() { }

        public NewCrop( Crop crop )
        {
            if (crop == null)
                return;

            phaseDays = crop.phaseDays;
            phaseToShow = crop.phaseToShow;
            seasonsToGrowIn = crop.seasonsToGrowIn;
            rowInSpriteSheet = crop.rowInSpriteSheet;
            currentPhase = crop.currentPhase;
            harvestMethod = crop.harvestMethod;
            indexOfHarvest = crop.indexOfHarvest;
            regrowAfterHarvest = crop.regrowAfterHarvest;
            dayOfCurrentPhase = crop.dayOfCurrentPhase;
            minHarvest = crop.minHarvest;
            maxHarvest = crop.maxHarvest;
            maxHarvestIncreasePerFarmingLevel = crop.maxHarvestIncreasePerFarmingLevel;
            daysOfUnclutteredGrowth = crop.daysOfUnclutteredGrowth;
            whichForageCrop = crop.whichForageCrop;
            tintColor = crop.tintColor;
            flip = crop.flip;
            fullyGrown = crop.fullyGrown;
            raisedSeeds = crop.raisedSeeds;
            programColored = crop.programColored;
            dead = crop.dead;
            forageCrop = crop.forageCrop;
            chanceForExtraCrops = crop.chanceForExtraCrops;
        }
        
        public bool canGrowHere( GameLocation loc, Vector2 pos )
        {
            if ( loc is ISeasonalLocation )
            {
                return seasonsToGrowIn.Contains((loc as ISeasonalLocation).Season);
            }
            return false;
        }

        public void newDay_(int state, int fertilizer, int xTile, int yTile, GameLocation environment)
        {
            Vector2 index = new Vector2((float)xTile, (float)yTile);
            if (!canGrowHere(environment, index))//&& !environment.name.Equals("Greenhouse") && (this.dead || !this.seasonsToGrowIn.Contains(Game1.currentSeason)))
            {
                this.dead = true;
            }
            else
            {
                if (state == 1)
                {
                    this.dayOfCurrentPhase = this.fullyGrown ? this.dayOfCurrentPhase - 1 : Math.Min(this.dayOfCurrentPhase + 1, this.phaseDays.Count > 0 ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase)] : 0);
                    if (this.dayOfCurrentPhase >= (this.phaseDays.Count > 0 ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase)] : 0) && this.currentPhase < this.phaseDays.Count - 1)
                    {
                        this.currentPhase = this.currentPhase + 1;
                        this.dayOfCurrentPhase = 0;
                    }
                    while (this.currentPhase < this.phaseDays.Count - 1 && this.phaseDays.Count > 0 && this.phaseDays[this.currentPhase] <= 0)
                        this.currentPhase = this.currentPhase + 1;
                    if (this.rowInSpriteSheet == 23 && this.phaseToShow == -1 && this.currentPhase > 0)
                        this.phaseToShow = Game1.random.Next(1, 7);
                    if (environment is Farm && this.currentPhase == this.phaseDays.Count - 1 && (this.indexOfHarvest == 276 || this.indexOfHarvest == 190 || this.indexOfHarvest == 254) && new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + xTile * 2000 + yTile).NextDouble() < 0.01)
                    {
                        for (int index1 = xTile - 1; index1 <= xTile + 1; ++index1)
                        {
                            for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                            {
                                Vector2 key = new Vector2((float)index1, (float)index2);
                                if (!environment.terrainFeatures.ContainsKey(key) || !(environment.terrainFeatures[key] is HoeDirt) || ((environment.terrainFeatures[key] as HoeDirt).crop == null || (environment.terrainFeatures[key] as HoeDirt).crop.indexOfHarvest != this.indexOfHarvest))
                                    return;
                            }
                        }
                        for (int index1 = xTile - 1; index1 <= xTile + 1; ++index1)
                        {
                            for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                            {
                                Vector2 index3 = new Vector2((float)index1, (float)index2);
                                (environment.terrainFeatures[index3] as HoeDirt).crop = (Crop)null;
                            }
                        }
                      (environment as Farm).resourceClumps.Add((ResourceClump)new GiantCrop(this.indexOfHarvest, new Vector2((float)(xTile - 1), (float)(yTile - 1))));
                    }
                }
                if (this.fullyGrown && this.dayOfCurrentPhase > 0 || (this.currentPhase < this.phaseDays.Count - 1 || this.rowInSpriteSheet != 23))
                    return;
                environment.objects.Remove(index);
                string season = ( environment is ISeasonalLocation) ? ( environment as ISeasonalLocation).Season : Game1.currentSeason;
                switch (this.whichForageCrop)
                {
                    case 495:
                        season = "spring";
                        break;
                    case 496:
                        season = "summer";
                        break;
                    case 497:
                        season = "fall";
                        break;
                    case 498:
                        season = "winter";
                        break;
                }
                environment.objects.Add(index, new SObject(index, this.getRandomWildCropForSeason(season), 1)
                {
                    isSpawnedObject = true,
                    canBeGrabbed = true
                });
                if (environment.terrainFeatures[index] == null || !(environment.terrainFeatures[index] is NewHoeDirt))
                    return;
                (environment.terrainFeatures[index] as NewHoeDirt).crop = (Crop)null;
            }
        }
    }
}
