using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game;
using StardewValley;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( Background ), nameof( Background.update ) )]
    public static class BackgroundUpdatePatch
    {
        public static void Postfix( Background __instance, xTile.Dimensions.Rectangle viewport )
        {
            if ( __instance is SpaceBackground bg )
            {
                bg.Update( viewport );
            }
        }
    }

    [HarmonyPatch( typeof( Background ), nameof( Background.draw ) )]
    public static class BackgroundDrawPatch
    {
        public static void Postfix( Background __instance, SpriteBatch b )
        {
            if ( __instance is SpaceBackground bg )
            {
                bg.Draw( b );
            }
        }
    }
}
