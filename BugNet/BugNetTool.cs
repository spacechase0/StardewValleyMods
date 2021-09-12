using System;
using System.Linq;
using System.Xml.Serialization;
using BugNet.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace BugNet
{
    [XmlType("Mods_spacechase0_BugNet")]
    public class BugNetTool : MeleeWeapon, ICustomWeaponDraw
    {
        internal static Texture2D Texture;

        public BugNetTool()
        {
            this.Category = SObject.toolCategory;
            this.Name = "Bug Net";
            this.ParentSheetIndex = MeleeWeapon.scythe; // Gets us out of annoying stuff

            this.minDamage.Value = 1;
            this.maxDamage.Value = 1;
            this.knockback.Value = 1;
            this.speed.Value = 0;
            this.addedPrecision.Value = 0;
            this.addedDefense.Value = 0;
            this.type.Value = 3; // ?
            this.addedAreaOfEffect.Value = 0;
            this.critChance.Value = 0.02f;
            this.critMultiplier.Value = 1;

            this.Stack = 1;
        }

        public override Item getOne()
        {
            return new BugNetTool();
        }

        protected override string loadDisplayName()
        {
            return Mod.Instance.Helper.Translation.Get("bug-net.name");
        }

        protected override string loadDescription()
        {
            return Mod.Instance.Helper.Translation.Get("bug-net.description");
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            // Copied from melee weapon
            float num1 = 0.0f;
            if (MeleeWeapon.defenseCooldown > 0)
                num1 = MeleeWeapon.defenseCooldown / 1500f;
            float num2 = Mod.Instance.Helper.Reflection.GetField<float>(typeof(MeleeWeapon), "addedSwordScale").GetValue();
            if (!drawShadow)
                num2 = 0;
            spriteBatch.Draw(BugNetTool.Texture, location + (this.type.Value == 1 ? new Vector2(Game1.tileSize * 2 / 3, Game1.tileSize / 3) : new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)), new Rectangle(0, 0, 16, 16), Color.White * transparency, 0.0f, new Vector2(8f, 8f), Game1.pixelZoom * (scaleSize + num2), SpriteEffects.None, layerDepth);
            if (num1 <= 0.0 || drawShadow)
                return;
            spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (Game1.tileSize - (int)(num1 * (double)Game1.tileSize)), Game1.tileSize, (int)(num1 * (double)Game1.tileSize)), Color.Red * 0.66f);
        }

        public override void tickUpdate(GameTime time, Farmer who)
        {
            base.tickUpdate(time, who);
            if (who.FarmerSprite.isUsingWeapon())
            {
                Vector2 toolLoc = who.GetToolLocation(true);
                Vector2 a = Vector2.Zero, b = Vector2.Zero;
                Rectangle area = this.getAreaOfEffect((int)toolLoc.X, (int)toolLoc.Y, who.facingDirection, ref a, ref b, who.GetBoundingBox(), who.FarmerSprite.currentAnimationIndex);

                var critters = who.currentLocation.critters;
                if (critters == null)
                    return;

                foreach (Critter critter in critters.ToList())
                {
                    if (critter.getBoundingBox(0, 0).Intersects(area))
                    {
                        if (!Mod.TryGetCritter(critter, out CritterData data))
                            continue; // not a supported critter

                        critters.Remove(critter);
                        int objId = Mod.Ja.GetObjectId($"Critter Cage: {data.DefaultName}");
                        Log.Trace($"Spawning a '{data.DefaultName}' critter cage with item ID {objId}");
                        who.currentLocation.debris.Add(new Debris(new SObject(objId, 1), critter.position));
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
            return this.loadDescription();
        }

        public void Draw(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            //if (!Mod.instance.Helper.Reflection.GetPrivateValue< bool >((MeleeWeapon)this, "attacking"))
            //    return;

            var meleeWeaponCenter = new Vector2(1f, 15f);
            sourceRect = new Rectangle(0, 0, 16, 16);

            switch (facingDirection)
            {
                case 1:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 40f, (float)(playerPosition.Y - (double)Game1.tileSize + 8.0)), sourceRect, Color.White, -0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 56f, (float)(playerPosition.Y - (double)Game1.tileSize + 28.0)), sourceRect, Color.White, 0.0f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - Game1.pixelZoom, playerPosition.Y - 4 * Game1.pixelZoom), sourceRect, Color.White, 0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - Game1.pixelZoom, playerPosition.Y - Game1.pixelZoom), sourceRect, Color.White, 1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - 7 * Game1.pixelZoom, playerPosition.Y + Game1.pixelZoom), sourceRect, Color.White, 1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - 12 * Game1.pixelZoom, playerPosition.Y + Game1.pixelZoom), sourceRect, Color.White, 2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.tileSize - 12 * Game1.pixelZoom, playerPosition.Y + Game1.pixelZoom), sourceRect, Color.White, 2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2((float)(playerPosition.X + (double)Game1.tileSize - 16.0), (float)(playerPosition.Y + (double)Game1.tileSize + 12.0)), sourceRect, Color.White, 1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                    }
                    break;

                case 3:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X - 4 * Game1.pixelZoom, playerPosition.Y - Game1.tileSize - 4 * Game1.pixelZoom), sourceRect, Color.White, 0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X - 12 * Game1.pixelZoom, playerPosition.Y - Game1.tileSize + 5 * Game1.pixelZoom), sourceRect, Color.White, 0.0f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X - Game1.tileSize + 8 * Game1.pixelZoom, playerPosition.Y + 4 * Game1.pixelZoom), sourceRect, Color.White, -0.7853982f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() - 1) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.pixelZoom, playerPosition.Y + 11 * Game1.pixelZoom), sourceRect, Color.White, -1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 11 * Game1.pixelZoom, playerPosition.Y + 13 * Game1.pixelZoom), sourceRect, Color.White, -1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 20 * Game1.pixelZoom, playerPosition.Y + 10 * Game1.pixelZoom), sourceRect, Color.White, -2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 20 * Game1.pixelZoom, playerPosition.Y + 10 * Game1.pixelZoom), sourceRect, Color.White, -2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipHorizontally, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), sourceRect, Color.White, -5.105088f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.FlipVertically, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize) / 10000f));
                            break;
                    }
                    break;

                case 0:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), sourceRect, Color.White, -2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), sourceRect, Color.White, -1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 2:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, -3f * (float)Math.PI / 8f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 3:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, -0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 4:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2((float)(playerPosition.X + (double)Game1.tileSize - 8.0), playerPosition.Y - 40f), sourceRect, Color.White, 0.0f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 5:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.tileSize, playerPosition.Y - 40f), sourceRect, Color.White, 0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 6:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + Game1.tileSize, playerPosition.Y - 40f), sourceRect, Color.White, 0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                        case 7:
                            spriteBatch.Draw(BugNetTool.Texture, new Vector2((float)(playerPosition.X + (double)Game1.tileSize - 44.0), playerPosition.Y + Game1.tileSize), sourceRect, Color.White, -1.963495f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() - Game1.tileSize / 2 - 8) / 10000f));
                            break;
                    }
                    break;

                case 2:
                    {
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), sourceRect, Color.White, 0.3926991f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 1:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), sourceRect, Color.White, 1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 2:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 40f, playerPosition.Y), sourceRect, Color.White, 1.570796f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 3:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), sourceRect, Color.White, 2.356194f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 4:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), sourceRect, Color.White, 3.141593f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 5:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.534292f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 6:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.534292f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                            case 7:
                                spriteBatch.Draw(BugNetTool.Texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + Game1.tileSize), sourceRect, Color.White, -5.105088f, meleeWeaponCenter, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + Game1.tileSize / 2) / 10000f));
                                break;
                        }
                        break;
                    }
            }
        }
    }
}
