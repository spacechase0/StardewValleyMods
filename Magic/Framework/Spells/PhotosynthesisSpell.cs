using System;
using System.Collections.Generic;
using System.Linq;
using Magic.Framework.Schools;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Magic.Framework.Spells
{
    internal class PhotosynthesisSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public PhotosynthesisSpell()
            : base(SchoolId.Nature, "photosynthesis") { }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override bool CanCast(Farmer player, int level)
        {
            return base.CanCast(player, level) && player.hasItemInInventory(SObject.prismaticShardIndex, 1);
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            List<GameLocation> locs = new List<GameLocation>
            {
                Game1.getLocationFromName("Farm"),
                Game1.getLocationFromName("Greenhouse"),
                Game1.getLocationFromName("IslandWest")
            };

            List<GameLocation> mod_compat_locs = new List<GameLocation>
            {
                // Ridgeside Village:
                Game1.getLocationFromName("Custom_Ridgeside_RSVGreenhouse1"),
                Game1.getLocationFromName("Custom_Ridgeside_RSVGreenhouse2"),
                Game1.getLocationFromName("Custom_Ridgeside_AguarCaveTemporary"),
                Game1.getLocationFromName("Custom_Ridgeside_SummitFarm"),
                // SVE:
                Game1.getLocationFromName("Custom_GrandpasShedGreenhouse"),
                Game1.getLocationFromName("Custom_GrampletonFields")
            };
            foreach (GameLocation loc in mod_compat_locs)
            {
                if (loc != null)
                {
                    locs.Add(loc);
                }
            }


            foreach (GameLocation location in Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                ))
            {
                if (location.IsGreenhouse || location.IsFarm)
                {
                    if (!locs.Contains(location))
                    {
                        locs.Add(location);
                    }
                }
            }
            // TODO: API for other places to grow
            // TODO: Garden pots
            // Such as the SDM farms

            foreach (GameLocation loc in locs)
            {
                foreach (var terrainFeature in loc.terrainFeatures.Values)
                {
                    switch (terrainFeature)
                    {
                        case HoeDirt dirt:
                            this.GrowHoeDirt(dirt);
                            break;

                        case FruitTree tree:
                            if (tree.daysUntilMature.Value > 0)
                            {
                                tree.daysUntilMature.Value = Math.Max(0, tree.daysUntilMature.Value - 7);
                                tree.growthStage.Value = tree.daysUntilMature.Value > 0 ? (tree.daysUntilMature.Value > 7 ? (tree.daysUntilMature.Value > 14 ? (tree.daysUntilMature.Value > 21 ? 0 : 1) : 2) : 3) : 4;
                            }
                            else if (!tree.stump.Value && tree.growthStage.Value == 4 && (Game1.currentSeason == tree.fruitSeason.Value || loc.Name == "Greenhouse"))
                                tree.fruitsOnTree.Value = 3;
                            break;

                        case Tree tree:
                            if (tree.growthStage.Value < 5)
                                tree.growthStage.Value++;
                            break;
                    }
                }

                foreach (var obj in loc.Objects.Values)
                {
                    if (obj is IndoorPot pot)
                        this.GrowHoeDirt(pot.hoeDirt.Value);
                }    
            }

            player.consumeObject(SObject.prismaticShardIndex, 1);
            return null;
        }

        private void GrowHoeDirt(HoeDirt dirt)
        {
            if (dirt?.crop is not null)
            {
                dirt.crop.currentPhase.Value = Math.Min(dirt.crop.phaseDays.Count - 1, dirt.crop.currentPhase.Value + 1);
                dirt.crop.dayOfCurrentPhase.Value = 0;
                if (dirt.crop.regrowAfterHarvest.Value != -1 && dirt.crop.currentPhase.Value == dirt.crop.phaseDays.Count - 1)
                    dirt.crop.fullyGrown.Value = true;
            }
        }
    }
}
