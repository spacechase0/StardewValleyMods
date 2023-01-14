using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miniscript;
using StardewValley;

namespace SpaceCore.Framework.ExtEngine.Script
{
    public class ValFarmer : Value
    {
        public readonly Farmer farmer;
        public ValFarmer(Farmer farmer)
        {
            this.farmer = farmer;
        }

        public override bool CanSetElem()
        {
            return false;
        }

        public override string ToString(TAC.Machine vm)
        {
            return farmer.ToString();
        }

        public override int Hash(int recursionDepth = 16)
        {
            return farmer.GetHashCode();
        }

        public override double Equality(Value rhs, int recursionDepth = 16)
        {
            return rhs is ValFarmer && ((ValFarmer)rhs).farmer == farmer ? 1 : 0;
        }
    }
}
