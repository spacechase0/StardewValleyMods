using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Events
{
    public class EventArgsCancelable : EventArgs
    {
        public bool Canceled { get; set; } = false;
    }
}
