using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miniscript;
using StardewValley;

namespace SpaceCore.Framework.ExtEngine.Script
{
    public class ValTAS : Value
    {
        public readonly TemporaryAnimatedSprite tas;
        public ValTAS(TemporaryAnimatedSprite tas)
        {
            this.tas = tas;
        }

        public override bool CanSetElem()
        {
            return false;
        }

        public override string ToString(TAC.Machine vm)
        {
            return tas.ToString();
        }

        public override int Hash(int recursionDepth = 16)
        {
            return tas.GetHashCode();
        }

        public override double Equality(Value rhs, int recursionDepth = 16)
        {
            return rhs is ValTAS && ((ValTAS)rhs).tas == tas ? 1 : 0;
        }
    }
}
