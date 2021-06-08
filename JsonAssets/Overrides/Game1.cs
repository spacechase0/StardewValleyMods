using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;

namespace JsonAssets.Overrides
{
    /// <summary>Applies Harmony patches to <see cref="Game1"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class Game1Patcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.loadForNewGame)),
                prefix: this.GetHarmonyMethod(nameof(Before_LoadForNewGame))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Game1.loadForNewGame"/>.</summary>
        private static void Before_LoadForNewGame()
        {
            Mod.instance.onBlankSave();
        }
    }
}
