using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace DynamicGameAssets.Patches
{

    [HarmonyPatch(typeof(Game1), nameof(Game1.drawTool), typeof(Farmer), typeof(int))]
    public static class Game1DrawToolPatch
    {
        public static bool Prefix(Farmer f, int currentToolIndex)
        {
            if (f.CurrentTool is CustomMeleeWeapon || f.FarmerSprite.isUsingWeapon() && Mod.itemLookup.ContainsKey(f.FarmerSprite.CurrentToolIndex))
            {
                Rectangle sourceRectangleForTool = new Rectangle(currentToolIndex * 16 % Game1.toolSpriteSheet.Width, currentToolIndex * 16 / Game1.toolSpriteSheet.Width * 16, 16, 32);
                Vector2 fPosition = f.getLocalPosition(Game1.viewport) + f.jitter + f.armOffset;
                float tool_draw_layer_offset = 0f;
                if (f.FacingDirection == 0)
                {
                    tool_draw_layer_offset = -0.002f;
                }


                if (f.CurrentTool is CustomMeleeWeapon cmw)
                {
                    CustomMeleeWeapon.DrawDuringUse(cmw.Data.GetTexture(), ((FarmerSprite)f.Sprite).currentAnimationIndex, f.FacingDirection, Game1.spriteBatch, fPosition, f, MeleeWeapon.getSourceRect(cmw.getDrawnItemIndex()), cmw.type, cmw.isOnSpecial);
                }
                else
                {
                    var currTex = (Mod.Find(Mod.itemLookup[f.FarmerSprite.CurrentToolIndex]) as MeleeWeaponPackData).GetTexture();
                    CustomMeleeWeapon.DrawDuringUse(currTex, ((FarmerSprite)f.Sprite).currentAnimationIndex, f.FacingDirection, Game1.spriteBatch, fPosition, f, MeleeWeapon.getSourceRect(f.FarmerSprite.CurrentToolIndex), f.FarmerSprite.getWeaponTypeFromAnimation(), isOnSpecial: false);
                }

                return false;
            }

            return true;
        }

    }
}
