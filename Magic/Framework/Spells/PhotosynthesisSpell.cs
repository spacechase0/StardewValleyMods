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
            return base.CanCast(player, level) && player.Items.ContainsId(SObject.prismaticShardID, 1);
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {

            Utility.ForEachLocation(delegate (GameLocation loc)
            {
                foreach (var terrainFeature in loc.terrainFeatures.Values)
                {
                    switch (terrainFeature)
                    {
                        case HoeDirt dirt:
                            this.GrowHoeDirt(dirt);
                            break;

                        case FruitTree tree:
                            tree.TryAddFruit();
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

                return true;
            }, true/*includeInteriors*/, false/*includeGenerated*/);

            player.Items.ReduceId(SObject.prismaticShardID, 1);
            return null;
        }

        private void GrowHoeDirt(HoeDirt dirt)
        {
            if (dirt?.crop is not null)
            {
                dirt.crop.currentPhase.Value = Math.Min(dirt.crop.phaseDays.Count - 1, dirt.crop.currentPhase.Value + 1);
                dirt.crop.dayOfCurrentPhase.Value = 0;
                if (dirt.crop.RegrowsAfterHarvest() && dirt.crop.currentPhase.Value == dirt.crop.phaseDays.Count - 1)
                    dirt.crop.fullyGrown.Value = true;
            }
        }
    }
}
