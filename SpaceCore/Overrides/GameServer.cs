using SpaceCore.Events;
using StardewValley.Network;

namespace SpaceCore.Overrides
{
    public class ServerGotClickHook
    {
        public static void Postfix(GameServer __instance, long peer)
        {
            SpaceEvents.InvokeServerGotClient(__instance, peer);
        }
    }
}
