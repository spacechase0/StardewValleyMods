using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige.Professions
{
    /// <summary>
    /// Special handling for the defender profession, which adds 25 to the player's maximum health.
    /// </summary>
    public class DefenderSpecialHandling : IProfessionSpecialHandling
    {
        public void ApplyEffect()
        {
            Logger.LogInformation("Applying defender effect.");
            Game1.player.maxHealth += 25;
        }

        public void RemoveEffect()
        {
            Logger.LogInformation("Removing defender effect.");
            Game1.player.maxHealth -= 25;
        }
    }
}
