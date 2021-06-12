using System.IO;
using Spacechase.Shared.Harmony;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StatueOfGenerosity.Patches;

namespace StatueOfGenerosity
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        private static IJsonAssetsApi Ja;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            HarmonyPatcher.Apply(this,
                new ObjectPatcher(getStatueId: () => Mod.Ja.GetBigCraftableId("Statue of Generosity"))
            );
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Mod.Ja = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            Mod.Ja.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets"));
        }
    }
}
