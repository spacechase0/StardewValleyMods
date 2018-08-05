using Magic.Game;
using StardewValley;
using System;
using SFarmer = StardewValley.Farmer;

namespace Magic.Spells
{
    public class ProjectileSpell : Spell
    {
        public int ManaBase { get; }
        public int ManaIncr { get; }
        public int DamageBase { get; }
        public int DamageIncr { get; }
        public string Sound { get; }
        public string SoundHit { get; }

        public ProjectileSpell(string school, string id, int manaBase, int manaIncr, int dmgBase, int dmgIncr) : this(school, id, manaBase, manaIncr, dmgBase, dmgIncr, null, null)
        {
        }

        public ProjectileSpell(string school, string id, int manaBase, int manaIncr, int dmgBase, int dmgIncr, string snd, string sndHit) : base(school, id)
        {
            ManaBase = manaBase;
            ManaIncr = manaIncr;
            DamageBase = dmgBase;
            DamageIncr = dmgIncr;
            Sound = snd;
            SoundHit = sndHit;
        }

        public override int getManaCost(SFarmer player, int level)
        {
            return ManaBase + ManaIncr * level;
        }

        public override void onCast(SFarmer player, int level, int targetX, int targetY)
        {
            Log.debug(player.Name + " casted " + Id + ".");

            int dmg = DamageBase + DamageIncr * level;
            float dir = ( float ) -Math.Atan2(player.getStandingY() - targetY, targetX - player.getStandingX());
            player.currentLocation.projectiles.Add(new SpellProjectile(player, this, dmg, dir, 3f + 2 * level));
            if ( Sound != null )
                Game1.playSound(Sound);
        }
    }
}
