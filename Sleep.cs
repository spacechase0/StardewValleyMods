using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore
{
    public class Sleep
    {
        public static bool SaveLocation { get; set; } = false;

        internal class Data
        {
            public string Location { get; set; }
            public float X { get; set; }
            public float Y { get; set; }

            public int Year { get; set; }
            public string Season { get; set; }
            public int Day { get; set; }

            public int MineLevel { get; set; }
        }
    }
}
