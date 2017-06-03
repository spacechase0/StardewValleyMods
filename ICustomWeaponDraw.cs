using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFarmer = StardewValley.Farmer;

namespace SpaceCore
{
    public interface ICustomWeaponDraw
    {
        void draw(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, SFarmer f, Rectangle sourceRect, int type, bool isOnSpecial);
    }
}
