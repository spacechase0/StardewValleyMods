using System;
using System.Diagnostics.CodeAnalysis;
using SpaceShared;
using StardewValley;

namespace JsonAssets.Overrides
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is set by Harmony.")]
    [SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast")]
    public static class RingPatches
    {
        public static bool LoadDisplayFields_Prefix(StardewValley.Objects.Ring __instance, ref bool __result)
        {
            try
            {
                if (Game1.objectInformation == null || __instance.indexInTileSheet == null)
                {
                    __result = false;
                    return false;
                }
                string[] strArray = Game1.objectInformation[__instance.indexInTileSheet].Split('/');
                __instance.displayName = strArray[4];
                __instance.description = strArray[5];
                __result = true;
                return false;
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(LoadDisplayFields_Prefix)} for #{__instance?.indexInTileSheet}:\n{ex}");
                return true;
            }
        }
    }
}
