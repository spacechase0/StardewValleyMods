using System;
using System.Linq;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGACrop")]
    public partial class CustomCrop : Crop
    {
        public CropPackData.PhaseData GetCurrentPhase()
        {
            return this.Data.Phases.Count > this.currentPhase.Value ? this.Data.Phases[this.currentPhase.Value] : new CropPackData.PhaseData();
        }

        partial void DoInit()
        {
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        partial void DoInit(CropPackData data)
        {
            if ((data.Colors?.Count ?? 0) > 0)
            {
                this.tintColor.Value = data.Colors[Game1.random.Next(data.Colors.Count)];
            }

            this.ResetPhaseDays();
            this.harvestMethod.Value = this.Data.Phases[0].Scythable ? Crop.sickleHarvest : Crop.grabHarvest;
            this.raisedSeeds.Value = this.Data.Phases[0].Trellis;
        }

        public CustomCrop(CropPackData data, int tileX, int tileY)
            : this(data)
        {
            this.updateDrawMath(new Vector2(tileX, tileY));
        }

        public override void ResetPhaseDays()
        {
            this.phaseDays.Clear();
            this.phaseDays.AddRange(this.Data.Phases.Select(p => p.Length));
        }

        public bool Harvest(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null)
        {
            var Game1_multiplayer = (Multiplayer)typeof(Game1).GetField("multiplayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            var this_tilePosition = Mod.instance.Helper.Reflection.GetField<Vector2>(this, "tilePosition").GetValue();

            var currPhase = this.GetCurrentPhase();

            if ((bool)this.dead)
            {
                if (junimoHarvester != null)
                {
                    return true;
                }
                return false;
            }
            bool success = false;
            Random r = new Random(xTile * 7 + yTile * 11 + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
            /*if ( ( bool ) this.forageCrop )
            {
                StardewValley.Object o = null;
                int experience2 = 3;
                Random r2 = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + xTile * 1000 + yTile * 2000);
                switch ( ( int ) this.whichForageCrop )
                {
                    case 1:
                        o = new StardewValley.Object( 399, 1 );
                        break;
                    case 2:
                        soil.shake( ( float ) Math.PI / 48f, ( float ) Math.PI / 40f, ( float ) ( xTile * 64 ) < Game1.player.Position.X );
                        return false;
                }
                if ( Game1.player.professions.Contains( 16 ) )
                {
                    o.Quality = 4;
                }
                else if ( r2.NextDouble() < ( double ) ( ( float ) Game1.player.ForagingLevel / 30f ) )
                {
                    o.Quality = 2;
                }
                else if ( r2.NextDouble() < ( double ) ( ( float ) Game1.player.ForagingLevel / 15f ) )
                {
                    o.Quality = 1;
                }
                Game1.stats.ItemsForaged += ( uint ) o.Stack;
                if ( junimoHarvester != null )
                {
                    junimoHarvester.tryToAddItemToHut( o );
                    return true;
                }
                if ( Game1.player.addItemToInventoryBool( o ) )
                {
                    Vector2 initialTile2 = new Vector2(xTile, yTile);
                    Game1.player.animateOnce( 279 + Game1.player.FacingDirection );
                    Game1.player.canMove = false;
                    Game1.player.currentLocation.playSound( "harvest" );
                    DelayedAction.playSoundAfterDelay( "coin", 260 );
                    if ( ( int ) this.regrowAfterHarvest == -1 )
                    {
                        Game1_multiplayer.broadcastSprites( Game1.currentLocation, new TemporaryAnimatedSprite( 17, new Vector2( initialTile2.X * 64f, initialTile2.Y * 64f ), Color.White, 7, r2.NextDouble() < 0.5, 125f ) );
                        Game1_multiplayer.broadcastSprites( Game1.currentLocation, new TemporaryAnimatedSprite( 14, new Vector2( initialTile2.X * 64f, initialTile2.Y * 64f ), Color.White, 7, r2.NextDouble() < 0.5, 50f ) );
                    }
                    Game1.player.gainExperience( 2, experience2 );
                    return true;
                }
                Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:Crop.cs.588" ) );
            }
            else*/
            if (currPhase.HarvestedDrops.Count > 0)
            {
                foreach (var drop in currPhase.HarvestedDrops)
                {
                    int numToHarvest = 1;
                    int cropQuality = 0;
                    /*if ( ( int ) this.indexOfHarvest == 0 )
                    {
                        return true;
                    }*/
                    int fertilizerQualityLevel = (int) soil.fertilizer switch
                    {
                        368 => 1,
                        369 => 2,
                        919 => 3,
                        _ => 0
                    };
                    double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
                    double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
                    if (fertilizerQualityLevel >= 3 && r.NextDouble() < chanceForGoldQuality / 2.0)
                    {
                        cropQuality = 4;
                    }
                    else if (r.NextDouble() < chanceForGoldQuality)
                    {
                        cropQuality = 2;
                    }
                    else if (r.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
                    {
                        cropQuality = 1;
                    }
                    if ((int)drop.MininumHarvestedQuantity > 1 || (int)drop.MaximumHarvestedQuantity > 1)
                    {
                        int max_harvest_increase = 0;
                        if (this.maxHarvestIncreasePerFarmingLevel.Value > 0)
                        {
                            max_harvest_increase = Game1.player.FarmingLevel / (int)this.maxHarvestIncreasePerFarmingLevel;
                        }
                        numToHarvest = r.Next(drop.MininumHarvestedQuantity, Math.Max((int)drop.MininumHarvestedQuantity + 1, (int)drop.MaximumHarvestedQuantity + 1 + max_harvest_increase));
                    }
                    if ((double)drop.ExtraQuantityChance > 0.0)
                    {
                        while (r.NextDouble() < Math.Min(0.9, drop.ExtraQuantityChance))
                        {
                            numToHarvest++;
                        }
                    }
                    /*if ( ( int ) this.indexOfHarvest == 771 || ( int ) this.indexOfHarvest == 889 )
                    {
                        cropQuality = 0;
                    }*/
                    Item harvestedItem = drop.Item.Choose(r)?.Create();
                    if (harvestedItem != null)
                    {
                        if (harvestedItem is StardewValley.Object obj)
                            obj.Quality = cropQuality;
                        if (this.Data.Colors != null)
                        {
                            if (harvestedItem is StardewValley.Objects.ColoredObject colObj)
                                colObj.color.Value = this.tintColor.Value;
                            else if (harvestedItem is CustomObject cobj)
                                cobj.ObjectColor = this.tintColor.Value;
                        }
                    }
                    if ((int)this.harvestMethod == 1)
                    {
                        if (junimoHarvester != null)
                        {
                            DelayedAction.playSoundAfterDelay("daggerswipe", 150, junimoHarvester.currentLocation);
                        }
                        if (junimoHarvester != null && Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
                        {
                            junimoHarvester.currentLocation.playSound("harvest");
                        }
                        if (junimoHarvester != null && Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
                        {
                            DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
                        }
                        if (harvestedItem != null)
                        {
                            if (junimoHarvester != null)
                            {
                                junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
                            }
                            else
                            {
                                for (int i = 0; i < numToHarvest; ++i)
                                    Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                            }
                        }
                        success = true;
                    }
                    else
                    {
                        if (harvestedItem != null)
                        {
                            if (junimoHarvester == null)
                            {
                                for (; numToHarvest > 0 && Game1.player.addItemToInventoryBool(harvestedItem.getOne()); --numToHarvest) ;
                            }
                        }

                        if (junimoHarvester != null || numToHarvest == 0)
                        {
                            Vector2 initialTile = new Vector2(xTile, yTile);
                            if (junimoHarvester == null)
                            {
                                Game1.player.animateOnce(279 + Game1.player.FacingDirection);
                                Game1.player.canMove = false;
                            }
                            else
                            {
                                if (harvestedItem != null)
                                {
                                    for (int i = 0; i < numToHarvest; ++i)
                                        junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
                                }
                            }
                            if (r.NextDouble() < Game1.player.team.AverageLuckLevel() / 1500.0 + Game1.player.team.AverageDailyLuck() / 1200.0 + 9.9999997473787516E-05)
                            {
                                numToHarvest *= 2;
                                if (junimoHarvester == null)
                                {
                                    Game1.player.currentLocation.playSound("dwoop");
                                }
                                else if (Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
                                {
                                    junimoHarvester.currentLocation.playSound("dwoop");
                                }
                            }
                            else if ((int)this.harvestMethod == 0)
                            {
                                if (junimoHarvester == null)
                                {
                                    Game1.player.currentLocation.playSound("harvest");
                                }
                                else if (Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
                                {
                                    junimoHarvester.currentLocation.playSound("harvest");
                                }
                                if (junimoHarvester == null)
                                {
                                    DelayedAction.playSoundAfterDelay("coin", 260, Game1.player.currentLocation);
                                }
                                else if (Utility.isOnScreen(junimoHarvester.getTileLocationPoint(), 64, junimoHarvester.currentLocation))
                                {
                                    DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
                                }
                                if ((int)this.regrowAfterHarvest == -1 && (junimoHarvester == null || junimoHarvester.currentLocation.Equals(Game1.currentLocation)))
                                {
                                    Game1_multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, Game1.random.NextDouble() < 0.5, 125f));
                                    Game1_multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile.X * 64f, initialTile.Y * 64f), Color.White, 7, Game1.random.NextDouble() < 0.5, 50f));
                                }
                            }
                            success = true;
                        }
                        else if (numToHarvest > 0)
                        {
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                        }
                    }
                }

                if (success)
                {
                    /*int numToHarvest = 0;
                    if ( ( int ) this.indexOfHarvest == 421 )
                    {
                        this.indexOfHarvest.Value = 431;
                        numToHarvest = r.Next( 1, 4 );
                    }*/
                    //int price = Convert.ToInt32(Game1.objectInformation[this.indexOfHarvest].Split('/')[1]);
                    //harvestedItem = ( this.programColored ? new ColoredObject( this.indexOfHarvest, 1, this.tintColor ) : new StardewValley.Object( this.indexOfHarvest, 1 ) );
                    float experience = currPhase.HarvestedExperience; // (float)(16.0 * Math.Log(0.018 * (double)price + 1.0, Math.E));
                    if (junimoHarvester == null)
                    {
                        Game1.player.gainExperience(0, (int)Math.Round(experience));
                    }
                    /*for ( int i = 0; i < numToHarvest - 1; i++ )
                    {
                        if ( junimoHarvester == null )
                        {
                            Game1.createItemDebris( harvestedItem.getOne(), new Vector2( xTile * 64 + 32, yTile * 64 + 32 ), -1 );
                        }
                        else
                        {
                            junimoHarvester.tryToAddItemToHut( harvestedItem.getOne() );
                        }
                    }*/
                    /*
                    if ( ( int ) this.indexOfHarvest == 262 && r.NextDouble() < 0.4 )
                    {
                        StardewValley.Object hay_item = new StardewValley.Object(178, 1);
                        if ( junimoHarvester == null )
                        {
                            Game1.createItemDebris( hay_item.getOne(), new Vector2( xTile * 64 + 32, yTile * 64 + 32 ), -1 );
                        }
                        else
                        {
                            junimoHarvester.tryToAddItemToHut( hay_item.getOne() );
                        }
                    }
                    else if ( ( int ) this.indexOfHarvest == 771 )
                    {
                        Game1.player.currentLocation.playSound( "cut" );
                        if ( r.NextDouble() < 0.1 )
                        {
                            StardewValley.Object mixedSeeds_item = new StardewValley.Object(770, 1);
                            if ( junimoHarvester == null )
                            {
                                Game1.createItemDebris( mixedSeeds_item.getOne(), new Vector2( xTile * 64 + 32, yTile * 64 + 32 ), -1 );
                            }
                            else
                            {
                                junimoHarvester.tryToAddItemToHut( mixedSeeds_item.getOne() );
                            }
                        }
                    }
                    */
                    if (currPhase.HarvestedNewPhase == -1)
                    {
                        return true;
                    }
                    //this.fullyGrown.Value = true;
                    this.currentPhase.Value = currPhase.HarvestedNewPhase;
                    this.dayOfCurrentPhase.Value = 0;
                    //if ( this.dayOfCurrentPhase.Value == ( int ) this.regrowAfterHarvest )
                    {
                        this.updateDrawMath(this_tilePosition);
                    }
                    //this.dayOfCurrentPhase.Value = this.regrowAfterHarvest;
                }
            }
            return false;
        }

        public void NewDay(int state, int fertilizer, int xTile, int yTile, GameLocation environment)
        {
            var OneTimeRandom_GetDouble = Mod.instance.Helper.Reflection.GetMethod(AccessTools.TypeByName("StardewValley.OneTimeRandom"), "GetDouble");

            this.ResetPhaseDays();

            if ((bool)environment.isOutdoors && ((bool)this.dead || !this.Data.CanGrowNow || this.Data.Type == CropPackData.CropType.Indoors /*( !environment.SeedsIgnoreSeasonsHere() && !this.seasonsToGrowIn.Contains( environment.GetSeasonForLocation() ) ) || ( !environment.SeedsIgnoreSeasonsHere() && ( int ) this.indexOfHarvest == 90 )*/ ))
            {
                this.Kill();
                return;
            }
            if (state == 1 || (int)this.indexOfHarvest == 771)
            {
                if (!this.fullyGrown)
                {
                    this.dayOfCurrentPhase.Value = Math.Min((int)this.dayOfCurrentPhase + 1, (this.phaseDays.Count > 0) ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase)] : 0);
                }
                else
                {
                    this.dayOfCurrentPhase.Value--;
                }
                if ((int)this.dayOfCurrentPhase >= ((this.phaseDays.Count > 0) ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase)] : 0) && (int)this.currentPhase < this.phaseDays.Count - 1)
                {
                    this.currentPhase.Value++;
                    this.dayOfCurrentPhase.Value = 0;
                }
                while ((int)this.currentPhase < this.phaseDays.Count - 1 && this.phaseDays.Count > 0 && this.phaseDays[this.currentPhase] <= 0)
                {
                    this.currentPhase.Value++;
                }
                if ((int)this.rowInSpriteSheet == 23 && (int)this.phaseToShow == -1 && (int)this.currentPhase > 0)
                {
                    this.phaseToShow.Value = Game1.random.Next(1, 7);
                }

                this.harvestMethod.Value = this.GetCurrentPhase().Scythable ? Crop.sickleHarvest : Crop.grabHarvest;
                this.raisedSeeds.Value = this.GetCurrentPhase().Trellis;

                if (this.Data.GiantTextureChoices != null /*&& environment is Farm*/ && (int)this.currentPhase == this.phaseDays.Count - 1 /*&& ( ( int ) this.indexOfHarvest == 276 || ( int ) this.indexOfHarvest == 190 || ( int ) this.indexOfHarvest == 254 )*/ && OneTimeRandom_GetDouble.Invoke<double>(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (ulong)xTile, (ulong)yTile) < this.Data.GiantChance)
                {
                    for (int x2 = xTile - 1; x2 <= xTile + 1; x2++)
                    {
                        for (int y = yTile - 1; y <= yTile + 1; y++)
                        {
                            Vector2 v2 = new Vector2(x2, y);
                            if (!environment.terrainFeatures.ContainsKey(v2) || environment.terrainFeatures[v2] is not HoeDirt || (environment.terrainFeatures[v2] as HoeDirt).crop == null || ((environment.terrainFeatures[v2] as HoeDirt).crop as CustomCrop).FullId != this.FullId)
                            {
                                return;
                            }
                        }
                    }
                    for (int x = xTile - 1; x <= xTile + 1; x++)
                    {
                        for (int y2 = yTile - 1; y2 <= yTile + 1; y2++)
                        {
                            Vector2 v3 = new Vector2(x, y2);
                            (environment.terrainFeatures[v3] as HoeDirt).crop = null;
                        }
                    }
                    (environment /*as Farm*/).resourceClumps.Add(new CustomGiantCrop(this.Data, new Vector2(xTile - 1, yTile - 1)));
                }
            }
            /*if ( ( !this.fullyGrown || ( int ) this.dayOfCurrentPhase <= 0 ) && ( int ) this.currentPhase >= this.phaseDays.Count - 1 && ( int ) this.rowInSpriteSheet == 23 )
            {
                Vector2 v = new Vector2(xTile, yTile);
                string season = Game1.currentSeason;
                switch ( ( int ) this.whichForageCrop )
                {
                    case 495:
                        season = "spring";
                        break;
                    case 496:
                        season = "summer";
                        break;
                    case 497:
                        season = "fall";
                        break;
                    case 498:
                        season = "winter";
                        break;
                }
                if ( environment.objects.ContainsKey( v ) )
                {
                    if ( environment.objects[ v ] is IndoorPot )
                    {
                        ( environment.objects[ v ] as IndoorPot ).heldObject.Value = new Object( v, this.getRandomWildCropForSeason( season ), 1 );
                        ( environment.objects[ v ] as IndoorPot ).hoeDirt.Value.crop = null;
                    }
                    else
                    {
                        environment.objects.Remove( v );
                    }
                }
                if ( !environment.objects.ContainsKey( v ) )
                {
                    environment.objects.Add( v, new Object( v, this.getRandomWildCropForSeason( season ), 1 )
                    {
                        IsSpawnedObject = true,
                        CanBeGrabbed = true
                    } );
                }
                if ( environment.terrainFeatures.ContainsKey( v ) && environment.terrainFeatures[ v ] != null && environment.terrainFeatures[ v ] is HoeDirt )
                {
                    ( environment.terrainFeatures[ v ] as HoeDirt ).crop = null;
                }
            }*/
            this.updateDrawMath(new Vector2(xTile, yTile));
        }

        public override bool isPaddyCrop()
        {
            return this.Data.Type == CropPackData.CropType.Paddy;
        }

        public void Draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
        {
            var this_drawPosition = Mod.instance.Helper.Reflection.GetField<Vector2>(this, "drawPosition").GetValue();
            float this_layerDepth = Mod.instance.Helper.Reflection.GetField<float>(this, "layerDepth").GetValue();
            float this_coloredLayerDepth = Mod.instance.Helper.Reflection.GetField<float>(this, "coloredLayerDepth").GetValue();

            var currTex = this.Data.pack.GetMultiTexture(this.GetCurrentPhase().TextureChoices, ((int)tileLocation.X * 7 + (int)tileLocation.Y * 11), 16, 32);

            Vector2 position = Game1.GlobalToLocal(Game1.viewport, this_drawPosition);
            /*
            if ((bool)this.forageCrop)
            {
                if ((int)this.whichForageCrop == 2)
                {
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 64f)), new Rectangle(128 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(tileLocation.X * 111f + tileLocation.Y * 77f)) % 800.0 / 200.0) * 16, 128, 16, 16), Color.White, rotation, new Vector2(8f, 16f), 4f, SpriteEffects.None, (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f);
                }
                else
                {
                    b.Draw(Game1.mouseCursors, position, this.sourceRect, Color.White, 0f, Crop.smallestTileSizeOrigin, 4f, SpriteEffects.None, this.layerDepth);
                }
                return;
            }
            */

            SpriteEffects effect = this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (this.dead.Value)
            {
                b.Draw(Game1.cropSpriteSheet, position, new Rectangle(192 + ((int)tileLocation.X * 7 + (int)tileLocation.Y * 11) % 4 * 16, 384, 16, 32), toTint, rotation, new Vector2(8f, 24f), 4f, effect, this_layerDepth);
                return;
            }


            b.Draw(currTex.Texture, position, currTex.Rect, toTint, rotation, /*Crop.origin*/new Vector2(8f, 24f), 4f, effect, this_layerDepth);
            Color tintColor = this.tintColor.Value;
            if (this.GetCurrentPhase().TextureColorChoices != null)
            {
                var colorTex = this.Data.pack.GetMultiTexture(this.GetCurrentPhase().TextureColorChoices, ((int)tileLocation.X * 7 + (int)tileLocation.Y * 11), 16, 32); ;
                b.Draw(colorTex.Texture, position, colorTex.Rect, tintColor, rotation, new Vector2(8f, 24f), 4f, effect, this_coloredLayerDepth);
            }
        }

        public void DrawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
        {
            var currTex = this.Data.GetTexture();

            b.Draw(currTex.Texture, screenPosition, currTex.Rect, toTint, rotation, new Vector2(32f, 96f), scale, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        public void DrawWithOffset(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
        {
            var currTex = this.Data.GetTexture();
            /*
            if ( ( bool ) this.forageCrop )
            {
                b.Draw( Game1.mouseCursors, Game1.GlobalToLocal( Game1.viewport, offset + new Vector2( tileLocation.X * 64f, tileLocation.Y * 64f ) ), this.sourceRect, Color.White, 0f, new Vector2( 8f, 8f ), 4f, SpriteEffects.None, ( tileLocation.Y + 0.66f ) * 64f / 10000f + tileLocation.X * 1E-05f );
                return;
            }*/
            b.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), currTex.Rect, toTint, rotation, new Vector2(8f, 24f), 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
            if (!this.tintColor.Equals(Color.White) && (int)this.currentPhase == this.phaseDays.Count - 1 && !this.dead)
            {
                // TODO: Colored
                //b.Draw( Game1.cropSpriteSheet, Game1.GlobalToLocal( Game1.viewport, offset + new Vector2( tileLocation.X * 64f, tileLocation.Y * 64f ) ), this.coloredSourceRect, this.tintColor, rotation, new Vector2( 8f, 24f ), 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ( tileLocation.Y + 0.67f ) * 64f / 10000f + tileLocation.X * 1E-05f );
            }
        }
    }
}
