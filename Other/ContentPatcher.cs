using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Other.ContentPatcher
{
    public class IdToken
    {
        public string Type { get; }
        public string TokenName { get; }
        private Func<IDictionary<string, int>> idsFunc;
        private IDictionary<string, int> ids;
        private int oldGen = -1;

        public IdToken(string type, Func<IDictionary<string, int>> theIdsFunc)
        {
            Type = type;
            TokenName = Type + "Id";
            idsFunc = theIdsFunc;
        }

        public bool IsReady()
        {
            return ContentPatcherIntegration.idsAssigned;
        }

        public bool UpdateContext()
        {
            if (oldGen != ContentPatcherIntegration.idsAssignedGen)
            {
                oldGen = ContentPatcherIntegration.idsAssignedGen;
                ids = idsFunc();
                return true;
            }
            return false;
        }

        public IEnumerable<string> GetValue(string input)
        {
            if (!IsReady())
                return new string[0];

            if (input == "")
                return ids.Values.Select((i) => i.ToString()).ToArray<string>();
            if (!ids.ContainsKey(input))
                return new string[0];
            return new string[] { ids[input].ToString() };
        }
    }

    public class ContentPatcherIntegration
    {
        private static ContentPatcherAPI cp;
        private static IApi ja;

        internal static bool idsAssigned = false;
        internal static int idsAssignedGen = -1;

        private static List<IdToken> tokens;

        public static void Initialize()
        {
            cp = Mod.instance.Helper.ModRegistry.GetApi<ContentPatcherAPI>("Pathoschild.ContentPatcher");
            ja = Mod.instance.Helper.ModRegistry.GetApi<IApi>("spacechase0.JsonAssets");
            if (cp == null)
                return;

            ja.IdsAssigned += (s, e) => idsAssigned = true;
            ja.IdsAssigned += (s, e) => idsAssignedGen++;
            Mod.instance.Helper.Events.GameLoop.ReturnedToTitle += (s, e) => idsAssigned = false;

            tokens = new List<IdToken>();
            tokens.Add(new IdToken("Object", ja.GetAllObjectIds));
            tokens.Add(new IdToken("BigCraftable", ja.GetAllBigCraftableIds));
            tokens.Add(new IdToken("Crop", ja.GetAllCropIds));
            tokens.Add(new IdToken("FruitTree", ja.GetAllFruitTreeIds));
            tokens.Add(new IdToken("Hat", ja.GetAllHatIds));
            tokens.Add(new IdToken("Weapon", ja.GetAllWeaponIds));

            foreach (var token in tokens)
            {
                cp.RegisterToken(Mod.instance.ModManifest, token.TokenName, token.UpdateContext, token.IsReady, token.GetValue, true, true);
            }
        }
    }
}
