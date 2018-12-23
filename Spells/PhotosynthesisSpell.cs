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

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override bool canCast(Farmer player, int level)
        {
            return base.canCast(player, level) && player.hasItemInInventory(SObject.prismaticShardIndex, 1);
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            List<GameLocation> locs = new List<GameLocation>();
            locs.Add(Game1.getLocationFromName("Farm"));
            locs.Add(Game1.getLocationFromName("Greenhouse"));
            // TODO: API for other places to grow
            // TODO: Garden pots
            // Such as the SDM farms

            foreach (GameLocation loc in locs)
            {
                foreach (var entry in loc.terrainFeatures.Pairs)
                {
                    var tf = entry.Value;
                    if (tf is HoeDirt dirt)
                    {
                        if (dirt.crop == null)
                            continue;

                        dirt.crop.currentPhase.Value = Math.Min(dirt.crop.phaseDays.Count - 1, dirt.crop.currentPhase.Value + 1);
                        dirt.crop.dayOfCurrentPhase.Value = 0;
                        if (dirt.crop.regrowAfterHarvest.Value != -1 && dirt.crop.currentPhase.Value == dirt.crop.phaseDays.Count - 1)
                        {
                            dirt.crop.fullyGrown.Value = true;
                        }
                    }
                    else if (tf is FruitTree ftree)
                    {
                        if (ftree.daysUntilMature.Value > 0)
                        {
                            ftree.daysUntilMature.Value = Math.Max(0, ftree.daysUntilMature.Value - 7);
                            ftree.growthStage.Value = ftree.daysUntilMature.Value > 0 ? (ftree.daysUntilMature.Value > 7 ? (ftree.daysUntilMature.Value > 14 ? (ftree.daysUntilMature.Value > 21 ? 0 : 1) : 2) : 3) : 4;
                        }
                        else if (!ftree.stump.Value && ftree.growthStage.Value == 4 && (Game1.currentSeason == ftree.fruitSeason.Value || loc.Name == "Greenhouse"))
                        {
                            ftree.fruitsOnTree.Value = 3;
                        }
                    }
                    else if ( tf is Tree tree )
                    {
                        if (tree.growthStage.Value < 5)
                            tree.growthStage.Value++;
                    }
                }
            }

            player.consumeObject( SObject.prismaticShardIndex, 1 );
            return null;
        }
    }
}
