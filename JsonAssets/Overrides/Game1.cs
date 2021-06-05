using Harmony;
using StardewValley;

namespace JsonAssets.Overrides
{
    [HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
    public static class GameLoadForNewGamePatch
    {
        public static void Prefix()
        {
            Mod.instance.onBlankSave();
        }
    }
}
