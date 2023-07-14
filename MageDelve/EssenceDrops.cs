using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace MageDelve
{
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.OnStoneDestroyed))]
    public static class DropEarthEssencePatch
    {
        public static void Postfix(GameLocation __instance, int x, int y, Farmer who)
        {
            long farmerId = who?.UniqueMultiplayerID ?? 0;
            if ( Game1.random.NextDouble() < 0.0075 )
                Game1.createObjectDebris("(O)spacechase0.MageDelve_EarthEssence", x, y, farmerId, __instance);
        }
    }

    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.monsterDrop))]
    public static class DropVariousMonsterEssencesPatch
    {
        public static void Postfix(GameLocation __instance, Monster monster, int x, int y, Farmer who)
        {
            if (monster.isGlider.Value && Game1.random.NextDouble() < 0.045)
            {
                Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_AirEssence"), new(x, y), Game1.random.Next(4), __instance);
            }
            if (__instance is MineShaft ms)
            {
                if (ms.mineLevel >= 40 && ms.mineLevel < 80 && Game1.random.NextDouble() < 0.075 )
                {
                    Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_WaterEssence"), new(x, y), Game1.random.Next(4), __instance);
                }
                if (ms.mineLevel >= 80 && ms.mineLevel < 120 && Game1.random.NextDouble() < 0.04 )
                {
                    Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_FireEssence"), new(x, y), Game1.random.Next(4), __instance);
                }
            }
            if (__instance is VolcanoDungeon vd && Game1.random.NextDouble() < 0.085)
            {
                Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_FireEssence"), new(x, y), Game1.random.Next(4), __instance);
            }
        }
    }
}
