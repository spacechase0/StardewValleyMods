using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Events
{
    public class EventArgsGiftGiven
    {
        internal EventArgsGiftGiven(NPC npc, StardewValley.Object o)
        {
            Npc = npc;
            Gift = o;
        }

        public NPC Npc { get; }
        public StardewValley.Object Gift { get; }
    }
}
