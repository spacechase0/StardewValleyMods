using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MoonMisadventures.VirtualProperties;
using StardewValley;
using StardewValley.Monsters;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( Farmer ), "farmerInit" )]
    public static class FarmerInjectNetFieldsPatch
    {
        public static void Postfix( Farmer __instance )
        {
            __instance.NetFields.AddFields( __instance.get_necklaceItem() );
        }
    }

    [HarmonyPatch( typeof( FarmerTeam ), MethodType.Constructor )]
    public static class FarmerTeamInjectNetFieldsPatch
    {
        public static void Postfix( FarmerTeam __instance )
        {
            __instance.NetFields.AddFields( __instance.get_hasLunarKey() );
        }
    }

    [HarmonyPatch( typeof( Monster ), "initNetFields" )]
    public static class MonsterShockedFieldPatch
    {
        public static void Postfix( Monster __instance )
        {
            __instance.NetFields.AddFields( __instance.get_shocked() );
        }
    }
}
