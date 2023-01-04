using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Displays.Framework
{
    [XmlType("Mods_spacechase0_Mannequin")]
    public class Mannequin : SObject // must be public for the XML serializer
    {
        /*********
        ** Fields
        *********/
        private static readonly Texture2D Tex = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/mannequin-base.png");
        private static readonly Texture2D TexM = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/mannequin-male.png");
        private static readonly Texture2D TexF = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/mannequin-female.png");

        /// <summary>The backing field for <see cref="GetFarmerForRendering"/>.</summary>
        private Farmer FarmerForRenderingCache;


        /*********
        ** Accessors
        *********/
        public readonly NetEnum<MannequinType> MannType = new(MannequinType.Plain);
        public readonly NetEnum<MannequinGender> MannGender = new(MannequinGender.Male);
        public readonly NetInt Facing = new(Game1.down);

        public readonly NetRef<Hat> Hat = new();
        public readonly NetRef<Clothing> Shirt = new();
        public readonly NetRef<Clothing> Pants = new();
        public readonly NetRef<Boots> Boots = new();

        public override string DisplayName
        {
            get => this.name;
            set { }
        }


        /*********
        ** Public methods
        *********/
        public Mannequin() { }

        public Mannequin(MannequinType type, MannequinGender gender, Vector2 placement)
        {
            this.MannType.Value = type;
            this.MannGender.Value = gender;
            this.name = this.loadDisplayName();
            this.DisplayName = this.loadDisplayName();
            this.bigCraftable.Value = true;
            this.Type = "Crafting"; // Makes performObjectDropInAction work for non-objects

            this.TileLocation = placement;
            this.boundingBox.Value = new Rectangle((int)placement.X * 64, (int)placement.Y * 64, 64, 64);
        }

        public override string getDescription()
        {
            return Mod.Instance.Helper.Translation.Get("mannequin.desc");
        }

        public override bool canStackWith(ISalable other)
        {
            return
                other is Mannequin m
                && m.MannType.Value == this.MannType.Value
                && m.MannGender.Value == this.MannGender.Value;
        }

        public override Item getOne()
        {
            var ret = new Mannequin(this.MannType.Value, this.MannGender.Value, Vector2.Zero);
            ret.Hat.Value = (Hat)this.Hat.Value?.getOne();
            ret.Shirt.Value = (Clothing)this.Shirt.Value?.getOne();
            ret.Pants.Value = (Clothing)this.Pants.Value?.getOne();
            ret.Boots.Value = (Boots)this.Boots.Value?.getOne();
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 placementTile = new Vector2(x / Game1.tileSize, y / Game1.tileSize);
            var m = new Mannequin(this.MannType.Value, this.MannGender.Value, placementTile);
            location.Objects.Add(placementTile, m);
            location.playSound("woodyStep");
            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (t == null)
                return false;

            if (t is not MeleeWeapon && t.isHeavyHitter())
            {
                if (this.Hat.Value != null || this.Shirt.Value != null || this.Pants.Value != null || this.Boots.Value != null)
                {
                    if (this.Hat.Value != null)
                    {
                        this.DropItem(location, this.Hat.Value);
                        this.Hat.Value = null;
                    }
                    else if (this.Shirt.Value != null)
                    {
                        this.DropItem(location, this.Shirt.Value);
                        this.Shirt.Value = null;
                    }
                    else if (this.Pants.Value != null)
                    {
                        this.DropItem(location, this.Pants.Value);
                        this.Pants.Value = null;
                    }
                    else if (this.Boots.Value != null)
                    {
                        this.DropItem(location, this.Boots.Value);
                        this.Boots.Value = null;
                    }
                    location.playSound("hammer");
                    this.shakeTimer = 100;
                    return false;
                }
                location.objects.Remove(this.TileLocation);
                this.DropItem(location, new Mannequin(this.MannType.Value, this.MannGender.Value, Vector2.Zero));
                return false;
            }

            return false;
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (who.CurrentItem == null)
            {
                if (justCheckingForActivity)
                    return true;

                this.Swap(this.Hat, who.hat, who);
                this.Swap(this.Shirt, who.shirtItem, who);
                this.Swap(this.Pants, who.pantsItem, who);
                this.Swap(this.Boots, who.boots, who);

                return true;
            }

            return false;
        }

        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
        {
            if (probe && (dropInItem is StardewValley.Objects.Hat or Clothing or StardewValley.Objects.Boots))
                return true;

            switch (dropInItem)
            {
                case Hat hat:
                    if (this.Hat.Value != null)
                        this.DropItem(who.currentLocation, this.Hat.Value);
                    this.Hat.Value = hat;
                    return true;

                case Clothing clothing:
                    switch (clothing.clothesType.Value)
                    {
                        case (int)Clothing.ClothesType.SHIRT:
                            if (this.Shirt.Value != null)
                                this.DropItem(who.currentLocation, this.Shirt.Value);
                            this.Shirt.Value = clothing;
                            return true;

                        case (int)Clothing.ClothesType.PANTS:
                            if (this.Pants.Value != null)
                                this.DropItem(who.currentLocation, this.Pants.Value);
                            this.Pants.Value = clothing;
                            return true;

                        default:
                            return false;
                    }

                case Boots boots:
                    if (this.Boots.Value != null)
                        this.DropItem(who.currentLocation, this.Boots.Value);
                    this.Boots.Value = boots;
                    return true;

                default:
                    return false;
            }
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            var tex = this.GetMainTexture();

            spriteBatch.Draw(tex, objectPosition, null, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (f.getStandingY() + 3) / 10000f));
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && scaleSize > 0.3 && this.Stack != int.MaxValue;

            var tex = this.GetMainTexture();

            spriteBatch.Draw(tex, location + new Vector2(32f, 32f), null, color * transparency, 0f, new Vector2(8f, 16f), 4f * (scaleSize < 0.2 ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(this.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(this.Stack, 3f * scaleSize) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            var tex = this.GetMainTexture();

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= Game1.pixelZoom;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Rectangle destination = new Rectangle(
                x: (int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0),
                y: (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0),
                width: (int)(Game1.tileSize + scaleFactor.X),
                height: (int)((Game1.tileSize * 2) + scaleFactor.Y / 2f)
            );
            spriteBatch.Draw(tex, destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            // draw mannequin
            float drawLayer = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;
            this.draw(spriteBatch, xNonTile: x * Game1.tileSize, yNonTile: y * Game1.tileSize - Game1.tileSize, layerDepth: drawLayer, alpha: alpha);

            // draw overlay
            Farmer farmerForRendering = this.GetFarmerForRendering();
            farmerForRendering.position.Value = new Vector2(x * 64, y * 64 + 12);
            farmerForRendering.FarmerRenderer.draw(spriteBatch, farmerForRendering.FarmerSprite, farmerForRendering.FarmerSprite.sourceRect, farmerForRendering.getLocalPosition(Game1.viewport), new Vector2(0, farmerForRendering.GetBoundingBox().Height), drawLayer + 0.001f, Color.White, 0, farmerForRendering);
        }


        /*********
        ** Protected methods
        *********/
        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.MannType, this.MannGender, this.Facing, this.Hat, this.Shirt, this.Pants, this.Boots);

            this.MannType.fieldChangeEvent += this.OnNetFieldChanged;
            this.Facing.fieldChangeEvent += this.OnNetFieldChanged;
            this.Hat.fieldChangeEvent += this.OnNetFieldChanged;
            this.Shirt.fieldChangeEvent += this.OnNetFieldChanged;
            this.Pants.fieldChangeEvent += this.OnNetFieldChanged;
            this.Boots.fieldChangeEvent += this.OnNetFieldChanged;
        }

        protected override string loadDisplayName()
        {
            return Mod.Instance.Helper.Translation.Get($"mannequin.name.{this.MannType.Value}-{this.MannGender.Value}");
        }

        private void OnNetFieldChanged<TNetField, TValue>(TNetField field, TValue oldValue, TValue newValue)
        {
            this.FarmerForRenderingCache = null;
        }

        /// <summary>Get the internal farmer instance used to render equipment on the mannequin, recreating it if needed.</summary>
        private Farmer GetFarmerForRendering()
        {
            Farmer CreateInstance()
            {
                Farmer farmer = new();

                // base info
                farmer.changeGender(this.MannGender.Value == MannequinGender.Male);
                if (this.MannGender.Value == MannequinGender.Female)
                    farmer.changeHairStyle(16);
                farmer.faceDirection(this.Facing.Value);
                if (this.MannType.Value == MannequinType.Plain)
                {
                    farmer.changeHairColor(Color.Transparent);
                    farmer.FarmerRenderer.textureName.Value = "Characters\\Farmer\\farmer_transparent";
                }

                // hat
                farmer.hat.Value = this.Hat.Value;

                // shirt
                farmer.shirtItem.Value = this.Shirt.Value;
                if (this.Shirt.Value != null)
                    farmer.shirt.Value = this.Shirt.Value.ItemID;

                // paints
                farmer.pantsItem.Value = this.Pants.Value;
                if (this.Pants.Value != null)
                {
                    farmer.pants.Value = this.Pants.Value.ItemID;
                    farmer.pantsColor.Value = this.Pants.Value.clothesColor.Value;
                }

                // boots
                farmer.boots.Value = this.Boots.Value;
                if (this.Boots.Value != null)
                    farmer.changeShoeColor(this.Boots.Value.appliedBootSheetIndex);

                return farmer;
            }

            return this.FarmerForRenderingCache ??= CreateInstance();
        }

        /// <summary>Get the main mannequin texture to render.</summary>
        private Texture2D GetMainTexture()
        {
            return this.MannGender.Value switch
            {
                MannequinGender.Male => Mannequin.TexM,
                MannequinGender.Female => Mannequin.TexF,
                _ => Mannequin.Tex
            };
        }

        /// <summary>Drop an item onto the ground near the mannequin.</summary>
        /// <param name="location">The location containing the mannequin.</param>
        /// <param name="item">The item to drop.</param>
        private void DropItem(GameLocation location, Item item)
        {
            var position = new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize);
            location.debris.Add(new Debris(item, position));
        }

        /// <summary>Swap an equipment slot between the display and a player.</summary>
        /// <typeparam name="T">The underlying type for the net reference.</typeparam>
        /// <param name="onDisplay">The field on the display.</param>
        /// <param name="onPlayer">The field on the player.</param>
        /// <param name="player">The player instance.</param>
        private void Swap<T>(NetRef<T> onDisplay, NetRef<T> onPlayer, Farmer player)
            where T : class, INetObject<INetSerializable>
        {
            T wasOnDisplay = onDisplay.Value;
            T wasOnPlayer = onPlayer.Value;

            onDisplay.Value = wasOnPlayer;
            onPlayer.Value = wasOnDisplay;

            this.OnUnEquipPlayer(player, wasOnPlayer);
            this.OnEquipPlayer(player, wasOnDisplay);
            this.FarmerForRenderingCache = null;
        }

        /// <summary>Perform any logic needed after un-equipping an item on a player.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="player">The player or display who un-equipped the item.</param>
        /// <param name="item">The item that was equipped.</param>
        /// <remarks>Derived from <see cref="InventoryPage.receiveLeftClick"/>.</remarks>
        private void OnUnEquipPlayer<T>(Farmer player, T item)
        {
            switch (item)
            {
                case Boots boots:
                    boots.onUnequip(player);
                    break;

                case Ring ring:
                    ring.onUnequip(player);
                    break;
            }
        }

        /// <summary>Perform any logic needed after equipping an item on a player.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="player">The player or display who equipped the item.</param>
        /// <param name="item">The item that was equipped.</param>
        /// <remarks>Derived from <see cref="InventoryPage.receiveLeftClick"/>.</remarks>
        private void OnEquipPlayer<T>(Farmer player, T item)
        {
            switch (item)
            {
                case Boots boots:
                    boots.onEquip(player);
                    break;

                case Ring ring:
                    ring.onEquip(player);
                    break;
            }
        }
    }
}
