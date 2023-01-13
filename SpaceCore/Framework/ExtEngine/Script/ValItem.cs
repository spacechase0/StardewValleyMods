using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miniscript;
using StardewValley;

namespace SpaceCore.Framework.ExtEngine.Script
{
    public class ValItem : Value
    {
        public readonly Item item;
        public ValItem(Item content)
        {
            this.item = content;
        }

        public override bool CanSetElem()
        {
            return false;
        }

        public override string ToString(TAC.Machine vm)
        {
            return item.ToString();
        }

        public override int Hash(int recursionDepth = 16)
        {
            return item.GetHashCode();
        }

        public override double Equality(Value rhs, int recursionDepth = 16)
        {
            return rhs is ValItem && ((ValItem)rhs).item == item ? 1 : 0;
        }
    }
}
