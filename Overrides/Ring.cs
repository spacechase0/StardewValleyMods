using Microsoft.Xna.Framework;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;
using System;

namespace JsonAssets.Overrides
{
    public static class RingLoadDisplayFieldsHook
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast")]
        public static bool Prefix(StardewValley.Objects.Ring __instance, ref bool __result)
        {
            try
            {
                if (Game1.objectInformation == null || !((NetFieldBase<int, NetInt>)__instance.indexInTileSheet != (NetInt)null))
                {
                    __result = false;
                    return false;
                }
                string[] strArray = Game1.objectInformation[(int)((NetFieldBase<int, NetInt>)__instance.indexInTileSheet)].Split('/');
                __instance.displayName = strArray[4];
                __instance.description = strArray[5];
                __result = true;
                return false;
            }
            catch ( Exception e )
            {
                Log.error("Exception doing ring stuff! Ring index: " + __instance.indexInTileSheet + "\nException: " + e);
                __result = false;
                return false;
            }
        }
    }
}
