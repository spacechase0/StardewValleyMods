using Magic.Framework.Schools;
using StardewValley;

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
            return 0;
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

            if (!this.DoesDisenchant && diff * obj.Stack > Game1.player.Money)
                return null;

            obj.Quality = one.Quality;
            Game1.player.Money -= diff * obj.Stack;

            return null;
        }
    }
}
