using System;
using StardewValley;

namespace ManaBar
{
    public static class Extensions
    {
        private static void dataCheck(Farmer player)
        {
            if (!Mod.Data.players.ContainsKey(player.UniqueMultiplayerID))
                Mod.Data.players.Add(player.UniqueMultiplayerID, new MultiplayerSaveData.PlayerData());
        }

        public static int getCurrentMana(this Farmer player)
        {
            Extensions.dataCheck(player);
            return Mod.Data.players[player.UniqueMultiplayerID].mana;
        }

        public static void addMana(this Farmer player, int amt)
        {
            Extensions.dataCheck(player);
            Mod.Data.players[player.UniqueMultiplayerID].mana = Math.Max(0, Math.Min(player.getCurrentMana() + amt, player.getMaxMana()));
            if (player == Game1.player)
                Mod.Data.syncMineMini();
        }

        public static int getMaxMana(this Farmer player)
        {
            Extensions.dataCheck(player);
            return Mod.Data.players[player.UniqueMultiplayerID].manaCap;
        }

        public static void setMaxMana(this Farmer player, int newCap)
        {
            Extensions.dataCheck(player);
            Mod.Data.players[player.UniqueMultiplayerID].manaCap = newCap;
            if (player == Game1.player)
                Mod.Data.syncMineMini();
        }
    }
}
