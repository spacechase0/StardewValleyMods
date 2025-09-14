using CombatOverhaulMod.Elements;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Buffs.Effects
{
    public class InstantStaminaEffect : InstantEffect
    {
        public bool Percentage { get; }

        public InstantStaminaEffect( bool percentage )
        {
            Percentage = percentage;
        }

        public override string Id => "stamina-" + ( Percentage ? "percentage" : "fixed" ) + ".instant";

        public override void Apply( Character character, float modifier )
        {
            if ( character is Farmer farmer )
            {
                farmer.Stamina = Math.Min( Math.Max( 0, farmer.Stamina + ( Percentage ? ( farmer.MaxStamina * modifier ) : modifier ) ), farmer.MaxStamina );
            }
        }
    }
}
