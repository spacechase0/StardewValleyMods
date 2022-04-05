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
            if (player.buffs.IsApplied("spell:life:buff"))
                return false;

            return base.CanCast(player, level);
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 25;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player.buffs.IsApplied("spell:life:buff"))
                return null;

            int l = level + 1;
            int farm = l, fish = l, mine = l, luck = l, forage = l, def = 0 /*1*/, atk = 2;
            atk = l switch
            {
                2 => 5,
                3 => 10,
                _ => atk
            };

            var effect = new StardewValley.Buffs.BuffEffects();
            effect.farmingLevel.Value = farm;
            effect.fishingLevel.Value = fish;
            effect.miningLevel.Value = mine;
            effect.luckLevel.Value = luck;
            effect.foragingLevel.Value = forage;
            effect.attack.Value = atk;
            effect.defense.Value = def;

            player.buffs.Apply(new Buff(
                buff_id: "spell:life:buff",
                duration: (60 + level * 120) / 10 * 7000,
                buff_effects: effect,
                display_source: "Buff (spell)"
            ));

            //player.buffs.Apply(new Buff(farm, fish, mine, 0, luck, forage, 0, 0, 0, 0, def, atk, 60 + level * 120, "spell:life:buff", "Buff (spell)"));
            player.AddCustomSkillExperience(Magic.Skill, 10);
            return null;
        }
    }
}
