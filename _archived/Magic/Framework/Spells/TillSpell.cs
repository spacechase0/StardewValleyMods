using Magic.Framework.Schools;
using Microsoft.Xna.Framework;
using SpaceCore;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Magic.Framework.Spells
{
    internal class TillSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public TillSpell()
            : base(SchoolId.Toil, "till") { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            level += 1;
            targetX /= Game1.tileSize;
            targetY /= Game1.tileSize;
            Vector2 target = new Vector2(targetX, targetY);

            Tool dummyHoe = new Hoe();
            Mod.Instance.Helper.Reflection.GetField<Farmer>(dummyHoe, "lastUser").SetValue(player);

            GameLocation loc = player.currentLocation;
            for (int tileX = targetX - level; tileX <= targetX + level; ++tileX)
            {
                for (int tileY = targetY - level; tileY <= targetY + level; ++tileY)
                {
                    Vector2 tile = new Vector2(tileX, tileY);

                    // skip if out of mana
                    if (player.GetCurrentMana() <= 2)
                        return null;

                    // skip if blocked
                    if (loc.terrainFeatures.ContainsKey(tile))
                        continue;

                    // handle artifact spot, else skip if blocked
                    if (loc.objects.TryGetValue(tile, out SObject obj))
                    {
                        if (obj.ParentSheetIndex == 590)
                        {
                            loc.digUpArtifactSpot(tileX, tileY, player);
                            loc.objects.Remove(tile);
                            player.AddMana(-1);
                        }
                        else
                            continue;
                    }

                    // till dirt
                    if (loc.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null && !loc.isTileOccupied(tile))
                    {
                        loc.makeHoeDirt(tile);
                        loc.playSoundAt("hoeHit", tile);
                        Game1.removeSquareDebrisFromTile(tileX, tileY);
                        loc.temporarySprites.Add(new TemporaryAnimatedSprite(12, new Vector2(tileX * (float)Game1.tileSize, tileY * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
                        loc.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(tileX * (float)Game1.tileSize, tileY * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(tile, target) * 30f));
                        loc.checkForBuriedItem(tileX, tileY, false, false, player);
                        player.AddMana(-3);
                        player.AddCustomSkillExperience(Magic.Skill, 2);
                    }
                }
            }

            return null;
        }
    }
}
