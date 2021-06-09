using MoreEnchantments.Enchantments;
using MoreEnchantments.Patches;
using Spacechase.Shared.Harmony;
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
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            BaseEnchantment.GetAvailableEnchantments().Add(new MoreLuresEnchantment());

            HarmonyPatcher.Apply(this,
                new FishingRodPatcher()
            );
        }
    }
}
