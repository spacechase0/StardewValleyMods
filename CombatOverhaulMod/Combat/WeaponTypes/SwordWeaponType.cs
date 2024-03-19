using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CombatOverhaulMod.FightStamina;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;

namespace CombatOverhaulMod.Combat.WeaponTypes
{
    internal class SwordWeaponType : WeaponType
    {
        private ConditionalWeakTable<MeleeWeapon, Holder<int>> combos = new();

        public override float BaseStaminaUsage => 0.25f;
        public override void StartSwipe(MeleeWeapon weapon, Farmer who)
        {
            var ssf = Mod.instance.Helper.Reflection.GetField<float>(weapon, "swipeSpeed");
            float swipeSpeed = ssf.GetValue();

            DoSwipe(weapon, who, swipeSpeed);
            //weapon.doSwipe(weapon.type, who.Position, who.FacingDirection, swipeSpeed / (float)(((int)weapon.type == 2) ? 5 : 8), who);
            who.lastClick = Vector2.Zero;
            Vector2 actionTile2 = who.GetToolLocation(ignoreClick: true);
            weapon.DoDamage(who.currentLocation, (int)actionTile2.X, (int)actionTile2.Y, who.FacingDirection, 1, who);
        }

        public override void DoSwipeAnimation(MeleeWeapon weapon, Farmer who, float speed)
        {
            bool anotherClick = Mod.instance.Helper.Reflection.GetField<bool>(weapon, "anotherClick").GetValue();

            Holder<int> combo = combos.GetOrCreateValue(weapon);
            int move = 0;
            if (anotherClick)
            {
                move = combo.Value;
                ++combo.Value;
            }
            else
            {
                combo.Value = 1;
            }
            if (move > 2)
            {
                // Reset the combo
                move = 0;
                combo.Value = 1;
            }

            void FixBackwardsAnim()
            {
                var fs = (who.Sprite as FarmerSprite);
                fs.currentAnimationIndex = Math.Min(fs.currentAnimation.Count - 1, 0);
                if (fs.CurrentAnimationFrame.milliseconds == 0)
                {
                    var f0 = fs.CurrentAnimation[0];
                    var fe = fs.CurrentAnimation[fs.CurrentAnimation.Count - 1];
                    fs.CurrentAnimation[0] = new(f0.frame, f0.milliseconds, f0.positionOffset, f0.secondaryArm, f0.flip, fe.frameStartBehavior, fe.frameEndBehavior, f0.xOffset);
                    fs.CurrentAnimation[fs.CurrentAnimation.Count - 1] = new(fe.frame, fe.milliseconds, fe.positionOffset, fe.secondaryArm, fe.flip, f0.frameStartBehavior, f0.frameEndBehavior, fe.xOffset);
                }
                fs.interval = fs.CurrentAnimationFrame.milliseconds;
                fs.timer = 0;
            }

            if (move == 0 || move == 1)
            {
                if (who.CurrentTool == weapon)
                {
                    switch (who.FacingDirection)
                    {
                        case 0:
                            if ( move == 0 )
                                ((FarmerSprite)who.Sprite).animateOnce(248, speed, 6);
                            else
                            {
                                ((FarmerSprite)who.Sprite).animateBackwardsOnce(248, speed);
                                FixBackwardsAnim();
                            }
                            weapon.Update(0, 0, who);
                            break;
                        case 1:
                            if (move == 0)
                                ((FarmerSprite)who.Sprite).animateOnce(240, speed, 6);
                            else
                            {
                                (who.Sprite as FarmerSprite).animateBackwardsOnce(240, speed);
                                FixBackwardsAnim();
                            }
                            weapon.Update(1, 0, who);
                            break;
                        case 2:
                            if (move == 0)
                                ((FarmerSprite)who.Sprite).animateOnce(232, speed, 6);
                            else
                            {
                                ((FarmerSprite)who.Sprite).animateBackwardsOnce(232, speed);
                                FixBackwardsAnim();
                            }
                            weapon.Update(2, 0, who);
                            break;
                        case 3:
                            if (move == 0)
                                ((FarmerSprite)who.Sprite).animateOnce(256, speed, 6);
                            else
                            {
                                ((FarmerSprite)who.Sprite).animateBackwardsOnce(256, speed);
                                FixBackwardsAnim();
                            }
                            weapon.Update(3, 0, who);
                            break;
                    }
                }
            }
            else if (move == 2)
            {
                switch (who.FacingDirection)
                {
                    case 0:
                        ((FarmerSprite)who.Sprite).animateOnce(276, speed, 2);
                        weapon.Update(0, 0, who);
                        break;
                    case 1:
                        ((FarmerSprite)who.Sprite).animateOnce(274, speed, 2);
                        weapon.Update(1, 0, who);
                        break;
                    case 2:
                        ((FarmerSprite)who.Sprite).animateOnce(272, speed, 2);
                        weapon.Update(2, 0, who);
                        break;
                    case 3:
                        ((FarmerSprite)who.Sprite).animateOnce(278, speed, 2);
                        weapon.Update(3, 0, who);
                        break;
                }
            }

            if (who.ShouldHandleAnimationSound())
                who.currentLocation.localSound("swordswipe");
        }

        public override void DrawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, bool isOnSpecial, Texture2D texture, Rectangle sourceRect, float sort_behind_layer, float sort_layer)
        {
            Vector2 MeleeWeapon_center = new Vector2(1f, 15f);

            int move = combos.GetOrCreateValue(f.CurrentTool as MeleeWeapon).Value;
            // move will be one higher than in DoSwipeAnimation

            if (move == 2)
                frameOfFarmerAnimation = 6 - frameOfFarmerAnimation;

            if (isOnSpecial)
            {
                switch (f.FacingDirection)
                {
                    case 0:
                        spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 44f), sourceRect, Color.White, (float)Math.PI * -9f / 16f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                        break;
                    case 1:
                        spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI * -3f / 16f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                        break;
                    case 2:
                        spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 52f, playerPosition.Y + 4f), sourceRect, Color.White, -5.105088f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                        break;
                    case 3:
                        spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 56f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI * 3f / 16f, new Vector2(15f, 15f), 4f, SpriteEffects.FlipHorizontally, sort_layer);
                        break;
                }
            }
            else if (move == 3)
            {
                frameOfFarmerAnimation %= 2;
                switch (facingDirection)
                {
                    case 1:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 24f), sourceRect, Color.White, (float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                    case 3:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 16f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 8f, playerPosition.Y - 24f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                    case 0:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 40f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 48f), sourceRect, Color.White, -(float)Math.PI / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                        }
                        break;
                    case 2:
                        switch (frameOfFarmerAnimation)
                        {
                            case 0:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 12f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
                            case 1:
                                spriteBatch.Draw(texture, new Vector2(playerPosition.X + 21f, playerPosition.Y), sourceRect, Color.White, (float)Math.PI * 3f / 4f, MeleeWeapon_center, 4f, SpriteEffects.None, sort_layer);
                                break;
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

        /*
        public override Rectangle GetNormalDamageArea(MeleeWeapon weapon, int x, int y, int facingDirection, Rectangle wielderBoundingBox, int indexInCurrentAnimation)
        {
            Rectangle areaOfEffect = Rectangle.Empty;
            int horizontalYOffset = 0;
            int upHeightOffset = 0;
            int width;
            int height;

            width = 64;
            height = 64;
            horizontalYOffset = -32;
            upHeightOffset = 0;

            int move = combos.GetOrCreateValue(weapon).Value;

            if (move == 3)
            {
                // vanilla dagger, TODO extend
                switch (facingDirection)
                {
                    case 0:
                        areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset, width / 2, height + upHeightOffset);
                        //tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
                        areaOfEffect.Offset(20, -16);
                        areaOfEffect.Height += 16;
                        areaOfEffect.Width += 20;
                        break;
                    case 1:
                        areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset, height, width);
                        //tileLocation1 = new Vector2(areaOfEffect.Center.X / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
                        areaOfEffect.Offset(-4, 0);
                        areaOfEffect.Width += 16;
                        break;
                    case 2:
                        areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, height);
                        //tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
                        areaOfEffect.Offset(12, -8);
                        areaOfEffect.Width -= 21;
                        break;
                    case 3:
                        areaOfEffect = new Rectangle(wielderBoundingBox.Left - height, y - height / 2 + horizontalYOffset, height, width);
                        //tileLocation1 = new Vector2(areaOfEffect.Left / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
                        areaOfEffect.Offset(-12, 0);
                        areaOfEffect.Width += 16;
                        break;
                }
            }
            else
            {
                switch (facingDirection)
                {
                    case 0:
                        areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset, width, height + upHeightOffset);
                        //tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
                        switch (indexInCurrentAnimation)
                        {
                            case 5:
                                areaOfEffect.Offset(76, -32);
                                break;
                            case 4:
                                areaOfEffect.Offset(56, -32);
                                areaOfEffect.Height += 32;
                                break;
                            case 3:
                                areaOfEffect.Offset(40, -60);
                                areaOfEffect.Height += 48;
                                break;
                            case 2:
                                areaOfEffect.Offset(-12, -68);
                                areaOfEffect.Height += 48;
                                break;
                            case 1:
                                areaOfEffect.Offset(-48, -56);
                                areaOfEffect.Height += 32;
                                break;
                            case 0:
                                areaOfEffect.Offset(-60, -12);
                                break;
                        }
                        break;
                    case 2:
                        areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, height);
                        //tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
                        switch (indexInCurrentAnimation)
                        {
                            case 0:
                                areaOfEffect.Offset(72, -92);
                                break;
                            case 1:
                                areaOfEffect.Offset(56, -32);
                                break;
                            case 2:
                                areaOfEffect.Offset(40, -28);
                                break;
                            case 3:
                                areaOfEffect.Offset(-12, -8);
                                break;
                            case 4:
                                areaOfEffect.Offset(-80, -24);
                                areaOfEffect.Width += 32;
                                break;
                            case 5:
                                areaOfEffect.Offset(-68, -44);
                                break;
                        }
                        break;
                    case 1:
                        areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset, height, width);
                        //tileLocation1 = new Vector2(areaOfEffect.Center.X / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
                        switch (indexInCurrentAnimation)
                        {
                            case 0:
                                areaOfEffect.Offset(-44, -84);
                                break;
                            case 1:
                                areaOfEffect.Offset(4, -44);
                                break;
                            case 2:
                                areaOfEffect.Offset(12, -4);
                                break;
                            case 3:
                                areaOfEffect.Offset(12, 37);
                                break;
                            case 4:
                                areaOfEffect.Offset(-28, 60);
                                break;
                            case 5:
                                areaOfEffect.Offset(-60, 72);
                                break;
                        }
                        break;
                    case 3:
                        areaOfEffect = new Rectangle(wielderBoundingBox.Left - height, y - height / 2 + horizontalYOffset, height, width);
                        //tileLocation1 = new Vector2(areaOfEffect.Left / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
                        //tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
                        switch (indexInCurrentAnimation)
                        {
                            case 0:
                                areaOfEffect.Offset(56, -76);
                                break;
                            case 1:
                                areaOfEffect.Offset(-8, -56);
                                break;
                            case 2:
                                areaOfEffect.Offset(-16, -4);
                                break;
                            case 3:
                                areaOfEffect.Offset(0, 37);
                                break;
                            case 4:
                                areaOfEffect.Offset(24, 60);
                                break;
                            case 5:
                                areaOfEffect.Offset(64, 64);
                                break;
                        }
                        break;
                }
            }
            areaOfEffect.Inflate(weapon.addedAreaOfEffect, weapon.addedAreaOfEffect);
            return areaOfEffect;
        }
        */
    }
}
