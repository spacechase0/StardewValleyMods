using Harmony;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
