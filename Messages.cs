using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfingFestival
{
    public class UseItemMessage
    {
        public const string TYPE = nameof(UseItemMessage);
        public Item ItemUsed { get; set; }
    }
}
