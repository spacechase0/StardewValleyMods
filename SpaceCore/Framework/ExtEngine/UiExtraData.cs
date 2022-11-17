using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miniscript;

namespace SpaceCore.Framework.ExtEngine
{
    internal class UiExtraData
    {
        public string Id { get; set; }
        public string OnClickFunction { get; set; }

        public Value ScriptData { get; set; }
    }
}
