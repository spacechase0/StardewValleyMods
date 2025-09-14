using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;

namespace CombatOverhaulMod.Combat.WeaponTypes
{
    internal class ClubWeaponType : WeaponType
    {
        public override float BaseStaminaUsage => 0.3f;
        public override float BaseSwipeSpeed => base.BaseSwipeSpeed * 1.5f;


        public override void StartSwipe(MeleeWeapon weapon, Farmer who)
        {
            var ssf = Mod.instance.Helper.Reflection.GetField<float>(weapon, "swipeSpeed");
            float swipeSpeed = ssf.GetValue();

            weapon.doSwipe(weapon.type, who.Position, who.FacingDirection, swipeSpeed / (float)(((int)weapon.type == 2) ? 5 : 8), who);
            who.lastClick = Vector2.Zero;
            Vector2 actionTile2 = who.GetToolLocation(ignoreClick: true);
            weapon.DoDamage(who.currentLocation, (int)actionTile2.X, (int)actionTile2.Y, who.FacingDirection, 1, who);
        }

        public override void DoSwipeAnimation(MeleeWeapon weapon, Farmer who, float speed)
        {
            if (who.CurrentTool == weapon)
            {
                switch (who.FacingDirection)
                {
                    case 0:
                        ((FarmerSprite)who.Sprite).animateOnce(248, speed, 8);
                        weapon.Update(0, 0, who);
                        break;
                    case 1:
                        ((FarmerSprite)who.Sprite).animateOnce(240, speed, 8);
                        weapon.Update(1, 0, who);
                        break;
                    case 2:
                        ((FarmerSprite)who.Sprite).animateOnce(232, speed, 8);
                        weapon.Update(2, 0, who);
                        break;
                    case 3:
                        ((FarmerSprite)who.Sprite).animateOnce(256, speed, 8);
                        weapon.Update(3, 0, who);
                        break;
                }
            }
            if ( who.ShouldHandleAnimationSound() )
                who.currentLocation.localSound("clubswipe");
        }

        public override void DrawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, bool isOnSpecial, Texture2D texture, Rectangle sourceRect, float sort_behind_layer, float sort_layer)
        {
            Vector2 MeleeWeapon_center = new Vector2(1f, 15f);

            if (isOnSpecial)
            {
                switch (facingDirection)
                {
                    case 1:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f - 12f, playerPosition.Y - 80f), sourceRect, Color.White, (float)Math.PI * -3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 64f - 48f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 2:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 128f - 16f, playerPosition.Y - 64f - 12f), sourceRect, Color.White, (float)Math.PI * 3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 3:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 72f, playerPosition.Y - 64f + 16f - 32f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 4:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f, playerPosition.Y - 64f + 16f - 16f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 5:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 12f, playerPosition.Y - 64f + 16f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 6:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 16f, playerPosition.Y - 64f + 40f - 8f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 7:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 96f - 8f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * 5f / 16f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                    case 3:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f + 8f, playerPosition.Y - 56f - 64f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f, playerPosition.Y - 32f), sourceRect, Color.White, (float)Math.PI * -5f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 2:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 12f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI * -7f / 8f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 3:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 32f - 4f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 4:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f - 24f, playerPosition.Y + 64f + 12f - 64f), sourceRect, Color.White, 4.31969f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 5:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 20f, playerPosition.Y + 64f + 40f - 64f), sourceRect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 6:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y + 64f + 56f), sourceRect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 7:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 8f, playerPosition.Y + 64f + 64f), sourceRect, Color.White, 3.73064137f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                    default:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 24f, playerPosition.Y - 21f - 8f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f - 64f + 4f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 2:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 20f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 3:
                                if (facingDirection == 2)
                                {
                                    spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), sourceRect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                }
                                else
                                {
                                    spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 32f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                }
                                break;
                            case 4:
                                if (facingDirection == 2)
                                {
                                    spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), sourceRect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                }
                                break;
                            case 5:
                                if (facingDirection == 2)
                                {
                                    spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f - 20f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                }
                                break;
                            case 6:
                                if (facingDirection == 2)
                                {
                                    spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 54f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                }
                                break;
                            case 7:
                                if (facingDirection == 2)
                                {
                                    spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 58f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
                                }
                                break;
                        }
                        if (f.FacingDirection == 0)
                        {
                            f.FarmerRenderer.draw(spriteBatch, f.FarmerSprite, f.FarmerSprite.SourceRect, f.getLocalPosition(Game1.viewport), new Vector2(0f, (f.yOffset + 128f - (float)(f.GetBoundingBox().Height / 2)) / 4f + 4f), sort_layer, Color.White, 0f, f);
                        }
                        break;
                }
            }
            else
            {
                switch (facingDirection)
                {
                    case 1:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 40f, playerPosition.Y - 64f + 8f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_behind_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 64f + 28f), sourceRect, Color.White, 0f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_behind_layer);
                                break;
                            case 2:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 3:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 4:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 28f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 5f / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 5:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 6:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 7:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 12f), sourceRect, Color.White, 1.96349537f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                    case 3:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 64f - 16f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, sort_behind_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 48f, playerPosition.Y - 64f + 20f), sourceRect, Color.White, 0f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, sort_behind_layer);
                                break;
                            case 2:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 64f + 32f, playerPosition.Y + 16f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, sort_layer);
                                break;
                            case 3:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 4f, playerPosition.Y + 44f), sourceRect, Color.White, -(float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, sort_layer);
                                break;
                            case 4:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 52f), sourceRect, Color.White, (float)Math.PI * -5f / 8f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, sort_layer);
                                break;
                            case 5:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, sort_layer);
                                break;
                            case 6:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.FlipHorizontally, sort_layer);
                                break;
                            case 7:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), sourceRect, Color.White, -5.105088f, MeleeWeapon_center, 4f, SpriteEffects.FlipVertically, sort_layer);
                                break;
                        }
                        break;
                    case 0:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), sourceRect, Color.White, -(float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 2:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, (float)Math.PI * -3f / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 3:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, -(float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 4:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 40f), sourceRect, Color.White, 0f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 5:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), sourceRect, Color.White, (float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 6:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), sourceRect, Color.White, (float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 7:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 44f, playerPosition.Y + 64f), sourceRect, Color.White, -1.96349537f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                    case 2:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 8f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), sourceRect, Color.White, (float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 2:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 40f, playerPosition.Y), sourceRect, Color.White, (float)Math.PI / 2f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 3:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 4:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 5:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.53429174f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 6:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.53429174f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 7:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 64f), sourceRect, Color.White, -5.105088f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                }
            }
        }

        public override Rectangle GetNormalDamageArea(MeleeWeapon weapon, int x, int y, int facingDirection, Rectangle wielderBoundingBox, int indexInCurrentAnimation)
        {
            return default(Rectangle); // TODO
        }
    }
}
