using Magic.Framework.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class BuffSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public BuffSpell()
            : base(SchoolId.Life, "buff") { }

        public override bool CanCast(Farmer player, int level)
        {
            if (player == Game1.player)
            {
                foreach (var buff in Game1.buffsDisplay.otherBuffs)
                {
                    if (buff.source == "spell:life:buff")
                        return false;
                }
            }

            return base.CanCast(player, level);
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 25;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            foreach (var buff in Game1.buffsDisplay.otherBuffs)
            {
                if (buff.source == "spell:life:buff")
                    return null;
            }

            Game1.player.removeBuffAttributes();
            Game1.player.attack = 0;

            int l = level + 1;
            int farm = l, fish = l, mine = l, luck = l, forage = l, def = 0 /*1*/, atk = 2;
            atk = l switch
            {
                2 => 5,
                3 => 10,
                _ => atk
            };

            Game1.buffsDisplay.addOtherBuff(new Buff(farm, fish, mine, 0, luck, forage, 0, 0, 0, 0, def, atk, 60 + level * 120, "spell:life:buff", "Buff (spell)"));
            player.AddCustomSkillExperience(Magic.Skill, 10);
            return null;
        }
    }
}
