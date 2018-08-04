using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Events
{
    public class CancelableEventArgs
    {
        public bool Cancel { get; set; } = false;
    }
}
