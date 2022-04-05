using System;
using System.Collections.Generic;
using Magic.Framework.Schools;
using StardewValley;
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
            return base.CanCast(player, level) && player.hasItemInInventory($"(O){SObject.prismaticShardIndex}", 1);
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            List<GameLocation> locs = new List<GameLocation>
            {
                Game1.getLocationFromName("Farm"),
                Game1.getLocationFromName("Greenhouse")
            };
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
                            if (dirt.crop != null)
                            {
                                dirt.crop.currentPhase.Value = Math.Min(dirt.crop.phaseDays.Count - 1, dirt.crop.currentPhase.Value + 1);
                                dirt.crop.dayOfCurrentPhase.Value = 0;
                                if (dirt.crop.regrowAfterHarvest.Value != -1 && dirt.crop.currentPhase.Value == dirt.crop.phaseDays.Count - 1)
                                    dirt.crop.fullyGrown.Value = true;
                            }
                            break;

                        case FruitTree tree:
                            if (tree.daysUntilMature.Value > 0)
                            {
                                tree.daysUntilMature.Value = Math.Max(0, tree.daysUntilMature.Value - 7);
                                tree.growthStage.Value = tree.daysUntilMature.Value > 0 ? (tree.daysUntilMature.Value > 7 ? (tree.daysUntilMature.Value > 14 ? (tree.daysUntilMature.Value > 21 ? 0 : 1) : 2) : 3) : 4;
                            }
                            else if (!tree.stump.Value && tree.growthStage.Value == 4 && (tree.IsInSeasonHere(loc) || loc.SeedsIgnoreSeasonsHere()))
                                while (tree.fruitObjectsOnTree.Count < 3)
                                {
                                    tree.fruitObjectsOnTree.Add(Utility.CreateItemByID(tree.indexOfFruit.Value, 1));
                                }
                            break;

                        case Tree tree:
                            if (tree.growthStage.Value < 5)
                                tree.growthStage.Value++;
                            break;
                    }
                }
            }

            player.consumeObject($"(O){SObject.prismaticShardIndex}", 1);
            return null;
        }
    }
}
