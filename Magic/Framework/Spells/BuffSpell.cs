using Magic.Framework.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class BuffSpell : Spell
    {
        private static readonly string BuffId = "space:spell:life:buff";
        /*********
        ** Public methods
        *********/
        public BuffSpell()
            : base(SchoolId.Life, "buff") { }

        public override bool CanCast(Farmer player, int level)
        {
            if (player == Game1.player)
            {
                if (Game1.player.buffs.IsApplied(BuffId))
                {
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

            if (Game1.player.buffs.AppliedBuffIds.Contains(BuffId)) { 
                    return null;
            }

            Game1.player.buffs.Clear();

            int l = level + 1;
            int farm = l, fish = l, mine = l, luck = l, forage = l, def = 0 /*1*/, atk = 2;
            atk = l switch
            {
                2 => 5,
                3 => 10,
                _ => atk
            };

            // TODO: Resurrect this with the new 1.6 buff framework. Losing a spell > losing the entire mod.
            //Game1.buffsDisplay.addOtherBuff(new Buff(BuffId,farm, fish, mine, 0, luck, forage, 0, 0, 0, 0, def, atk, 60 + level * 120, , "Buff (spell)"));
            player.AddCustomSkillExperience(Magic.Skill, 10);
            return null;
        }
    }
}
