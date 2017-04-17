using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeExterior
{
    public class Config
    {
        public Dictionary<string, List<string>> choices = new Dictionary< string, List< string > >();
        public TimeSpan clickWindow = new TimeSpan(250 * TimeSpan.TicksPerMillisecond);

        public Config()
        {
            var choices_ = new List<string>();
            choices_.Add("ExampleExtra");
            choices.Add("EXAMPLE_B", choices_);
        }
    }
}
