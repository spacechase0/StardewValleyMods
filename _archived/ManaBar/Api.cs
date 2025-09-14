using StardewValley;

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
        public const int BaseMaxMana = 100;

        public int GetMana(Farmer farmer)
        {
            return farmer.GetCurrentMana();
        }

        public void AddMana(Farmer farmer, int amt)
        {
            farmer.AddMana(amt);
        }

        public int GetMaxMana(Farmer farmer)
        {
            return farmer.GetMaxMana();
        }

        public void SetMaxMana(Farmer farmer, int newMaxMana)
        {
            farmer.SetMaxMana(newMaxMana);
        }
    }
}
