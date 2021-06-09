using LocationLayerTool.Patches;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace LocationLayerTool
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            HarmonyPatcher.Apply(this,
                new xTileLayerPatcher()
            );

            this.Helper.ConsoleCommands.Add("llt_adddummy", "", this.doCommand);
        }

        private void doCommand(string cmd, string[] args)
        {
            Game1.locations.Add(new GameLocation(this.Helper.Content.GetActualAssetKey("assets/Farm_overlay.tbin"), "Farm_overlay"));
            Game1.game1.parseDebugInput("warp Farm_overlay 39 31");
        }
    }
}
