using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch( typeof( FruitTree ), nameof( FruitTree.shake ) )]
    public static class FruitTreeShakePatch
    {
        public static bool Prefix( FruitTree __instance, Vector2 tileLocation, bool doEvenIfStillShaking, GameLocation location )
        {
            if ( __instance is CustomFruitTree cftree )
            {
                cftree.Shake( tileLocation, doEvenIfStillShaking, location );
                return false;
            }

            return true;
        }
    }
}
