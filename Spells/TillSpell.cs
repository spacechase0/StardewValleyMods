using Microsoft.Xna.Framework;
using SpaceCore.Utilities;
using Magic.Schools;
using StardewValley;
using StardewValley.Tools;

namespace Magic.Spells
{
    public class TillSpell : Spell
    {
        public TillSpell() : base( SchoolId.Toil, "till" )
        {
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 0;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            level += 1;
            targetX /= Game1.tileSize;
            targetY /= Game1.tileSize;
            Vector2 target = new Vector2(targetX, targetY);

            Tool dummyHoe = new Hoe();
            Mod.instance.Helper.Reflection.GetField<Farmer>(dummyHoe, "lastUser").SetValue(player);

            GameLocation loc = player.currentLocation;
            for (int ix = targetX - level; ix <= targetX + level; ++ix)
            {
                for (int iy = targetY - level; iy <= targetY + level; ++iy)
                {
                    if (player.getCurrentMana() <= 0)
                        return;

                    Vector2 pos = new Vector2(ix, iy);
                    if (loc.terrainFeatures.ContainsKey(pos))
                        continue; // ?
                    
                    if ( loc.objects.ContainsKey( pos ) )
                    {
                        var obj = loc.objects[pos];
                        if ( obj.ParentSheetIndex == 590 )
                        {
                            loc.digUpArtifactSpot( ix, iy, player);
                            loc.objects.Remove(pos);
                            player.addMana(-1);
                        }
                        else if ( obj.performToolAction(dummyHoe, loc) )
                        {
                            if ( obj.Type == "Crafting" && obj.Fragility != 2 )
                            {
                                loc.debris.Add(new Debris(obj.bigCraftable.Value ? -obj.ParentSheetIndex : obj.ParentSheetIndex, pos, pos));
                            }
                            obj.performRemoveAction(pos, loc);
                            loc.objects.Remove(pos);
                            player.addMana(-1);
                        }
                    }

                    if ( loc.terrainFeatures.ContainsKey( pos ) )
                    {
                        if (loc.terrainFeatures[pos].performToolAction(dummyHoe, 0, pos, loc))
                        {
                            loc.terrainFeatures.Remove(pos);
                            player.addMana(-1);
                        }
                    }

                    if (loc.doesTileHaveProperty(ix, iy, "Diggable", "Back") != null && !loc.isTileOccupied(pos, ""))
                    {
                        loc.makeHoeDirt(pos);
                        Game1.playSound("hoeHit");
                        Game1.removeSquareDebrisFromTile(ix, iy);
                        loc.temporarySprites.Add(new TemporaryAnimatedSprite(12, new Vector2(ix * (float)Game1.tileSize, iy * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, -1, 0));
                        loc.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(ix * (float)Game1.tileSize, iy * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(pos, target) * 30f, 0, -1, -1f, -1, 0));
                        loc.checkForBuriedItem(ix, iy, false, false);
                        player.addMana(-1);
                        player.addMagicExp(2);
                    }
                }
            }
        }
    }
}
