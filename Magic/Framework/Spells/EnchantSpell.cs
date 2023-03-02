using System;
using Magic.Framework.Schools;
using SpaceCore;
using StardewValley;
using Object = StardewValley.Object;

namespace Magic.Framework.Spells
{
    internal class EnchantSpell : Spell
    {
        /*********
        ** Accessors
        *********/
        public bool DoesDisenchant { get; }


        /*********
        ** Public methods
        *********/
        public EnchantSpell(bool dis)
            : base(SchoolId.Arcane, dis ? "disenchant" : "enchant")
        {
            this.DoesDisenchant = dis;
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override int GetMaxCastingLevel()
        {
            return 3;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            var obj = Game1.player.ActiveObject;
            if (obj == null || obj.bigCraftable.Value)
                return null;
            if (!this.DoesDisenchant && obj.Quality == 4 || this.DoesDisenchant && obj.Quality == 0)
                return null;

            var one = (Object)obj.getOne();
            int oldPrice = one.sellToStorePrice();
            if (!this.DoesDisenchant)
            {
                if (++one.Quality == 3)
                    ++one.Quality;
            }
            else
            {
                if (--one.Quality == 3)
                    --one.Quality;
            }
            int newPrice = one.sellToStorePrice();
            int diff = newPrice - oldPrice;

            //Balance the skill out a tiny bit. Enchanting Costs x2 the money it would go up. Disenchanting only gives back half the money.
            if (!this.DoesDisenchant)
            {
                //Base level costs the player x4 the difference
                //level 2 costs the player x3 the difference
                //level 3 costs the player x2 the difference
                diff = diff*(5-level);
            } else
            {
                //Base level gives the player 40% the gold when disenchanting
                //level 2 gives the player 60% of the gold
                //level 3 gives the player 80% of the gold
                double multiplier = 0.2 + (level * 0.2);
                diff = (int)Math.Floor(diff* multiplier);
            }

            if (!this.DoesDisenchant && diff * obj.Stack > Game1.player.Money)
                return null;

            obj.Quality = one.Quality;
            Game1.player.Money -= diff * obj.Stack;
            if (!this.DoesDisenchant)
            {
                // As this costs money and mana to cast, it now gives the player EXP.
                player.AddCustomSkillExperience(Magic.Skill, 5);
            }

            return null;
        }
    }
}
