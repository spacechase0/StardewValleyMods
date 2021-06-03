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
        // So I just realized this is exactly the same as the vanilla method.
        // The key here though is we try/catch around it when something fails, but don't let the vanilla method run still
        // This way if it fails (like for some reason equipped custom rings do for farmhands when first connecting),
        //  the game will still run.
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
                return false;
            }
        }
    }
}
