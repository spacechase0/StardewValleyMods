using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Data
{
    public abstract class DataNeedsId
    {
        public string Name { get; set; }

        internal int id = -1;
    }
}
