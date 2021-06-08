using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;

namespace SpaceCore.Overrides
{
    /// <summary>Applies Harmony patches to <see cref="Multiplayer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class MultiplayerPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Multiplayer>(nameof(Multiplayer.processIncomingMessage)),
                prefix: this.GetHarmonyMethod(nameof(Before_ProcessIncomingMessage))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Multiplayer.processIncomingMessage"/>.</summary>
        private static bool Before_ProcessIncomingMessage(Multiplayer __instance, IncomingMessage msg)
        {
            // MTN uses packets 30, 31, and 50, PyTK uses 99

            if (msg.MessageType == 234)
            {
                string msgType = msg.Reader.ReadString();
                if (Networking.messageHandlers.ContainsKey(msgType))
                    Networking.messageHandlers[msgType].Invoke(msg);

                if (Game1.IsServer)
                {
                    foreach (var key in Game1.otherFarmers.Keys)
                        if (key != msg.FarmerID)
                            Game1.server.sendMessage(key, 234, Game1.otherFarmers[msg.FarmerID], msg.Data);
                }
            }

            return true;
        }
    }
}
