using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MoonMisadventures.Game.Locations;
using StardewValley;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( Game1 ), nameof( Game1.getLocationFromNameInLocationsList ) )]
    public static class Game1FetchDungeonInstancePatch
    {
        public static bool Prefix( string name, bool isStructure, ref GameLocation __result )
        {
            if ( name.StartsWith( AsteroidsDungeon.BaseLocationName ) )
            {
                __result = AsteroidsDungeon.GetLevelInstance( name );
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Game1 ), "UpdateLocations" )]
    public static class Game1UpdateDungeonLocationsPatch
    {
        public static void Postfix( GameTime time )
        {
            if ( Game1.menuUp && !Game1.IsMultiplayer )
            {
                return;
            }
            if ( Game1.IsClient )
            {
                return;
            }

            AsteroidsDungeon.UpdateLevels( time );
        }
    }

    [HarmonyPatch( typeof( Multiplayer ), nameof( Multiplayer.updateRoots ) )]
    public static class MultiplayerUpdateDungeonRootsPatch
    {
        public static void Postfix( Multiplayer __instance )
        {
            foreach ( var level in AsteroidsDungeon.activeLevels )
            {
                if ( level.Root != null )
                {
                    level.Root.Clock.InterpolationTicks = __instance.interpolationTicks();
                    __instance.updateRoot( level.Root );
                }
            }
        }
    }
}
