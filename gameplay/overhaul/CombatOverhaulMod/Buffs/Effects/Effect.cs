using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Buffs.Effects
{
    public abstract class Effect
    {
        public abstract string Id { get; }
        public virtual string Name => Mod.instance.Helper.Translation.Get( $"effect.{Id}.name" );
        public virtual string Description => Mod.instance.Helper.Translation.Get( $"effect.{Id}.description" );

        public abstract void Apply( Character character, float modifier );
        public abstract void Tick( Character character, float delta, float durationUsed, float duration, float modifier );
        public abstract void Unapply( Character character, float modifier );
    }
}
