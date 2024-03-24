using System;
using Magic.Framework.Schools;
using SpaceCore;
using StardewValley;
using Object = StardewValley.Object;

namespace Magic.Framework.Spells
{
    internal class EnchantCostlySpell : Spell
    {
        /*********
        ** Accessors
        *********/
        public bool DoesDisenchant { get; }


        /*********
        ** Public methods
        *********/
        public EnchantCostlySpell(bool dis)
            : base(SchoolId.Arcane, dis ? "disenchant_costly" : "enchant_costly")
        {
            this.DoesDisenchant = dis;
        }

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

            //Balance the skill out a tiny bit. Enchanting Costs x3 the money it would go up. Disenchanting only gives back half the money.
            if (!this.DoesDisenchant)
            {
                //Make it so upgrading Quality costs x3 the difference in price
                diff = diff*2;
            } else
            {
                //Make it so lowering quality only refunds 50% of the difference in price
                diff = (int)Math.Floor(diff*0.5);
            }

            if (!this.DoesDisenchant && diff * obj.Stack > Game1.player.Money)
                return null;

            obj.Quality = one.Quality;
            Game1.player.Money -= diff * obj.Stack;

            //As it costs the player Mana to cast this spell, reward 5 exp
            player.AddCustomSkillExperience(Magic.Skill, 5);

            return null;
        }
    }
}
