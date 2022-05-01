using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAObject")]
    public partial class CustomObject : SObject
    {
        public readonly NetBool NetHasColor = new();
        public readonly NetColor NetColor = new();

        [XmlIgnore]
        public Color? ObjectColor
        {
            get => this.NetHasColor.Value
                ? this.NetColor.Value
                : null;
            set
            {
                if (value == null)
                    this.NetHasColor.Value = false;
                else
                {
                    this.NetHasColor.Value = true;
                    this.NetColor.Value = value.Value;
                }
            }
        }

        public override string DisplayName
        {
            get => this.loadDisplayName();
            set { }
        }

        partial void DoInit(ObjectPackData data)
        {
            this.ParentSheetIndex = Mod.BaseFakeObjectId;
            this.name = data.ID;
            this.edibility.Value = data.Edibility;
            this.type.Value = "Basic";
            this.category.Value = (int)data.Category;
            this.price.Value = data.SellPrice ?? 0;
            this.fragility.Value = SObject.fragility_Removable;

            this.canBeSetDown.Value = true;
            this.canBeGrabbed.Value = true;
            this.isHoedirt.Value = false;
            this.isSpawnedObject.Value = false;
            this.boundingBox.Value = new Rectangle(0, 0, 64, 64);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
            this.NetFields.AddFields(this.NetHasColor, this.NetColor);
        }

        protected override string loadDisplayName()
        {
            return this.Data.Name;
        }

        public override bool canBeShipped()
        {
            return this.Data.SellPrice.HasValue;
        }

        public override bool canBeTrashed()
        {
            return this.Data.CanTrash;
        }

        public override bool canBeGivenAsGift()
        {
            return this.Data.IsGiftable;
        }

        public override string[] ModifyItemBuffs(string[] buffs)
        {
            var buffData = this.Data.EdibleBuffs ?? new ObjectPackData.FoodBuffsData();
            string[] buffStr = new[]
            {
                buffData.Farming.ToString(),
                buffData.Fishing.ToString(),
                buffData.Mining.ToString(),
                "0",
                buffData.Luck.ToString(),
                buffData.Foraging.ToString(),
                "0",
                buffData.MaxStamina.ToString(),
                buffData.MagnetRadius.ToString(),
                buffData.Speed.ToString(),
                buffData.Defense.ToString(),
                buffData.Attack.ToString()
            };
            for (int i = 0; i < Math.Min(buffStr.Length, buffs.Length); ++i)
            {
                buffs[i] = buffStr[i];
            }

            return base.ModifyItemBuffs(buffs);
        }

        public override string getCategoryName()
        {
            return this.Data.CategoryTextOverride ?? base.getCategoryName();
        }

        public override Color getCategoryColor()
        {
            if (this.Data.CategoryColorOverride.HasValue)
                return this.Data.CategoryColorOverride.Value;
            return base.getCategoryColor();
        }

        public override int healthRecoveredOnConsumption()
        {
            if (this.Data.EatenHealthRestoredOverride.HasValue)
                return this.Data.EatenHealthRestoredOverride.Value;
            return base.healthRecoveredOnConsumption();
        }

        public override int staminaRecoveredOnConsumption()
        {
            if (this.Data.EatenStaminaRestoredOverride.HasValue)
                return this.Data.EatenStaminaRestoredOverride.Value;
            return base.staminaRecoveredOnConsumption();
        }

        public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
        {
            base.drawTooltip(spriteBatch, ref x, ref y, font, alpha, overrideText);
            string str = I18n.ItemTooltip_AddedByMod(this.Data.pack.smapiPack.Manifest.Name);
            Utility.drawTextWithShadow(spriteBatch, Game1.parseText(str, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), new Color(100, 100, 100));
            y += (int)font.MeasureString(Game1.parseText(str, Game1.smallFont, this.getDescriptionWidth())).Y + 10;
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
        {
            var ret = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom);
            ret.Y = startingHeight;
            string str = I18n.ItemTooltip_AddedByMod(this.Data.pack.smapiPack.Manifest.Name);
            ret.Y += (int)font.MeasureString(Game1.parseText(str, Game1.smallFont, this.getDescriptionWidth())).Y + 10;
            return ret;
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
            spriteBatch.Draw(tex.Texture, objectPosition, tex.Rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
            if (this.NetHasColor.Value)
            {
                var colorTex = this.Data.pack.GetTexture(this.Data.TextureColor, 16, 16);
                spriteBatch.Draw(colorTex.Texture, objectPosition, colorTex.Rect, this.ObjectColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f + 2e-05f));
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if ((bool)this.isRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            if ((int)this.parentSheetIndex != 590 && drawShadow)
            {
                spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            }
            var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
            spriteBatch.Draw(tex.Texture, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), tex.Rect, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
            if (this.NetHasColor.Value)
            {
                var colorTex = this.Data.pack.GetTexture(this.Data.TextureColor, 16, 16);
                spriteBatch.Draw(colorTex.Texture, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), colorTex.Rect, this.ObjectColor.Value * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth + 2e-05f);
            }
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 1f), 3f * scaleSize, 1f, color);
            }
            if (drawStackNumber != 0 && (int)this.quality > 0)
            {
                Rectangle quality_rect = ((int)this.quality < 4) ? new Rectangle(338 + ((int)this.quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8);
                Texture2D quality_sheet = Game1.mouseCursors;
                float yOffset = ((int)this.quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f);
                spriteBatch.Draw(quality_sheet, location + new Vector2(12f, 52f + yOffset), quality_rect, color * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + yOffset), SpriteEffects.None, layerDepth);
            }
        }

        public override void drawAsProp(SpriteBatch b)
        {
            if (this.isTemporarilyInvisible)
            {
                return;
            }
            int x = (int)this.tileLocation.X;
            int y = (int)this.tileLocation.Y;

            b.Draw(Game1.shadowTexture, this.getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)this.getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
            var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
            Texture2D objectSpriteSheet = tex.Texture;
            Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32));
            Rectangle? sourceRectangle = tex.Rect;// GameLocation.getSourceRectForObject(base.ParentSheetIndex);
            Color white = Color.White;
            Vector2 origin = new Vector2(8f, 8f);
            _ = this.scale;
            b.Draw(objectSpriteSheet, position2, sourceRectangle, white, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)this.getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
            if (this.NetHasColor.Value)
            {
                var colorTex = this.Data.pack.GetTexture(this.Data.TextureColor, 16, 16);
                b.Draw(colorTex.Texture, position2, colorTex.Rect, this.ObjectColor.Value, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)this.getBoundingBox(new Vector2(x, y)).Bottom / 10000f + 2e-05f);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (this.isTemporarilyInvisible)
                return;

            if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(x, y))
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)this.getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
                var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
                Texture2D objectSpriteSheet = tex.Texture;
                Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
                Rectangle? sourceRectangle2 = tex.Rect; // GameLocation.getSourceRectForObject(base.ParentSheetIndex);
                Color color2 = Color.White * alpha;
                Vector2 origin2 = new Vector2(8f, 8f);
                _ = this.scale;
                spriteBatch.Draw(objectSpriteSheet, position3, sourceRectangle2, color2, 0f, origin2, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? this.getBoundingBox(new Vector2(x, y)).Top : this.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
                if (this.NetHasColor.Value)
                {
                    var colorTex = this.Data.pack.GetTexture(this.Data.TextureColor, 16, 16);
                    spriteBatch.Draw(colorTex.Texture, position3, colorTex.Rect, this.ObjectColor.Value, 0f, origin2, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? this.getBoundingBox(new Vector2(x, y)).Top : this.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f + 2e-05f);
                }
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            if (this.isTemporarilyInvisible)
                return;

            if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(xNonTile / 64, yNonTile / 64))
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32, yNonTile + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth - 1E-06f);
                var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
                Texture2D objectSpriteSheet = tex.Texture;
                Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), yNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
                Rectangle? sourceRectangle = tex.Rect; // GameLocation.getSourceRectForObject(base.ParentSheetIndex);
                Color color = Color.White * alpha;
                Vector2 origin = new Vector2(8f, 8f);
                _ = this.scale;
                spriteBatch.Draw(objectSpriteSheet, position2, sourceRectangle, color, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
                if (this.NetHasColor.Value)
                {
                    var colorTex = this.Data.pack.GetTexture(this.Data.TextureColor, 16, 16);
                    spriteBatch.Draw(colorTex.Texture, position2, colorTex.Rect, this.ObjectColor.Value, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 2e-05f);
                }
            }
        }

        public void drawWithoutShadow(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            if (this.isTemporarilyInvisible)
                return;

            if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(xNonTile / 64, yNonTile / 64))
            {
                var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
                Texture2D objectSpriteSheet = tex.Texture;
                Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), yNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
                Rectangle? sourceRectangle = tex.Rect; // GameLocation.getSourceRectForObject(base.ParentSheetIndex);
                Color color = Color.White * alpha;
                Vector2 origin = new Vector2(8f, 8f);
                _ = this.scale;
                spriteBatch.Draw(objectSpriteSheet, position2, sourceRectangle, color, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
                if (this.NetHasColor.Value)
                {
                    var colorTex = this.Data.pack.GetTexture(this.Data.TextureColor, 16, 16);
                    spriteBatch.Draw(colorTex.Texture, position2, colorTex.Rect, this.ObjectColor.Value, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 2e-05f);
                }
            }
        }

        public void drawWhenProduced(SpriteBatch spriteBatch, Vector2 position, float layerDepth, float alpha = 1)
        {
            if (this.isTemporarilyInvisible)
                return;

            var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
            Texture2D objectSpriteSheet = tex.Texture;
            Rectangle? sourceRectangle = tex.Rect;
            spriteBatch.Draw(objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), sourceRectangle, Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            var ret = new CustomObject(this.Data);
            // TODO: All the other fields objects does??
            ret.Quality = this.Quality;
            ret.Stack = 1;
            ret.Price = this.Price;
            ret.ObjectColor = this.ObjectColor;
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool canStackWith(ISalable other)
        {
            if (other is not CustomObject obj)
                return false;

            return obj.FullId == this.FullId && base.canStackWith(other);
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            Vector2 nonTile = tile * 64f * 64f;
            nonTile.X += 32f;
            nonTile.Y += 32f;
            foreach (Furniture f in l.furniture)
            {
                if ((int)f.furniture_type.Value == 11 && f.getBoundingBox(f.TileLocation).Contains((int)nonTile.X, (int)nonTile.Y) && f.heldObject.Value == null)
                {
                    return true;
                }
                if (f.getBoundingBox(f.TileLocation).Intersects(new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64)) && !f.isPassable() && !f.AllowPlacementOnThisTile((int)tile.X, (int)tile.Y))
                {
                    return false;
                }
            }
            return this.isPlaceable() && !l.isTileOccupiedForPlacement(tile, this);
        }

        public override bool isPlaceable()
        {
            return this.Data.Placeable || !string.IsNullOrEmpty(this.Data.Plants);
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (t == null)
                return false;

            if (t is not MeleeWeapon && t.isHeavyHitter())
            {
                location.playSound("hammer");
                location.debris.Add(new Debris(this, this.TileLocation * Game1.tileSize + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)));
                return true;
            }

            return false;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);

            if (!string.IsNullOrEmpty(this.Data.Plants))
            {
                var data = Mod.Find(this.Data.Plants);
                if (data is CropPackData cropData && location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is HoeDirt)
                {
                    if (this.CanPlantThisSeedHere(((HoeDirt)location.terrainFeatures[placementTile]), (int)placementTile.X, (int)placementTile.Y, who.ActiveObject.Category == -19))
                    {
                        if (this.Plant(((HoeDirt)location.terrainFeatures[placementTile]), (int)placementTile.X, (int)placementTile.Y, who, who.ActiveObject.Category == -19, location) && who.IsLocalPlayer)
                        {
                            if (this.Category == -74)
                            {
                                foreach (SObject o in location.Objects.Values)
                                {
                                    if (!o.IsSprinkler() || o.heldObject.Value is not { ParentSheetIndex: 913 } || !o.IsInSprinklerRangeBroadphase(placementTile) || !o.GetSprinklerTiles().Contains(placementTile))
                                    {
                                        continue;
                                    }
                                    Chest chest2 = o.heldObject.Value.heldObject.Value as Chest;
                                    if (chest2 == null || chest2.items.Count <= 0 || chest2.items[0] == null || chest2.GetMutex().IsLocked())
                                    {
                                        continue;
                                    }
                                    chest2.GetMutex().RequestLock(delegate
                                   {
                                       if (chest2.items.Count > 0 && chest2.items[0] != null)
                                       {
                                           Item item = chest2.items[0];
                                           if (item.Category == -19 && ((HoeDirt)location.terrainFeatures[placementTile]).plant(item.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who, isFertilizer: true, location))
                                           {
                                               item.Stack--;
                                               if (item.Stack <= 0)
                                               {
                                                   chest2.items[0] = null;
                                               }
                                           }
                                       }
                                       chest2.GetMutex().ReleaseLock();
                                   });
                                    break;
                                }
                            }
                            Game1.haltAfterCheck = false;
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (data is FruitTreePackData ftreeData && !location.terrainFeatures.ContainsKey(placementTile))
                {
                    Vector2 v2 = default(Vector2);
                    for (int i = x / 64 - 2; i <= x / 64 + 2; i++)
                    {
                        for (int k = y / 64 - 2; k <= y / 64 + 2; k++)
                        {
                            v2.X = i;
                            v2.Y = k;
                            if (location.terrainFeatures.ContainsKey(v2) && (location.terrainFeatures[v2] is Tree || location.terrainFeatures[v2] is FruitTree))
                            {
                                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"));
                                return false;
                            }
                        }
                    }

                    if (FruitTree.IsGrowthBlocked(new Vector2(x / 64, y / 64), location))
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", this.DisplayName));
                        return false;
                    }
                    if (location.terrainFeatures.ContainsKey(placementTile))
                    {
                        if ((location.terrainFeatures[placementTile] as HoeDirt)?.crop != null)
                        {
                            return false;
                        }
                        location.terrainFeatures.Remove(placementTile);
                    }
                    if ((location is Farm && (location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Grass") || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Dirt")) && !location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "NoSpawn", "Back").Equals("Tree")) || (location.CanPlantTreesHere(this.parentSheetIndex, (int)placementTile.X, (int)placementTile.Y) && (location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Stone"))))
                    {
                        location.playSound("dirtyHit");
                        DelayedAction.playSoundAfterDelay("coin", 100);
                        bool actAsGreenhouse = location.IsGreenhouse || (((int)this.parentSheetIndex == 69 || (int)this.parentSheetIndex == 835) && location is IslandWest);
                        location.terrainFeatures.Add(placementTile, new CustomFruitTree(ftreeData)
                        {
                            GreenHouseTree = actAsGreenhouse,
                            GreenHouseTileTree = location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Stone")
                        });
                        return true;
                    }
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068"));
                    return false;
                }
                return false;
            }

            return base.placementAction(location, x, y, who);
        }

        public override bool IsSprinkler()
        {
            return this.Data.SprinklerTiles?.Count > 0;
        }

        public override int GetBaseRadiusForSprinkler()
        {
            if (this.Data.SprinklerTiles != null)
                return 1;
            else return -1;
        }

        public override List<Vector2> GetSprinklerTiles()
        {
            var tiles = this.Data.SprinklerTiles;
            if (this.Data.UpgradedSprinklerTiles != null && this.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex(this.heldObject.Value, 915))
                tiles = this.Data.UpgradedSprinklerTiles;

            var ret = new List<Vector2>();
            foreach (var tile in tiles)
                ret.Add(this.tileLocation.Value + tile);

            return ret;
        }

        public override bool needsToBeDonated()
        {
            return false;
        }

        public override string getDescription()
        {
            return Game1.parseText(this.Data.Description, Game1.smallFont, this.getDescriptionWidth());
        }

        public override int salePrice()
        {
            return this.Data.ForcePriceOnAllInstances ? (this.Data.SellPrice ?? 0) : this.Price;
        }

        public override int sellToStorePrice(long specificPlayerID = -1)
        {
            float price = this.salePrice() * (1 + this.Quality * 0.25f);
            price = Mod.instance.Helper.Reflection.GetMethod(this, "getPriceAfterMultipliers").Invoke<float>(price, specificPlayerID);

            if (price > 0)
                price = Math.Max(1, price * Game1.MasterPlayer.difficultyModifier);

            return (int)price;
        }

        public bool CanPlantThisSeedHere(HoeDirt this_, int tileX, int tileY, bool isFertilizer = false)
        {
            /*if ( isFertilizer )
            {
                if ( ( int ) this.fertilizer == 0 )
                {
                    return true;
                }
            }
            else */
            if (this_.crop == null)
            {
                CustomCrop c = new CustomCrop(Mod.Find(this.Data.Plants) as CropPackData, tileX, tileY);
                /*if ( c.seasonsToGrowIn.Count == 0 )
                {
                    return false;
                }*/
                if (!Game1.currentLocation.IsOutdoors || Game1.currentLocation.IsGreenhouse || Game1.currentLocation.SeedsIgnoreSeasonsHere() || c.Data.CanGrowNow /*c.seasonsToGrowIn.Contains( Game1.currentLocation.GetSeasonForLocation() )*/ )
                {
                    if ((bool)c.raisedSeeds && Utility.doesRectangleIntersectTile(Game1.player.GetBoundingBox(), tileX, tileY))
                    {
                        return false;
                    }
                    return true;
                }
                /*
                if ( objectIndex == 309 || objectIndex == 310 || objectIndex == 311 )
                {
                    return true;
                }
                */
                if (Game1.didPlayerJustClickAtAll() && !Game1.doesHUDMessageExist(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924")))
                {
                    Game1.playSound("cancel");
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
                }
            }
            return false;
        }

        public bool Plant(HoeDirt this_, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
        {
            var this_applySpeedIncreases = Mod.instance.Helper.Reflection.GetMethod(this_, "applySpeedIncreases");
            /*
            if ( isFertilizer )
            {
                if ( this.crop != null && ( int ) this.crop.currentPhase != 0 && ( index == 368 || index == 369 ) )
                {
                    return false;
                }
                if ( ( int ) this.fertilizer != 0 )
                {
                    return false;
                }
                this.fertilizer.Value = index;
                this.applySpeedIncreases( who );
                location.playSound( "dirtyHit" );
                return true;
            }*/
            CustomCrop c = new CustomCrop(Mod.Find(this.Data.Plants) as CropPackData, tileX, tileY);
            /*if ( c.seasonsToGrowIn.Count == 0 )
            {
                return false;
            }
            */
            if (!who.currentLocation.isFarm && !who.currentLocation.IsGreenhouse && !who.currentLocation.CanPlantSeedsHere(this.FullId.GetDeterministicHashCode(), tileX, tileY) && who.currentLocation.IsOutdoors)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13919"));
                return false;
            }
            if (!who.currentLocation.isOutdoors || who.currentLocation.IsGreenhouse || c.Data.CanGrowNow/*c.seasonsToGrowIn.Contains( location.GetSeasonForLocation() )*/ || who.currentLocation.SeedsIgnoreSeasonsHere())
            {
                this_.crop = c;
                if ((bool)c.raisedSeeds)
                {
                    location.playSound("stoneStep");
                }
                location.playSound("dirtyHit");
                Game1.stats.SeedsSown++;

                this_applySpeedIncreases.Invoke(who);
                this_.nearWaterForPaddy.Value = -1;
                if (this_.hasPaddyCrop() && this_.paddyWaterCheck(location, new Vector2(tileX, tileY)))
                {
                    this_.state.Value = 1;
                    this_.updateNeighbors(location, new Vector2(tileX, tileY));
                }
                return true;
            }
            //if ( c.seasonsToGrowIn.Count > 0 && !c.seasonsToGrowIn.Contains( location.GetSeasonForLocation() ) )
            if (c.Data.CanGrowNow)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
            }
            else
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13925"));
            }
            return false;
        }

        protected override void _PopulateContextTags(HashSet<string> tags)
        {
            base._PopulateContextTags(tags);
            foreach (string tag in this.Data.ContextTags)
                tags.Add(tag);
        }
    }
}
