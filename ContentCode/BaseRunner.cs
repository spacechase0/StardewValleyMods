using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace ContentCode
{
    public abstract class BaseRunner
    {
        public IContentPack ContentPack { get; internal set; }
        public IReflectionHelper Reflection { get; internal set; }
        public Dictionary<string, object> State { get; internal set; }

        public abstract void Run( object[] args );
    }
}
