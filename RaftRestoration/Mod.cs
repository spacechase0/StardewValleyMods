using RaftRestoration.Patches;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley.Tools;

namespace RaftRestoration
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Display.MenuChanged += this.onMenuChanged;

            HarmonyPatcher.Apply(this,
                new FarmerPatcher(),
                new RaftPatcher(),
                new ToolPatcher()
            );
        }

        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shop)
            {
                if (shop.portraitPerson?.Name != "Pierre")
                    return;

                var raft = new Raft();
                shop.forSale.Add(raft);
                shop.itemPriceAndStock.Add(raft, new[] { 5000, 1 });
            }
        }
    }
}
