using SpaceShared;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManaBar
{
    public interface IApi
    {
        int GetMana(Farmer farmer);
        void AddMana(Farmer farmer, int amt);

        int GetMaxMana(Farmer farmer);
        void SetMaxMana(Farmer farmer, int newMaxMana);
    }

    public class Api : IApi
    {
        public int GetMana(Farmer farmer)
        {
            return farmer.getCurrentMana();
        }

        public void AddMana(Farmer farmer, int amt)
        {
            farmer.addMana(amt);
        }

        public int GetMaxMana(Farmer farmer)
        {
            return farmer.getMaxMana();
        }

        public void SetMaxMana(Farmer farmer, int newMaxMana)
        {
            farmer.setMaxMana(newMaxMana);
        }
    }
}
