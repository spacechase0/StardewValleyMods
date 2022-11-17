using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miniscript;
using SpaceCore.UI;

namespace SpaceCore.Framework.ExtEngine.Script
{
    // From Farmtronics: https://github.com/JoeStrout/Farmtronics/blob/b10b837d18f14cbcd38d75680d0410dee3a66064/Farmtronics/M1/M1API.cs#L1834
    public class ValUiElement : Value
    {
        public readonly Element Element;

        public ValUiElement(Element content)
        {
            this.Element = content;
        }

        public override string ToString(TAC.Machine vm)
        {
            return Element.ToString();
        }

        public override int Hash(int recursionDepth = 16)
        {
            return Element.GetHashCode();
        }

        public override double Equality(Value rhs, int recursionDepth = 16)
        {
            return rhs is ValUiElement && ((ValUiElement)rhs).Element == Element ? 1 : 0;
        }
    }
}
