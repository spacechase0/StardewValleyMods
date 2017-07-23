using Harmony;
using SpaceCore.Events;
using SpaceCore.Utilities;
using StardewValley;
using StardewValley.Events;
using System;
using System.Reflection;

namespace SpaceCore.Overrides
{
    public class NewUtility
    {
        internal static void hijack( HarmonyInstance harmony )
        {
            Hijack.hijack(typeof(Utility).GetMethod("pickFarmEvent", BindingFlags.Static | BindingFlags.Public),
                          typeof(NewUtility).GetMethod("pickFarmEvent", BindingFlags.Static | BindingFlags.Public));
        }

        public static FarmEvent pickFarmEvent()
        {
            FarmEvent ret = null;
            {
                Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
                if (Game1.weddingToday)
                    ret = (FarmEvent)null;
                else if ((int)Game1.stats.DaysPlayed == 31)
                    ret = (FarmEvent)new SoundInTheNightEvent(4);
                else if (Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry"))
                    ret = (FarmEvent)new WorldChangeEvent(0);
                else if (Game1.player.mailForTomorrow.Contains("ccPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("ccPantry"))
                    ret = (FarmEvent)new WorldChangeEvent(1);
                else if (Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault"))
                    ret = (FarmEvent)new WorldChangeEvent(6);
                else if (Game1.player.mailForTomorrow.Contains("ccVault%&NL&%") || Game1.player.mailForTomorrow.Contains("ccVault"))
                    ret = (FarmEvent)new WorldChangeEvent(7);
                else if (Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom"))
                    ret = (FarmEvent)new WorldChangeEvent(2);
                else if (Game1.player.mailForTomorrow.Contains("ccBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccBoilerRoom"))
                    ret = (FarmEvent)new WorldChangeEvent(3);
                else if (Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom"))
                    ret = (FarmEvent)new WorldChangeEvent(4);
                else if (Game1.player.mailForTomorrow.Contains("ccCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccCraftsRoom"))
                    ret = (FarmEvent)new WorldChangeEvent(5);
                else if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaFishTank"))
                    ret = (FarmEvent)new WorldChangeEvent(8);
                else if (Game1.player.mailForTomorrow.Contains("ccFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("ccFishTank"))
                    ret = (FarmEvent)new WorldChangeEvent(9);
                else if (Game1.player.isMarried() && Game1.player.spouse != null && Game1.getCharacterFromName(Game1.player.spouse, false).daysUntilBirthing == 0)
                    ret = (FarmEvent)new BirthingEvent();
                else if (Game1.player.isMarried() && Game1.player.spouse != null && (Game1.getCharacterFromName(Game1.player.spouse, false).canGetPregnant() && random.NextDouble() < 0.05))
                    ret = (FarmEvent)new QuestionEvent(1);
                else if (random.NextDouble() < 0.01 && !Game1.currentSeason.Equals("winter"))
                    ret = (FarmEvent)new FairyEvent();
                else if (random.NextDouble() < 0.01)
                    ret = (FarmEvent)new WitchEvent();
                else if (random.NextDouble() < 0.01)
                    ret = (FarmEvent)new SoundInTheNightEvent(1);
                else if (random.NextDouble() < 0.01 && Game1.year > 1)
                    ret = (FarmEvent)new SoundInTheNightEvent(0);
                else if (random.NextDouble() < 0.01)
                    ret = (FarmEvent)new SoundInTheNightEvent(3);
                else if (random.NextDouble() < 0.5)
                    ret = (FarmEvent)new QuestionEvent(2);
                else
                    ret = (FarmEvent)new SoundInTheNightEvent(2);
            }

            return SpaceEvents.InvokeChooseNightlyFarmEvent( ret );
        }
    }
}
