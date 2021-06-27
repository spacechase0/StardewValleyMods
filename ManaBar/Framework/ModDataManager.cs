using System.Globalization;
using StardewValley;

namespace ManaBar.Framework
{
    /// <summary>Encapsulates reading and writing values in players' <see cref="Character.modData"/> field.</summary>
    internal static class ModDataManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>The prefix added to mod data keys.</summary>
        private const string Prefix = "spacechase0.ManaBar";

        /// <summary>The data key for the player's current mana points.</summary>
        private const string CurrentManaKey = ModDataManager.Prefix + "/CurrentMana";

        /// <summary>The data key for the player's max mana points.</summary>
        private const string MaxManaKey = ModDataManager.Prefix + "/MaxMana";


        /*********
        ** Public methods
        *********/
        /// <summary>Get a player's current mana points.</summary>
        /// <param name="player">The player to check.</param>
        public static int GetCurrentMana(Farmer player)
        {
            return player.modData.TryGetValue(ModDataManager.CurrentManaKey, out string raw) && int.TryParse(raw, out int mana) && mana > 0
                ? mana
                : 0;
        }

        /// <summary>Set a player's current mana points.</summary>
        /// <param name="player">The player to check.</param>
        /// <param name="mana">The value to set.</param>
        public static void SetCurrentMana(Farmer player, int mana)
        {
            if (mana <= 0)
                player.modData.Remove(ModDataManager.CurrentManaKey);
            else
                player.modData[ModDataManager.CurrentManaKey] = mana.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Get a player's max mana points.</summary>
        /// <param name="player">The player to check.</param>
        public static int GetMaxMana(Farmer player)
        {
            return player.modData.TryGetValue(ModDataManager.MaxManaKey, out string raw) && int.TryParse(raw, out int mana) && mana > 0
                ? mana
                : 0;
        }

        /// <summary>Set a player's max mana points.</summary>
        /// <param name="player">The player to check.</param>
        /// <param name="mana">The value to set.</param>
        public static void SetMaxMana(Farmer player, int mana)
        {
            if (mana <= 0)
                player.modData.Remove(ModDataManager.MaxManaKey);
            else
                player.modData[ModDataManager.MaxManaKey] = mana.ToString(CultureInfo.InvariantCulture);
        }
    }
}
