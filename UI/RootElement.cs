using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    public class RootElement : Container
    {
        public bool Obscured { get; set; } = false;

        internal override RootElement GetRootImpl()
        {
            return this;
        }
    }
}
