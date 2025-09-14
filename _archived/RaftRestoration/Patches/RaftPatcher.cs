using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace RaftRestoration.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Raft"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class RaftPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Raft>(nameof(Raft.DoFunction)),
                prefix: this.GetHarmonyMethod(nameof(Before_DoFunction))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Raft.DoFunction"/>.</summary>
        public static bool Before_DoFunction(Raft __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            //x = (int) who.lastClick.X;
            //y = (int) who.lastClick.Y;
            who.CanMove = true;
            /*Rectangle position = new Rectangle(x - 32, y - 32, 64, 64);
            if (location.isCollidingPosition( position, Game1.viewport, true ))
            {
                Log.debug( "was colliding" );
                switch ( who.FacingDirection )
                {
                    case 0: y -= 64; break;
                    case 1: x += 64; break;
                    case 2: y += 64; break;
                    case 3: x -= 64; break;
                }
            }*/

            if (!who.isRafting && location.doesTileHaveProperty(x / 64, y / 64, "Water", "Back") != null)
            {
                who.isRafting = true;
                /*
                Rectangle position = new Rectangle(x - 32, y - 32, 64, 64);
                if (location.isCollidingPosition(position, Game1.viewport, true))
                {
                    who.isRafting = false;
                    return false;
                }
                */
                int xo = 0, yo = 0;
                /*switch (who.FacingDirection)
                {
                    case 0: yo -= 64; break;
                    case 1: xo += 64; break;
                    case 2: yo += 64; break;
                    case 3: xo -= 64; break;
                }*/
                who.xVelocity = who.FacingDirection == 1 ? 3f : (who.FacingDirection == 3 ? -3f : 0.0f);
                who.yVelocity = who.FacingDirection == 2 ? 3f : (who.FacingDirection == 0 ? -3f : 0.0f);
                who.Position = new Vector2((float)(x - 32) + xo, (float)(y - 32 - 32 - (y < who.getStandingY() ? 64 : 0)) + yo);
                Game1.playSound("dropItemInWater");
            }
            __instance.CurrentParentTileIndex = __instance.IndexOfMenuItemView;

            return false;
        }
    }
}
