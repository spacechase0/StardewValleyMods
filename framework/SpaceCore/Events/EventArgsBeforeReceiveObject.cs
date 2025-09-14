using SpaceShared;
using StardewValley;
using SObject = StardewValley.Object;

namespace SpaceCore.Events
{
    public class EventArgsBeforeReceiveObject : CancelableEventArgs
    {
        internal EventArgsBeforeReceiveObject(NPC npc, SObject o, bool probe)
        {
            this.Npc = npc;
            this.Gift = o;
            this.Probe = probe;
        }

        public NPC Npc { get; }
        public SObject Gift { get; }
        public bool Probe { get; }
    }
}
