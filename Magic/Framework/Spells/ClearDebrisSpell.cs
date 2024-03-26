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
        /*********
        ** Public methods
        *********/
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
            Axe axe = new();
            Pickaxe pickaxe = new();
            foreach (var tool in new Tool[] { axe, pickaxe })
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
                    Vector2 toolPixel = (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2f); // center of tile

                    if (loc.objects.TryGetValue(tile, out SObject obj))
                    {
                        // select tool
                        Tool tool = null;
                        if (this.IsAxeDebris(loc, obj))
                            tool = axe;
                        else if (this.IsPickaxeDebris(loc, obj))
                            tool = pickaxe;

                        // apply
                        if (tool == null)
                            continue;
                        player.lastClick = toolPixel;
                        tool.DoFunction(loc, (int)toolPixel.X, (int)toolPixel.Y, 0, player);

                        if (!loc.objects.ContainsKey(tile))
                        {
                            player.AddMana(-3);
                            player.AddCustomSkillExperience(Magic.Skill, 1);
                        }
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
                            if (feature.performToolAction(axe, 0, tile) || feature is Grass || (feature is Tree && feature.performToolAction(axe, 0, tile)))
                            {
                                if (feature is Tree)
                                    player.AddCustomSkillExperience(Magic.Skill, 5);
                                loc.terrainFeatures.Remove(tile);
                            }
                            if (feature is Grass && loc is Farm farm)
                            {
                                farm.tryToAddHay(1);
                                loc.localSound("swordswipe", tile);
                                farm.temporarySprites.Add(new TemporaryAnimatedSprite(28, tile * Game1.tileSize + new Vector2(Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4), Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4)), Color.Green, 8, Game1.random.NextDouble() < 0.5, Game1.random.Next(60, 100)));
                                farm.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheetName, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, player.position.Value - new Vector2(0.0f, Game1.tileSize * 2), false, false, player.position.Y / 10000f, 0.005f, Color.White, Game1.pixelZoom, -0.005f, 0.0f, 0.0f)
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

                        if (clumps != null)
                        {
                            foreach (var rc in clumps)
                            {
                                if (new Rectangle((int)rc.Tile.X, (int)rc.Tile.Y, rc.width.Value, rc.height.Value).Contains(tileX, tileY))
                                {
                                    player.AddMana(-3);
                                    if (rc.performToolAction(axe, 1, tile) || rc.performToolAction(pickaxe, 1, tile))
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


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a given object is debris which can be cleared with a pickaxe.</summary>
        /// <param name="location">The location containing the object.</param>
        /// <param name="obj">The world object.</param>
        private bool IsPickaxeDebris(GameLocation location, SObject obj)
        {
            if (obj is not Chest or null)
            {
                // stones
                if (obj.Name is "Weeds" or "Stone")
                    return true;

                // spawned mine objects
                if (location is MineShaft && obj.IsSpawnedObject)
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a given object is debris which can be cleared with an axe.</summary>
        /// <param name="location">The location containing the object.</param>
        /// <param name="obj">The world object.</param>
        private bool IsAxeDebris(GameLocation location, SObject obj)
        {
            if (obj is not Chest or null)
            {
                // twig
                if (obj.ParentSheetIndex is 294 or 295)
                    return true;

                // weeds
                if (obj.Name is "Weeds")
                    return true;

                // spawned mine objects
                if (location is MineShaft && obj.IsSpawnedObject)
                    return true;
            }

            return false;
        }
    }
}
