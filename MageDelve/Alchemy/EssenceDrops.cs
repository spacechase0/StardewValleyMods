using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MageDelve.Skill;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace MageDelve.Alchemy
{
    [HarmonyPatch(typeof(StardewValley.Object), "cutWeed")]
    public static class DropEarthEssencePatch1
    {
        public static void Postfix(StardewValley.Object __instance, Farmer who)
        {
            long farmerId = who?.UniqueMultiplayerID ?? 0;
            if (Game1.random.NextDouble() < 0.001 * (who.professions.Contains(ArcanaSkill.MoreEssencesProfession.GetVanillaId()) ? 2 : 1) )
                Game1.createObjectDebris("(O)spacechase0.MageDelve_EarthEssence", (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, farmerId, __instance.Location);
        }
    }

    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.OnStoneDestroyed))]
    public static class DropEarthEssencePatch2
    {
        public static void Postfix(GameLocation __instance, int x, int y, Farmer who)
        {
            long farmerId = who?.UniqueMultiplayerID ?? 0;
            if (who != null && Game1.random.NextDouble() < 0.0025 * (who.professions.Contains(ArcanaSkill.MoreEssencesProfession.GetVanillaId()) ? 2 : 1))
                Game1.createObjectDebris("(O)spacechase0.MageDelve_EarthEssence", x, y, farmerId, __instance);
        }
    }

    [HarmonyPatch(typeof(FishingRod), "doneFishing")]
    public static class DropWaterEssencePatch
    {
        public static void Postfix(Farmer who, bool consumeBaitAndTackle)
        {
            long farmerId = who?.UniqueMultiplayerID ?? 0;
            if (Game1.random.NextDouble() < 0.025 * (who.professions.Contains(ArcanaSkill.MoreEssencesProfession.GetVanillaId()) ? 2 : 1))
                Game1.createObjectDebris("(O)spacechase0.MageDelve_WaterEssence", (int)who.Tile.X, (int)who.Tile.Y, farmerId, who.currentLocation);
        }
    }

    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.monsterDrop))]
    public static class DropVariousMonsterEssencesPatch
    {
        public static void Postfix(GameLocation __instance, Monster monster, int x, int y, Farmer who)
        {
            if (monster.isGlider.Value && Game1.random.NextDouble() < 0.045 * (who.professions.Contains(ArcanaSkill.MoreEssencesProfession.GetVanillaId()) ? 2 : 1))
            {
                Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_AirEssence"), new(x, y), Game1.random.Next(4), __instance);
            }
            if (__instance is MineShaft ms)
            {
                if (ms.mineLevel >= 40 && ms.mineLevel < 80 && Game1.random.NextDouble() < 0.075 * (who.professions.Contains(ArcanaSkill.MoreEssencesProfession.GetVanillaId()) ? 2 : 1))
                {
                    Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_WaterEssence"), new(x, y), Game1.random.Next(4), __instance);
                }
                if (ms.mineLevel >= 80 && ms.mineLevel < 120 && Game1.random.NextDouble() < 0.04 * (who.professions.Contains(ArcanaSkill.MoreEssencesProfession.GetVanillaId()) ? 2 : 1))
                {
                    Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_FireEssence"), new(x, y), Game1.random.Next(4), __instance);
                }
            }
            if (__instance is VolcanoDungeon vd && Game1.random.NextDouble() < 0.085 * (who.professions.Contains(ArcanaSkill.MoreEssencesProfession.GetVanillaId()) ? 2 : 1))
            {
                Game1.createItemDebris(ItemRegistry.Create("(O)spacechase0.MageDelve_FireEssence"), new(x, y), Game1.random.Next(4), __instance);
            }
        }
    }
}
