using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MoonMisadventures.Game.Locations;
using SpaceShared;
using StardewValley;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch]
    public class CropMoonCropsPatch
    {
        // Need to do this so places like the island don't override it as well
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.FullName.Contains( "Steamworks.NET" ) )
                             from type in asm.GetTypes()
                             where type.IsSubclassOf( typeof( GameLocation ) )
                             select type;

            yield return AccessTools.Method( typeof( GameLocation ), nameof( GameLocation.CanPlantSeedsHere ) );
            foreach ( var subclass in subclasses )
            {
                var meth = subclass.GetMethod( nameof( GameLocation.CanPlantSeedsHere ) );
                if ( meth != null && meth.DeclaringType == subclass )
                    yield return meth;
            }
        }

        public static bool Prefix( GameLocation __instance, int crop_index, int tile_x, int tile_y, ref bool __result )
        {
            if ( crop_index == CropIds.LunarWheat.GetDeterministicHashCode() )
            {
                __result = __instance is LunarLocation;
                return false;
            }

            return true;
        }
    }
}
