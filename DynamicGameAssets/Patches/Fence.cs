using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using StardewValley;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch( typeof( Fence ), nameof( Fence.CanRepairWithThisItem ) )]
    public static class FenceCanRepairWithThisPatch
    {
        public static bool Prefix( Fence __instance, Item item, ref bool __result )
        {
            if ( __instance is CustomFence cfence )
            {
                __result = cfence.CanRepairWithThisItem( item );
                return false;
            }

            return true;
        }
    }
}
