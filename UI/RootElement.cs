using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    public class RootElement : Container
    {
        internal override RootElement GetRootImpl()
        {
            return this;
        }
    }
}
