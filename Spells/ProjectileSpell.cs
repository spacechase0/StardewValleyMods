using Magic.Game;
using StardewValley;
using System;

namespace Magic.Spells
{
    public class ProjectileSpell : Spell
    {
        public int ManaBase { get; }
        public int DamageBase { get; }
        public int DamageIncr { get; }
        public string Sound { get; }
        public string SoundHit { get; }

        public ProjectileSpell(string school, string id, int manaBase, int dmgBase, int dmgIncr) : this(school, id, manaBase, dmgBase, dmgIncr, null, null)
        {
        }

        public ProjectileSpell(string school, string id, int manaBase, int dmgBase, int dmgIncr, string snd, string sndHit) : base(school, id)
        {
            ManaBase = manaBase;
            DamageBase = dmgBase;
            DamageIncr = dmgIncr;
            Sound = snd;
            SoundHit = sndHit;
        }

        public override int getManaCost(Farmer player, int level)
        {
            return ManaBase;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            int dmg = (DamageBase + DamageIncr * level) * (player.CombatLevel + 1);
            float dir = ( float ) -Math.Atan2(player.getStandingY() - targetY, targetX - player.getStandingX());
            player.currentLocation.projectiles.Add(new SpellProjectile(player, this, dmg, dir, 3f + 2 * level));
            if ( Sound != null )
                Game1.playSound(Sound);

            return null;
        }
    }
}
