using System;
using ManaBar.Framework;
using StardewValley;

namespace ManaBar
{
    public static class Extensions
    {
        private static void DataCheck(Farmer player)
        {
            if (!Mod.Data.Players.ContainsKey(player.UniqueMultiplayerID))
                Mod.Data.Players.Add(player.UniqueMultiplayerID, new MultiplayerSaveData.PlayerData());
        }

        public static int GetCurrentMana(this Farmer player)
        {
            Extensions.DataCheck(player);
            return Mod.Data.Players[player.UniqueMultiplayerID].Mana;
        }

        public static void AddMana(this Farmer player, int amt)
        {
            Extensions.DataCheck(player);
            Mod.Data.Players[player.UniqueMultiplayerID].Mana = Math.Max(0, Math.Min(player.GetCurrentMana() + amt, player.GetMaxMana()));
            if (player == Game1.player)
                Mod.Data.SyncMineMini();
        }

        public static int GetMaxMana(this Farmer player)
        {
            Extensions.DataCheck(player);
            return Mod.Data.Players[player.UniqueMultiplayerID].ManaCap;
        }

        public static void SetMaxMana(this Farmer player, int newCap)
        {
            Extensions.DataCheck(player);
            Mod.Data.Players[player.UniqueMultiplayerID].ManaCap = newCap;
            if (player == Game1.player)
                Mod.Data.SyncMineMini();
        }
    }
}
