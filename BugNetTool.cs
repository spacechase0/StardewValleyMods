using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using SpaceCore;
using SpaceShared;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugNet
{
    public class BugNetTool : MeleeWeapon, ICustomWeaponDraw, ISaveElement
    {
        internal static Texture2D Texture;

        public BugNetTool()
        {
            category.Value = StardewValley.Object.toolCategory;
            Name = "Bug Net";
            ParentSheetIndex = MeleeWeapon.scythe; // Gets us out of annoying stuff

            minDamage.Value = 1;
            maxDamage.Value = 1;
            knockback.Value = 1;
            speed.Value = 0;
            addedPrecision.Value = 0;
            addedDefense.Value = 0;
            type.Value = 3; // ?
            addedAreaOfEffect.Value = 0;
            critChance.Value = 0.02f;
            critMultiplier.Value = 1;

            Stack = 1;
        }

        public override Item getOne()
        {
            return new BugNetTool();
        }

        // ISaveElement
        public object getReplacement()
        {
            return new StardewValley.Object();
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public void rebuild(Dictionary<string, string> saveData, object replacement)
        {
        }

        protected override string loadDisplayName()
        {
            return "Bug Net";
        }

        protected override string loadDescription()
        {
            return "Catches critters and stuff";
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            // Copied from melee weapon
            float num1 = 0.0f;
            float num2 = 0.0f;
            if (MeleeWeapon.defenseCooldown > 0)
                num1 = (float)MeleeWeapon.defenseCooldown / 1500f;
            num2 = Mod.instance.Helper.Reflection.GetField<float>(typeof(MeleeWeapon), "addedSwordScale").GetValue();
            if (!drawShadow)
                num2 = 0;
            spriteBatch.Draw(Texture, location + (this.type == 1 ? new Vector2((float)(Game1.tileSize * 2 / 3), (float)(Game1.tileSize / 3)) : new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))), new Rectangle(0,0,16,16), Color.White * transparency, 0.0f, new Vector2(8f, 8f), (float)Game1.pixelZoom * (scaleSize + num2), SpriteEffects.None, layerDepth);
            if ((double)num1 <= 0.0 || drawShadow)
                return;
            spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (Game1.tileSize - (int)(num1 * (double)Game1.tileSize)), Game1.tileSize, (int)((double)num1 * (double)Game1.tileSize)), Color.Red * 0.66f);
        }

        public override void tickUpdate(GameTime time, Farmer who)
        {
            base.tickUpdate(time, who);
            if ( who.FarmerSprite.isUsingWeapon() )
            {
                Vector2 toolLoc = who.GetToolLocation(true);
                Vector2 a = Vector2.Zero, b = Vector2.Zero;
                Rectangle area = getAreaOfEffect( (int)toolLoc.X, (int)toolLoc.Y, who.facingDirection, ref a, ref b, who.GetBoundingBox(), who.FarmerSprite.currentAnimationIndex);

                var critters = Mod.instance.Helper.Reflection.GetField<List<Critter>>(who.currentLocation, "critters", false)?.GetValue();
                if (critters == null)
                    return;
                
                foreach (var critter in critters.ToList())
                {
                    if (critter.getBoundingBox(0, 0).Intersects(area))
                    {
                        critters.Remove(critter);
                        int bframe = critter.baseFrame;
                        if (critter is Cloud)
                            bframe = -2;
                        if (critter is Frog frog)
                            bframe = Mod.instance.Helper.Reflection.GetField<bool>(frog, "waterLeaper").GetValue() ? -3 : -4;

                        string critterId = Mod.GetCritterIdFrom(critter);
                        Log.trace("Spawning a " + critterId);
                        who.currentLocation.debris.Add(new Debris(new CritterItem(critterId), critter.position));
                    }
                }
            }
        }

        public override int salePrice()
        {
            return 100;
        }

        public override string getDescription()
        {
            return loadDescription();
        }

        public void draw(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            //if (!Mod.instance.Helper.Reflection.GetPrivateValue< bool >((MeleeWeapon)this, "attacking"))
            //    return;

            var MeleeWeapon_center = new Vector2(1f, 15f);
            sourceRect = new Rectangle(0, 0, 16, 16);

            if (facingDirection == 1)
            {
                switch (frameOfFarmerAnimation)
                {
                    case 0:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 40f, (float)((double)playerPosition.Y - (double)Game1.tileSize + 8.0)), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                        break;
                    case 1:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 56f, (float)((double)playerPosition.Y - (double)Game1.tileSize + 28.0)), new Rectangle?(sourceRect), Color.White, 0.0f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                        break;
                    case 2:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)Game1.pixelZoom, playerPosition.Y - (float)(4 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                        break;
                    case 3:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)Game1.pixelZoom, playerPosition.Y - (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 4:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(7 * Game1.pixelZoom), playerPosition.Y + (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 5:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(12 * Game1.pixelZoom), playerPosition.Y + (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 6:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.tileSize - (float)(12 * Game1.pixelZoom), playerPosition.Y + (float)Game1.pixelZoom), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 7:
                        spriteBatch.Draw(Texture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 16.0), (float)((double)playerPosition.Y + (double)Game1.tileSize + 12.0)), new Rectangle?(sourceRect), Color.White, 1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                }
            }
            else if (facingDirection == 3)
            {
                switch (frameOfFarmerAnimation)
                {
                    case 0:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X - (float)(4 * Game1.pixelZoom), playerPosition.Y - (float)Game1.tileSize - (float)(4 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, 0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                        break;
                    case 1:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X - (float)(12 * Game1.pixelZoom), playerPosition.Y - (float)Game1.tileSize + (float)(5 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, 0.0f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                        break;
                    case 2:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X - (float)Game1.tileSize + (float)(8 * Game1.pixelZoom), playerPosition.Y + (float)(4 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -0.7853982f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() - 1) / 10000f));
                        break;
                    case 3:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.pixelZoom, playerPosition.Y + (float)(11 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 4:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)(11 * Game1.pixelZoom), playerPosition.Y + (float)(13 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 5:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)(20 * Game1.pixelZoom), playerPosition.Y + (float)(10 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 6:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)(20 * Game1.pixelZoom), playerPosition.Y + (float)(10 * Game1.pixelZoom)), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                    case 7:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.FlipVertically, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize) / 10000f));
                        break;
                }
            }
            else if (facingDirection == 0)
            {
                switch (frameOfFarmerAnimation)
                {
                    case 0:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), new Rectangle?(sourceRect), Color.White, -2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                    case 1:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), new Rectangle?(sourceRect), Color.White, -1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                    case 2:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), new Rectangle?(sourceRect), Color.White, -3f * (float)Math.PI / 8f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                    case 3:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), new Rectangle?(sourceRect), Color.White, -0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                    case 4:
                        spriteBatch.Draw(Texture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 8.0), playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.0f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                    case 5:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.tileSize, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                    case 6:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + (float)Game1.tileSize, playerPosition.Y - 40f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                    case 7:
                        spriteBatch.Draw(Texture, new Vector2((float)((double)playerPosition.X + (double)Game1.tileSize - 44.0), playerPosition.Y + (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, -1.963495f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                        break;
                }
            }
            else
            {
                if (facingDirection != 2)
                    return;
                switch (frameOfFarmerAnimation)
                {
                    case 0:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), new Rectangle?(sourceRect), Color.White, 0.3926991f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                    case 1:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), new Rectangle?(sourceRect), Color.White, 1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                    case 2:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 40f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 1.570796f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                    case 3:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), new Rectangle?(sourceRect), Color.White, 2.356194f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                    case 4:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), new Rectangle?(sourceRect), Color.White, 3.141593f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                    case 5:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 3.534292f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                    case 6:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), new Rectangle?(sourceRect), Color.White, 3.534292f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                    case 7:
                        spriteBatch.Draw(Texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + (float)Game1.tileSize), new Rectangle?(sourceRect), Color.White, -5.105088f, MeleeWeapon_center, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));
                        break;
                }
            }
        }
    }
}
