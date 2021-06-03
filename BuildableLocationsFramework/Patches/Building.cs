using Harmony;
using StardewValley;
using StardewValley.Buildings;

namespace BuildableLocationsFramework.Patches
{
    [HarmonyPatch( typeof( Building ), nameof( Building.updateInteriorWarps ) )]
    public static class BuildingUpdateInteriorWarpsPatch
    {
        public static void Postfix( Building __instance, GameLocation interior )
        {
            var targetName = Mod.findOutdoorsOf( __instance )?.Name;
            if ( targetName == null )
                return;

            if ( interior == null )
                interior = __instance.indoors.Value;
            if ( interior == null )
                return;
            foreach ( Warp warp in interior.warps )
            {
                warp.TargetName = targetName;
            }
        }
    }
}
