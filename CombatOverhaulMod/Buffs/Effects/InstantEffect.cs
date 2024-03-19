using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Buffs.Effects
{
    public abstract class InstantEffect : Effect
    {
        public override void Tick( Character character, float delta, float durationUsed, float duration, float modifier )
        {
        }

        public override void Unapply( Character character, float modifier )
        {
        }
    }
}
