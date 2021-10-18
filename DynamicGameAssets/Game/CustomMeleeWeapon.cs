using System;
using System.Text;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAMeleeWeapon")]
    public partial class CustomMeleeWeapon : MeleeWeapon
    {
        partial void DoInit()
        {
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        partial void DoInit(MeleeWeaponPackData data)
        {
            this.Name = this.Id;

            this.type.Value = (int)data.Type;
            this.minDamage.Value = data.MinimumDamage;
            this.maxDamage.Value = data.MaximumDamage;
            this.knockback.Value = (float)data.Knockback;
            this.speed.Value = data.Speed;
            this.addedPrecision.Value = data.Accuracy;
            this.addedDefense.Value = data.Defense;
            this.addedAreaOfEffect.Value = data.ExtraSwingArea;
            this.critChance.Value = (float)data.CritChance;
            this.critMultiplier.Value = (float)data.CritMultiplier;

            this.CurrentParentTileIndex = this.FullId.GetDeterministicHashCode();
        }

        protected override string loadDisplayName()
        {
            return this.Data.Name;
        }

        protected override string loadDescription()
        {
            return this.Data.Description;
        }

        public override bool canBeTrashed()
        {
            return this.Data.CanTrash;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            var currTex = this.Data.GetTexture();

            float coolDownLevel = 0f;
            float addedScale = 0f;
            if (!this.isScythe())
            {
                switch ((int)this.type)
                {
                    case 0:
                    case 3:
                        if (MeleeWeapon.defenseCooldown > 0)
                        {
                            coolDownLevel = (float)MeleeWeapon.defenseCooldown / 1500f;
                        }
                        addedScale = Mod.instance.Helper.Reflection.GetField<float>(typeof(MeleeWeapon), "addedSwordScale").GetValue();
                        break;
                    case 2:
                        if (MeleeWeapon.clubCooldown > 0)
                        {
                            coolDownLevel = (float)MeleeWeapon.clubCooldown / 6000f;
                        }
                        addedScale = Mod.instance.Helper.Reflection.GetField<float>(typeof(MeleeWeapon), "addedClubScale").GetValue();
                        break;
                    case 1:
                        if (MeleeWeapon.daggerCooldown > 0)
                        {
                            coolDownLevel = (float)MeleeWeapon.daggerCooldown / 3000f;
                        }
                        addedScale = Mod.instance.Helper.Reflection.GetField<float>(typeof(MeleeWeapon), "addedDaggerScale").GetValue();
                        break;
                }
            }
            bool drawing_as_debris = drawShadow && drawStackNumber == StackDrawType.Hide;
            if (!drawShadow || drawing_as_debris)
            {
                addedScale = 0f;
            }
            spriteBatch.Draw(currTex.Texture, location + (((int)this.type == 1) ? new Vector2(38f, 25f) : new Vector2(32f, 32f)), currTex.Rect, color * transparency, 0f, new Vector2(8f, 8f), 4f * (scaleSize + addedScale), SpriteEffects.None, layerDepth);
            if (coolDownLevel > 0f && drawShadow && !drawing_as_debris && !this.isScythe() && (Game1.activeClickableMenu is not ShopMenu || scaleSize != 1f))
            {
                spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (64 - (int)(coolDownLevel * 64f)), 64, (int)(coolDownLevel * 64f)), Color.Red * 0.66f);
            }
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

        public static void DrawDuringUse(TexturedRect currTex, int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            var MeleeWeapon_center = new Vector2(1, 15);

            if (type != 1)
            {
                if (isOnSpecial)
                {
                    switch (type)
                    {
                        case 3:
                            switch (f.FacingDirection)
                            {
                                case 0:
                                    spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 44f), currTex.Rect, Color.White, (float)Math.PI * -9f / 16f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
                                    break;
                                case 1:
                                    spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 4f), currTex.Rect, Color.White, (float)Math.PI * -3f / 16f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 1) / 10000f));
                                    break;
                                case 2:
                                    spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 52f, playerPosition.Y + 4f), currTex.Rect, Color.White, -5.105088f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
                                    break;
                                case 3:
                                    spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 56f, playerPosition.Y - 4f), currTex.Rect, Color.White, (float)Math.PI * 3f / 16f, new Vector2(15f, 15f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 1) / 10000f));
                                    break;
                            }
                            break;
                        case 2:
                            switch (facingDirection)
                            {
                                case 1:
                                    switch (frameOfFarmerAnimation)
                                    {
                                        case 0:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 32f - 12f, playerPosition.Y - 80f), currTex.Rect, Color.White, (float)Math.PI * -3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 1:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 64f - 48f), currTex.Rect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 2:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 128f - 16f, playerPosition.Y - 64f - 12f), currTex.Rect, Color.White, (float)Math.PI * 3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 3:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 72f, playerPosition.Y - 64f + 16f - 32f), currTex.Rect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 4:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 96f, playerPosition.Y - 64f + 16f - 16f), currTex.Rect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 5:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 96f - 12f, playerPosition.Y - 64f + 16f), currTex.Rect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 6:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 96f - 16f, playerPosition.Y - 64f + 40f - 8f), currTex.Rect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 7:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 96f - 8f, playerPosition.Y + 40f), currTex.Rect, Color.White, (float)Math.PI * 5f / 16f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                    }
                                    break;
                                case 3:
                                    switch (frameOfFarmerAnimation)
                                    {
                                        case 0:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 4f + 8f, playerPosition.Y - 56f - 64f), currTex.Rect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 1:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 32f, playerPosition.Y - 32f), currTex.Rect, Color.White, (float)Math.PI * -5f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 2:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 12f, playerPosition.Y + 8f), currTex.Rect, Color.White, (float)Math.PI * -7f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 3:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 32f - 4f, playerPosition.Y + 8f), currTex.Rect, Color.White, (float)Math.PI * -3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 4:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 16f - 24f, playerPosition.Y + 64f + 12f - 64f), currTex.Rect, Color.White, 4.31969f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 5:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 20f, playerPosition.Y + 64f + 40f - 64f), currTex.Rect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 6:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 16f, playerPosition.Y + 64f + 56f), currTex.Rect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                        case 7:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 8f, playerPosition.Y + 64f + 64f), currTex.Rect, Color.White, 3.73064137f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                            break;
                                    }
                                    break;
                                default:
                                    switch (frameOfFarmerAnimation)
                                    {
                                        case 0:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 24f, playerPosition.Y - 21f - 8f - 64f), currTex.Rect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            break;
                                        case 1:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f - 64f + 4f), currTex.Rect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            break;
                                        case 2:
                                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 20f - 64f), currTex.Rect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            break;
                                        case 3:
                                            if (facingDirection == 2)
                                            {
                                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), currTex.Rect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            }
                                            else
                                            {
                                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 32f - 64f), currTex.Rect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            }
                                            break;
                                        case 4:
                                            if (facingDirection == 2)
                                            {
                                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), currTex.Rect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            }
                                            break;
                                        case 5:
                                            if (facingDirection == 2)
                                            {
                                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f - 20f), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            }
                                            break;
                                        case 6:
                                            if (facingDirection == 2)
                                            {
                                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 54f), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            }
                                            break;
                                        case 7:
                                            if (facingDirection == 2)
                                            {
                                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 58f), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                            }
                                            break;
                                    }
                                    if (f.FacingDirection == 0)
                                    {
                                        f.FarmerRenderer.draw(spriteBatch, f.FarmerSprite, f.FarmerSprite.SourceRect, f.getLocalPosition(Game1.viewport), new Vector2(0f, (f.yOffset + 128f - (float)(f.GetBoundingBox().Height / 2)) / 4f + 4f), Math.Max(0f, (float)f.getStandingY() / 10000f + 0.0099f), Color.White, 0f, f);
                                    }
                                    break;
                            }
                            break;
                    }
                    return;
                }
                switch (facingDirection)
                {
                    case 1:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 40f, playerPosition.Y - 64f + 8f), currTex.Rect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
                                break;
                            case 1:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 64f + 28f), currTex.Rect, Color.White, 0f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
                                break;
                            case 2:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 16f), currTex.Rect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
                                break;
                            case 3:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 4f), currTex.Rect, Color.White, (float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 4:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 28f, playerPosition.Y + 4f), currTex.Rect, Color.White, (float)Math.PI * 5f / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 5:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 6:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 7:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 12f), currTex.Rect, Color.White, 1.96349537f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                        }
                        break;
                    case 3:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 64f - 16f), currTex.Rect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
                                break;
                            case 1:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 48f, playerPosition.Y - 64f + 20f), currTex.Rect, Color.White, 0f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
                                break;
                            case 2:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 64f + 32f, playerPosition.Y + 16f), currTex.Rect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
                                break;
                            case 3:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 4f, playerPosition.Y + 44f), currTex.Rect, Color.White, -(float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 4:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 52f), currTex.Rect, Color.White, (float)Math.PI * -5f / 8f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 5:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), currTex.Rect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 6:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), currTex.Rect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                            case 7:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), currTex.Rect, Color.White, -5.105088f, MeleeWeapon_center, 4f, SpriteEffects.FlipVertically, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                                break;
                        }
                        break;
                    case 0:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), currTex.Rect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                            case 1:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), currTex.Rect, Color.White, -(float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                            case 2:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), currTex.Rect, Color.White, (float)Math.PI * -3f / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                            case 3:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), currTex.Rect, Color.White, -(float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                            case 4:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 40f), currTex.Rect, Color.White, 0f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                            case 5:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), currTex.Rect, Color.White, (float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                            case 6:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), currTex.Rect, Color.White, (float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                            case 7:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 44f, playerPosition.Y + 64f), currTex.Rect, Color.White, -1.96349537f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
                                break;
                        }
                        break;
                    case 2:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), currTex.Rect, Color.White, (float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                            case 1:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), currTex.Rect, Color.White, (float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                            case 2:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 40f, playerPosition.Y), currTex.Rect, Color.White, (float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                            case 3:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                            case 4:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), currTex.Rect, Color.White, (float)Math.PI, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                            case 5:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), currTex.Rect, Color.White, 3.53429174f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                            case 6:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), currTex.Rect, Color.White, 3.53429174f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                            case 7:
                                spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 64f), currTex.Rect, Color.White, -5.105088f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                                break;
                        }
                        break;
                }
                return;
            }
            frameOfFarmerAnimation %= 2;
            switch (facingDirection)
            {
                case 1:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 16f), currTex.Rect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 24f), currTex.Rect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                            break;
                    }
                    break;
                case 3:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 16f, playerPosition.Y - 16f), currTex.Rect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 8f, playerPosition.Y - 24f), currTex.Rect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
                            break;
                    }
                    break;
                case 0:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 40f), currTex.Rect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 48f), currTex.Rect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32) / 10000f));
                            break;
                    }
                    break;
                case 2:
                    switch (frameOfFarmerAnimation)
                    {
                        case 0:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 12f), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                            break;
                        case 1:
                            spriteBatch.Draw(currTex.Texture, new Vector2(playerPosition.X + 21f, playerPosition.Y), currTex.Rect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
                            break;
                    }
                    break;
            }
        }

        public override Item getOne()
        {
            var ret = new CustomMeleeWeapon(this.Data);
            // TODO: the field from tailoring boots over another?
            ret._GetOneFrom(this);
            return ret;
        }

        public void RecalculateAppliedForges(bool force = false)
        {
            if (this.enchantments.Count == 0 && !force)
            {
                return;
            }
            foreach (BaseEnchantment enchantment2 in this.enchantments)
            {
                if (enchantment2.IsForge())
                {
                    enchantment2.UnapplyTo(this);
                }
            }

            this.type.Value = (int)this.Data.Type;
            this.minDamage.Value = this.Data.MinimumDamage;
            this.maxDamage.Value = this.Data.MaximumDamage;
            this.knockback.Value = (float)this.Data.Knockback;
            this.speed.Value = this.Data.Speed;
            this.addedPrecision.Value = this.Data.Accuracy;
            this.addedDefense.Value = this.Data.Defense;
            this.addedAreaOfEffect.Value = this.Data.ExtraSwingArea;
            this.critChance.Value = (float)this.Data.CritChance;
            this.critMultiplier.Value = (float)this.Data.CritMultiplier;

            foreach (BaseEnchantment enchantment in this.enchantments)
            {
                if (enchantment.IsForge())
                {
                    enchantment.ApplyTo(this);
                }
            }
        }
    }
}
