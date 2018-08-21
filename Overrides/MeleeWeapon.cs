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
    public class CustomWeaponDrawPatch
    {
        public static bool Prefix(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, SFarmer f, Rectangle sourceRect, int type, bool isOnSpecial)
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
