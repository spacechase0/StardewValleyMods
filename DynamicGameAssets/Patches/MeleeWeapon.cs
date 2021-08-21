using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using StardewValley.Tools;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch( typeof( MeleeWeapon ), nameof( MeleeWeapon.RecalculateAppliedForges ) )]
    public static class MeleeWeaponRecalculateForgesPatch
    {
        public static bool Prefix(MeleeWeapon __instance, bool force)
        {
            if ( __instance is CustomMeleeWeapon cmw )
            {
                cmw.RecalculateAppliedForges( force );
                return false;
            }

            return true;
        }
    }
}
