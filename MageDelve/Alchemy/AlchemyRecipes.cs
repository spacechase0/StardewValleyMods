using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace MageDelve.Alchemy
{
    internal static class AlchemyRecipes
    {
        public static void Init()
        {
            Mod.instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private static void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MageDelve/Alchemy"))
                e.LoadFrom(() => GetDefaultRecipes(), StardewModdingAPI.Events.AssetLoadPriority.Low);
        }

        public static Dictionary<string, string[]> Get()
        {
            return Game1.content.Load<Dictionary<string, string[]>>("spacechase0.MageDelve/Alchemy");
        }
        internal static Dictionary<string, string[]> GetDefaultRecipes()
        {
            string manaPotionId = "(O)773"; // TODO

            Dictionary<string, string[]> ret = new();
            ret.Add(manaPotionId, new string[] { "(O)768", "(O)769", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_AirEssence", "(O)spacechase0.MageDelve_WaterEssence", "(O)spacechase0.MageDelve_FireEssence" });
            ret.Add("(O)390/10", new string[] { "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence" });
            ret.Add("(O)766/10", new string[] { "(O)spacechase0.MageDelve_WaterEssence", "(O)spacechase0.MageDelve_WaterEssence", "(O)spacechase0.MageDelve_WaterEssence" });
            ret.Add("(O)330/10", new string[] { "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_WaterEssence" });
            ret.Add("(O)382/5", new string[] { "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_FireEssence", "(O)spacechase0.MageDelve_FireEssence" });
            ret.Add("(O)535/1", new string[] { "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_AirEssence", "(O)spacechase0.MageDelve_AirEssence", "(O)spacechase0.MageDelve_AirEssence" });
            ret.Add("(O)536/1", new string[] { "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_WaterEssence", "(O)spacechase0.MageDelve_WaterEssence", "(O)spacechase0.MageDelve_WaterEssence" });
            ret.Add("(O)537/1", new string[] { "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_EarthEssence", "(O)spacechase0.MageDelve_FireEssence", "(O)spacechase0.MageDelve_FireEssence", "(O)spacechase0.MageDelve_FireEssence" });
            ret.Add("(O)709/3", new string[] { "(O)spacechase0.MageDelve_EarthEssence", "(O)388", "(O)388", "(O)388", "(O)388", "(O)388" });

            // TODO: Transmute metal recipes

            return ret;
        }
    }
}
