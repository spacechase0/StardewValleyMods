using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAFruitTree")]
    public partial class CustomFruitTree : FruitTree
    {
        /// <summary>The backing field for <see cref="GrownFruits"/>.</summary>
        public readonly NetObjectList<Item> NetGrownFruits = new();

        public IList<Item> GrownFruits => this.NetGrownFruits;

        partial void DoInit()
        {
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
            this.NetFields.AddFields(this.NetGrownFruits);

            this.treeType.Value = 0;
        }

        partial void DoInit(FruitTreePackData data)
        {
            this.daysUntilMature.Value = 28;
        }

        public void Shake(Vector2 tileLocation, bool doEvenIfStillShaking, GameLocation location)
        {
            var this_maxShake = Mod.instance.Helper.Reflection.GetField<float>(this, "maxShake");
            var this_leaves = Mod.instance.Helper.Reflection.GetField<List<Leaf>>(this, "leaves").GetValue();

            if ((this_maxShake.GetValue() == 0f || doEvenIfStillShaking) && (int)this.growthStage >= 3 && !this.stump)
            {
                this.shakeLeft.Value = ((float)Game1.player.getStandingX() > (tileLocation.X + 0.5f) * 64f || ((Game1.player.getTileLocation().X == tileLocation.X && Game1.random.NextDouble() < 0.5) ? true : false));
                this_maxShake.SetValue((float)(((int)this.growthStage >= 4) ? (Math.PI / 128.0) : (Math.PI / 64.0)));
                if ((int)this.growthStage >= 4)
                {
                    if (Game1.random.NextDouble() < 0.66 && this.Data.CanGrowNow)
                    {
                        int numberOfLeaves2 = Game1.random.Next(1, 6);
                        for (int k = 0; k < numberOfLeaves2; k++)
                        {
                            this_leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), Game1.random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
                        }
                    }
                    int fruitquality = 0;
                    if ((int)this.daysUntilMature <= -112)
                    {
                        fruitquality = 1;
                    }
                    if ((int)this.daysUntilMature <= -224)
                    {
                        fruitquality = 2;
                    }
                    if ((int)this.daysUntilMature <= -336)
                    {
                        fruitquality = 4;
                    }
                    if ((int)this.struckByLightningCountdown > 0)
                    {
                        fruitquality = 0;
                    }
                    if (!location.terrainFeatures.ContainsKey(tileLocation) || !location.terrainFeatures[tileLocation].Equals(this))
                    {
                        return;
                    }
                    for (int j = 0; j < (int)this.GrownFruits.Count; j++)
                    {
                        Vector2 offset = new Vector2(0f, 0f);
                        switch (j)
                        {
                            case 0:
                                offset.X = -64f;
                                break;
                            case 1:
                                offset.X = 64f;
                                offset.Y = -32f;
                                break;
                            case 2:
                                offset.Y = 32f;
                                break;
                        }
                        Debris d = new Debris(((int)this.struckByLightningCountdown > 0) ? new StardewValley.Object(382, 1) : this.GrownFruits[j].getOne(), new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()))
                        {
                            itemQuality = fruitquality
                        };
                        d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
                        d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
                        location.debris.Add(d);
                    }
                    this.GrownFruits.Clear();
                }
                else if (Game1.random.NextDouble() < 0.66 && this.Data.CanGrowNow)
                {
                    int numberOfLeaves = Game1.random.Next(1, 3);
                    for (int i = 0; i < numberOfLeaves; i++)
                    {
                        this_leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 96f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
                    }
                }
            }
        }

        public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
        {
            this.treeType.Value = 0;
            var this_destroy = Mod.instance.Helper.Reflection.GetField<bool>(this, "destroy");
            var Game1_multiplayer = Mod.instance.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            if ((float)this.health <= -99f)
            {
                this_destroy.SetValue(true);
            }
            if ((int)this.struckByLightningCountdown > 0)
            {
                this.struckByLightningCountdown.Value--;
                if ((int)this.struckByLightningCountdown <= 0)
                {
                    this.GrownFruits.Clear();
                }
            }
            bool foundSomething = FruitTree.IsGrowthBlocked(tileLocation, environment);
            if (!foundSomething || (int)this.daysUntilMature <= 0)
            {
                if ((int)this.daysUntilMature > 28)
                {
                    this.daysUntilMature.Value = 28;
                }
                this.daysUntilMature.Value--;
                if ((int)this.daysUntilMature <= 0)
                {
                    this.growthStage.Value = 4;
                }
                else if ((int)this.daysUntilMature <= 7)
                {
                    this.growthStage.Value = 3;
                }
                else if ((int)this.daysUntilMature <= 14)
                {
                    this.growthStage.Value = 2;
                }
                else if ((int)this.daysUntilMature <= 21)
                {
                    this.growthStage.Value = 1;
                }
                else
                {
                    this.growthStage.Value = 0;
                }
            }
            else if (foundSomething && this.growthStage.Value != 4)
            {
                Game1_multiplayer.broadcastGlobalMessage("Strings\\UI:FruitTree_Warning", true, this.Data.Product[0].Value.Create().DisplayName);
            }
            if (!this.stump && (int)this.growthStage == 4 && (((int)this.struckByLightningCountdown > 0 && !Game1.IsWinter) || this.IsInSeasonHere(environment) || environment.SeedsIgnoreSeasonsHere()))
            {
                if (this.GrownFruits.Count < 3)
                {
                    var product = this.Data.Product.Choose()?.Create();
                    if (product != null)
                        this.GrownFruits.Add(product);
                }
                if (environment.IsGreenhouse)
                {
                    this.greenHouseTree.Value = true;
                }
            }
            if ((bool)this.stump)
            {
                this.GrownFruits.Clear();
            }
        }

        public override bool IsInSeasonHere(GameLocation location)
        {
            return this.Data.CanGrowNow;
        }

        public override bool seasonUpdate(bool onLoad)
        {
            if (!this.IsInSeasonHere(this.currentLocation) && !onLoad && !this.greenHouseTree)
            {
                this.GrownFruits.Clear();
            }
            return false;
        }

        public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation, GameLocation location)
        {
            var this_lastPlayerToHit = Mod.instance.Helper.Reflection.GetField<NetLong>(this, "lastPlayerToHit").GetValue();
            var this_falling = Mod.instance.Helper.Reflection.GetField<NetBool>(this, "falling").GetValue();
            var Game1_multiplayer = Mod.instance.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            if ((float)this.health <= -99f)
            {
                return false;
            }
            if (t is MeleeWeapon)
            {
                return false;
            }
            if ((int)this.growthStage >= 4)
            {
                if (t is Axe)
                {
                    location.playSound("axchop");
                    location.debris.Add(new Debris(12, Game1.random.Next((int)t.upgradeLevel * 2, (int)t.upgradeLevel * 4), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), t.getLastFarmerToUse().Position, 0));
                    this_lastPlayerToHit.Value = t.getLastFarmerToUse().UniqueMultiplayerID;
                    int fruitquality = 0;
                    if ((int)this.daysUntilMature <= -112)
                    {
                        fruitquality = 1;
                    }
                    if ((int)this.daysUntilMature <= -224)
                    {
                        fruitquality = 2;
                    }
                    if ((int)this.daysUntilMature <= -336)
                    {
                        fruitquality = 4;
                    }
                    if ((int)this.struckByLightningCountdown > 0)
                    {
                        fruitquality = 0;
                    }
                    if (location.terrainFeatures.ContainsKey(tileLocation) && location.terrainFeatures[tileLocation].Equals(this))
                    {
                        for (int i = 0; i < (int)this.GrownFruits.Count; i++)
                        {
                            Vector2 offset = new Vector2(0f, 0f);
                            switch (i)
                            {
                                case 0:
                                    offset.X = -64f;
                                    break;
                                case 1:
                                    offset.X = 64f;
                                    offset.Y = -32f;
                                    break;
                                case 2:
                                    offset.Y = 32f;
                                    break;
                            }
                            Debris d2 = new Debris(((int)this.struckByLightningCountdown > 0) ? new StardewValley.Object(382, 1) : this.GrownFruits[i].getOne(), new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 3f) * 64f + 32f) + offset, new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()))
                            {
                                itemQuality = fruitquality
                            };
                            d2.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
                            d2.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
                            location.debris.Add(d2);
                        }
                        this.GrownFruits.Clear();
                    }
                }
                else if (explosion <= 0)
                {
                    return false;
                }
                this.shake(tileLocation, doEvenIfStillShaking: true, location);
                float damage2;
                if (explosion > 0)
                {
                    damage2 = explosion;
                }
                else
                {
                    if (t == null)
                    {
                        return false;
                    }
                    damage2 = (int)t.upgradeLevel switch
                    {
                        0 => 1f,
                        1 => 1.25f,
                        2 => 1.67f,
                        3 => 2.5f,
                        4 => 5f,
                        _ => (int)t.upgradeLevel + 1,
                    };
                }
                this.health.Value -= damage2;
                if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(damage2 / 5f))
                {
                    Debris d = new Debris(388, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()));
                    d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
                    d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
                    location.debris.Add(d);
                }
                if ((float)this.health <= 0f)
                {
                    if (!this.stump)
                    {
                        location.playSound("treecrack");
                        this.stump.Value = true;
                        this.health.Value = 5f;
                        this_falling.Value = true;
                        if (t == null || t.getLastFarmerToUse() == null)
                        {
                            this.shakeLeft.Value = true;
                        }
                        else
                        {
                            this.shakeLeft.Value = ((float)t.getLastFarmerToUse().getStandingX() > (tileLocation.X + 0.5f) * 64f);
                        }
                    }
                    else
                    {
                        this.health.Value = -100f;
                        Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(30, 40), resource: false);
                        int whatToDrop = 92;
                        if (Game1.IsMultiplayer)
                        {
                            Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 2000 + (int)tileLocation.Y);
                            _ = Game1.recentMultiplayerRandom;
                        }
                        else
                        {
                            new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
                        }
                        if (t == null || t.getLastFarmerToUse() == null)
                        {
                            Game1.createMultipleObjectDebris(92, (int)tileLocation.X, (int)tileLocation.Y, 2, location);
                        }
                        else if (Game1.IsMultiplayer)
                        {
                            Game1.createMultipleObjectDebris(whatToDrop, (int)tileLocation.X, (int)tileLocation.Y, 1, this_lastPlayerToHit, location);
                            Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.getFarmer(this_lastPlayerToHit).professions.Contains(12) ? 5 : 4, resource: true);
                        }
                        else
                        {
                            Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((Game1.getFarmer(this_lastPlayerToHit).professions.Contains(12) ? 1.25 : 1.0) * 5.0), resource: true);
                            Game1.createMultipleObjectDebris(whatToDrop, (int)tileLocation.X, (int)tileLocation.Y, 1, location);
                        }
                    }
                }
            }
            else if ((int)this.growthStage >= 3)
            {
                if (t != null && t.BaseName.Contains("Ax"))
                {
                    location.playSound("axchop");
                    location.playSound("leafrustle");
                    location.debris.Add(new Debris(12, Game1.random.Next((int)t.upgradeLevel * 2, (int)t.upgradeLevel * 4), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), new Vector2(t.getLastFarmerToUse().GetBoundingBox().Center.X, t.getLastFarmerToUse().GetBoundingBox().Center.Y), 0));
                }
                else if (explosion <= 0)
                {
                    return false;
                }
                this.shake(tileLocation, doEvenIfStillShaking: true, location);
                float damage;
                Random debrisRandom = (!Game1.IsMultiplayer) ? new Random((int)((float)Game1.uniqueIDForThisGame + tileLocation.X * 7f + tileLocation.Y * 11f + (float)Game1.stats.DaysPlayed + (float)this.health)) : Game1.recentMultiplayerRandom;
                if (explosion > 0)
                {
                    damage = explosion;
                }
                else
                {
                    damage = (int)t.upgradeLevel switch
                    {
                        0 => 2f,
                        1 => 2.5f,
                        2 => 3.34f,
                        3 => 5f,
                        4 => 10f,
                        _ => 1f
                    };
                }
                int debris = 0;
                while (t != null && debrisRandom.NextDouble() < (double)damage * 0.08 + (double)((float)t.getLastFarmerToUse().ForagingLevel / 200f))
                {
                    debris++;
                }
                this.health.Value -= damage;
                if (debris > 0)
                {
                    Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, debris, location);
                }
                if ((float)this.health <= 0f)
                {
                    Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), resource: false);
                    return true;
                }
            }
            else if ((int)this.growthStage >= 1)
            {
                if (explosion > 0)
                {
                    return true;
                }
                if (t != null && t.BaseName.Contains("Axe"))
                {
                    location.playSound("axchop");
                    Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
                }
                if (t is Axe or Pickaxe or Hoe or MeleeWeapon)
                {
                    Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
                    if (t.BaseName.Contains("Axe") && Game1.recentMultiplayerRandom.NextDouble() < (double)((float)t.getLastFarmerToUse().ForagingLevel / 10f))
                    {
                        Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, 1, location);
                    }
                    Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
                    return true;
                }
            }
            else
            {
                if (explosion > 0)
                {
                    return true;
                }
                if (t.BaseName.Contains("Axe") || t.BaseName.Contains("Pick") || t.BaseName.Contains("Hoe"))
                {
                    location.playSound("woodyHit");
                    location.playSound("axchop");
                    Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
                    return true;
                }
            }
            return false;
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            var this_falling = Mod.instance.Helper.Reflection.GetField<NetBool>(this, "falling").GetValue();
            float this_shakeTimer = Mod.instance.Helper.Reflection.GetField<float>(this, "shakeTimer").GetValue();
            float this_shakeRotation = Mod.instance.Helper.Reflection.GetField<float>(this, "shakeRotation").GetValue();
            var this_leaves = Mod.instance.Helper.Reflection.GetField<List<Leaf>>(this, "leaves").GetValue();
            float this_alpha = Mod.instance.Helper.Reflection.GetField<float>(this, "alpha").GetValue();

            var currTex = this.Data.GetTexture();

            string season = Game1.GetSeasonForLocation(this.currentLocation);
            if ((bool)this.greenHouseTileTree)
            {
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(669, 1957, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
            }
            if ((int)this.growthStage < 4)
            {
                Vector2 positionOffset = new Vector2((float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)(tileLocation.X * 200f) / (Math.PI * 2.0)) * -16.0)), (float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)(tileLocation.X * 200f) / (Math.PI * 2.0)) * -16.0))) / 2f;
                Rectangle sourceRect = Rectangle.Empty;
                sourceRect = (int)this.growthStage switch
                {
                    0 => new Rectangle(0, (int)this.treeType * 5 * 16, 48, 80),
                    1 => new Rectangle(48, (int)this.treeType * 5 * 16, 48, 80),
                    2 => new Rectangle(96, (int)this.treeType * 5 * 16, 48, 80),
                    _ => new Rectangle(144, (int)this.treeType * 5 * 16, 48, 80),
                };
                sourceRect = new Rectangle((currTex.Rect?.X ?? 0) + sourceRect.X, (currTex.Rect?.Y ?? 0) + sourceRect.Y, sourceRect.Width, sourceRect.Height);
                spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f + positionOffset.X, tileLocation.Y * 64f - (float)sourceRect.Height + 128f + positionOffset.Y)), sourceRect, Color.White, this_shakeRotation, new Vector2(24f, 80f), 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)this.getBoundingBox(tileLocation).Bottom / 10000f - tileLocation.X / 1000000f);
            }
            else
            {
                if (!this.stump || (bool)this_falling)
                {
                    if (!this_falling)
                    {
                        spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Rectangle((12 + (this.greenHouseTree ? 1 : Utility.getSeasonNumber(season)) * 3) * 16, (int)this.treeType * 5 * 16 + 64, 48, 16), ((int)this.struckByLightningCountdown > 0) ? (Color.Gray * this_alpha) : (Color.White * this_alpha), 0f, new Vector2(24f, 16f), 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-07f);
                    }
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Rectangle((12 + (this.greenHouseTree ? 1 : Utility.getSeasonNumber(season)) * 3) * 16, (int)this.treeType * 5 * 16, 48, 64), ((int)this.struckByLightningCountdown > 0) ? (Color.Gray * this_alpha) : (Color.White * this_alpha), this_shakeRotation, new Vector2(24f, 80f), 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)this.getBoundingBox(tileLocation).Bottom / 10000f + 0.001f - tileLocation.X / 1000000f);
                }
                if ((float)this.health >= 1f || (!this_falling && (float)this.health > -99f))
                {
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f + ((this_shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)this_shakeTimer) * 2f) : 0f), tileLocation.Y * 64f + 64f)), new Rectangle(384, (int)this.treeType * 5 * 16 + 48, 48, 32), ((int)this.struckByLightningCountdown > 0) ? (Color.Gray * this_alpha) : (Color.White * this_alpha), 0f, new Vector2(24f, 32f), 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((bool)this.stump && !this_falling) ? ((float)this.getBoundingBox(tileLocation).Bottom / 10000f) : ((float)this.getBoundingBox(tileLocation).Bottom / 10000f - 0.001f - tileLocation.X / 1000000f));
                }

                this.drawFruits(spriteBatch, tileLocation, this_alpha);
            }
            foreach (Leaf j in this_leaves)
            {
                spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, j.position), new Rectangle((24 + Utility.getSeasonNumber(season)) * 16, (int)this.treeType * 5 * 16, 8, 8), Color.White, j.rotation, Vector2.Zero, 4f, SpriteEffects.None, (float)this.getBoundingBox(tileLocation).Bottom / 10000f + 0.01f);
            }
        }

        private void drawFruits(SpriteBatch spriteBatch, Vector2 tileLocation, float alpha)
        {
            alpha = 1;
            Vector2 p = Vector2.Zero;
            for (int i = 0; i < (int)this.GrownFruits.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        p = /*Game1.GlobalToLocal*/( /*Game1.viewport,*/ new Vector2(tileLocation.X * 64 - 64 + tileLocation.X * 200f % 64 / 2, tileLocation.Y * 64 - 192 - tileLocation.X % 64 / 3));
                        (this.GrownFruits[i] as StardewValley.Object).draw(spriteBatch, (int)p.X, (int)p.Y, (float)this.getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f, alpha);
                        //spriteBatch.Draw( Game1.objectSpriteSheet, Game1.GlobalToLocal( Game1.viewport, new Vector2( tileLocation.X * 64f - 64f + tileLocation.X * 200f % 64f / 2f, tileLocation.Y * 64f - 192f - tileLocation.X % 64f / 3f ) ), Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, ( ( int ) this.struckByLightningCountdown > 0 ) ? 382 : ( ( int ) 382 ), 16, 16 ), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, ( float ) this.getBoundingBox( tileLocation ).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f );
                        break;
                    case 1:
                        p = /*Game1.GlobalToLocal*/( /*Game1.viewport,*/ new Vector2(tileLocation.X * 64 + 32, tileLocation.Y * 64 - 256 + tileLocation.X % 232 % 64 / 3));
                        (this.GrownFruits[i] as StardewValley.Object).draw(spriteBatch, (int)p.X, (int)p.Y, (float)this.getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f, alpha);
                        //spriteBatch.Draw( Game1.objectSpriteSheet, Game1.GlobalToLocal( Game1.viewport, new Vector2( tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 256f + tileLocation.X * 232f % 64f / 3f ) ), Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, ( ( int ) this.struckByLightningCountdown > 0 ) ? 382 : ( ( int ) 382 ), 16, 16 ), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, ( float ) this.getBoundingBox( tileLocation ).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f );
                        break;
                    case 2:
                        p = /*Game1.GlobalToLocal*/( /*Game1.viewport,*/ new Vector2(tileLocation.X * 64 + tileLocation.X * 200f % 64f / 3f, tileLocation.Y * 64f - 160f + tileLocation.X * 200f % 64f / 3f));
                        (this.GrownFruits[i] as StardewValley.Object).draw(spriteBatch, (int)p.X, (int)p.Y, (float)this.getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f, alpha);
                        //spriteBatch.Draw( Game1.objectSpriteSheet, Game1.GlobalToLocal( Game1.viewport, new Vector2( tileLocation.X * 64f + tileLocation.X * 200f % 64f / 3f, tileLocation.Y * 64f - 160f + tileLocation.X * 200f % 64f / 3f ) ), Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, ( ( int ) this.struckByLightningCountdown > 0 ) ? 382 : ( ( int ) 382 ), 16, 16 ), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, ( float ) this.getBoundingBox( tileLocation ).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f );
                        break;
                }
            }
        }
    }
}
