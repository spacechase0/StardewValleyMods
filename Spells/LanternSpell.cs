using System;
using Magic.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Spells
{
    class LanternSpell : Spell
    {
        private readonly Func<long> getNewId;

        public LanternSpell(Func<long> getNewId)
            : base( SchoolId.Nature, "lantern" )
        {
            this.getNewId = getNewId;
        }

        public override int getManaCost(Farmer player, int level)
        {
            return level * 3;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            int power = 4;
            if (level == 1)
                power = 8;
            else if (level == 2)
                power = 16;

            player.currentLocation.sharedLights.Add(getUnusedLightSourceID(player.currentLocation), new LightSource(1, Game1.player.position, power));
            player.AddCustomSkillExperience(Magic.Skill, level);

            return null;
        }

        private int getUnusedLightSourceID(GameLocation location)
        {
            while (true)
            {
                int id = (int)this.getNewId();
                if (!location.hasLightSource(id))
                    return id;
            }
        }
    }
}
