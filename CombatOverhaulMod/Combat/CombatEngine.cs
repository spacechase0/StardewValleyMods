using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Monsters;

namespace CombatOverhaulMod.Combat
{
    internal class CombatEngine
    {
        public static Monster lastHitMonster;
        public static int hitCount = 0;

        private static float combatTimer;

        public CombatEngine()
        {
            Mod.instance.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
        }

        public static bool IsInCombat() => combatTimer > 0;
        public static void ResetCombatTimer()
        {
            combatTimer = 5;
            var buff = Game1.player.buffs.AppliedBuffs.FirstOrDefault(b => b.Key == "in_combat").Value;
            if (buff != null)
                buff.millisecondsDuration = (int)(combatTimer * 1000);
            else
            {
                var be = new BuffEffects();
                be.Speed.Value = 2;
                buff = new Buff("in_combat", "In Combat", I18n.Buff_InCombat(), (int)(combatTimer * 1000), icon_sheet_index: 0, buff_effects: be );
                Game1.player.applyBuff(buff);
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.shouldTimePass())
                return;

            if (combatTimer > 0)
                combatTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
