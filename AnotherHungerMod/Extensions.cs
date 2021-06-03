using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherHungerMod
{
    public static class Extensions
    {
        public static double GetFullness(this Farmer player)
        {
            if (player != Game1.player)
                return -1;
            return Mod.Data.Fullness;
        }

        public static void UseFullness(this Farmer player, double amt)
        {
            if (player != Game1.player)
                return;

            Mod.Data.Fullness = Math.Max(0, Math.Min(Mod.Data.Fullness - amt, player.GetMaxFullness()));
            Mod.Data.SyncToHost();
        }

        public static int GetMaxFullness(this Farmer player)
        {
            return Mod.Config.MaxFullness;
        }

        public static bool HasFedSpouse(this Farmer player)
        {
            return Mod.Data.FedSpouseMeal;
        }

        public static void SetFedSpouse(this Farmer player, bool fed)
        {
            Mod.Data.FedSpouseMeal = fed;
            Mod.Data.SyncToHost();
        }
    }
}
