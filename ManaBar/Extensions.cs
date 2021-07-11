using System;
using ManaBar.Framework;
using StardewValley;

namespace ManaBar
{
    /// <summary>Provides extensions on <see cref="Farmer"/> for managing mana points.</summary>
    public static class Extensions
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the player's current mana points.</summary>
        /// <param name="player">The player to check.</param>
        public static int GetCurrentMana(this Farmer player)
        {
            return ModDataManager.GetCurrentMana(player);
        }

        /// <summary>Add points to the player's mana pool.</summary>
        /// <param name="player">The player to check.</param>
        /// <param name="amt">The number of mana points to add.</param>
        public static void AddMana(this Farmer player, int amt)
        {
            int mana = player.GetCurrentMana() + amt;
            ModDataManager.SetCurrentMana(
                player,
                Math.Max(0, Math.Min(player.GetMaxMana(), mana))
            );
        }

        /// <summary>Get the player's max mana points.</summary>
        /// <param name="player">The player to check.</param>
        public static int GetMaxMana(this Farmer player)
        {
            return ModDataManager.GetMaxMana(player);
        }

        /// <summary>Set the player's max mana points.</summary>
        /// <param name="player">The player to check.</param>
        /// <param name="newCap">The value to set.</param>
        public static void SetMaxMana(this Farmer player, int newCap)
        {
            ModDataManager.SetMaxMana(player, Math.Max(0, newCap));
        }
    }
}
