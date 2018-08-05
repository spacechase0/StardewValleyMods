using Microsoft.Xna.Framework;
using SpaceCore.Utilities;
using Magic.Schools;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Reflection;
using Netcode;

namespace Magic.Spells
{
    public class ClearDebrisSpell : Spell
    {
        public ClearDebrisSpell() : base( SchoolId.Toil, "cleardebris" )
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

            Tool dummyAxe = new Axe(); dummyAxe.UpgradeLevel = level;
            Tool dummyPick = new Pickaxe(); dummyPick.UpgradeLevel = level;
            Mod.instance.Helper.Reflection.GetField<Farmer>(dummyAxe, "lastUser").SetValue(player);
            Mod.instance.Helper.Reflection.GetField<Farmer>(dummyPick, "lastUser").SetValue(player);

            GameLocation loc = player.currentLocation;
            for (int ix = targetX - level; ix <= targetX + level; ++ix)
            {
                for (int iy = targetY - level; iy <= targetY + level; ++iy)
                {
                    if (player.getCurrentMana() <= 0)
                        return;

                    Vector2 pos = new Vector2(ix, iy);

                    if (loc.objects.ContainsKey(pos))
                    {
                        var obj = loc.objects[pos];
                        if (obj.performToolAction(dummyAxe, loc))
                        {
                            if (obj.Type == "Crafting" && obj.Fragility != 2)
                            {
                                loc.debris.Add(new Debris(obj.bigCraftable.Value ? -obj.ParentSheetIndex : obj.ParentSheetIndex, pos, pos));
                            }
                            obj.performRemoveAction(pos, loc);
                            loc.objects.Remove(pos);
                            player.addMana(-1);
                            player.addMagicExp(1);
                        }
                        else
                        {
                            var oldStam = player.stamina;
                            dummyPick.DoFunction(loc, ix * Game1.tileSize, iy * Game1.tileSize, 0, player);
                            player.stamina = oldStam;
                            player.addMana(-1);
                            player.addMagicExp(1);
                        }
                    }

                    // Trees
                    if (level >= 2)
                    {
                        if (loc.terrainFeatures.ContainsKey(pos) && !(loc.terrainFeatures[pos] is HoeDirt))
                        {
                            TerrainFeature tf = loc.terrainFeatures[pos];
                            if (tf is Tree)
                            {
                                player.addMana(-1);
                            }
                            if (tf.performToolAction(dummyAxe, 0, pos, loc) || tf is Grass || (tf is Tree && tf.performToolAction(dummyAxe, 0, pos, loc)))
                            {
                                if ( tf is Tree )
                                    player.addMagicExp(10);
                                loc.terrainFeatures.Remove(pos);
                            }
                            if (tf is Grass && loc is Farm)
                            {
                                (loc as Farm).tryToAddHay(1);
                                Game1.playSound("swordswipe");
                                loc.temporarySprites.Add(new TemporaryAnimatedSprite(28, pos * (float)Game1.tileSize + new Vector2((float)Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4), (float)Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4)), Color.Green, 8, Game1.random.NextDouble() < 0.5, (float)Game1.random.Next(60, 100), 0, -1, -1f, -1, 0));
                                loc.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheetName, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, player.position - new Vector2(0.0f, (float)(Game1.tileSize * 2)), false, false, player.position.Y / 10000f, 0.005f, Color.White, (float)Game1.pixelZoom, -0.005f, 0.0f, 0.0f, false)
                                {
                                    motion = { Y = -1f },
                                    layerDepth = (float)(1.0 - (double)Game1.random.Next(100) / 10000.0),
                                    delayBeforeAnimationStart = Game1.random.Next(350)
                                });
                            }
                        }
                    }

                    if (level >= 3)
                    {
                        ICollection< ResourceClump > clumps = (NetCollection<ResourceClump>) loc.GetType().GetField("resourceClumps", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(loc);
                        if (loc is Woods)
                            clumps = (loc as Woods).stumps;
                        if ( clumps != null )
                        {
                            foreach ( var rc in clumps )
                            {
                                if (new Rectangle((int)rc.tile.X, (int)rc.tile.Y, rc.width.Value, rc.height.Value).Contains(ix, iy))
                                {
                                    player.addMana(-1);
                                    if (rc.performToolAction(dummyAxe, 1, pos, loc) || rc.performToolAction(dummyPick, 1, pos, loc))
                                    {
                                        clumps.Remove(rc);
                                        player.addMagicExp(25);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
