using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Overrides
{
    public class MultiplayerPackets
    {
        public static bool Prefix(Multiplayer __instance, IncomingMessage msg)
        {
            // MTN uses packets 30, 31, and 50, PyTK uses 99
            
            if ( msg.MessageType == 234 )
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
