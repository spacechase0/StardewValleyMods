using Magic.Schools;
using StardewValley;
using System.Collections.Generic;
using System;
using SpaceCore;

namespace Magic.Spells
{
    public class LuckStealSpell : Spell
    {
        public LuckStealSpell() : base(SchoolId.Eldritch, "lucksteal")
        {
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override bool canCast(Farmer player, int level)
        {
            return base.canCast(player, level) && Game1.dailyLuck != 0.12;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            var num = Game1.random.Next(player.friendshipData.Count());
            var friendshipData = player.friendshipData[new List<string>(player.friendshipData.Keys)[num]];
            friendshipData.Points = Math.Max(0, friendshipData.Points - 250);
            Game1.dailyLuck = 0.12;
            Game1.playSound("death");
            player.AddCustomSkillExperience(Magic.Skill, 50);

            return null;
        }
    }
}
