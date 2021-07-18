using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="MeleeWeapon"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class MeleeWeaponPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<MeleeWeapon>(nameof(MeleeWeapon.drawDuringUse), new[] { typeof(int), typeof(int), typeof(SpriteBatch), typeof(Vector2), typeof(Farmer), typeof(Rectangle), typeof(int), typeof(bool) }),
                prefix: this.GetHarmonyMethod(nameof(Before_DrawDuringUse))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="MeleeWeapon.drawDuringUse(int,int,SpriteBatch,Vector2,Farmer,Rectangle,int,bool)"/>.</summary>
        private static bool Before_DrawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            if (f.CurrentTool is ICustomWeaponDraw tool)
            {
                tool.Draw(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, sourceRect, type, isOnSpecial);
                return false;
            }

            return true;
        }
    }
}
