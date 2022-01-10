using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Multiplayer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class MultiplayerPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Multiplayer>(nameof(Multiplayer.processIncomingMessage)),
                prefix: this.GetHarmonyMethod(nameof(Before_ProcessIncomingMessage))
            );

            harmony.Patch(
                original: this.RequireMethod<Multiplayer>( "_receivePassoutRequest" ),
                transpiler: this.GetHarmonyMethod( nameof( Transpiler__receivePassoutRequest ) )
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
                if (Networking.MessageHandlers.TryGetValue(msgType, out var handler))
                    handler.Invoke(msg);

                if (Game1.IsServer)
                {
                    foreach (long key in Game1.otherFarmers.Keys)
                    {
                        if (key != msg.FarmerID)
                            Game1.server.sendMessage(key, 234, Game1.otherFarmers[msg.FarmerID], msg.Data);
                    }
                }
            }

            return true;
        }

        public static void AlterPassoutWakeupLocation( Farmer farmer, ref string loc )
        {/*
            if ( SpaceCore.CustomLocationContexts.TryGetValue( farmer.currentLocation.GetLocationContext(), out CustomLocationContext custom ) )
            {
                string val = custom.PassoutWakeupLocation?.Invoke( farmer );
                if ( val != null )
                    loc = val;
            }*/
        }
        public static void AlterPassoutWakeupPoint( Farmer farmer, ref Point pos )
        {/*
            if ( SpaceCore.CustomLocationContexts.TryGetValue( farmer.currentLocation.GetLocationContext(), out CustomLocationContext custom ) )
            {
                Point? val = custom.PassoutWakeupPoint?.Invoke( farmer );
                if ( val.HasValue )
                    pos = val.Value;
            }*/
        }

        private static IEnumerable< CodeInstruction > Transpiler__receivePassoutRequest( IEnumerable<CodeInstruction> insns, ILGenerator ilgen )
        {
            List< CodeInstruction > ret = new();

            int serverCheckCount = 0;
            foreach ( var insn in insns )
            {
                if ( insn.Calls( AccessTools.PropertyGetter( typeof( Game1 ), nameof( Game1.IsServer ) ) ) )
                {
                    if ( ++serverCheckCount == 2 )
                    {
                        CodeInstruction[] newInsns = new[]
                        {
                            new CodeInstruction( OpCodes.Ldarg_1 ),
                            new CodeInstruction( OpCodes.Ldloca, 1 ),
                            new CodeInstruction( OpCodes.Call, AccessTools.Method( typeof( MultiplayerPatcher ), nameof( MultiplayerPatcher.AlterPassoutWakeupLocation ) ) ),
                            new CodeInstruction( OpCodes.Ldarg_1 ),
                            new CodeInstruction( OpCodes.Ldloca, 2 ),
                            new CodeInstruction( OpCodes.Call, AccessTools.Method( typeof( MultiplayerPatcher ), nameof( MultiplayerPatcher.AlterPassoutWakeupPoint ) ) ),
                        };
                        newInsns[ 0 ].labels.AddRange( insn.labels );
                        insn.labels.Clear();
                        ret.AddRange( newInsns );
                    }
                }
                ret.Add( insn );
            }

            return ret;
        }
    }
}
