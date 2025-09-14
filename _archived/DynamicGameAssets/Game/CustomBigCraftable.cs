using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGABigCraftable")]
    public partial class CustomBigCraftable : StardewValley.Object
    {
        /// <summary>The backing field for <see cref="TextureOverride"/>.</summary>
        private readonly NetString NetTextureOverride = new();

        /// <summary>The backing field for <see cref="PendingTextureOverride"/>.</summary>
        private readonly NetString NetPendingTextureOverride = new();

        /// <summary>The backing field for <see cref="PulseIfWorking"/>.</summary>
        private readonly NetBool NetPulseIfWorking = new();

        public string TextureOverride
        {
            get => this.NetTextureOverride.Value;
            set => this.NetTextureOverride.Value = value;
        }
        public string PendingTextureOverride // Triggers when recipe is finished
        {
            get => this.NetPendingTextureOverride.Value;
            set => this.NetPendingTextureOverride.Value = value;
        }
        public bool PulseIfWorking
        {
            get => this.NetPulseIfWorking.Value;
            set => this.NetPulseIfWorking.Value = value;
        }

        public override string DisplayName { get => this.loadDisplayName(); set { } }

        public CustomBigCraftable(BigCraftablePackData data, Vector2 tileLocation)
            : this(data)
        {
            this.tileLocation.Value = tileLocation;
            this.boundingBox.Value = new Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.X * Game1.tileSize, Game1.tileSize, Game1.tileSize);
        }

        partial void DoInit(BigCraftablePackData data)
        {
            this.name = data.ID;

            this.canBeSetDown.Value = true;
            this.canBeGrabbed.Value = true;
            this.bigCraftable.Value = true;
            this.price.Value = data.SellPrice ?? 0;
            this.edibility.Value = StardewValley.Object.inedible;
            this.type.Value = "Crafting";
            this.Category = -9;
            this.setOutdoors.Value = this.setIndoors.Value = true;
            this.fragility.Value = StardewValley.Object.fragility_Removable;
            this.isLamp.Value = data.ProvidesLight;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
            this.NetFields.AddFields(this.NetTextureOverride, this.NetPendingTextureOverride, this.NetPulseIfWorking);
        }

        protected override string loadDisplayName()
        {
            return this.Data.Name;
        }

        public override bool minutesElapsed(int minutes, GameLocation environment)
        {
            if (Game1.IsMasterGame)
                this.MinutesUntilReady -= minutes;

            if (this.minutesUntilReady.Value <= 0)
            {
                if (!this.readyForHarvest.Value)
                {
                    environment.playSound("dwop");
                }
                this.readyForHarvest.Value = true;
                this.minutesUntilReady.Value = 0;
                this.onReadyForHarvest(environment);

                this.TextureOverride = null;
                if (this.lightSource != null)
                {
                    if (!this.Data.ProvidesLight)
                    {
                        environment.removeLightSource(this.lightSource.identifier);
                        this.lightSource = null;
                    }
                }
            }

            return false;
        }

        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
        {
            if (this.isTemporarilyInvisible)
            {
                return false;
            }

            if (this.heldObject.Value != null)
                return false;

            if (!Mod.customMachineRecipes.ContainsKey(this.FullId))
                return false;

            IList<Item> items = who.items;
            if (StardewValley.Object.autoLoadChest != null)
                items = StardewValley.Object.autoLoadChest.items;

            // TODO: This could be optimized I'm pretty sure
            foreach (var recipe in Mod.customMachineRecipes[this.FullId])
            {
                /*
                if ( recipe.liveConditionsObj != null )
                {
                    recipe.liveConditionsObj.UpdateContext();
                    if ( !recipe.liveConditionsObj.IsMatch )
                        continue;
                }
                */
                if (!recipe.Ingredients[0].Matches(dropInItem))
                    continue;

                bool hadIngredients = true;
                ItemAbstraction missingIngred = null;
                foreach (var ingred in recipe.Ingredients)
                {
                    int left = ingred.Quantity;
                    foreach (var item in items)
                    {
                        if (!ingred.Matches(item))
                            continue;
                        left -= item.Stack;
                        if (left <= 0)
                            break;
                    }

                    if (left > 0)
                    {
                        hadIngredients = false;
                        missingIngred = ingred;
                        break;
                    }
                }

                string missingIngredientName = null;
                if (missingIngred != null)
                {
                    if (missingIngred.Type != ItemAbstraction.ItemType.ContextTag)
                    {
                        missingIngredientName = missingIngred.Create().DisplayName;
                    }
                }

                if (hadIngredients)
                {
                    if (probe)
                        return true;
                    else
                    {
                        foreach (var ingred in recipe.Ingredients)
                        {
                            int left = ingred.Quantity;
                            for (int i = 0; i < items.Count; ++i)
                            {
                                if (!ingred.Matches(items[i]))
                                    continue;

                                if (items[i].Stack <= left)
                                {
                                    left -= items[i].Stack;
                                    items[i] = null;
                                }
                                else
                                {
                                    items[i].Stack -= left;
                                    left = 0;
                                }

                                if (left <= 0)
                                    break;
                            }
                        }

                        this.TextureOverride = recipe.MachineWorkingTextureOverride;
                        this.PendingTextureOverride = recipe.MachineFinishedTextureOverride;
                        this.PulseIfWorking = recipe.MachinePulseWhileWorking;

                        this.heldObject.Value = (StardewValley.Object)recipe.Result.Choose().Create();

                        this.MinutesUntilReady = recipe.MinutesToProcess;

                        if (recipe.StartWorkingSound != null)
                            who.currentLocation.playSound(recipe.StartWorkingSound);

                        if (recipe.WorkingLightOverride.HasValue)
                        {
                            bool oldIsLamp = this.isLamp.Value;
                            this.isLamp.Value = recipe.WorkingLightOverride.Value;
                            if (!oldIsLamp && this.isLamp.Value)
                                this.initializeLightSource(this.tileLocation.Value);
                            else if (oldIsLamp && !this.isLamp.Value)
                                who.currentLocation.removeLightSource((int)(this.tileLocation.X * 797f + this.tileLocation.Y * 13f + 666f));
                        }

                        return false;
                    }
                }
                else
                {
                    if (!probe && StardewValley.Object.autoLoadChest == null)
                    {
                        if (missingIngredientName != null)
                        {
                            Game1.showRedMessage(I18n.NotEnoughIngredients(item: missingIngredientName));
                        }
                        else
                        {
                            Game1.showRedMessage(I18n.NotEnoughIngredientsGeneric());
                        }
                    }
                    return false;
                }
            }

            if (!probe && StardewValley.Object.autoLoadChest == null)
                Game1.showRedMessage(I18n.WrongIngredientSelected(item: dropInItem.DisplayName));
            return false;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            var who = t.getLastFarmerToUse();
            if (t is Pickaxe or Axe)
            {
                this.performRemoveAction(this.tileLocation.Value, location);
                Game1.currentLocation.debris.Add(new Debris(this.getOne(), who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
                Game1.currentLocation.objects.Remove(this.tileLocation.Value);
                return false;
            }

            return base.performToolAction(t, location);
        }

        public string GetCurrentTexture()
        {
            if (this.heldObject.Value != null && this.MinutesUntilReady == 0)
            {
                return this.PendingTextureOverride ?? this.Data.Texture;
            }

            return this.TextureOverride ?? this.Data.Texture;
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
            ret.Y += 48;
            return ret;
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            var tex = this.Data.pack.GetTexture(this.GetCurrentTexture(), 16, 32);

            spriteBatch.Draw(tex.Texture, objectPosition, tex.Rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if ((bool)this.isRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            var tex = this.Data.pack.GetTexture(this.GetCurrentTexture(), 16, 32);

            Rectangle sourceRect = tex.Rect ?? new Rectangle(0, 0, tex.Texture.Width, tex.Texture.Height);
            spriteBatch.Draw(tex.Texture, location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
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

            var tex = this.Data.pack.GetTexture(this.GetCurrentTexture(), 16, 32);
            Vector2 scaleFactor = this.PulseIfWorking ? this.getScale() : Vector2.One;
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            b.Draw(destinationRectangle: new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f)), texture: tex.Texture, sourceRectangle: tex.Rect, color: Color.White, rotation: 0f, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f) + (((int)this.parentSheetIndex == 105 || (int)this.parentSheetIndex == 264) ? 0.0015f : 0f));
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (this.isTemporarilyInvisible)
                return;

            var tex = this.Data.pack.GetTexture(this.GetCurrentTexture(), 16, 32);

            Vector2 scaleFactor = this.PulseIfWorking ? this.getScale() : Vector2.One;
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw(tex.Texture, destination, tex.Rect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
            if ((bool)this.isLamp && Game1.isDarkOut())
            {
                spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x / 1000000f);
            }
            if (!this.readyForHarvest)
            {
                return;
            }

            float base_sort = (float)((y + 1) * 64) / 10000f + this.tileLocation.X / 50000f;
            float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort + 1E-06f);
            if (this.heldObject.Value != null)
            {
                if (this.heldObject.Value is CustomObject custObj)
                {
                    Vector2 custObjPosition = new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset);
                    custObj.drawWhenProduced(spriteBatch, custObjPosition, base_sort + 1E-05f);
                }
                else
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.heldObject.Value.parentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1E-05f);
                    if (this.heldObject.Value is ColoredObject)
                    {
                        spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)this.heldObject.Value.parentSheetIndex + 1, 16, 16), (this.heldObject.Value as ColoredObject).color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1.1E-05f);
                    }
                }
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            if (this.isTemporarilyInvisible)
                return;

            var tex = this.Data.pack.GetTexture(this.GetCurrentTexture(), 16, 32);

            Vector2 scaleFactor = this.PulseIfWorking ? this.getScale() : Vector2.One;
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw(tex.Texture, destination, tex.Rect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
            if ((bool)this.isLamp && Game1.isDarkOut())
            {
                spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
            }
        }

        public override Item getOne()
        {
            var ret = new CustomBigCraftable(this.Data, Vector2.Zero);
            // TODO: All the other fields objects does??
            ret.Stack = 1;
            ret.Price = this.Price;
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool canStackWith(ISalable other)
        {
            if (other is not CustomBigCraftable obj)
                return false;

            return obj.FullId == this.FullId && base.canStackWith(other);
        }

        public override string getDescription()
        {
            return this.Data.Description;
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
    }
}
