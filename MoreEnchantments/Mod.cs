using Harmony;
using MoreEnchantments.Enchantments;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace MoreEnchantments
{
    public class Mod : StardewModdingAPI.Mod, IAssetEditor
    {
        public static Mod instance;

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Strings\\EnchantmentNames");
        }

        public void Edit<T>(IAssetData asset)
        {
            asset.AsDictionary<string, string>().Data.Add("MoreLures", "A-lure-ing");
        }

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            BaseEnchantment.GetAvailableEnchantments().Add(new MoreLuresEnchantment());

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }
}
