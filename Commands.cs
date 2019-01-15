using StardewValley;

namespace SpaceCore
{
    public static class Commands
    {
        internal static void register()
        {
            Command.register("player_giveexp", expCommand);
        }

        private static void expCommand( string[] args )
        {
            if ( args.Length != 2 )
            {
                Log.info("Usage: player_giveexp <skill> <amt>");
            }

            var skillName = args[0];
            int amt = int.Parse(args[1]);

                 if (skillName == "farming" ) Game1.player.gainExperience(Farmer.farmingSkill,  amt);
            else if (skillName == "foraging") Game1.player.gainExperience(Farmer.foragingSkill, amt);
            else if (skillName == "mining"  ) Game1.player.gainExperience(Farmer.miningSkill,   amt);
            else if (skillName == "fishing" ) Game1.player.gainExperience(Farmer.fishingSkill,  amt);
            else if (skillName == "combat"  ) Game1.player.gainExperience(Farmer.combatSkill,   amt);
            else if (skillName == "luck"    ) Game1.player.gainExperience(Farmer.luckSkill,     amt);
            else
            {
                var skill = Skills.GetSkill(skillName);
                if ( skill == null )
                {
                    Log.info("No such skill exists");
                }
                else
                {
                    Game1.player.AddCustomSkillExperience(skill, amt);
                }
            }
        }
    }
}
