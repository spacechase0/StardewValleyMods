using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace MajesticArcana
{
    internal static class AlchemyRecipes
    {
        public static void Init()
        {
            Mod.instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private static void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MajesticArcana/Alchemy"))
                e.LoadFrom(() => GetDefaultRecipes(), StardewModdingAPI.Events.AssetLoadPriority.Low);
        }

        public static Dictionary<string, string[]> Get()
        {
            return GetDefaultRecipes();// Game1.content.Load<Dictionary<string, string[]>>("spacechase0.MajesticArcana/Alchemy");
        }
        internal static Dictionary<string, string[]> GetDefaultRecipes()
        {
            string moteOfMagicId = "(O)74"; // TODO
            string manaPotionId = "(O)773"; // TODO

            Dictionary<string, string[]> ret = new();
            ret.Add(moteOfMagicId, new string[]
            {
                "(O)768",
                "(O)769",
                "(O)771",
                "(O)766",
                "(O)82",
                "(O)444"
            });
            ret.Add("(O)768x3", new string[] { "(O)769", "(O)769", "(O)769", moteOfMagicId });
            ret.Add("(O)769x3", new string[] { "(O)768", "(O)768", "(O)768", moteOfMagicId });
            ret.Add("(O)771x3", new string[] { "(O)330", "(O)330", "(O)330", moteOfMagicId });
            ret.Add("(O)766x5", new string[] { "-4", "-4", "-4", "-4", "-4", moteOfMagicId });
            ret.Add("(O)82x5", new string[] { "(O)80", "(O)80", "(O)80", "(O)80", "(O)80", moteOfMagicId });
            ret.Add("(O)444x3", new string[] { "(O)442", "(O)442", "(O)442", moteOfMagicId });
            ret.Add(manaPotionId, new string[] { "(O)422", moteOfMagicId });

            return ret;
        }
    }
}
