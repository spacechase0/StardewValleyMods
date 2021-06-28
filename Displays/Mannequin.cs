using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace Displays
{
    [XmlType("Mods_spacechase0_Mannequin")]
    public class Mannequin : StardewValley.Object
    {
        public static Texture2D Tex = Mod.Instance.Helper.Content.Load<Texture2D>("assets/mannequin-base.png");
        public static Texture2D TexM = Mod.Instance.Helper.Content.Load<Texture2D>("assets/mannequin-male.png");
        public static Texture2D TexF = Mod.Instance.Helper.Content.Load<Texture2D>("assets/mannequin-female.png");

        public enum MannequinType
        {
            Plain,
            Magic
        }
        public enum MannequinGender
        {
            Male,
            Female
        }

        public readonly NetEnum<MannequinType> MannType = new(MannequinType.Plain);
        public readonly NetEnum<MannequinGender> MannGender = new(MannequinGender.Male);
        public readonly NetInt Facing = new(Game1.down);

        public readonly NetRef<Hat> Hat = new();
        public readonly NetRef<Clothing> Shirt = new();
        public readonly NetRef<Clothing> Pants = new();
        public readonly NetRef<Boots> Boots = new();

        // TODO: Magic properties (skin color, eye color, hair stuff, accessories)

        [XmlIgnore]
        private Farmer FarmerForRendering;

        public override string DisplayName
        {
            get => this.name;
            set { }
        }

        public Mannequin() { }

        public Mannequin(MannequinType type, MannequinGender gender, Vector2 placement)
        {
            this.MannType.Value = type;
            this.MannGender.Value = gender;
            //ParentSheetIndex = MODID;
            this.name = this.loadDisplayName();
            this.DisplayName = this.loadDisplayName();
            this.bigCraftable.Value = true;
            this.Type = "Crafting"; // Makes performObjectDropInAction work for non-objects

            this.TileLocation = placement;
            this.boundingBox.Value = new Rectangle((int)placement.X * 64, (int)placement.Y * 64, 64, 64);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.MannType, this.MannGender, this.Facing, this.Hat, this.Shirt, this.Pants, this.Boots);

            this.MannType.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.Facing.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.Hat.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.Shirt.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.Pants.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.Boots.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
        }

        private void CacheFarmerSprite()
        {
            this.FarmerForRendering ??= new Farmer();
            this.FarmerForRendering.changeGender(this.MannGender.Value == MannequinGender.Male);
            if (this.MannGender.Value == MannequinGender.Female)
                this.FarmerForRendering.changeHairStyle(16);
            this.FarmerForRendering.faceDirection(this.Facing.Value);
            this.FarmerForRendering.hat.Value = this.Hat.Value;
            this.FarmerForRendering.shirtItem.Value = this.Shirt.Value;
            if (this.Shirt.Value != null)
            {
                this.FarmerForRendering.shirt.Value = this.MannGender.Value == MannequinGender.Male ? this.Shirt.Value.indexInTileSheetMale.Value : this.Shirt.Value.indexInTileSheetFemale.Value;
            }

            this.FarmerForRendering.pantsItem.Value = this.Pants.Value;
            if (this.Pants.Value != null)
            {
                this.FarmerForRendering.pants.Value = this.MannGender.Value == MannequinGender.Male ? this.Pants.Value.indexInTileSheetMale.Value : this.Pants.Value.indexInTileSheetFemale.Value;
                this.FarmerForRendering.pantsColor.Value = this.Pants.Value.clothesColor.Value;
            }

            this.FarmerForRendering.boots.Value = this.Boots.Value;
            if (this.Boots.Value != null)
            {
                this.FarmerForRendering.changeShoeColor(this.Boots.Value.indexInColorSheet.Value);
            }
            if (this.MannType.Value == MannequinType.Plain)
            {
                this.FarmerForRendering.changeHairColor(Color.Transparent);
                this.FarmerForRendering.FarmerRenderer.textureName.Value = "Characters\\Farmer\\farmer_transparent";
            }
        }

        protected override string loadDisplayName()
        {
            string type = Mod.Instance.Helper.Translation.Get($"mannequin.type.{this.MannType.Value}");
            string gender = Mod.Instance.Helper.Translation.Get($"mannequin.gender.{this.MannGender.Value}");
            return Mod.Instance.Helper.Translation.Get("mannequin.name", new { type, gender });
        }

        public override string getDescription()
        {
            return Mod.Instance.Helper.Translation.Get("mannequin.desc");
        }

        public override bool canStackWith(ISalable other)
        {
            if (other is Mannequin m)
            {
                return m.MannType.Value == this.MannType.Value && m.MannGender.Value == this.MannGender.Value;
            }
            return false;
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

            if (!(t is MeleeWeapon) && t.isHeavyHitter())
            {
                if (this.Hat.Value != null || this.Shirt.Value != null || this.Pants.Value != null || this.Boots.Value != null)
                {
                    if (this.Hat.Value != null)
                    {
                        location.debris.Add(new Debris(this.Hat.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.Hat.Value = null;
                    }
                    else if (this.Shirt.Value != null)
                    {
                        location.debris.Add(new Debris(this.Shirt.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.Shirt.Value = null;
                    }
                    else if (this.Pants.Value != null)
                    {
                        location.debris.Add(new Debris(this.Pants.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.Pants.Value = null;
                    }
                    else if (this.Boots.Value != null)
                    {
                        location.debris.Add(new Debris(this.Boots.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.Boots.Value = null;
                    }
                    location.playSound("hammer");
                    this.shakeTimer = 100;
                    return false;
                }
                location.objects.Remove(this.TileLocation);
                location.debris.Add(new Debris(new Mannequin(this.MannType.Value, this.MannGender.Value, Vector2.Zero), new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
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
            if (probe && (dropInItem is Hat || dropInItem is Clothing || dropInItem is Boots))
                return true;

            switch (dropInItem)
            {
                case Hat hat:
                    if (this.Hat.Value != null)
                        who.currentLocation.debris.Add(new Debris(this.Hat.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                    this.Hat.Value = hat;
                    return true;

                case Clothing clothing:
                    switch (clothing.clothesType.Value)
                    {
                        case (int)Clothing.ClothesType.SHIRT:
                            if (this.Shirt.Value != null)
                                who.currentLocation.debris.Add(new Debris(this.Shirt.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                            this.Shirt.Value = clothing;
                            return true;

                        case (int)Clothing.ClothesType.PANTS:
                            if (this.Pants.Value != null)
                                who.currentLocation.debris.Add(new Debris(this.Pants.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                            this.Pants.Value = clothing;
                            return true;

                        default:
                            return false;
                    }

                case Boots boots:
                    if (this.Boots.Value != null)
                        who.currentLocation.debris.Add(new Debris(this.Boots.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                    this.Boots.Value = boots;
                    return true;

                default:
                    return false;
            }
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            var tex = this.MannGender.Value switch
            {
                MannequinGender.Male => Mannequin.TexM,
                MannequinGender.Female => Mannequin.TexF,
                _ => Mannequin.Tex
            };

            spriteBatch.Draw(tex, objectPosition, null, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (f.getStandingY() + 3) / 10000f));
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && scaleSize > 0.3 && this.Stack != int.MaxValue;

            var tex = this.MannGender.Value switch
            {
                MannequinGender.Male => Mannequin.TexM,
                MannequinGender.Female => Mannequin.TexF,
                _ => Mannequin.Tex
            };

            spriteBatch.Draw(tex, location + new Vector2(32f, 32f), null, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(this.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(this.Stack, 3f * scaleSize) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            var tex = this.MannGender.Value switch
            {
                MannequinGender.Male => Mannequin.TexM,
                MannequinGender.Female => Mannequin.TexF,
                _ => Mannequin.Tex
            };

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw(tex, destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            var tex = this.MannGender.Value switch
            {
                MannequinGender.Male => Mannequin.TexM,
                MannequinGender.Female => Mannequin.TexF,
                _ => Mannequin.Tex
            };

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float drawLayer = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;
            spriteBatch.Draw(tex, destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, drawLayer);

            if (this.FarmerForRendering == null)
                this.CacheFarmerSprite();
            //SpaceShared.Log.trace( "meow!? " + farmerForRendering.shirtItem.Value + " " + farmerForRendering.pantsItem.Value + " " + farmerForRendering.hat.Value );
            this.FarmerForRendering.position.Value = new Vector2(x * 64, y * 64 + 12);
            this.FarmerForRendering.FarmerRenderer.draw(spriteBatch, this.FarmerForRendering.FarmerSprite, this.FarmerForRendering.FarmerSprite.sourceRect, this.FarmerForRendering.getLocalPosition(Game1.viewport), new Vector2(0, this.FarmerForRendering.GetBoundingBox().Height), drawLayer + 0.001f, Color.White, 0, this.FarmerForRendering);
        }

        /// <summary>Swap an equipment slot between the display and a player.</summary>
        /// <typeparam name="T">The underlying type for the net reference.</typeparam>
        /// <param name="onDisplay">The field on the display.</param>
        /// <param name="onPlayer">The field on the player.</param>
        /// <param name="player">The player instance.</param>
        private void Swap<T>(NetRef<T> onDisplay, NetRef<T> onPlayer, Farmer player)
            where T : class, INetObject<INetSerializable>
        {
            T wasOnDisplay = this.OnUnEquip(this.FarmerForRendering, onDisplay.Value);
            T wasOnPlayer = this.OnUnEquip(player, onPlayer.Value);

            onPlayer.Value = this.OnEquip(player, wasOnDisplay);
            onDisplay.Value = this.OnEquip(this.FarmerForRendering, wasOnPlayer);
        }

        /// <summary>Perform any logic needed after un-equipping an item on a player or display.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="farmer">The player or display who un-equipped the item.</param>
        /// <param name="item">The item that was equipped.</param>
        /// <remarks>Derived from <see cref="InventoryPage.receiveLeftClick"/>.</remarks>
        private T OnUnEquip<T>(Farmer farmer, T item)
        {
            switch (item)
            {
                case Boots boots:
                    if (object.ReferenceEquals(farmer, this.FarmerForRendering))
                        farmer.changeShoeColor(12);
                    else
                        boots.onUnequip();
                    break;

                case Ring ring:
                    ring.onUnequip(farmer, farmer.currentLocation);
                    break;
            }

            return item;
        }

        /// <summary>Perform any logic needed after equipping an item on a player or display.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="farmer">The player or display who equipped the item.</param>
        /// <param name="item">The item that was equipped.</param>
        /// <remarks>Derived from <see cref="InventoryPage.receiveLeftClick"/>.</remarks>
        private T OnEquip<T>(Farmer farmer, T item)
        {
            switch (item)
            {
                case Boots boots:
                    if (object.ReferenceEquals(farmer, this.FarmerForRendering))
                        farmer.changeShoeColor(boots.indexInColorSheet.Value);
                    else
                        boots.onEquip();
                    break;

                case Ring ring:
                    ring.onEquip(farmer, farmer.currentLocation);
                    break;
            }

            return item;
        }
    }
}
