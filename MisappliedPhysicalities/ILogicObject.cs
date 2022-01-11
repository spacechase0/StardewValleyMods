using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisappliedPhysicalities
{
    public interface ILogicObject
    {
        // Signals are from 0 to 1
        public InOutType GetLogicTypeForSide( Side side );
        double GetLogicFrom( Side side );
        void SendLogicTo( Side side, double signal );
    }
}
