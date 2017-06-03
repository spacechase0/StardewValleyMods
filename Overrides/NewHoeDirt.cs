using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SFarmer = StardewValley.Farmer;

namespace SpaceCore.Overrides
{
    //[XmlType(TypeName = "HoeDirt")]
    public class NewHoeDirt : HoeDirt
    {
        // Only used for XML serialization
        public NewHoeDirt() { } 

        public NewHoeDirt( HoeDirt dirt )
        {
            C = Reflect.getField<Color>(dirt, "c");
            if ( dirt.crop != null )
                crop = new NewCrop(dirt.crop);
            state = dirt.state;
            fertilizer = dirt.fertilizer;
            ShakeLeft = Reflect.getField<bool>(dirt, "shakeLeft");
            ShakeRotation = Reflect.getField<float>(dirt, "shakeRotation");
            MaxShake = Reflect.getField<float>(dirt, "maxShake");
            ShakeRate = Reflect.getField<float>(dirt, "shakeRate");
        }

        public bool canGrowHere(GameLocation loc, Vector2 pos)
        {
            if (crop.GetType() == typeof(Crop))
            {
                crop = new NewCrop(crop);
            }

            return ( crop as NewCrop ).canGrowHere(loc, pos);
        }

        public bool plant_( int index, int tileX, int tileY, SFarmer who, bool isFertilizer = false )
        {
            bool ret = base.plant(index, tileX, tileY, who, isFertilizer);
            if ( crop != null && !( crop is NewCrop ) )
            {
                crop = new NewCrop(crop);
            }
            return ret;
        }

        public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
        {
            if (this.crop != null)
            {
                if (crop.GetType() == typeof(Crop))
                {
                    crop = new NewCrop(crop);
                }

                (crop as NewCrop).newDay_(this.state, this.fertilizer, (int)tileLocation.X, (int)tileLocation.Y, environment);

                bool vanillaCheck = !environment.name.Equals("Greenhouse") && Game1.currentSeason.Equals("winter") && (this.crop != null && !this.crop.isWildSeedCrop());
                if ( !canGrowHere( environment, tileLocation ) )//&& vanillaCheck)
                    this.destroyCrop(tileLocation, false);
            }
            if (this.fertilizer == 370 && Game1.random.NextDouble() < 0.33 || this.fertilizer == 371 && Game1.random.NextDouble() < 0.66)
                return;
            this.state = 0;
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            if (this.state != 2)
            {
                int index1 = 0;
                int index2 = 0;
                Vector2 key = tileLocation;
                ++key.X;
                if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
                {
                    index1 += 100;
                    if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == this.state)
                        index2 += 100;
                }
                key.X -= 2f;
                if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
                {
                    index1 += 10;
                    if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == this.state)
                        index2 += 10;
                }
                ++key.X;
                ++key.Y;
                if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
                {
                    index1 += 500;
                    if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == this.state)
                        index2 += 500;
                }
                key.Y -= 2f;
                if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is HoeDirt)
                {
                    index1 += 1000;
                    if (((HoeDirt)Game1.currentLocation.terrainFeatures[key]).state == this.state)
                        index2 += 1000;
                }
                int num1 = HoeDirt.drawGuide[index1];
                int num2 = HoeDirt.drawGuide[index2];
                Texture2D texture = Game1.currentLocation.Name.Equals("Mountain") || Game1.currentLocation.Name.Equals("Mine") || Game1.currentLocation is MineShaft && Game1.mine.getMineArea(-1) != 121 ? HoeDirt.darkTexture : HoeDirt.lightTexture;
                if (Game1.currentSeason.Equals("winter") && !(Game1.currentLocation is Desert) && (!Game1.currentLocation.Name.Equals("Greenhouse") && !(Game1.currentLocation is MineShaft)) || Game1.currentLocation is MineShaft && Game1.mine.getMineArea(-1) == 40 && !Game1.mine.isLevelSlimeArea())
                    ;// texture = HoeDirt.snowTexture;
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), new Rectangle?(new Rectangle(num1 % 4 * 16, num1 / 4 * 16, 16, 16)), this.C, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-08f);
                if (this.state == 1)
                    spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), new Rectangle?(new Rectangle(num2 % 4 * 16 + 64, num2 / 4 * 16, 16, 16)), this.C, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1.2E-08f);
                if (this.fertilizer != 0)
                {
                    int num3 = 0;
                    switch (this.fertilizer)
                    {
                        case 369:
                            num3 = 1;
                            break;
                        case 370:
                            num3 = 2;
                            break;
                        case 371:
                            num3 = 3;
                            break;
                        case 465:
                            num3 = 4;
                            break;
                        case 466:
                            num3 = 5;
                            break;
                    }
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), new Rectangle?(new Rectangle(173 + num3 / 2 * 16, 466 + num3 % 2 * 16, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1.9E-08f);
                }
            }
            if (this.crop == null)
                return;
            this.crop.draw(spriteBatch, tileLocation, this.state != 1 || this.crop.currentPhase != 0 || this.crop.raisedSeeds ? Color.White : new Color(180, 100, 200) * 1f, this.ShakeRotation);
        }

        private Color C
        {
            get { return Reflect.getField<Color>(this, "c"); }
            set { Reflect.setField<Color>(this, "c", value); }
        }

        private bool ShakeLeft
        {
            get { return Reflect.getField<bool>(this, "shakeLeft"); }
            set { Reflect.setField<bool>(this, "shakeLeft", value); }
        }

        private float ShakeRotation
        {
            get { return Reflect.getField<float>(this, "shakeRotation"); }
            set { Reflect.setField<float>(this, "shakeRotation", value); }
        }

        private float MaxShake
        {
            get { return Reflect.getField<float>(this, "maxShake"); }
            set { Reflect.setField<float>(this, "maxShake", value); }
        }

        private float ShakeRate
        {
            get { return Reflect.getField<float>(this, "shakeRate"); }
            set { Reflect.setField<float>(this, "shakeRate", value); }
        }
    }
}
