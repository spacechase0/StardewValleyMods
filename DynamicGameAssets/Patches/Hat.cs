using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Patches
{
    /*
    [HarmonyPatch( typeof( Hat ), nameof( Hat.draw ) )]
    public static class HatDrawPatch
    {
        public static bool Prefix( Hat  __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction )
        {
            if ( __instance is CustomHat ch )
            {
                ch.Draw( spriteBatch, location, scaleSize, transparency, layerDepth, direction );
                return false;
            }

            return true;
        }
    }
    */
}
