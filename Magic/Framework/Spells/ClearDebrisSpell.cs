using System.Collections.Generic;
using Magic.Framework.Schools;
using Microsoft.Xna.Framework;
using SpaceCore;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
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

            // create fake tools
            Axe dummyAxe = new();
            Pickaxe dummyPick = new();
            foreach (var tool in new Tool[] { dummyAxe, dummyPick })
            {
                tool.UpgradeLevel = level;
                tool.IsEfficient = true; // don't drain stamina
                Mod.Instance.Helper.Reflection.GetField<Farmer>(tool, "lastUser").SetValue(player);
            }

            // scan location
            GameLocation loc = player.currentLocation;
            for (int tileX = targetX - level; tileX <= targetX + level; ++tileX)
            {
                for (int tileY = targetY - level; tileY <= targetY + level; ++tileY)
                {
                    if (player.GetCurrentMana() <= 0)
                        return null;

                    Vector2 tile = new(tileX, tileY);

                    if (loc.objects.TryGetValue(tile, out SObject obj) && this.IsDebris(loc, obj))
                    {
                        if (obj.performToolAction(dummyAxe, loc))
                        {
                            obj.performRemoveAction(tile, loc);
                            loc.objects.Remove(tile);
                        }
                        else
                            dummyPick.DoFunction(loc, tileX * Game1.tileSize, tileY * Game1.tileSize, 0, player);

                        player.AddMana(-3);
                        player.AddCustomSkillExperience(Magic.Skill, 1);
                    }

                    // Trees
                    if (level >= 2)
                    {
                        if (loc.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is not HoeDirt or Flooring)
                        {
                            if (feature is Tree)
                            {
                                player.AddMana(-3);
                            }
                            if (feature.performToolAction(dummyAxe, 0, tile, loc) || feature is Grass || (feature is Tree && feature.performToolAction(dummyAxe, 0, tile, loc)))
                            {
                                if (feature is Tree)
                                    player.AddCustomSkillExperience(Magic.Skill, 5);
                                loc.terrainFeatures.Remove(tile);
                            }
                            if (feature is Grass && loc is Farm farm)
                            {
                                farm.tryToAddHay(1);
                                loc.localSoundAt("swordswipe", tile);
                                farm.temporarySprites.Add(new TemporaryAnimatedSprite(28, tile * Game1.tileSize + new Vector2(Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4), Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4)), Color.Green, 8, Game1.random.NextDouble() < 0.5, Game1.random.Next(60, 100)));
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
                                if (new Rectangle((int)rc.tile.X, (int)rc.tile.Y, rc.width.Value, rc.height.Value).Contains(tileX, tileY))
                                {
                                    player.AddMana(-3);
                                    if (rc.performToolAction(dummyAxe, 1, tile, loc) || rc.performToolAction(dummyPick, 1, tile, loc))
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

        /// <summary>Get whether a given object counts as debris.</summary>
        /// <param name="location">The location containing the object.</param>
        /// <param name="obj">The world object.</param>
        private bool IsDebris(GameLocation location, SObject obj)
        {
            if (obj is Chest or null)
                return false;

            // twig
            if (obj.ParentSheetIndex is 294 or 295)
                return true;

            // weeds/stones
            if (obj.Name is "Weeds" or "Stone")
                return true;

            // spawned mine objects
            if (location is MineShaft && obj.IsSpawnedObject)
                return true;

            return false;
        }
    }
}
