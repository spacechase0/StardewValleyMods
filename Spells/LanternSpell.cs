using Entoarox.Framework;
using StardewModdingAPI.Events;
using Magic.Schools;
using StardewValley;
using System;

namespace Magic.Spells
{
    class LanternSpell : Spell
    {
        public LanternSpell() : base( SchoolId.Nature, "lantern" )
        {
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 0;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return;

            if ( lightMod != null )
            {
                return;
            }

            lightDur = 60 * 6 + level * 2 * 6;
            lightMod = makeLightModifier( level );
            
            Mod.instance.Helper.Player().Modifiers.Add(lightMod);
            GameEvents.UpdateTick += waitForLightExpiration;
            player.addMagicExp(5 + level * 5);
        }

        private int lightDur = -1;
        private PlayerModifier lightMod;
        private void waitForLightExpiration(object sender, EventArgs args)
        {
            if (--lightDur == 0)
            {
                Mod.instance.Helper.Player().Modifiers.Remove(lightMod);

                // Tmep. fix for a bug due to Entoarox Framework
                var f = new PlayerModifier();
                Mod.instance.Helper.Player().Modifiers.Add(f);
                Mod.instance.Helper.Player().Modifiers.Remove(f);

                lightMod = null;
                GameEvents.UpdateTick -= waitForLightExpiration;
            }
        }

        private static PlayerModifier makeLightModifier( int level )
        {
            int power = 4;
            if (level == 1)
                power = 8;
            else if (level == 2)
                power = 16;

            var ret = new PlayerModifier();
            ret.GlowDistance = power;
            return ret;
        }
    }
}
