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
        public static Mod instance;
        private static JsonAssetsAPI ja;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;

            HarmonyPatcher.Apply(this,
                new ObjectPatcher(getStatueId: () => ja.GetBigCraftableId("Statue of Generosity"))
            );
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ja = this.Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            ja.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets"));
        }
    }
}
