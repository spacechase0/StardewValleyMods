using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Reflection;
using SFarmer = StardewValley.Farmer;

namespace SpaceCore.Overrides
{
    class NewMeleeWeapon
    {
        internal static void hijack( HarmonyInstance harmony )
        {
            harmony.Patch(typeof(MeleeWeapon).GetMethod("drawDuringUse", BindingFlags.Static | BindingFlags.Public),
                          new HarmonyMethod(typeof(NewMeleeWeapon).GetMethod("drawDuringUse_pre")),
                          null);
        }

        public static bool drawDuringUse_pre(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, SFarmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            if (f.CurrentTool is ICustomWeaponDraw)
            {
                (f.CurrentTool as ICustomWeaponDraw).draw(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, sourceRect, type, isOnSpecial);
                return false;
            }

            return true;
        }
    }
}
