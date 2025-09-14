using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Game1"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class Game1Patcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.drawTool), new[] { typeof(Farmer), typeof(int) }),
                prefix: this.GetHarmonyMethod(nameof(Before_DrawTool))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Game1.drawTool(Farmer,int)"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_DrawTool(Farmer f, int currentToolIndex)
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


                if (f.CurrentTool is CustomMeleeWeapon weapon)
                {
                    CustomMeleeWeapon.DrawDuringUse(weapon.Data.GetTexture(), ((FarmerSprite)f.Sprite).currentAnimationIndex, f.FacingDirection, Game1.spriteBatch, fPosition, f, MeleeWeapon.getSourceRect(weapon.getDrawnItemIndex()), weapon.type, weapon.isOnSpecial);
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
