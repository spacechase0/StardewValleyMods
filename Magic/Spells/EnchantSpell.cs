using Magic.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Spells
{
    public class EnchantSpell : Spell
    {
        public bool DoesDisenchant { get; }

        public EnchantSpell(bool dis) : base(SchoolId.Arcane, dis ? "disenchant" : "enchant")
        {
            DoesDisenchant = dis;
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            var obj = Game1.player.ActiveObject;
            if (obj == null || obj.bigCraftable.Value)
                return null;
            if ( !DoesDisenchant && obj.Quality == 4 ||
                  DoesDisenchant && obj.Quality == 0 )
                return null;

            var one = (Object) obj.getOne();
            int oldPrice = one.sellToStorePrice();
            if ( !DoesDisenchant )
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
            
            if (!DoesDisenchant && diff * obj.Stack > Game1.player.Money)
                return null;

            obj.Quality = one.Quality;
            Game1.player.Money -= diff * obj.Stack;

            return null;
        }
    }
}
