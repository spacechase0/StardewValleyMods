using Microsoft.Xna.Framework;
using Magic.Schools;
using StardewValley;
using SFarmer = StardewValley.Farmer;
using System.Collections.Generic;
using System;

namespace Magic.Spells
{
    public class LuckStealSpell : Spell
    {
        public LuckStealSpell() : base(SchoolId.Eldritch, "lucksteal")
        {
        }

        public override int getManaCost(SFarmer player, int level)
        {
            return 0;
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override bool canCast(SFarmer player, int level)
        {
            return base.canCast(player, level) && Game1.dailyLuck != 0.12;
        }

        public override void onCast(SFarmer player, int level, int targetX, int targetY)
        {
            Log.debug(player.name + " casted Luck Steal.");
            var num = Game1.random.Next(player.friendships.Count);
            var friendshipData = player.friendships[new List<string>(player.friendships.Keys)[num]];
            friendshipData[0] = Math.Max(0, friendshipData[0] - 250);
            Game1.dailyLuck = 0.12;
            Game1.playSound("death");
            player.addMagicExp(50);
        }
    }
}
