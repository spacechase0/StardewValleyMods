using System.Collections.Generic;
using Magic.Framework.Schools;
using Microsoft.Xna.Framework;
using SpaceCore;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Magic.Framework.Spells
{
    internal class ClearDebrisSpell : Spell
    {
        public ClearDebrisSpell()
            : base(SchoolId.Toil, "cleardebris") { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            level += 1;
            targetX /= Game1.tileSize;
            targetY /= Game1.tileSize;

            Tool dummyAxe = new Axe(); dummyAxe.UpgradeLevel = level;
            Tool dummyPick = new Pickaxe(); dummyPick.UpgradeLevel = level;
            Mod.Instance.Helper.Reflection.GetField<Farmer>(dummyAxe, "lastUser").SetValue(player);
            Mod.Instance.Helper.Reflection.GetField<Farmer>(dummyPick, "lastUser").SetValue(player);

            GameLocation loc = player.currentLocation;
            for (int ix = targetX - level; ix <= targetX + level; ++ix)
            {
                for (int iy = targetY - level; iy <= targetY + level; ++iy)
                {
                    if (player.GetCurrentMana() <= 0)
                        return null;

                    Vector2 pos = new Vector2(ix, iy);

                    if (loc.objects.TryGetValue(pos, out SObject obj))
                    {
                        if (obj.performToolAction(dummyAxe, loc))
                        {
                            if (obj.Type == "Crafting" && obj.Fragility != 2)
                            {
                                loc.debris.Add(new Debris(obj.bigCraftable.Value ? -obj.ParentSheetIndex : obj.ParentSheetIndex, pos, pos));
                            }
                            obj.performRemoveAction(pos, loc);
                            loc.objects.Remove(pos);
                            player.AddMana(-3);
                            player.AddCustomSkillExperience(Magic.Skill, 1);
                        }
                        else
                        {
                            float oldStam = player.stamina;
                            dummyPick.DoFunction(loc, ix * Game1.tileSize, iy * Game1.tileSize, 0, player);
                            player.stamina = oldStam;
                            player.AddMana(-3);
                            player.AddCustomSkillExperience(Magic.Skill, 1);
                        }
                    }

                    // Trees
                    if (level >= 2)
                    {
                        if (loc.terrainFeatures.TryGetValue(pos, out TerrainFeature feature) && !(feature is HoeDirt))
                        {
                            if (feature is Tree)
                            {
                                player.AddMana(-3);
                            }
                            if (feature.performToolAction(dummyAxe, 0, pos, loc) || feature is Grass || (feature is Tree && feature.performToolAction(dummyAxe, 0, pos, loc)))
                            {
                                if (feature is Tree)
                                    player.AddCustomSkillExperience(Magic.Skill, 5);
                                loc.terrainFeatures.Remove(pos);
                            }
                            if (feature is Grass && loc is Farm farm)
                            {
                                farm.tryToAddHay(1);
                                Game1.playSound("swordswipe");
                                farm.temporarySprites.Add(new TemporaryAnimatedSprite(28, pos * Game1.tileSize + new Vector2(Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4), Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4)), Color.Green, 8, Game1.random.NextDouble() < 0.5, Game1.random.Next(60, 100)));
                                farm.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheetName, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, player.position - new Vector2(0.0f, Game1.tileSize * 2), false, false, player.position.Y / 10000f, 0.005f, Color.White, Game1.pixelZoom, -0.005f, 0.0f, 0.0f)
                                {
                                    motion = { Y = -1f },
                                    layerDepth = (float)(1.0 - Game1.random.Next(100) / 10000.0),
                                    delayBeforeAnimationStart = Game1.random.Next(350)
                                });
                            }
                        }
                    }

                    if (level >= 3)
                    {
                        ICollection<ResourceClump> clumps = loc.resourceClumps;

                        if (loc is Woods woods)
                            clumps = woods.stumps;
                        if (clumps != null)
                        {
                            foreach (var rc in clumps)
                            {
                                if (new Rectangle((int)rc.tile.X, (int)rc.tile.Y, rc.width.Value, rc.height.Value).Contains(ix, iy))
                                {
                                    player.AddMana(-3);
                                    if (rc.performToolAction(dummyAxe, 1, pos, loc) || rc.performToolAction(dummyPick, 1, pos, loc))
                                    {
                                        clumps.Remove(rc);
                                        player.AddCustomSkillExperience(Magic.Skill, 10);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
