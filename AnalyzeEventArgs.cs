using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic
{
    public class AnalyzeEventArgs
    {
        public int TargetX;
        public int TargetY;

        public AnalyzeEventArgs(int tx, int ty)
        {
            TargetX = tx;
            TargetY = ty;
        }
    }
}
