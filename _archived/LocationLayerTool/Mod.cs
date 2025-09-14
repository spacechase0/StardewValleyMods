using LocationLayerTool.Patches;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace LocationLayerTool
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            HarmonyPatcher.Apply(this,
                new xTileLayerPatcher()
            );

            this.Helper.ConsoleCommands.Add("llt_adddummy", "", this.DoCommand);
        }

        private void DoCommand(string cmd, string[] args)
        {
            Game1.locations.Add(new GameLocation(this.Helper.ModContent.GetInternalAssetName("assets/Farm_overlay.tbin").BaseName, "Farm_overlay"));
            Game1.game1.parseDebugInput("warp Farm_overlay 39 31");
        }
    }
}
