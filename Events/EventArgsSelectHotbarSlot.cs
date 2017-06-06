using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Events
{
    public class EventArgsSelectHotbarSlot : EventArgs
    {
        public int Slot { get; set; }
        public bool Canceled { get; set; } = false;
    }
}
