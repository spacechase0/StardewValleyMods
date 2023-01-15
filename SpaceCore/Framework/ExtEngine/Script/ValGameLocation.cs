using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miniscript;
using StardewValley;

namespace SpaceCore.Framework.ExtEngine.Script
{
    public class ValGameLocation : Value
    {
        public readonly GameLocation location;
        public ValGameLocation(GameLocation location)
        {
            this.location = location;
        }

        public override bool CanSetElem()
        {
            return false;
        }

        public override string ToString(TAC.Machine vm)
        {
            return location.ToString();
        }

        public override int Hash(int recursionDepth = 16)
        {
            return location.GetHashCode();
        }

        public override double Equality(Value rhs, int recursionDepth = 16)
        {
            return rhs is ValGameLocation && ((ValGameLocation)rhs).location == location ? 1 : 0;
        }
    }
}
