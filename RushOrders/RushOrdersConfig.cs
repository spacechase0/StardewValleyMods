using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushOrders
{
    public class RushOrdersConfig
    {
        public class PriceFactor_
        {
            public class Tool_
            {
                public double Rush { get; set; } = 1.5;
            }

            public Tool_ Tool { get; } = new Tool_();
        }

        public PriceFactor_ PriceFactor { get; } = new PriceFactor_();
        //public double PriceFactor_Tool_Rush { get; set; } = 1.5;
    }
}
