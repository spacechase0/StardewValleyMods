using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace SpaceCore.Overrides
{
    public class CustomWeaponDrawPatch
    {
        public static bool Prefix(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
            if (f.CurrentTool is ICustomWeaponDraw tool)
            {
                tool.draw(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, sourceRect, type, isOnSpecial);
                return false;
            }

            return true;
        }
    }
}
