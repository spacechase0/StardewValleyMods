using System;
using System.Collections.Generic;

namespace CustomizeExterior
{
    public class Config
    {
        public Dictionary<string, string> chosen = new Dictionary<string, string>();
        public TimeSpan clickWindow = new TimeSpan(250 * TimeSpan.TicksPerMillisecond);
    }
}
