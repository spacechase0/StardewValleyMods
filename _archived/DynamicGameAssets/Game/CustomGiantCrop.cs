using System;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAGiantCrop")]
    public partial class CustomGiantCrop : ResourceClump
    {
        partial void DoInit()
        {
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        partial void DoInit(CropPackData data)
        {
            this.width.Value = 3;
            this.height.Value = 3;
            this.health.Value = 3;
        }

        public CustomGiantCrop(CropPackData data, Vector2 tile)
            : this(data)
        {
            this.tile.Value = tile;
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            var currTex = this.Data.pack.GetMultiTexture(this.Data.GiantTextureChoices, ((int)tileLocation.X * 7 + (int)tileLocation.Y * 11), 48, 63);

            spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2((this.shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)this.shakeTimer) * 2f) : 0f, 64f)), currTex.Rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + 2f) * 64f / 10000f);
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
        {
            var Game1_multiplayer = Mod.instance.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            if (t is not Axe)
            {
                return false;
            }
            location.playSound("axchop");
            int power = (int)t.upgradeLevel / 2 + 1;
            this.health.Value -= power;
            Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X + 1, (int)tileLocation.Y + 1, Game1.random.Next(4, 9), resource: false);
            foreach (var drop in this.Data.GiantDrops)
            {
                if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)((float)power / 5f))
                {
                    var item = drop.Item.Choose(Game1.random).Create();
                    Debris d = new Debris(item, new Vector2(tileLocation.X * 64f + 96f, (tileLocation.Y + 0.5f) * 64f), new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()));
                    d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
                    d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 128f);
                    location.debris.Add(d);
                }
            }
            if (this.shakeTimer <= 0f)
            {
                this.shakeTimer = 100f;
                this.NeedsUpdate = true;
            }
            if ((float)this.health <= 0f)
            {
                t.getLastFarmerToUse().gainExperience(5, 50 * (((int)t.getLastFarmerToUse().luckLevel + 1) / 2));
                if (location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()))
                {
                    StardewValley.Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
                    if (o != null)
                    {
                        Game1.createItemDebris(o, tileLocation * 64f, -1, location);
                    }
                }
                Random r;
                if (Game1.IsMultiplayer)
                {
                    Game1.recentMultiplayerRandom = new Random((int)tileLocation.X * 1000 + (int)tileLocation.Y);
                    r = Game1.recentMultiplayerRandom;
                }
                else
                {
                    r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
                }
                foreach (var drop in this.Data.GiantDrops)
                {
                    int numChunks = r.Next(drop.MininumHarvestedQuantity, drop.MaximumHarvestedQuantity + 1);
                    while (r.NextDouble() <= Math.Min(0.9, drop.ExtraQuantityChance))
                        ++numChunks;
                    if (Game1.IsMultiplayer)
                    {
                        for (int i = 0; i < numChunks; ++i)
                            location.debris.Add(new Debris(drop.Item.Choose(r).Create(), new Vector2((tileLocation.X + 1) * 64 + 32, (tileLocation.Y + 1) * 64 + 32), Game1.getFarmer(t.getLastFarmerToUse().UniqueMultiplayerID).getStandingPosition()));
                        //Game1.createMultipleObjectDebris( base.parentSheetIndex, ( int ) tileLocation.X + 1, ( int ) tileLocation.Y + 1, numChunks, t.getLastFarmerToUse().UniqueMultiplayerID, location );
                    }
                    else
                    {
                        CustomGiantCrop.Game1_createRadialDebris(location, drop.Item.Choose(r).Create(), (int)tileLocation.X, (int)tileLocation.Y, numChunks, resource: false, -1, item: true);
                    }
                }
                //Object tmp = new Object(Vector2.Zero, base.parentSheetIndex, 1);
                //Game1.setRichPresence( "giantcrop", tmp.Name );
                Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), resource: false);
                location.playSound("stumpCrack");
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f, Color.White));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 0f)) * 64f, Color.White, 8, flipped: false, 110f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 1f)) * 64f, Color.White, 8, flipped: true, 80f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(0f, 1f)) * 64f, Color.White, 8, flipped: false, 90f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f + new Vector2(32f, 32f), Color.White, 8, flipped: false, 70f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f, Color.White));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(2f, 0f)) * 64f, Color.White, 8, flipped: false, 110f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(2f, 1f)) * 64f, Color.White, 8, flipped: true, 80f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(2f, 2f)) * 64f, Color.White, 8, flipped: false, 90f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, tileLocation * 64f + new Vector2(96f, 96f), Color.White, 8, flipped: false, 70f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(0f, 2f)) * 64f, Color.White, 8, flipped: false, 110f));
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 2f)) * 64f, Color.White, 8, flipped: true, 80f));
                return true;
            }
            return false;
        }

        private static void Game1_createRadialDebris(GameLocation location, Item debrisType, int xTile, int yTile, int numberOfChunks, bool resource, int groundLevel = -1, bool item = false, int color = -1)
        {
            if (groundLevel == -1)
            {
                groundLevel = yTile * 64 + 32;
            }
            Vector2 debrisOrigin = new Vector2(xTile * 64 + 64, yTile * 64 + 64);
            if (item)
            {
                while (numberOfChunks > 0)
                {
                    switch (Game1.random.Next(4))
                    {
                        case 0:
                            location.debris.Add(new Debris(debrisType.getOne(), debrisOrigin, debrisOrigin + new Vector2(-64f, 0f)));
                            break;
                        case 1:
                            location.debris.Add(new Debris(debrisType.getOne(), debrisOrigin, debrisOrigin + new Vector2(64f, 0f)));
                            break;
                        case 2:
                            location.debris.Add(new Debris(debrisType.getOne(), debrisOrigin, debrisOrigin + new Vector2(0f, 64f)));
                            break;
                        case 3:
                            location.debris.Add(new Debris(debrisType.getOne(), debrisOrigin, debrisOrigin + new Vector2(0f, -64f)));
                            break;
                    }
                    numberOfChunks--;
                }
            }/*
            if ( resource )
            {
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( -64f, 0f ) ) );
                numberOfChunks++;
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( 64f, 0f ) ) );
                numberOfChunks++;
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( 0f, -64f ) ) );
                numberOfChunks++;
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( 0f, 64f ) ) );
            }
            else
            {
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( -64f, 0f ), groundLevel, color ) );
                numberOfChunks++;
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( 64f, 0f ), groundLevel, color ) );
                numberOfChunks++;
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( 0f, -64f ), groundLevel, color ) );
                numberOfChunks++;
                location.debris.Add( new Debris( debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2( 0f, 64f ), groundLevel, color ) );
            }*/
        }
    }
}
