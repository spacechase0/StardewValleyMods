using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeExterior
{
    public class Config
    {
        public Dictionary<string, string> chosen = new Dictionary<string, string>();
        public TimeSpan clickWindow = new TimeSpan(250 * TimeSpan.TicksPerMillisecond);
    }
}
