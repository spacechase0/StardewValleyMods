using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreGrassStarters
{
    public class CustomGrass : Grass, ISaveElement
    {
        public CustomGrass() { }

        public CustomGrass(int which, int numberOfWeeds)
        :   base( which, numberOfWeeds )
        {
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            var data = new Dictionary<string, string>();
            data[ "Type" ] = ((int)grassType).ToString();
            data[ "WeedCount"] = ((int)numberOfWeeds).ToString();
            return data;
        }

        public object getReplacement()
        {
            return new FruitTree(); // new Grass(Grass.springGrass, numberOfWeeds);
        }

        public void rebuild(Dictionary<string, string> data, object replacement)
        {
            grassType = (byte)int.Parse(data["Type"]);
            numberOfWeeds = int.Parse(data["WeedCount"]);
            loadSprite();
        }

        public override void loadSprite()
        {
            base.loadSprite();
            if ( grassType >= 5 )
            {
                texture = GrassStarterItem.tex2;
                grassSourceOffset = 20 * (grassType - 5);
            }
        }

        // This is to fix a problem with the vanilla other grass types
        // It references `Game1.mine.mineLevel`, but `Game1.mine` is null.
        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;
            if (t != null && t is MeleeWeapon && ((MeleeWeapon)t).type != 2 || explosion > 0)
            {
                if (t != null && (t as MeleeWeapon).type != 1)
                    DelayedAction.playSoundAfterDelay("daggerswipe", 50);
                else if (location.Equals((object)Game1.currentLocation))
                    Game1.playSound("swordswipe");
                this.shake(3f * (float)Math.PI / 32f, (float)Math.PI / 40f, Game1.random.NextDouble() < 0.5);
                this.numberOfWeeds = this.numberOfWeeds - (explosion <= 0 ? 1 : Math.Max(1, explosion + 2 - Game1.recentMultiplayerRandom.Next(2)));
                Color color = Color.Green;
                switch (this.grassType)
                {
                    case 1:
                        string currentSeason = Game1.currentSeason;
                        if (!(currentSeason == "spring"))
                        {
                            if (!(currentSeason == "summer"))
                            {
                                if (currentSeason == "fall")
                                {
                                    color = new Color(219, 102, 58);
                                    break;
                                }
                                break;
                            }
                            color = new Color(110, 190, 24);
                            break;
                        }
                        color = new Color(60, 180, 58);
                        break;
                    case 2:
                        color = new Color(148, 146, 71);
                        break;
                    case 3:
                        color = new Color(216, 240, (int)byte.MaxValue);
                        break;
                    case 4:
                        color = new Color(165, 93, 58);
                        break;
                }
                location.temporarySprites.Add(new TemporaryAnimatedSprite(28, tileLocation * (float)Game1.tileSize + new Vector2((float)Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4), (float)Game1.random.Next(-Game1.pixelZoom * 4, Game1.pixelZoom * 4)), color, 8, Game1.random.NextDouble() < 0.5, (float)Game1.random.Next(60, 100), 0, -1, -1f, -1, 0));
                if (this.numberOfWeeds <= 0)
                {
                    if ((int)this.grassType != 1)
                    {
                        Random random = Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)((double)Game1.uniqueIDForThisGame + (double)tileLocation.X * 1000.0 + (double)tileLocation.Y * 11.0 + /*(double)Game1.mine.mineLevel +*/ (double)Game1.player.timesReachedMineBottom));
                        if (random.NextDouble() < 0.005)
                            Game1.createObjectDebris(114, (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, (GameLocation)null);
                        else if (random.NextDouble() < 0.01)
                            Game1.createDebris(4, (int)tileLocation.X, (int)tileLocation.Y, random.Next(1, 2), (GameLocation)null);
                        else if (random.NextDouble() < 0.02)
                            Game1.createDebris(92, (int)tileLocation.X, (int)tileLocation.Y, random.Next(2, 4), (GameLocation)null);
                    }
                    else if (t is MeleeWeapon && (t.Name.Contains("Scythe") || t.parentSheetIndex == 47) && ((Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : new Random((int)((double)Game1.uniqueIDForThisGame + (double)tileLocation.X * 1000.0 + (double)tileLocation.Y * 11.0))).NextDouble() < 0.5 && (Game1.getLocationFromName("Farm") as Farm).tryToAddHay(1) == 0))
                    {
                        t.getLastFarmerToUse().currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, t.getLastFarmerToUse().position - new Vector2(0.0f, (float)(Game1.tileSize * 2)), false, false, t.getLastFarmerToUse().position.Y / 10000f, 0.005f, Color.White, (float)Game1.pixelZoom, -0.005f, 0.0f, 0.0f, false)
                        {
                            motion = {
                Y = -1f
              },
                            layerDepth = (float)(1.0 - (double)Game1.random.Next(100) / 10000.0),
                            delayBeforeAnimationStart = Game1.random.Next(350)
                        });
                        Game1.addHUDMessage(new HUDMessage("Hay", 1, true, Color.LightGoldenrodYellow, (Item)new StardewValley.Object(178, 1, false, -1, 0)));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
