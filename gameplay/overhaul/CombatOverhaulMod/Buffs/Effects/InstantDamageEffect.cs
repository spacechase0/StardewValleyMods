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
    public class InstantDamageEffect : InstantEffect
    {
        public bool Percentage { get; }
        public string Element { get; }

        public InstantDamageEffect( bool percentage, string element )
        {
            Percentage = percentage;
            Element = element;
        }

        public override string Id => "damage-" + ( Percentage ? "percentage" : "fixed" ) + ( Element == null ? "" : $".{Element}" ) + ".instant";

        public override void Apply( Character character, float modifier )
        {
            if ( character is Farmer farmer )
            {
                character.TakeDamage( Element, ( int ) ( Percentage ? ( farmer.maxHealth * modifier ) : modifier ), overrideParry: Element != null );
            }
            else if ( character is Monster monster )
            {
                monster.TakeDamage( Element, ( int ) ( Percentage ? ( monster.MaxHealth * modifier ) : modifier ), overrideParry: false );
            }
        }
    }
}
