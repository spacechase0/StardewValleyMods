using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Buffs
{
    public static class Extensions
    {
        public static CustomBuffInstance[] GetBuffs( this Character character )
        {
            return character.get_Buffs().ToArray();
        }

        public static void AddBuff( this Character character, CustomBuffInstance buff )
        {
            character.get_Buffs().Add( buff );
            buff.Apply( character );
            if (character == Game1.player)
                Game1.player.buffs.Apply(new DummyBuff(buff));
        }

        public static void TickBuffs( this Character character, GameTime time )
        {
            foreach ( var buff in character.GetBuffs() )
            {
                buff.Tick( character, time );
                if ( buff.DurationUsed > buff.Duration )
                    character.RemoveBuff( buff );
            }
        }

        public static void RemoveBuff( this Character character, CustomBuffInstance buff )
        {
            character.get_Buffs().Remove( buff );
            buff.Unapply( character );

            if (character == Game1.player)
                Game1.player.buffs.Remove(Game1.player.buffs.AppliedBuffs.First(b => b.Value is DummyBuff dummy && dummy.CustomBuff == buff).Key);
        }
    }
}
