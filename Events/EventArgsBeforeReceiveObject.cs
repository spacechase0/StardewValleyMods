using SpaceShared;
using StardewValley;
using SObject = StardewValley.Object;

namespace SpaceCore.Events
{
    public class EventArgsBeforeReceiveObject : CancelableEventArgs
    {
        internal EventArgsBeforeReceiveObject( NPC npc, SObject o )
        {
            Npc = npc;
            Gift = o;
        }

        public NPC Npc { get; }
        public SObject Gift { get; }
    }
}
