using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige.Professions
{
    /// <summary>
    /// Special handling for the fighter profession, which adds 15 to the player's maximum health.
    /// </summary>
    public class FighterSpecialHandling : IProfessionSpecialHandling
    {
        public void ApplyEffect()
        {
            Logger.LogInformation("Applying fighter effect.");
            Game1.player.maxHealth += 15;
        }

        public void RemoveEffect()
        {
            Logger.LogInformation("Removing fighter effect.");
            Game1.player.maxHealth -= 15;
        }
    }
}
