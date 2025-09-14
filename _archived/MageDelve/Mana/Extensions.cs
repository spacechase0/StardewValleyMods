using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageDelve.Mana
{
    public static class Extensions
    {
        public static float GetCurrentMana( this Farmer farmer )
        {
            return farmer.get_Mana().Value;
        }

        public static void SetCurrentMana( this Farmer farmer, float val )
        {
            farmer.get_Mana().Value = Math.Min( Math.Max( 0, val ), farmer.GetMaxMana() );
        }

        public static void AddMana( this Farmer farmer, float val )
        {
            farmer.SetCurrentMana( farmer.GetCurrentMana() + val );
        }

        public static float GetMaxMana( this Farmer farmer )
        {
            return farmer.get_MaxMana().Value;
        }

        public static void SetMaxMana( this Farmer farmer, float val )
        {
            farmer.get_MaxMana().Value = Math.Max( 0, val );
            if ( farmer.GetCurrentMana() > farmer.GetMaxMana() )
                farmer.SetCurrentMana( farmer.GetMaxMana() );
        }
    }
}
