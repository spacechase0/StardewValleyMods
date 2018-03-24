using Magic.Schools;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace Magic.Spells
{
    class PhotosynthesisSpell : Spell
    {
        public PhotosynthesisSpell() : base( SchoolId.Nature, "photosynthesis" )
        {
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 0;
        }

        public override bool canCast(StardewValley.Farmer player, int level)
        {
            return base.canCast(player, level) && player.hasItemInInventory(SObject.prismaticShardIndex, 1);
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            List<GameLocation> locs = new List<GameLocation>();
            locs.Add(Game1.getLocationFromName("Farm"));
            locs.Add(Game1.getLocationFromName("Greenhouse"));
            // TODO: API for other places to grow
            // Such as the SDM farms

            foreach (GameLocation loc in locs)
            {
                foreach (var entry in loc.terrainFeatures)
                {
                    var tf = entry.Value;
                    if (tf is HoeDirt dirt)
                    {
                        if (dirt.crop == null)
                            continue;

                        dirt.crop.currentPhase = Math.Min(dirt.crop.phaseDays.Count - 1, dirt.crop.currentPhase + 1);
                        dirt.crop.dayOfCurrentPhase = 0;
                        if (dirt.crop.regrowAfterHarvest != -1 && dirt.crop.currentPhase == dirt.crop.phaseDays.Count - 1)
                        {
                            dirt.crop.fullyGrown = true;
                        }
                    }
                    else if (tf is FruitTree ftree)
                    {
                        if (ftree.daysUntilMature > 0)
                        {
                            ftree.daysUntilMature = Math.Max(0, ftree.daysUntilMature - 7);
                            ftree.growthStage = ftree.daysUntilMature > 0 ? (ftree.daysUntilMature > 7 ? (ftree.daysUntilMature > 14 ? (ftree.daysUntilMature > 21 ? 0 : 1) : 2) : 3) : 4;
                        }
                        else if (!ftree.stump && ftree.growthStage == 4 && (Game1.currentSeason == ftree.fruitSeason || loc.name == "Greenhouse"))
                        {
                            ftree.fruitsOnTree = 3;
                        }
                    }
                    else if ( tf is Tree tree )
                    {
                        if (tree.growthStage < 5)
                            tree.growthStage++;
                    }
                }
            }

            player.consumeObject( SObject.prismaticShardIndex, 1 );
        }
    }
}
