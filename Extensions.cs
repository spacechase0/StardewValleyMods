using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace SpaceCore
{
    public static class Extensions
    {
        public static int countStardropsEaten(this SFarmer player)
        {
            int count = 0;
            if (Game1.player.hasOrWillReceiveMail("CF_Fair"))
                ++count;
            if (Game1.player.hasOrWillReceiveMail("CF_Fish"))
                ++count;
            if (Game1.player.hasOrWillReceiveMail("CF_Mines"))
                ++count;
            if (Game1.player.hasOrWillReceiveMail("CF_Sewer"))
                ++count;
            if (Game1.player.hasOrWillReceiveMail("museumComplete"))
                ++count;
            if (Game1.player.hasOrWillReceiveMail("CF_Spouse"))
                ++count;
            if (Game1.player.hasOrWillReceiveMail("CF_Statue"))
                ++count;
            return count;
        }
    }
}
