using HarmonyLib;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SpennyDeluxe
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            var harmony = new Harmony(ModManifest.UniqueID);
            new SpriteBatchPatcher().Apply(harmony, Monitor);
            harmony.PatchAll();
        }
    }
}
