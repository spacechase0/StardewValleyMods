using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace MoreGrassStarters
{
    public class CustomGrass : Grass, ISaveElement
    {
        public CustomGrass() { }

        public CustomGrass(int which, int numberOfWeeds)
            : base(which, numberOfWeeds) { }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new()
            {
                ["Type"] = ((int)this.grassType.Value).ToString(),
                ["WeedCount"] = this.numberOfWeeds.Value.ToString()
            };
        }

        public object getReplacement()
        {
            return new FruitTree(); // new Grass(Grass.springGrass, numberOfWeeds);
        }

        public void rebuild(Dictionary<string, string> data, object replacement)
        {
            this.grassType.Value = (byte)int.Parse(data["Type"]);
            this.numberOfWeeds.Value = int.Parse(data["WeedCount"]);
            this.loadSprite();
        }

        public override void loadSprite()
        {
            base.loadSprite();
            if (this.grassType.Value >= 5)
            {
                this.texture = new Lazy<Texture2D>(() => GrassStarterItem.Tex2);
                this.grassSourceOffset.Value = 20 * (this.grassType.Value - 5);
            }
        }

        // This is to fix a problem with the vanilla other grass types
        // It references `Game1.mine.mineLevel`, but `Game1.mine` is null.
        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location = null)
        {
            location ??= Game1.currentLocation;
            if (t is MeleeWeapon weapon && (weapon.type.Value != 2 || explosion > 0))
            {
                if (weapon.type.Value != 1)
                    DelayedAction.playSoundAfterDelay("daggerswipe", 50);
                else if (location.Equals((object)Game1.currentLocation))
                    Game1.playSound("swordswipe");
                this.shake(3f * (float)Math.PI / 32f, (float)Math.PI / 40f, Game1.random.NextDouble() < 0.5);
                this.numberOfWeeds.Value = this.numberOfWeeds.Value - (explosion <= 0 ? 1 : Math.Max(1, explosion + 2 - Game1.recentMultiplayerRandom.Next(2)));
                Color color = Color.Green;
                switch (this.grassType.Value)
                {
                    case 1:
                        switch (Game1.currentSeason)
                        {
                            case "spring":
                                color = new Color(60, 180, 58);
                                break;

                            case "summer":
                                color = new Color(110, 190, 24);
                                break;

                            case "fall":
                                color = new Color(219, 102, 58);
                                break;
                        }
                        break;
                    case 2:
                        color = new Color(148, 146, 71);
                        break;
                    case 3:
                        color = new Color(216, 240, byte.MaxValue);
                        break;
                    case 4:
                        color = new Color(165, 93, 58);
                        break;
                }
                location.temporarySprites.Add(new TemporaryAnimatedSprite(28, tileLocation * Game1.tileSize + new Vector2(Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4), Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4)), color, 8, Game1.random.NextDouble() < 0.5, Game1.random.Next(60, 100)));
                if (this.numberOfWeeds.Value <= 0)
                {
                    if (this.grassType.Value != 1)
                    {
                        Random random = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)(Game1.uniqueIDForThisGame + tileLocation.X * 1000.0 + tileLocation.Y * 11.0 + /*(double)Game1.mine.mineLevel +*/ Game1.player.timesReachedMineBottom));
                        if (random.NextDouble() < 0.005)
                            Game1.createObjectDebris(114, (int)tileLocation.X, (int)tileLocation.Y);
                        else if (random.NextDouble() < 0.01)
                            Game1.createDebris(4, (int)tileLocation.X, (int)tileLocation.Y, random.Next(1, 2), null);
                        else if (random.NextDouble() < 0.02)
                            Game1.createDebris(92, (int)tileLocation.X, (int)tileLocation.Y, random.Next(2, 4), null);
                    }
                    else if (t is MeleeWeapon && (t.Name.Contains("Scythe") || t.ParentSheetIndex == 47) && ((Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)(Game1.uniqueIDForThisGame + tileLocation.X * 1000.0 + tileLocation.Y * 11.0))).NextDouble() < 0.5 && (Game1.getLocationFromName("Farm") as Farm).tryToAddHay(1) == 0))
                    {
                        t.getLastFarmerToUse().currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheetName, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, t.getLastFarmerToUse().position - new Vector2(0.0f, Game1.tileSize * 2), false, false, t.getLastFarmerToUse().position.Y / 10000f, 0.005f, Color.White, Game1.pixelZoom, -0.005f, 0.0f, 0.0f)
                        {
                            motion = {
                Y = -1f
              },
                            layerDepth = (float)(1.0 - Game1.random.Next(100) / 10000.0),
                            delayBeforeAnimationStart = Game1.random.Next(350)
                        });
                        Game1.addHUDMessage(new HUDMessage("Hay", 1, true, Color.LightGoldenrodYellow, new StardewValley.Object(178, 1)));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
