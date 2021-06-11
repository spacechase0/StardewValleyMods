using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace Displays
{
    [XmlType("Mods_spacechase0_Mannequin")]
    public class Mannequin : StardewValley.Object
    {
        public static Texture2D tex = Mod.instance.Helper.Content.Load<Texture2D>("assets/mannequin-base.png");
        public static Texture2D texM = Mod.instance.Helper.Content.Load<Texture2D>("assets/mannequin-male.png");
        public static Texture2D texF = Mod.instance.Helper.Content.Load<Texture2D>("assets/mannequin-female.png");

        public enum MannequinType
        {
            Plain,
            Magic,
        }
        public enum MannequinGender
        {
            Male,
            Female,
        }

        public readonly NetEnum<MannequinType> mannType = new(MannequinType.Plain);
        public readonly NetEnum<MannequinGender> mannGender = new(MannequinGender.Male);
        public readonly NetInt facing = new(Game1.down);

        public readonly NetRef<Hat> hat = new();
        public readonly NetRef<Clothing> shirt = new();
        public readonly NetRef<Clothing> pants = new();
        public readonly NetRef<Boots> boots = new();

        // TODO: Magic properties (skin color, eye color, hair stuff, accessories)

        [XmlIgnore]
        private Farmer farmerForRendering;

        public override string DisplayName
        {
            get => this.name;
            set { }
        }

        public Mannequin() { }
        public Mannequin(MannequinType type, MannequinGender gender, Vector2 placement)
        {
            this.mannType.Value = type;
            this.mannGender.Value = gender;
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
            this.NetFields.AddFields(this.mannType, this.mannGender, this.facing, this.hat, this.shirt, this.pants, this.boots);

            this.mannType.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.facing.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.hat.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.shirt.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.pants.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
            this.boots.fieldChangeEvent += (field, oldVal, newVal) => this.CacheFarmerSprite();
        }

        private void CacheFarmerSprite()
        {
            if (this.farmerForRendering == null)
                this.farmerForRendering = new Farmer();
            this.farmerForRendering.changeGender(this.mannGender == MannequinGender.Male);
            if (this.mannGender == MannequinGender.Female)
                this.farmerForRendering.changeHairStyle(16);
            this.farmerForRendering.faceDirection(this.facing.Value);
            this.farmerForRendering.hat.Value = this.hat.Value;
            this.farmerForRendering.shirtItem.Value = this.shirt.Value;
            if (this.shirt.Value != null)
            {
                this.farmerForRendering.shirt.Value = this.mannGender.Value == MannequinGender.Male ? this.shirt.Value.indexInTileSheetMale.Value : this.shirt.Value.indexInTileSheetFemale.Value;
            }

            this.farmerForRendering.pantsItem.Value = this.pants.Value;
            if (this.pants.Value != null)
            {
                this.farmerForRendering.pants.Value = this.mannGender.Value == MannequinGender.Male ? this.pants.Value.indexInTileSheetMale.Value : this.pants.Value.indexInTileSheetFemale.Value;
                this.farmerForRendering.pantsColor.Value = this.pants.Value.clothesColor.Value;
            }

            this.farmerForRendering.boots.Value = this.boots.Value;
            if (this.boots.Value != null)
            {
                this.farmerForRendering.changeShoeColor(this.boots.Value.indexInColorSheet.Value);
            }
            if (this.mannType == MannequinType.Plain)
            {
                this.farmerForRendering.changeHairColor(Color.Transparent);
                this.farmerForRendering.FarmerRenderer.textureName.Value = "Characters\\Farmer\\farmer_transparent";
            }
        }

        protected override string loadDisplayName()
        {
            string type = Mod.instance.Helper.Translation.Get("mannequin.type." + this.mannType.Value.ToString());
            string gender = Mod.instance.Helper.Translation.Get("mannequin.gender." + this.mannGender.Value.ToString());
            return Mod.instance.Helper.Translation.Get("mannequin.name", new { type = type, gender = gender });
        }

        public override string getDescription()
        {
            return Mod.instance.Helper.Translation.Get("mannequin.desc");
        }

        public override bool canStackWith(ISalable other)
        {
            if (other is Mannequin m)
            {
                return m.mannType.Value == this.mannType.Value && m.mannGender.Value == this.mannGender.Value;
            }
            return false;
        }

        public override Item getOne()
        {
            var ret = new Mannequin(this.mannType.Value, this.mannGender.Value, Vector2.Zero);
            ret.hat.Value = (Hat)this.hat.Value?.getOne();
            ret.shirt.Value = (Clothing)this.shirt.Value?.getOne();
            ret.pants.Value = (Clothing)this.pants.Value?.getOne();
            ret.boots.Value = (Boots)this.boots.Value?.getOne();
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);
            var m = new Mannequin(this.mannType.Value, this.mannGender.Value, placementTile);
            if (who != null)
                ;// m.facing.Value = who.FacingDirection;
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
                if (this.hat.Value != null || this.shirt.Value != null || this.pants.Value != null || this.boots.Value != null)
                {
                    if (this.hat.Value != null)
                    {
                        location.debris.Add(new Debris(this.hat.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.hat.Value = null;
                    }
                    else if (this.shirt.Value != null)
                    {
                        location.debris.Add(new Debris(this.shirt.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.shirt.Value = null;
                    }
                    else if (this.pants.Value != null)
                    {
                        location.debris.Add(new Debris(this.pants.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.pants.Value = null;
                    }
                    else if (this.boots.Value != null)
                    {
                        location.debris.Add(new Debris(this.boots.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                        this.boots.Value = null;
                    }
                    location.playSound("hammer");
                    this.shakeTimer = 100;
                    return false;
                }
                location.objects.Remove(this.TileLocation);
                location.debris.Add(new Debris(new Mannequin(this.mannType.Value, this.mannGender.Value, Vector2.Zero), new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
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

                //if ( who.hat.Value != null )
                {
                    var tmp = this.hat.Value;
                    this.hat.Value = who.hat.Value;
                    who.hat.Value = tmp;
                }
                //if ( who.shirtItem.Value != null )
                {
                    var tmp = this.shirt.Value;
                    this.shirt.Value = who.shirtItem.Value;
                    who.shirtItem.Value = tmp;
                }
                //if ( who.pantsItem.Value != null )
                {
                    var tmp = this.pants.Value;
                    this.pants.Value = who.pantsItem.Value;
                    who.pantsItem.Value = tmp;
                }
                if (who.boots.Value != null)
                {
                    var tmp = this.boots.Value;
                    this.boots.Value = who.boots.Value;
                    who.boots.Value = tmp;
                }
                return true;
            }

            return false;

        }

        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
        {
            if (probe && (dropInItem is Hat || dropInItem is Clothing || dropInItem is Boots))
                return true;

            if (dropInItem is Hat hat)
            {
                if (this.hat.Value != null)
                    who.currentLocation.debris.Add(new Debris(this.hat.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                this.hat.Value = hat;
                return true;
            }
            else if (dropInItem is Clothing clothing)
            {
                if (clothing.clothesType.Value == (int)Clothing.ClothesType.SHIRT)
                {
                    if (this.shirt.Value != null)
                        who.currentLocation.debris.Add(new Debris(this.shirt.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                    this.shirt.Value = clothing;
                    return true;
                }
                else if (clothing.clothesType.Value == (int)Clothing.ClothesType.PANTS)
                {
                    if (this.pants.Value != null)
                        who.currentLocation.debris.Add(new Debris(this.pants.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                    this.pants.Value = clothing;
                    return true;
                }
            }
            else if (dropInItem is Boots boots)
            {
                if (this.boots.Value != null)
                    who.currentLocation.debris.Add(new Debris(this.boots.Value, new Vector2((this.TileLocation.X + 0.5f) * Game1.tileSize, (this.TileLocation.Y + 0.5f) * Game1.tileSize)));
                this.boots.Value = boots;
                return true;
            }

            return false;
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            var tex = Mannequin.tex;
            if (this.mannGender.Value == MannequinGender.Male)
                tex = Mannequin.texM;
            else if (this.mannGender.Value == MannequinGender.Female)
                tex = Mannequin.texF;

            spriteBatch.Draw(tex, objectPosition, null, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            var tex = Mannequin.tex;
            if (this.mannGender.Value == MannequinGender.Male)
                tex = Mannequin.texM;
            else if (this.mannGender.Value == MannequinGender.Female)
                tex = Mannequin.texF;

            spriteBatch.Draw(tex, location + new Vector2(32f, 32f), null, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(this.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            var tex = Mannequin.tex;
            if (this.mannGender.Value == MannequinGender.Male)
                tex = Mannequin.texM;
            else if (this.mannGender.Value == MannequinGender.Female)
                tex = Mannequin.texF;

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw(tex, destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            var tex = Mannequin.tex;
            if (this.mannGender.Value == MannequinGender.Male)
                tex = Mannequin.texM;
            else if (this.mannGender.Value == MannequinGender.Female)
                tex = Mannequin.texF;

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw(tex, destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);

            if (this.farmerForRendering == null)
                this.CacheFarmerSprite();
            //SpaceShared.Log.trace( "meow!? " + farmerForRendering.shirtItem.Value + " " + farmerForRendering.pantsItem.Value + " " + farmerForRendering.hat.Value );
            this.farmerForRendering.position.Value = new Vector2(x * 64, y * 64 + 12);
            this.farmerForRendering.FarmerRenderer.draw(spriteBatch, this.farmerForRendering.FarmerSprite, this.farmerForRendering.FarmerSprite.sourceRect, this.farmerForRendering.getLocalPosition(Game1.viewport), new Vector2(0, this.farmerForRendering.GetBoundingBox().Height), draw_layer + 0.001f, Color.White, 0, this.farmerForRendering);
        }
    }
}
