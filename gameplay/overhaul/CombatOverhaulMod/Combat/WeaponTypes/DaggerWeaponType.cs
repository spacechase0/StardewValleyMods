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
    internal class DaggerWeaponType : WeaponType
    {
        public override float BaseStaminaUsage => 0.175f;
        public override float BaseSwipeSpeed => base.BaseSwipeSpeed / 4;

        public override void StartSwipe(MeleeWeapon weapon, Farmer who)
        {
            if (who.IsLocalPlayer)
                who.currentLocation.playSound("daggerswipe");

            float swipeSpeed = Mod.instance.Helper.Reflection.GetField<float>(weapon, "swipeSpeed").GetValue();

            // TODO: Modify this to aim where the mouse clicked

            switch (who.FacingDirection)
            {
                case 0:
                    ((FarmerSprite)who.Sprite).animateOnce(276, swipeSpeed, 2);
                    weapon.Update(0, 0, who);
                    break;
                case 1:
                    ((FarmerSprite)who.Sprite).animateOnce(274, swipeSpeed, 2);
                    weapon.Update(1, 0, who);
                    break;
                case 2:
                    ((FarmerSprite)who.Sprite).animateOnce(272, swipeSpeed, 2);
                    weapon.Update(2, 0, who);
                    break;
                case 3:
                    ((FarmerSprite)who.Sprite).animateOnce(278, swipeSpeed, 2);
                    weapon.Update(3, 0, who);
                    break;
            }
            Vector2 actionTile = who.GetToolLocation(ignoreClick: true);
            weapon.DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
        }

        public override void DrawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, bool isOnSpecial, Texture2D texture, Rectangle sourceRect, float sort_behind_layer, float sort_layer)
        {
            Vector2 MeleeWeapon_center = new Vector2(1f, 15f);

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
    }
}
