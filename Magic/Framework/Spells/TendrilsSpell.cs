using System.Linq;
using Magic.Framework.Schools;
using Magic.Framework.Spells.Effects;
using Microsoft.Xna.Framework;
using SpaceCore;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Framework.Spells
{
    // TODO: Change into trap?
    internal class TendrilsSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public TendrilsSpell()
            : base(SchoolId.Nature, "tendrils") { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            TendrilGroup tendrils = new TendrilGroup();
            foreach (var npc in player.currentLocation.characters)
            {
                if (npc is Monster mob)
                {
                    float rad = Game1.tileSize;
                    int dur = 11 * 60;
                    if (Vector2.Distance(mob.position, new Vector2(targetX, targetY)) <= rad)
                    {
                        tendrils.Add(new Tendril(mob, new Vector2(targetX, targetY), rad, dur));
                        player.AddCustomSkillExperience(Magic.Skill, 3);
                    }
                }
            }

            return tendrils.Any()
                ? tendrils
                : null;
        }
    }
}
