using Microsoft.Xna.Framework;
using StardewValley;

namespace Magic.Framework
{
    internal static class Extensions
    {
        /*********
        ** Public methods
        *********/
        public static int GetCurrentMana(this Farmer player)
        {
            return Mod.Mana.GetMana(player);
        }

        public static void AddMana(this Farmer player, int amt)
        {
            Mod.Mana.AddMana(player, amt);
        }

        public static int GetMaxMana(this Farmer player)
        {
            return Mod.Mana.GetMaxMana(player);
        }

        public static void SetMaxMana(this Farmer player, int newCap)
        {
            Mod.Mana.SetMaxMana(player, newCap);
        }

        /// <summary>Get a self-updating cached view of the player's magic metadata.</summary>
        public static SpellBook GetSpellBook(this Farmer player)
        {
            return Magic.GetSpellBook(player);
        }

        /// <summary>Play a local sound in a location at the given pixel position.</summary>
        /// <param name="location">The location containing the sound.</param>
        /// <param name="audioName">The audio cue name to play.</param>
        /// <param name="pixelPosition">The absolute pixel position of the sound within the location, relative to the top-left corner of the map.</param>
        public static void LocalSoundAtPixel(this GameLocation location, string audioName, Vector2 pixelPosition)
        {
            if (location == null)
                return;

            Vector2 tile = new(
                x: (int)(pixelPosition.X / Game1.tileSize),
                y: (int)(pixelPosition.Y / Game1.tileSize)
            );
            location.localSoundAt(audioName, tile);
        }

        /// <summary>Play a local sound centered on the given player.</summary>
        /// <param name="player">The player on which to center the sound.</param>
        /// <param name="audioName">The audio cue name to play.</param>
        public static void LocalSound(this Farmer player, string audioName)
        {
            player?.currentLocation.LocalSoundAtPixel(audioName, player.Position);
        }
    }
}
