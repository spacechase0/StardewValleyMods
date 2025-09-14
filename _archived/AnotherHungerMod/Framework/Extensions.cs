using System;
using StardewValley;

namespace AnotherHungerMod.Framework
{
    internal static class Extensions
    {
        public static float GetFullness(this Farmer player)
        {
            if (player != Game1.player)
                return -1;

            return Math.Min(ModDataManager.GetFullness(player), Mod.Config.MaxFullness);
        }

        public static void UseFullness(this Farmer player, float amt)
        {
            if (player != Game1.player)
                return;

            float fullness = ModDataManager.GetFullness(player);
            ModDataManager.SetFullness(player, fullness - amt);
        }

        public static int GetMaxFullness(this Farmer _)
        {
            return Mod.Config.MaxFullness;
        }

        public static bool HasFedSpouse(this Farmer player)
        {
            return ModDataManager.GetHasFedSpouse(player);
        }

        public static void SetFedSpouse(this Farmer player, bool fed)
        {
            ModDataManager.SetHasFedSpouse(player, fed);
        }
    }
}
