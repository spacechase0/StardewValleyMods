using StardewModdingAPI;

namespace Magic
{
    public class Configuration
    {
        public SButton Key_SwapSpells = SButton.Tab;
        public SButton Key_Cast = SButton.Q;
        public SButton Key_Spell1 = SButton.D1;
        public SButton Key_Spell2 = SButton.D2;
        public SButton Key_Spell3 = SButton.D3;
        public SButton Key_Spell4 = SButton.D4;

        public string ToilAltarLocation = "FarmCave";
        public int ToilAltarX = 5;
        public int ToilAltarY = 2;

        public string NatureAltarLocation = "Woods";
        public int NatureAltarX = 49;
        public int NatureAltarY = 28;

        public string LifeAltarLocation = "SeedShop";
        public int LifeAltarX = 36;
        public int LifeAltarY = 16;

        public string ElementalAltarLocation = "WizardHouseBasement";
        public int ElementalAltarX = 8;
        public int ElementalAltarY = 3;

        public string EldritchAltarLocation = "WitchHut";
        public int EldritchAltarX = 6;
        public int EldritchAltarY = 8;
    }
}
