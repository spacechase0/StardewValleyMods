using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using StardewValley.Objects;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch(typeof(Furniture), nameof( Furniture.rotate ))]
    public static class FurnitureRotatePatch
    {
        public static bool Prefix( Furniture __instance )
        {
            if ( __instance is CustomBasicFurniture cbf )
            {
                if ( __instance.rotations.Value > 1 )
                {
                    __instance.currentRotation.Value = ( __instance.currentRotation.Value + 1 ) % __instance.rotations.Value;
                    cbf.UpdateRotation();
                }
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Furniture ), nameof( Furniture.updateRotation ) )]
    public static class FurnitureUpdateRotationPatch
    {
        public static bool Prefix( Furniture __instance )
        {
            if ( __instance is CustomBasicFurniture cbf )
            {
                cbf.UpdateRotation();
                return false;
            }

            return true;
        }
    }
}
