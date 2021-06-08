using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceCore.Events;
using StardewModdingAPI;
using StardewValley.Network;

namespace SpaceCore.Overrides
{
    /// <summary>Applies Harmony patches to <see cref="GameServer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class GameServerPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameServer>(nameof(GameServer.sendServerIntroduction)),
                postfix: this.GetHarmonyMethod(nameof(After_SendServerIntroduction))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="GameServer.sendServerIntroduction"/>.</summary>
        private static void After_SendServerIntroduction(GameServer __instance, long peer)
        {
            SpaceEvents.InvokeServerGotClient(__instance, peer);
        }
    }
}
