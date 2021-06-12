using StardewValley;

namespace SpaceShared.APIs
{
    internal interface ManaBarAPI
    {
        int GetMana(Farmer farmer);
        void AddMana(Farmer farmer, int amt);

        int GetMaxMana(Farmer farmer);
        void SetMaxMana(Farmer farmer, int newMaxMana);
    }
}
